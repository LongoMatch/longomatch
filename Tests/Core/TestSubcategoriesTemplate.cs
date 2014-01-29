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
using LongoMatch.Common;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using System.Collections.Generic;

namespace Tests.Core
{
	[TestFixture()]
	public class TestSubcategoriesTemplate
	{
		[Test()]
		public void TestSerialization ()
		{
			string tag1="tag1", tag2="tag2";
			SubCategoryTemplate t = new SubCategoryTemplate {Name="Test",
				AllowMultiple = true, FastTag = true};
				
			Utils.CheckSerialization (t);
			t.Add (tag1);
			t.Add (tag2);
			Utils.CheckSerialization (t);
			
			SubCategoryTemplate newt = Utils.SerializeDeserialize (t);
			Assert.AreEqual (t.Name, newt.Name);
			Assert.AreEqual (t.AllowMultiple, newt.AllowMultiple);
			Assert.AreEqual (t.Count, newt.Count);
			Assert.AreEqual (t.FastTag, newt.FastTag);
			Assert.AreEqual (t.Count, 2);
			Assert.AreEqual (t[0], tag1);
			Assert.AreEqual (t[1], tag2);
		
		}
	}
}

