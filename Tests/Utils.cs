//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using LongoMatch.Core.Common;
using NUnit.Framework;

namespace Tests
{
	public class Utils
	{
		static bool debugLine = false;

		public Utils ()
		{
		}
		
		public static T SerializeDeserialize<T> (T obj)
		{
			var stream = new MemoryStream ();
			Serializer.Save (obj, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			if (debugLine) {
				var jsonString = new StreamReader(stream).ReadToEnd();
				Console.WriteLine (jsonString);
			}
			stream.Seek(0, SeekOrigin.Begin);
			
			return Serializer.Load<T>(stream, SerializationType.Json);
		}
		 
		public static void CheckSerialization<T> (T obj)
		{
			var stream = new MemoryStream ();
			Serializer.Save (obj, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			var jsonString = new StreamReader (stream).ReadToEnd ();
			if (debugLine) {
				Console.WriteLine (jsonString);
			}
			stream.Seek (0, SeekOrigin.Begin);
			
			var newobj = Serializer.Load<T> (stream, SerializationType.Json);
			
			stream = new MemoryStream ();
			Serializer.Save (newobj, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			var newJsonString = new StreamReader (stream).ReadToEnd ();
			if (debugLine) {
				Console.WriteLine (newJsonString);
			}
			Assert.AreEqual (jsonString, newJsonString);
		}
	}
}

