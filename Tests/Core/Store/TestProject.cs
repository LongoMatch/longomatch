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
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Core.Store.Templates;
using NUnit.Framework;

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
				p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null);
				p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null);
				p.AddEvent (p.EventTypes [1], new Time (1000), new Time (2000), null, null);
				p.AddEvent (p.EventTypes [2], new Time (1000), new Time (2000), null, null);
				p.AddEvent (p.EventTypes [2], new Time (1000), new Time (2000), null, null);
				p.AddEvent (p.EventTypes [2], new Time (1000), new Time (2000), null, null);
				p.AddEvent (p.EventTypes [6], new Time (1000), new Time (2000), null, null);
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
			Assert.AreEqual (newp.Timeline.Count, p.Timeline.Count);
		}

		[Test ()]
		public void TestIsFakeCapture ()
		{
			Project p = new Project ();
			Assert.IsFalse (p.IsFakeCapture);
			p.Description = new ProjectDescription ();
			Assert.IsFalse (p.IsFakeCapture);
			p.Description.FileSet = new MediaFileSet ();
			Assert.IsFalse (p.IsFakeCapture);
			p.Description.FileSet.Add (new MediaFile ());
			Assert.IsFalse (p.IsFakeCapture);
			p.Description.FileSet [0].FilePath = Constants.FAKE_PROJECT;
			Assert.IsTrue (p.IsFakeCapture);
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
			TimelineEvent evt = p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null, false);
			Assert.AreEqual (p, evt.Project);

			Assert.AreEqual (p.Timeline.Count, 0);
			p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null);
			Assert.AreEqual (p.Timeline.Count, 1);
			p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null);
			Assert.AreEqual (p.Timeline.Count, 2);

			evt = new TimelineEvent ();
			p.AddEvent (evt);
			Assert.AreEqual (p, evt.Project);
			Assert.AreEqual (p.Description.FileSet, evt.FileSet);
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

		[Test ()]
		public void TestResyncEvents ()
		{
			Project p = CreateProject (false);
			int offset1 = 100, offset2 = 120, offset3 = 150;
			Period period;
			List<Period> syncedPeriods;

			period = new Period ();
			period.Nodes.Add (new TimeNode { Start = new Time (0),
				Stop = new Time (3000)
			});
			p.Periods.Add (period);
			period = new Period ();
			period.Nodes.Add (new TimeNode { Start = new Time (3001),
				Stop = new Time (6000)
			});
			p.Periods.Add (period);
			period = new Period ();
			period.Nodes.Add (new TimeNode { Start = new Time (6001),
				Stop = new Time (6500)
			});
			p.Periods.Add (period);

			/* Test with a list of periods that don't match */
			Assert.Throws<IndexOutOfRangeException> (
				delegate {
					p.ResyncEvents (new List<Period> ());
				});

			syncedPeriods = new List<Period> ();
			period = new Period ();
			period.Nodes.Add (new TimeNode {
				Start = new Time (0 + offset1),
				Stop = new Time (3000 + offset1)
			});
			syncedPeriods.Add (period);
			period = new Period ();
			period.Nodes.Add (new TimeNode {
				Start = new Time (3001 + offset2),
				Stop = new Time (6000 + offset2)
			});
			syncedPeriods.Add (period);
			period = new Period ();
			period.Nodes.Add (new TimeNode {
				Start = new Time (6001 + offset3),
				Stop = new Time (6500 + offset3)
			});
			syncedPeriods.Add (period);

			/* 1st Period */
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (0) });
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (1500) });
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (3000) });
			/* 2nd Period */
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (3001) });
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (4500) });
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (6000) });
			/* 3nd Period */
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (6001) });
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (6200) });
			p.Timeline.Add (new TimelineEvent { EventTime = new Time (6500) });

			IList<TimelineEvent> oldTimeline = p.Timeline.Clone ();

			p.ResyncEvents (syncedPeriods);
			Assert.AreEqual (oldTimeline [0].EventTime + offset1, p.Timeline [0].EventTime);
			Assert.AreEqual (oldTimeline [1].EventTime + offset1, p.Timeline [1].EventTime);
			Assert.AreEqual (oldTimeline [2].EventTime + offset1, p.Timeline [2].EventTime);

			Assert.AreEqual (oldTimeline [3].EventTime + offset2, p.Timeline [3].EventTime);
			Assert.AreEqual (oldTimeline [4].EventTime + offset2, p.Timeline [4].EventTime);
			Assert.AreEqual (oldTimeline [5].EventTime + offset2, p.Timeline [5].EventTime);

			Assert.AreEqual (oldTimeline [6].EventTime + offset3, p.Timeline [6].EventTime);
			Assert.AreEqual (oldTimeline [7].EventTime + offset3, p.Timeline [7].EventTime);
			Assert.AreEqual (oldTimeline [8].EventTime + offset3, p.Timeline [8].EventTime);
		}

		[Test ()]
		public void TestIsChanged ()
		{
			Project p = new Project ();
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Dashboard = new Dashboard ();
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.LocalTeamTemplate = new Team ();
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.VisitorTeamTemplate = new Team ();
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Description = new ProjectDescription ();
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Timeline.Add (new TimelineEvent ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Timeline = null;
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.EventTypes.Add (new EventType ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.EventTypes = null;
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Playlists.Add (new Playlist ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Playlists = null;
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Periods.Add (new Period ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Periods = null;
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Timers.Add (new Timer ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Timers = null;
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
		}


	}
}
