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
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using NUnit.Framework;
using VAS.Core.Store;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestPlaysFilter
	{
	
		[Test ()]
		public void TestEmptyFilter ()
		{
			ProjectLongoMatch p = Utils.CreateProject ();

			try {
				EventsFilter filter = new EventsFilter (p);
			
				Assert.AreEqual (15, filter.VisibleEventTypes.Count);
				Assert.AreEqual (10, filter.VisiblePlayers.Count);
				Assert.AreEqual (3, filter.VisiblePlays.Count);
			} finally {
				Utils.DeleteProject (p);
			}
		}

		[Test ()]
		public void TestFilterCategory ()
		{
			ProjectLongoMatch p = Utils.CreateProject ();

			try {
				EventsFilter filter = new EventsFilter (p);
			
				filter.FilterEventType (p.EventTypes [0], true);
				Assert.AreEqual (1, filter.VisibleEventTypes.Count);
				Assert.AreEqual (1, filter.VisiblePlays.Count);
			
				filter.FilterEventType (p.EventTypes [1], true);
				Assert.AreEqual (2, filter.VisibleEventTypes.Count);
				Assert.AreEqual (2, filter.VisiblePlays.Count);

				filter.FilterEventType (p.EventTypes [2], true);
				Assert.AreEqual (3, filter.VisibleEventTypes.Count);
				Assert.AreEqual (3, filter.VisiblePlays.Count);
			
				filter.FilterEventType (p.EventTypes [0], true);
				Assert.AreEqual (3, filter.VisibleEventTypes.Count);
				Assert.AreEqual (3, filter.VisiblePlays.Count);
			
				filter.FilterEventType (p.EventTypes [0], false);
				Assert.AreEqual (2, filter.VisibleEventTypes.Count);
				Assert.AreEqual (2, filter.VisiblePlays.Count);

				filter.FilterEventType (p.EventTypes [1], false);
				Assert.AreEqual (1, filter.VisibleEventTypes.Count);
				Assert.AreEqual (1, filter.VisiblePlays.Count);
			
				filter.FilterEventType (p.EventTypes [2], false);
				Assert.AreEqual (15, filter.VisibleEventTypes.Count);
				Assert.AreEqual (3, filter.VisiblePlays.Count);
			} finally {
				Utils.DeleteProject (p);
			}
		}

		[Test ()]
		public void TestFilterCategoryTags ()
		{
			ProjectLongoMatch p = Utils.CreateProject ();

			try {
				EventsFilter filter = new EventsFilter (p);
				AnalysisEventType a;
			
				Assert.AreEqual (3, filter.VisiblePlays.Count);
			
				a = p.EventTypes [0] as AnalysisEventType;
				filter.FilterEventTag (a, a.Tags [0], true);
				Assert.AreEqual (1, filter.VisibleEventTypes.Count);
				Assert.AreEqual (0, filter.VisiblePlays.Count);

				a = p.EventTypes [1] as AnalysisEventType;
				filter.FilterEventTag (a, a.Tags [0], true);
				Assert.AreEqual (2, filter.VisibleEventTypes.Count);
				Assert.AreEqual (1, filter.VisiblePlays.Count);
			
				a = p.EventTypes [2] as AnalysisEventType;
				filter.FilterEventTag (a, a.Tags [0], true);
				Assert.AreEqual (3, filter.VisibleEventTypes.Count);
				Assert.AreEqual (1, filter.VisiblePlays.Count);

				filter.FilterEventTag (a, a.Tags [1], true);
				Assert.AreEqual (2, filter.VisiblePlays.Count);
			
				a = p.EventTypes [0] as AnalysisEventType;
				filter.FilterEventTag (a, a.Tags [0], false);
				Assert.AreEqual (3, filter.VisiblePlays.Count);
			
				a = p.EventTypes [1] as AnalysisEventType;
				filter.FilterEventTag (a, a.Tags [0], false);
				filter.FilterEventTag (a, a.Tags [1], true);
				Assert.AreEqual (2, filter.VisiblePlays.Count);
				Assert.AreEqual (p.Timeline [0], filter.VisiblePlays [0]);
				Assert.AreEqual (p.Timeline [2], filter.VisiblePlays [1]);
			
				/* One tag filtered now, but not the one of this play */
				a = p.EventTypes [2] as AnalysisEventType;
				filter.FilterEventTag (a, a.Tags [1], false);
				Assert.AreEqual (1, filter.VisiblePlays.Count);
				Assert.AreEqual (p.Timeline [0], filter.VisiblePlays [0]);
				/* No more tags filtered, if the category matches we are ok */
				filter.FilterEventTag (a, a.Tags [0], false);
				Assert.AreEqual (2, filter.VisiblePlays.Count);
				Assert.AreEqual (p.Timeline [0], filter.VisiblePlays [0]);
				Assert.AreEqual (p.Timeline [2], filter.VisiblePlays [1]);

				filter.ClearAll ();
				Assert.AreEqual (3, filter.VisiblePlays.Count);
			} finally {
				Utils.DeleteProject (p);
			}
		}

		[Test ()]
		public void TestFilterPlayers ()
		{
			ProjectLongoMatch p = Utils.CreateProject ();

			try {
				EventsFilter filter = new EventsFilter (p);
			
				Assert.AreEqual (10, filter.VisiblePlayers.Count);
				Assert.AreEqual (3, filter.VisiblePlays.Count);
				filter.FilterPlayer (p.LocalTeamTemplate.List [4], true);
				Assert.AreEqual (0, filter.VisiblePlays.Count);
				Assert.AreEqual (1, filter.VisiblePlayers.Count);
				filter.FilterPlayer (p.LocalTeamTemplate.List [0], true);
				Assert.AreEqual (1, filter.VisiblePlays.Count);
				Assert.AreEqual (2, filter.VisiblePlayers.Count);
				filter.FilterPlayer (p.LocalTeamTemplate.List [0], true);
				Assert.AreEqual (1, filter.VisiblePlays.Count);
				Assert.AreEqual (2, filter.VisiblePlayers.Count);
				filter.FilterPlayer (p.LocalTeamTemplate.List [0], false);
				Assert.AreEqual (0, filter.VisiblePlays.Count);
				Assert.AreEqual (1, filter.VisiblePlayers.Count);
				filter.FilterPlayer (p.LocalTeamTemplate.List [4], false);
				Assert.AreEqual (10, filter.VisiblePlayers.Count);
				Assert.AreEqual (3, filter.VisiblePlays.Count);
			} finally {
				Utils.DeleteProject (p);
			}
		}


		[Test ()]
		public void TestFilterPlayersDuplicated ()
		{
			ProjectLongoMatch p = Utils.CreateProject ();
			p.VisitorTeamTemplate = p.LocalTeamTemplate;

			try {
				EventsFilter filter = new EventsFilter (p);

				Assert.AreEqual (5, filter.VisiblePlayers.Count);
				Assert.AreEqual (3, filter.VisiblePlays.Count);

				foreach (PlayerLongoMatch player in p.LocalTeamTemplate.List) {
					filter.FilterPlayer (player, true);
				}
				Assert.AreEqual (5, filter.VisiblePlayers.Count);
				Assert.AreEqual (3, filter.VisiblePlays.Count);

				foreach (PlayerLongoMatch player in p.VisitorTeamTemplate.List) {
					filter.FilterPlayer (player, true);
				}
				Assert.AreEqual (5, filter.VisiblePlayers.Count);
				Assert.AreEqual (3, filter.VisiblePlays.Count);


				filter.ClearAll ();
				Assert.AreEqual (5, filter.VisiblePlayers.Count);
				Assert.AreEqual (3, filter.VisiblePlays.Count);
			} finally {
				Utils.DeleteProject (p);
			}
		}


		[Test ()]
		public void TestClearAll ()
		{
			ProjectLongoMatch p = Utils.CreateProject ();

			try {
				EventsFilter filter = new EventsFilter (p);

				filter.FilterPlayer (p.LocalTeamTemplate.List [0], true);
				Assert.AreEqual (1, filter.VisiblePlays.Count);
				Assert.AreEqual (1, filter.VisiblePlayers.Count);
				filter.ClearAll ();
				Assert.AreEqual (3, filter.VisiblePlays.Count);
				Assert.AreEqual (10, filter.VisiblePlayers.Count);
			
				filter.FilterEventType (p.EventTypes [0], true);
				Assert.AreEqual (1, filter.VisiblePlays.Count);
				Assert.AreEqual (1, filter.VisibleEventTypes.Count);
				filter.ClearAll ();
				Assert.AreEqual (3, filter.VisiblePlays.Count);
				Assert.AreEqual (15, filter.VisibleEventTypes.Count);
			
				filter.FilterEventTag (p.EventTypes [0], (p.EventTypes [0] as AnalysisEventType).Tags [0], true);
				Assert.AreEqual (0, filter.VisiblePlays.Count);
				Assert.AreEqual (1, filter.VisibleEventTypes.Count);
				filter.ClearAll ();
				Assert.AreEqual (3, filter.VisiblePlays.Count);
				Assert.AreEqual (15, filter.VisibleEventTypes.Count);
			} finally {
				Utils.DeleteProject (p);
			}
		}
	}
}
