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
using NUnit.Framework;
using System;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.Common;

namespace Tests.Core.Store
{
	[TestFixture()]
	public class TestPlaysFilter
	{
	
		Project CreateProject () {
			TimelineEvent pl;
			Project p = new Project ();
			p.Dashboard = Dashboard.DefaultTemplate (10);
			p.LocalTeamTemplate = TeamTemplate.DefaultTemplate (5);
			p.VisitorTeamTemplate = TeamTemplate.DefaultTemplate (5);
			MediaFile mf = new MediaFile ("path", 34000, 25, true, true, "mp4", "h264",
			                              "aac", 320, 240, 1.3, null);
			ProjectDescription pd = new ProjectDescription ();
			pd.FileSet = new MediaFileSet ();
			pd.FileSet.SetAngle (MediaFileAngle.Angle1, mf);
			p.Description = pd;
			p.UpdateEventTypesAndTimers ();
			
			AnalysisEventButton b = p.Dashboard.List[0] as AnalysisEventButton;
			
			/* No tags, no players */
			pl = new TimelineEvent {EventType = b.EventType};
			p.Timeline.Add (pl);
			/* tags, but no players */
			b = p.Dashboard.List[1] as AnalysisEventButton;
			pl = new TimelineEvent {EventType = b.EventType};
			pl.Tags.Add (b.AnalysisEventType.Tags[0]);
			p.Timeline.Add (pl);
			/* tags and players */
			b = p.Dashboard.List[2] as AnalysisEventButton;
			pl = new TimelineEvent {EventType = b.EventType};
			pl.Tags.Add (b.AnalysisEventType.Tags[1]);
			pl.Players.Add (p.LocalTeamTemplate.List[0]);
			p.Timeline.Add (pl);
			return p;
		}
		
		[Test()]
		public void TestEmptyFilter ()
		{
			Project p = CreateProject ();
			EventsFilter filter = new EventsFilter (p);
			
			Assert.AreEqual (13, filter.VisibleEventTypes.Count);
			Assert.AreEqual (10, filter.VisiblePlayers.Count);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
		}
		
		[Test()]
		public void TestFilterCategory ()
		{
			Project p = CreateProject ();
			EventsFilter filter = new EventsFilter (p);
			
			filter.FilterEventType (p.EventTypes[0], true);
			Assert.AreEqual (1, filter.VisibleEventTypes.Count);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			
			filter.FilterEventType (p.EventTypes[1], true);
			Assert.AreEqual (2, filter.VisibleEventTypes.Count);
			Assert.AreEqual (2, filter.VisiblePlays.Count);

			filter.FilterEventType (p.EventTypes[2], true);
			Assert.AreEqual (3, filter.VisibleEventTypes.Count);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			
			filter.FilterEventType (p.EventTypes[0], true);
			Assert.AreEqual (3, filter.VisibleEventTypes.Count);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			
			filter.FilterEventType (p.EventTypes[0], false);
			Assert.AreEqual (2, filter.VisibleEventTypes.Count);
			Assert.AreEqual (2, filter.VisiblePlays.Count);

			filter.FilterEventType (p.EventTypes[1], false);
			Assert.AreEqual (1, filter.VisibleEventTypes.Count);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			
			filter.FilterEventType (p.EventTypes[2], false);
			Assert.AreEqual (13, filter.VisibleEventTypes.Count);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
		}
		
		[Test()]
		public void TestFilterCategoryTags ()
		{
			Project p = CreateProject ();
			EventsFilter filter = new EventsFilter (p);
			AnalysisEventType a;
			
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			
			a = p.EventTypes[0] as AnalysisEventType;
			filter.FilterEventTag (a, a.Tags[0], true);
			Assert.AreEqual (1, filter.VisibleEventTypes.Count);
			Assert.AreEqual (0, filter.VisiblePlays.Count);

			a = p.EventTypes[1] as AnalysisEventType;
			filter.FilterEventTag (a, a.Tags[0], true);
			Assert.AreEqual (2, filter.VisibleEventTypes.Count);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			
			a = p.EventTypes[2] as AnalysisEventType;
			filter.FilterEventTag (a, a.Tags[0], true);
			Assert.AreEqual (3, filter.VisibleEventTypes.Count);
			Assert.AreEqual (1, filter.VisiblePlays.Count);

			filter.FilterEventTag (a, a.Tags[1], true);
			Assert.AreEqual (2, filter.VisiblePlays.Count);
			
			a = p.EventTypes[0] as AnalysisEventType;
			filter.FilterEventTag (a, a.Tags[0], false);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			
			a = p.EventTypes[1] as AnalysisEventType;
			filter.FilterEventTag (a, a.Tags[0], false);
			filter.FilterEventTag (a, a.Tags[1], true);
			Assert.AreEqual (2, filter.VisiblePlays.Count);
			Assert.AreEqual (p.Timeline[0], filter.VisiblePlays[0]);
			Assert.AreEqual (p.Timeline[2], filter.VisiblePlays[1]);
			
			/* One tag filtered now, but not the one of this play */
			a = p.EventTypes[2] as AnalysisEventType;
			filter.FilterEventTag (a, a.Tags[1], false);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			Assert.AreEqual (p.Timeline[0], filter.VisiblePlays[0]);
			/* No more tags filtered, if the category matches we are ok */
			filter.FilterEventTag (a, a.Tags[0], false);
			Assert.AreEqual (2, filter.VisiblePlays.Count);
			Assert.AreEqual (p.Timeline[0], filter.VisiblePlays[0]);
			Assert.AreEqual (p.Timeline[2], filter.VisiblePlays[1]);

			filter.ClearAll ();
			Assert.AreEqual (3, filter.VisiblePlays.Count);
		}
		
		[Test()]
		public void TestFilterPlayers ()
		{
			Project p = CreateProject ();
			EventsFilter filter = new EventsFilter (p);
			
			Assert.AreEqual (10, filter.VisiblePlayers.Count);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			filter.FilterPlayer (p.LocalTeamTemplate.List[4], true);
			Assert.AreEqual (0, filter.VisiblePlays.Count);
			Assert.AreEqual (1, filter.VisiblePlayers.Count);
			filter.FilterPlayer (p.LocalTeamTemplate.List[0], true);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			Assert.AreEqual (2, filter.VisiblePlayers.Count);
			filter.FilterPlayer (p.LocalTeamTemplate.List[0], true);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			Assert.AreEqual (2, filter.VisiblePlayers.Count);
			filter.FilterPlayer (p.LocalTeamTemplate.List[0], false);
			Assert.AreEqual (0, filter.VisiblePlays.Count);
			Assert.AreEqual (1, filter.VisiblePlayers.Count);
			filter.FilterPlayer (p.LocalTeamTemplate.List[4], false);
			Assert.AreEqual (10, filter.VisiblePlayers.Count);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
		}
		
		[Test()]
		public void TestClearAll ()
		{
			Project p = CreateProject ();
			EventsFilter filter = new EventsFilter (p);

			filter.FilterPlayer (p.LocalTeamTemplate.List[0], true);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			Assert.AreEqual (1, filter.VisiblePlayers.Count);
			filter.ClearAll();
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			Assert.AreEqual (10, filter.VisiblePlayers.Count);
			
			filter.FilterEventType (p.EventTypes[0], true);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			Assert.AreEqual (1, filter.VisibleEventTypes.Count);
			filter.ClearAll ();
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			Assert.AreEqual (13, filter.VisibleEventTypes.Count);
			
			filter.FilterEventTag (p.EventTypes[0], (p.EventTypes[0] as AnalysisEventType).Tags[0], true);
			Assert.AreEqual (0, filter.VisiblePlays.Count);
			Assert.AreEqual (1, filter.VisibleEventTypes.Count);
			filter.ClearAll ();
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			Assert.AreEqual (13, filter.VisibleEventTypes.Count);
		}
	}
}
