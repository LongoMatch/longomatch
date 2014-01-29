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
using System.Collections.Generic;
using NUnit.Framework;
using LongoMatch.Common;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace Tests.Core
{
	[TestFixture()]
	public class TestCategoriesTemplate
	{
		[Test()]
		public void TestSerialization ()
		{
			Categories cat = new Categories ();
			
			Utils.CheckSerialization (cat);
			
			cat.Name = "test";
			cat.Version = new Version (1, 2);
			cat.GamePeriods = new List<string> ();
			cat.GamePeriods.Add ("1");
			cat.GamePeriods.Add ("2");
			cat.Add ( new Category {Name = "cat1"});
			cat.Add ( new Category {Name = "cat2"});
			cat.Add ( new Category {Name = "cat3"});
			
			Utils.CheckSerialization (cat);
			
			Categories newcat = Utils.SerializeDeserialize (cat);
			Assert.AreEqual (cat.Name, newcat.Name);
			Assert.AreEqual (cat.Version, newcat.Version);
			Assert.AreEqual (cat.GamePeriods.Count, newcat.GamePeriods.Count);
			Assert.AreEqual (cat.GamePeriods[0], newcat.GamePeriods[0]);
			Assert.AreEqual (cat.GamePeriods[1], newcat.GamePeriods[1]);
			Assert.AreEqual (cat.Count, newcat.Count);
			Assert.AreEqual (cat[0].UUID, newcat[0].UUID);
			Assert.AreEqual (cat[1].UUID, newcat[1].UUID);
			Assert.AreEqual (cat[2].UUID, newcat[2].UUID);
		}
	}
}

