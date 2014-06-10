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
		public void TestTagSubcategorySerialization ()
		{
			string tag1="tag1", tag2="tag2";
			List<string> elementsDesc;
			MemoryStream stream;
			SubCategory subcat, newsubcat;
			
			subcat = new SubCategory {Name="Test",
				AllowMultiple = true};
			subcat.Options.Add (tag1);
			subcat.Options.Add (tag2);
			
			Utils.CheckSerialization (subcat);
			newsubcat = Utils.SerializeDeserialize (subcat);

			Assert.AreEqual (subcat.Name, newsubcat.Name);
			Assert.AreEqual (subcat.AllowMultiple, newsubcat.AllowMultiple);
			Assert.AreEqual (subcat.Options.Count, newsubcat.Options.Count);
			Assert.AreEqual (subcat.Options[0], newsubcat.Options[0]);
			Assert.AreEqual (subcat.Options[1], newsubcat.Options[1]);
		}
	}
}

