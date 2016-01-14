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
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using LongoMatch.Core.Store;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using LongoMatch.Core.Interfaces;

namespace LongoMatch.Core.Common
{
	public sealed class Serializer : ISerializer
	{
		static readonly Serializer instance = new Serializer ();

		private Serializer ()
		{
		}

		public static Serializer Instance {
			get {
				return instance;
			}
		}

		public void Save<T> (T obj, Stream stream,
		                     SerializationType type = SerializationType.Json)
		{
			switch (type) {
			case SerializationType.Binary:
				BinaryFormatter formatter = new  BinaryFormatter ();
				formatter.Serialize (stream, obj);
				break;
			case SerializationType.Xml:
				XmlSerializer xmlformatter = new XmlSerializer (typeof(T));
				xmlformatter.Serialize (stream, obj);
				break;
			case SerializationType.Json:
				StreamWriter sw = new StreamWriter (stream, Encoding.UTF8);
				sw.NewLine = "\n";
				sw.Write (JsonConvert.SerializeObject (obj, JsonSettings));
				sw.Flush ();
				break;
			}
		}

		public void Save<T> (T obj, string filepath,
		                     SerializationType type = SerializationType.Json)
		{
			string tmpPath = filepath + ".tmp";
			using (Stream stream = new FileStream (tmpPath, FileMode.Create,
				                       FileAccess.Write, FileShare.None)) {
				Save<T> (obj, stream, type);
			}
			if (File.Exists (filepath)) {
				File.Replace (tmpPath, filepath, null);
			} else {
				File.Move (tmpPath, filepath);
			}
		}

		public object Load (Type type, Stream stream,
		                    SerializationType serType = SerializationType.Json)
		{
			switch (serType) {
			case SerializationType.Binary:
				BinaryFormatter formatter = new BinaryFormatter ();
				return formatter.Deserialize (stream);
			case SerializationType.Xml:
				XmlSerializer xmlformatter = new XmlSerializer (type);
				return xmlformatter.Deserialize (stream);
			case SerializationType.Json:
				StreamReader sr = new StreamReader (stream, Encoding.UTF8);
				JsonSerializerSettings settings = JsonSettings;
				settings.ContractResolver = new IsChangedContractResolver ();
				return JsonConvert.DeserializeObject (sr.ReadToEnd (), type, settings);
			default:
				throw new Exception ();
			}
		}

		public T Load<T> (Stream stream,
		                  SerializationType type = SerializationType.Json)
		{
			return (T)Load (typeof(T), stream, type);
		}

		public T Load<T> (string filepath,
		                  SerializationType type = SerializationType.Json)
		{
			Stream stream = new FileStream (filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using (stream) {
				return Load<T> (stream, type);
			}
		}

		public T LoadSafe<T> (string filepath)
		{
		
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

		public static JsonSerializerSettings JsonSettings {
			get {
				JsonSerializerSettings settings = new JsonSerializerSettings ();
				settings.Formatting = Formatting.Indented;
				settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
				settings.TypeNameHandling = TypeNameHandling.Objects;
				settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
				settings.Converters.Add (new VersionConverter ());
				settings.Converters.Add (new LongoMatchConverter (true));
				//settings.ReferenceResolver = new IdReferenceResolver ();
				return settings;
			}
		}
	}

	public class LongoMatchConverter : JsonConverter
	{
		bool handleImages;

		public LongoMatchConverter (bool handleImages)
		{
			this.handleImages = handleImages;
		}

		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is Time) {
				Time time = value as Time;
				if (time != null) {
					writer.WriteValue (time.MSeconds);
				}
			} else if (value is Color) {
				Color color = value as Color;
				if (color != null) {
					writer.WriteValue (String.Format ("#{0}{1}{2}{3}",
						color.R.ToString ("X2"),
						color.G.ToString ("X2"),
						color.B.ToString ("X2"),
						color.A.ToString ("X2")));
				}
			} else if (value is Image) {
				Image image = value as Image;
				if (image != null) {
					writer.WriteValue (image.Serialize ());
				}
			} else if (value is HotKey) {
				HotKey hotkey = value as HotKey;
				if (hotkey != null) {
					writer.WriteValue (String.Format ("{0} {1}", hotkey.Key, hotkey.Modifier));
				}
			} else if (value is Point) {
				Point p = value as Point;
				if (p != null) {
					writer.WriteValue (String.Format ("{0} {1}",
						p.X.ToString (NumberFormatInfo.InvariantInfo),
						p.Y.ToString (NumberFormatInfo.InvariantInfo)));
				}
			} else {
			}
		}

		public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			object ret = null;

			if (reader.Value != null) {
				if (objectType == typeof(Time)) {
					if (reader.ValueType == typeof(Int64)) {
						ret = new Time ((int)(Int64)reader.Value);
					} else {
						ret = new Time ((Int32)reader.Value);
					}
				} else if (objectType == typeof(Color)) {
					string rgbStr = (string)reader.Value;
					ret = Color.Parse (rgbStr);
				} else if (objectType == typeof(Image)) {
					byte[] buf = Convert.FromBase64String ((string)reader.Value); 
					ret = Image.Deserialize (buf);
				} else if (objectType == typeof(HotKey)) {
					string[] hk = ((string)reader.Value).Split (' '); 
					ret = new HotKey { Key = int.Parse (hk [0]), Modifier = int.Parse (hk [1]) };
				} else if (objectType == typeof(Point)) {
					string[] ps = ((string)reader.Value).Split (' '); 
					ret = new Point (double.Parse (ps [0], NumberFormatInfo.InvariantInfo),
						double.Parse (ps [1], NumberFormatInfo.InvariantInfo));
				}
			}
			if (ret is IChanged) {
				(ret as IChanged).IsChanged = false;
			}
			return ret;
		}

		public override bool CanConvert (Type objectType)
		{
			return (
			    objectType == typeof(Time) ||
			    objectType == typeof(Color) ||
			    objectType == typeof(Point) ||
			    objectType == typeof(HotKey) ||
			    objectType == typeof(Image) && handleImages);
		}
	}

	public class IsChangedContractResolver : DefaultContractResolver
	{

		protected override JsonContract CreateContract (Type type)
		{
			JsonContract contract = base.CreateContract (type);
			if (typeof(IChanged).IsAssignableFrom (type)) {
				contract.OnDeserializedCallbacks.Add (
					(o, context) => {
						(o as IChanged).IsChanged = false;
					});
			}
			return contract;
		}
	}
}

