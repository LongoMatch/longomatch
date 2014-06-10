//
//  Copyright (C) 2010 Andoni Morales Alastruey
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using LongoMatch.Interfaces;
using System.Reflection;
using LongoMatch.Store.Templates;
using Newtonsoft.Json.Converters;
using LongoMatch.Store;
using System.Globalization;
using System.Text;

namespace LongoMatch.Common
{
	public class Serializer
	{
		public static void Save<T>(T obj, Stream stream,
		                           SerializationType type=SerializationType.Json) {
			switch (type) {
			case SerializationType.Binary:
				BinaryFormatter formatter = new  BinaryFormatter();
				formatter.Serialize(stream, obj);
				break;
			case SerializationType.Xml:
				XmlSerializer xmlformatter = new XmlSerializer(typeof(T));
				xmlformatter.Serialize(stream, obj);
				break;
			case SerializationType.Json:
				StreamWriter sw = new StreamWriter (stream);
				sw.Write (JsonConvert.SerializeObject (obj, JsonSettings));
				sw.Flush();
				break;
			}
		}
		
		public static void Save<T>(T obj, string filepath,
		                           SerializationType type=SerializationType.Json) {
			Stream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None);
			using (stream) {
				Save<T> (obj, stream, type);
				stream.Close();
			}
		}

		public static T Load<T>(Stream stream,
		                        SerializationType type=SerializationType.Json) {
			switch (type) {
			case SerializationType.Binary:
				BinaryFormatter formatter = new BinaryFormatter();
				return (T)formatter.Deserialize(stream);
			case SerializationType.Xml:
				XmlSerializer xmlformatter = new XmlSerializer(typeof(T));
				return (T) xmlformatter.Deserialize(stream);
			case SerializationType.Json:
				StreamReader sr = new StreamReader (stream);
				return JsonConvert.DeserializeObject<T> (sr.ReadToEnd(), JsonSettings);
			default:
				throw new Exception();
			}
		}
		
		public static T Load<T>(string filepath,
		                        SerializationType type=SerializationType.Json) {
			Stream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using (stream) {
				return Load<T> (stream, type);
			}
		}
		
		public static T LoadSafe<T>(string filepath) {
		
			Stream stream = new FileStream (filepath, FileMode.Open,
			                               FileAccess.Read, FileShare.Read);
			using (stream) {
				try {
					return Load<T> (stream, SerializationType.Json);
				} catch (Exception e) {
					Log.Exception (e);
					stream.Seek (0, SeekOrigin.Begin);
					return Load<T> (stream, SerializationType.Binary);
				}
			}
		}
		
		static JsonSerializerSettings JsonSettings {
			get{
				JsonSerializerSettings settings = new JsonSerializerSettings ();
				settings.Formatting = Formatting.Indented;
				settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
				settings.TypeNameHandling = TypeNameHandling.Objects;
				settings.Converters.Add (new VersionConverter ());
				//settings.ReferenceResolver = new IdReferenceResolver ();
				return settings;
			}
		}
	}
	
	public class LongoMatchConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is Time) {
				Time time = value as Time;
				if (time != null) {
					writer.WriteValue(time.MSeconds);
				}
			} else if (value is Color) {
				Color color = value as Color;
				if (color != null) {
					writer.WriteValue(String.Format ("#{0}{1}{2}{3}",
					                                 color.R.ToString ("X2"),
					                                 color.G.ToString ("X2"),
					                                 color.B.ToString ("X2"),
					                                 color.A.ToString ("X2")));
				}
			} else if (value is Image) {
				Image image = value as Image;
				if (image != null) {
					writer.WriteValue(image.Serialize());
				}
			} else if (value is HotKey) {
				HotKey hotkey = value as HotKey;
				if (hotkey != null) {
					writer.WriteValue(String.Format ("{0} {1}", hotkey.Key, hotkey.Modifier));
				}
			} else if (value is Point) {
				Point p = value as Point;
				if (p != null) {
					writer.WriteValue(String.Format ("{0} {1}", p.X, p.Y));
				}
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.Value != null) {
				if (objectType == typeof (Time)) {
					Int64 t = (Int64) reader.Value;
					return new Time((int)t);
				} else if (objectType == typeof (Color)) {
					string rgbStr = (string) reader.Value;
					return new Color(Byte.Parse (rgbStr.Substring(1,2), NumberStyles.HexNumber),
					                 Byte.Parse (rgbStr.Substring(3,2), NumberStyles.HexNumber),
					                 Byte.Parse (rgbStr.Substring(5,2), NumberStyles.HexNumber),
					                 Byte.Parse (rgbStr.Substring(7,2), NumberStyles.HexNumber));
				} else if (objectType == typeof (Image)) {
					byte[] buf = Convert.FromBase64String ((string)reader.Value); 
					return Image.Deserialize (buf);
				} else if (objectType == typeof (HotKey)) {
					string[] hk = ((string)reader.Value).Split (' '); 
					return new HotKey {Key = int.Parse(hk[0]), Modifier = int.Parse(hk[1])};
				} else if (objectType == typeof (Point)) {
					string[] ps = ((string)reader.Value).Split (' '); 
					return new Point (double.Parse(ps[0]), double.Parse(ps[1]));
				}
			}
			return null;
		}
		
		public override bool CanConvert(Type objectType)
		{
			return (
				objectType == typeof(Time) ||
				objectType == typeof(Color) ||
				objectType == typeof(HotKey) ||
				objectType == typeof(Image));
		}
	}
	
    public class IdReferenceResolver : IReferenceResolver
    {
		private int _references;
        private readonly Dictionary<string, object> _idtoobjects;
        private readonly Dictionary<object, string> _objectstoid;

		public IdReferenceResolver () {
			_references = 0;
			_idtoobjects = new Dictionary<string, object>();
			_objectstoid = new Dictionary<object, string>();
		}
		
        public object ResolveReference(object context, string reference)
        {
			object p;
            _idtoobjects.TryGetValue(reference, out p);
            return p;
        }

        public string GetReference(object context, object value)
        {
			string referenceStr;
			if (value is IIDObject) {
				IIDObject p = (IIDObject)value;
				referenceStr = p.ID.ToString();
			} else {
				if (!_objectstoid.TryGetValue (value, out referenceStr)) {
					_references++;
					referenceStr = _references.ToString(CultureInfo.InvariantCulture); 
				}
			}
			_idtoobjects[referenceStr] = value;
			_objectstoid[value] = referenceStr;
			return referenceStr;
        }

        public bool IsReferenced(object context, object value)
        {
			string reference;
			return _objectstoid.TryGetValue (value, out reference);
        }

        public void AddReference(object context, string reference, object value)
        {
			_idtoobjects[reference] = value;
			_objectstoid[value] = reference;
        }
    }
}

