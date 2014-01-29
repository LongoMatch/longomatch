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
using System.Collections.Generic;
using NUnit.Framework;

using LongoMatch.Common;
using LongoMatch.Store;

namespace Tests.Core
{
	[TestFixture()]
	public class TestSubCategory
	{
		[Test()]
		public void TestTagSubcategoryProps ()
		{
			string tag1="tag1", tag2="tag2";
			List<string> elementsDesc;
			TagSubCategory subcat = new TagSubCategory {Name="Test",
				AllowMultiple = true, FastTag = true};
				
			subcat.Add (tag1);
			subcat.Add (tag2);
			elementsDesc = subcat.ElementsDesc ();
			Assert.AreEqual (elementsDesc.Count, 2);
			Assert.AreEqual (elementsDesc[0], tag1);
			Assert.AreEqual (elementsDesc[1], tag2);
		}
		
		
		[Test()]
		public void TestTagSubcategorySerialization ()
		{
			string tag1="tag1", tag2="tag2";
			List<string> elementsDesc;
			MemoryStream stream;
			TagSubCategory subcat, newsubcat;
			
			subcat = new TagSubCategory {Name="Test",
				AllowMultiple = true, FastTag = true};
			subcat.Add (tag1);
			subcat.Add (tag2);
			
			Utils.CheckSerialization (subcat);
			
			stream = new MemoryStream ();
			SerializableObject.Save (subcat, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			var reader = new StreamReader (stream);
			var jsonString = reader.ReadToEnd();
			Console.WriteLine (jsonString);
			/* Count property is removed */
			Assert.False (jsonString.Contains ("Count"));
			Assert.True (jsonString.Contains ("_items"));
			Assert.True (jsonString.Contains ("_size"));
			stream.Seek (0, SeekOrigin.Begin);

			newsubcat = SerializableObject.Load<TagSubCategory> (stream, SerializationType.Json);
			
			Assert.AreEqual (subcat.Name, newsubcat.Name);
			Assert.AreEqual (subcat.AllowMultiple, newsubcat.AllowMultiple);
			Assert.AreEqual (subcat.Count, newsubcat.Count);
			Assert.AreEqual (subcat.FastTag, newsubcat.FastTag);
			elementsDesc = newsubcat.ElementsDesc ();
			Assert.AreEqual (elementsDesc.Count, 2);
			Assert.AreEqual (elementsDesc[0], tag1);
			Assert.AreEqual (elementsDesc[1], tag2);
		}
	}
}

