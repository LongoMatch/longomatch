//
//  Copyright (C) 2017 Fluendo S.A.
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
using System.Linq;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Stats;
using LongoMatch.Core.Store;
using NUnit.Framework;
using VAS.Core.Store;

namespace Tests.Core.Stats
{

	[TestFixture]
	public class TestEventTypeStats
	{
		[Test]
		public void Update_NoCommonTags ()
		{
			LMProject project = Utils.CreateProject ();
			EventTypeStats stats = new EventTypeStats (project, new EventsFilter (project), project.EventTypes [0]);
			stats.Update ();
			Assert.AreEqual (1, stats.TotalCount);
			Assert.AreEqual (1, stats.SubcategoriesStats.Count);
		}

		[Test]
		public void Update_WithCommonTags ()
		{
			LMProject project = Utils.CreateProject ();
			EventType evtType = project.EventTypes [0];
			var evt = project.EventsByType (evtType) [0];
			evt.Tags.Add (project.Dashboard.CommonTagsByGroup.Values.First () [0]);
			EventTypeStats stats = new EventTypeStats (project, new EventsFilter (project), evtType);
			stats.Update ();
			Assert.AreEqual (1, stats.TotalCount);
			Assert.AreEqual (2, stats.SubcategoriesStats.Count);
			Assert.AreEqual ("", stats.SubcategoriesStats [1].Name);
		}
	}
}
