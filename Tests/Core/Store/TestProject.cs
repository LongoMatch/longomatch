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

using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestProject
	{
	
		Project CreateProject (bool fill = true)
		{
			Project p = new Project ();
			p.Dashboard = Dashboard.DefaultTemplate (10);
			p.UpdateEventTypesAndTimers ();
			p.LocalTeamTemplate = Team.DefaultTemplate (10);
			p.VisitorTeamTemplate = Team.DefaultTemplate (12);
			MediaFile mf = new MediaFile ("path", 34000, 25, true, true, "mp4", "h264",
				               "aac", 320, 240, 1.3, null, "Test asset");
			ProjectDescription pd = new ProjectDescription ();
			pd.FileSet = new MediaFileSet ();
			pd.FileSet.Add (mf);
			p.Description = pd;
			if (fill) {
				p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null, null, null);
				p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null, null, null);
				p.AddEvent (p.EventTypes [1], new Time (1000), new Time (2000), null, null, null, null);
				p.AddEvent (p.EventTypes [2], new Time (1000), new Time (2000), null, null, null, null);
				p.AddEvent (p.EventTypes [2], new Time (1000), new Time (2000), null, null, null, null);
				p.AddEvent (p.EventTypes [2], new Time (1000), new Time (2000), null, null, null, null);
				p.AddEvent (p.EventTypes [6], new Time (1000), new Time (2000), null, null, null, null);
			}

			return p;
		}

		[Test ()]
		public void TestSerialization ()
		{
			Project p = new Project ();
			
			Utils.CheckSerialization (p);
			
			p = CreateProject ();
			Utils.CheckSerialization (p);
			p.AddEvent (new TimelineEvent ());
			Utils.CheckSerialization (p);
			
			Project newp = Utils.SerializeDeserialize (p);
			Assert.AreEqual (newp.CompareTo (p), 0);
			Assert.AreEqual (newp.Description.CompareTo (p.Description), 0);
			Assert.AreEqual (newp.Timeline.Count, p.Timeline.Count);
		}

		[Test ()]
		public void TestSetDescription ()
		{
			ProjectDescription pd = new ProjectDescription ();
			Project p = new Project ();
			p.Description = pd;
			Assert.IsNotNull (pd.ID);
			Assert.AreEqual (p.ID, pd.ProjectID);
		}

		[Test ()]
		[Ignore ("Not implemented")]
		public void TestGetScores ()
		{
		}

		[Test ()]
		[Ignore ("Not implemented")]
		public void TestGetPenaltyCards ()
		{
		}

		[Test ()]
		[Ignore ("Not implemented")]
		public void TestGetScoreEvents ()
		{
		}

		[Test ()]
		[Ignore ("Not implemented")]
		public void TestGetPenaltyCardEvents ()
		{
		}

		[Test ()]
		[Ignore ("Not implemented")]
		public void TestGetSubstitutionEventType ()
		{
		}

		[Test ()]
		[Ignore ("Not implemented")]
		public void TestGetLineup ()
		{
		}

		[Test ()]
		public void TestEventsGroupedByEventType ()
		{
			Project p = CreateProject ();
			var g = p.EventsGroupedByEventType;
			Assert.AreEqual (g.Count (), 4);
			var gr = g.ElementAt (0);
			Assert.AreEqual (p.EventTypes [0], gr.Key);
			Assert.AreEqual (2, gr.Count ());
			
			gr = g.ElementAt (1);
			Assert.AreEqual (p.EventTypes [1], gr.Key);
			Assert.AreEqual (1, gr.Count ());
			
			gr = g.ElementAt (2);
			Assert.AreEqual (p.EventTypes [2], gr.Key);
			Assert.AreEqual (3, gr.Count ());
			
			gr = g.ElementAt (3);
			Assert.AreEqual (p.EventTypes [6], gr.Key);
			Assert.AreEqual (1, gr.Count ());
		}

		[Test ()]
		[Ignore ("Not implemented")]
		public void Clear ()
		{
		}

		[Test ()]
		[Ignore ("Not implemented")]
		public void UpdateScore ()
		{
		}


		[Test ()]
		public void TestAddEvent ()
		{
			Project p = CreateProject (false);
			p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null, null, null, false);
			Assert.AreEqual (p.Timeline.Count, 0);
			p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null, null, null);
			Assert.AreEqual (p.Timeline.Count, 1);
			p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null, null, null);
			Assert.AreEqual (p.Timeline.Count, 2);
			p.AddEvent (new TimelineEvent ());
			Assert.AreEqual (p.Timeline.Count, 3);
			p.AddEvent (new TimelineEvent ());
			Assert.AreEqual (p.Timeline.Count, 4);
			/*FIXME: add test for score event updating pd score */
		}

		[Test ()]
		public void TestRemoveEvents ()
		{
			TimelineEvent p1, p2, p3;
			List<TimelineEvent> plays = new List<TimelineEvent> ();
			Project p = CreateProject (false);
			
			p1 = new TimelineEvent ();
			p2 = new TimelineEvent ();
			p3 = new TimelineEvent ();
			p.AddEvent (p1);
			p.AddEvent (p2);
			p.AddEvent (p3);
			plays.Add (p1);
			plays.Add (p2);
			p.RemoveEvents (plays);
			Assert.AreEqual (p.Timeline.Count, 1);
			Assert.AreEqual (p.Timeline [0], p3);
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestCleanupTimers ()
		{
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestUpdateEventTypesAndTimers ()
		{
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestSubstituePlayer ()
		{
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestCurrentLineup ()
		{
		}

		[Test ()] 
		public void TestEventsByType ()
		{
			Project p = CreateProject ();
			Assert.AreEqual (2, p.EventsByType (p.EventTypes [0]).Count);
			Assert.AreEqual (1, p.EventsByType (p.EventTypes [1]).Count);
			Assert.AreEqual (3, p.EventsByType (p.EventTypes [2]).Count);
			Assert.AreEqual (0, p.EventsByType (p.EventTypes [3]).Count);
			Assert.AreEqual (1, p.EventsByType (p.EventTypes [6]).Count);
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestGetScore ()
		{
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestEventTaggedTeam ()
		{
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestConsolidateDescription ()
		{
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestEquals ()
		{
			Project p1 = CreateProject ();
			Project p2 = Utils.SerializeDeserialize (p1);
			Project p3 = new Project ();
			
			Assert.IsTrue (p1.Equals (p2));
			Assert.IsFalse (p1.Equals (p3));
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestExport ()
		{
		}

		[Test ()] 
		[Ignore ("Not implemented")]
		public void TestImport ()
		{
		}
	}
}
