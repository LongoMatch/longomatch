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
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;

namespace Tests.Core
{
	[TestFixture()]
	public class TestDashboard
	{
		[Test()]
		public void TestSerialization ()
		{
			Dashboard cat = new Dashboard ();
			
			Utils.CheckSerialization (cat);
			
			cat.Name = "test";
			cat.GamePeriods = new List<string> ();
			cat.GamePeriods.Add ("1");
			cat.GamePeriods.Add ("2");
			cat.List.Add ( new AnalysisEventButton {Name = "cat1"});
			cat.List.Add ( new {Name = "cat2"});
			cat.List.Add ( new CategoryButton {Name = "cat3"});
			
			Utils.CheckSerialization (cat);
			
			Dashboard newcat = Utils.SerializeDeserialize (cat);
			Assert.AreEqual (cat.ID, newcat.ID);
			Assert.AreEqual (cat.Name, newcat.Name);
			Assert.AreEqual (cat.GamePeriods.Count, newcat.GamePeriods.Count);
			Assert.AreEqual (cat.GamePeriods[0], newcat.GamePeriods[0]);
			Assert.AreEqual (cat.GamePeriods[1], newcat.GamePeriods[1]);
			Assert.AreEqual (cat.List.Count, newcat.List.Count);
			Assert.AreEqual (cat.CategoriesList[0].ID, newcat.CategoriesList[0].ID);
			Assert.AreEqual (cat.CategoriesList[1].ID, newcat.CategoriesList[1].ID);
			Assert.AreEqual (cat.CategoriesList[2].ID, newcat.CategoriesList[2].ID);
		}
	}
}

