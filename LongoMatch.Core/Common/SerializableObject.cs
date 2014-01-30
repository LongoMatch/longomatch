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

namespace LongoMatch.Common
{
	public class SerializableObject
	{
		public static void Save<T>(T obj, Stream stream,
		                           SerializationType type=SerializationType.Binary) {
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
		                           SerializationType type=SerializationType.Binary) {
			Stream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None);
			using (stream) {
				Save<T> (obj, stream, type);
				stream.Close();
			}
		}

		public static T Load<T>(Stream stream,
		                        SerializationType type=SerializationType.Binary) {
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
		                        SerializationType type=SerializationType.Binary) {
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
					return Load<T> (stream, SerializationType.Binary);
				} catch {
					return Load<T> (stream, SerializationType.Xml);
				}
			}
		}
		
		static JsonSerializerSettings JsonSettings {
			get{
				JsonSerializerSettings settings = new JsonSerializerSettings ();
				settings.Formatting = Formatting.Indented;
				settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
				settings.TypeNameHandling = TypeNameHandling.Objects;
				settings.ContractResolver = new ListObjectContractResolver ();
				settings.Converters.Add (new VersionConverter ());
				return settings;
			}
		}
	}
	
	public class ListObjectContractResolver : DefaultContractResolver
	{
		/* To serialize/desarialize List objects by including private fields
		 * _size and _items */
		public ListObjectContractResolver()
		{
		}
		
		protected override IList<JsonProperty> CreateProperties (Type type, MemberSerialization memberSerialization)
		{
			IList<JsonProperty> props = new List<JsonProperty>();
			
			props = base.CreateProperties (type, memberSerialization);
			if (typeof(ISubCategory).IsAssignableFrom (type) ||
			    type == typeof(Categories) ||
			    type == typeof(TeamTemplate) ||
			    type == typeof(SubCategoryTemplate))
			{
				JsonProperty itprop;
				BindingFlags flags;
				
				props = props.Where (p => p.PropertyName != "Count").ToList();
				flags =  base.DefaultMembersSearchFlags;
				IList<JsonProperty> allprops = new List<JsonProperty>();
				base.DefaultMembersSearchFlags = flags | System.Reflection.BindingFlags.NonPublic;
				allprops = base.CreateProperties (type, MemberSerialization.Fields);
				base.DefaultMembersSearchFlags = flags;
				itprop = allprops.FirstOrDefault (p => p.PropertyName == "_items");
				if (itprop != null) {
					props.Add (itprop);
				}
				itprop = allprops.FirstOrDefault (p => p.PropertyName == "_size");
				if (itprop != null) {
					props.Add (itprop);
				}
			}
			
			return props;
		}
	}


}

