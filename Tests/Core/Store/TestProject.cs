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
using LongoMatch.Core.Store.Templates;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using Constants = LongoMatch.Core.Common.Constants;
using Timer = VAS.Core.Store.Timer;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestProject
	{
		LMProject CreateProject (bool fill = true)
		{
			LMProject p = new LMProject ();
			p.Dashboard = LMDashboard.DefaultTemplate (10);
			p.UpdateEventTypesAndTimers ();
			p.LocalTeamTemplate = LMTeam.DefaultTemplate (10);
			p.VisitorTeamTemplate = LMTeam.DefaultTemplate (12);
			MediaFile mf = new MediaFile ("path", 34000, 25, true, true, "mp4", "h264",
							   "aac", 320, 240, 1.3, null, "Test asset");
			var pd = new ProjectDescription ();
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

		[OneTimeSetUp]
		public void FixtureSetup () {
			SetupClass.SetUp ();
		}

		[Test ()]
		public void TestSerialization ()
		{
			LMProject p = new LMProject ();

			Utils.CheckSerialization (p);

			p = CreateProject ();
			Utils.CheckSerialization (p);
			p.AddEvent (new LMTimelineEvent ());
			Utils.CheckSerialization (p);

			LMProject newp = Utils.SerializeDeserialize (p);
			Assert.AreEqual (newp.CompareTo (p), 0);
			Assert.AreEqual (newp.Timeline.Count, p.Timeline.Count);
		}

		[Test ()]
		public void TestProjectSetInTimelineEvents ()
		{
			LMProject p = CreateProject ();
			LMTimelineEvent evt = new LMTimelineEvent ();
			p.AddEvent (evt);
			LMProject newp = Utils.SerializeDeserialize (p);
			Assert.AreEqual (newp, newp.Timeline [0].Project);
		}

		[Test ()]
		public void TestIsFakeCapture ()
		{
			LMProject p = new LMProject ();
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
			LMProject p = CreateProject (false);
			LMTimelineEvent evt = p.CreateEvent (p.EventTypes [0], new Time (1000), new Time (2000),
											  null, null, 0) as LMTimelineEvent;
			Assert.AreEqual (p, evt.Project);

			Assert.AreEqual (p.Timeline.Count, 0);
			p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null);
			Assert.AreEqual (p.Timeline.Count, 1);
			p.AddEvent (p.EventTypes [0], new Time (1000), new Time (2000), null, null);
			Assert.AreEqual (p.Timeline.Count, 2);

			evt = new LMTimelineEvent ();
			p.AddEvent (evt);
			Assert.AreEqual (p, evt.Project);
			Assert.AreEqual (p.Description.FileSet, evt.FileSet);
			Assert.AreEqual (p.Timeline.Count, 3);
			p.AddEvent (new LMTimelineEvent ());
			Assert.AreEqual (p.Timeline.Count, 4);
			/*FIXME: add test for score event updating pd score */
		}

		[Test ()]
		public void TestRemoveEvents ()
		{
			LMTimelineEvent p1, p2, p3;
			List<LMTimelineEvent> plays = new List<LMTimelineEvent> ();
			LMProject p = CreateProject (false);

			p1 = new LMTimelineEvent ();
			p2 = new LMTimelineEvent ();
			p3 = new LMTimelineEvent ();
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
		public void TestUpdateEventTypesAndTimers ()
		{
			LMProject p = new LMProject ();
			p.Dashboard = LMDashboard.DefaultTemplate (5);
			Assert.AreEqual (0, p.Timers.Count);
			Assert.AreEqual (0, p.EventTypes.Count);
			p.UpdateEventTypesAndTimers ();
			Assert.AreEqual (1, p.Timers.Count);
			Assert.AreEqual (10, p.EventTypes.Count);

			// Delete a category button with no events
			p.Dashboard.List.Remove (p.Dashboard.List.OfType<AnalysisEventButton> ().First ());
			p.UpdateEventTypesAndTimers ();
			Assert.AreEqual (1, p.Timers.Count);
			Assert.AreEqual (9, p.EventTypes.Count);

			// Delete a category button with events in the timeline
			AnalysisEventButton button = p.Dashboard.List.OfType<AnalysisEventButton> ().First ();
			p.Timeline.Add (new LMTimelineEvent { EventType = button.EventType });
			p.UpdateEventTypesAndTimers ();
			Assert.AreEqual (1, p.Timers.Count);
			Assert.AreEqual (9, p.EventTypes.Count);
			p.Dashboard.List.Remove (button);
			p.UpdateEventTypesAndTimers ();
			Assert.AreEqual (1, p.Timers.Count);
			Assert.AreEqual (9, p.EventTypes.Count);

			// Remove the event from the timeline, the event type is no longuer in the dashboard or the timeline
			p.Timeline.Clear ();
			p.UpdateEventTypesAndTimers ();
			Assert.AreEqual (1, p.Timers.Count);
			Assert.AreEqual (8, p.EventTypes.Count);
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
			LMProject p1 = CreateProject ();
			LMProject p2 = Utils.SerializeDeserialize (p1);
			LMProject p3 = new LMProject ();

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
		public void TestIsChanged ()
		{
			LMProject p = new LMProject ();
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Dashboard = new LMDashboard ();
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.LocalTeamTemplate = new LMTeam ();
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.VisitorTeamTemplate = new LMTeam ();
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Description = new ProjectDescription ();
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Timeline.Add (new LMTimelineEvent ());
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
			p.Periods.Add (new Period ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
			p.Timers.Add (new Timer ());
			Assert.IsTrue (p.IsChanged);
			p.IsChanged = false;
		}


	}
}
