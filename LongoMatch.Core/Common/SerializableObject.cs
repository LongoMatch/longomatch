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
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace LongoMatch.Common
{
	public class SerializableObject
	{
		public enum SerializationType {
			Binary,
			Xml
		}
		
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
	}
}

