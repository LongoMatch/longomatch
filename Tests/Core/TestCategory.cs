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
using NUnit.Framework;
using Newtonsoft.Json;

using LongoMatch.Common;
using LongoMatch.Store;
using System.IO;
using System.Collections.Generic;

namespace Tests.Core
{
	[TestFixture()]
	public class TestCategory
	{
		[Test()]
		public void TestSerialization ()
		{
			string jsonString;
			CategoryButton cat;
			MemoryStream stream;
			StreamReader reader;
			
			cat = new CategoryButton();
			cat.Color = new Color (255, 0, 0);
 			cat.HotKey = new HotKey {Key=2, Modifier=4};
			cat.Name = "test";
			cat.SortMethod = SortMethodType.SortByDuration;
			cat.Start = new Time (3000);
			cat.Stop = new Time (4000);
			cat.Tags = new List<Tag>();
			cat.Tags.Add (new Tag ("foo", "bar"));
			cat.TagFieldPosition = true;
			cat.TagGoalPosition = true;
			cat.TagHalfFieldPosition = true;
			cat.FieldPositionIsDistance = true;
			cat.HalfFieldPositionIsDistance = false;
			
			Utils.CheckSerialization (cat);
			
			stream = new MemoryStream ();
			Serializer.Save (cat, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			reader = new StreamReader (stream);
			jsonString = reader.ReadToEnd();
			Assert.False (jsonString.Contains ("SortMethodString"));
			stream.Seek (0, SeekOrigin.Begin);
			CategoryButton newcat = Serializer.Load<CategoryButton> (stream, SerializationType.Json);
			
			Assert.AreEqual (cat.ID, newcat.ID);
			Assert.AreEqual (cat.Name, newcat.Name);
			Assert.AreEqual (cat.Position, newcat.Position);
			Assert.AreEqual (cat.SortMethod, newcat.SortMethod);
			Assert.AreEqual (cat.Start.MSeconds, newcat.Start.MSeconds);
			Assert.AreEqual (cat.Stop.MSeconds, newcat.Stop.MSeconds);
			Assert.AreEqual (cat.TagFieldPosition, newcat.TagFieldPosition);
			Assert.AreEqual (cat.TagGoalPosition, newcat.TagGoalPosition);
			Assert.AreEqual (cat.TagHalfFieldPosition, newcat.TagHalfFieldPosition);
			Assert.AreEqual (cat.FieldPositionIsDistance, newcat.FieldPositionIsDistance);
			Assert.AreEqual (cat.HalfFieldPositionIsDistance, newcat.HalfFieldPositionIsDistance);
			Assert.AreEqual (cat.HotKey, newcat.HotKey);
			Assert.AreEqual (255, newcat.Color.R);
			Assert.AreEqual (0, newcat.Color.G);
			Assert.AreEqual (0, newcat.Color.B);
			Assert.AreEqual (newcat.Tags.Count, 1);
			Assert.AreEqual (newcat.Tags[0].Value, "foo");
			Assert.AreEqual (newcat.Tags[0].Group, "bar");
		}
		
		public static void Main (string [] args)
		{
		}
	}
}
