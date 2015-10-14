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
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using NUnit.Framework;
using System.Collections.ObjectModel;
using LongoMatch.Core.Store.Templates;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestTimelineEvent
	{
		EventType evtType1;

		public TimelineEvent CreateTimelineEvent ()
		{
			TimelineEvent evt = new TimelineEvent ();
			evtType1 = new EventType { Name = "Cat1" };
			
			evt.EventType = evtType1;
			evt.Notes = "notes";
			evt.Selected = true;
			evt.Team = TeamType.LOCAL;
			evt.FieldPosition = new Coordinates ();
			evt.FieldPosition.Points.Add (new Point (1, 2));
			evt.HalfFieldPosition = new Coordinates ();
			evt.HalfFieldPosition.Points.Add (new Point (4, 5));
			evt.GoalPosition = new Coordinates ();
			evt.GoalPosition.Points.Add (new Point (6, 7));
			evt.Rate = 1.5f;
			evt.Name = "Play";
			evt.Start = new Time (1000);
			evt.EventTime = new Time (1500);
			evt.Stop = new Time (2000);
			evt.Rate = 2.3f;

			evt.Tags.Add (new Tag ("test"));
			return evt;
		}

		[Test ()]
		public void TestSerialization ()
		{
			TimelineEvent p = new TimelineEvent ();
			Utils.CheckSerialization (p);
			
			p = CreateTimelineEvent ();
			var newp = Utils.SerializeDeserialize (p);
			
			Assert.AreEqual (p.EventType.ID, newp.EventType.ID);
			Assert.AreEqual (p.Notes, newp.Notes);
			Assert.AreEqual (p.Team, newp.Team);
			Assert.AreEqual (p.FieldPosition, newp.FieldPosition);
			Assert.AreEqual (p.HalfFieldPosition, newp.HalfFieldPosition);
			Assert.AreEqual (p.GoalPosition, newp.GoalPosition);
			Assert.AreEqual (p.Rate, newp.Rate);
			Assert.AreEqual (p.Name, newp.Name);
			Assert.AreEqual (p.Start, newp.Start);
			Assert.AreEqual (p.Stop, newp.Stop);
			Assert.AreEqual (p.Rate, newp.Rate);
			Assert.AreEqual (p.CamerasConfig, new List<CameraConfig> { new CameraConfig (0) });
			Assert.IsNull (p.CamerasLayout);
		}

		[Test()]
		public void TestEquals() {
			TimelineEvent evt1 = new TimelineEvent ();
			TimelineEvent evt2 = new TimelineEvent ();
			Assert.AreNotEqual (evt1, evt2);
			evt2.ID = evt1.ID;
			Assert.AreEqual (evt1, evt2);
			evt2.ID = Guid.Parse (evt1.ID.ToString());
			Assert.AreEqual (evt1, evt2);
		}

		[Test ()]
		public void TestProperties ()
		{
			TimelineEvent evt = CreateTimelineEvent ();
			Assert.AreEqual (evt.HasDrawings, false);
			Assert.AreEqual (evt.Color, evt.EventType.Color);
			Assert.AreEqual (evt.Description,
				String.Format ("Play\ntest\n0:01,000 - 0:02,000 ({0}X)", 2.3));
		}

		[Test ()]
		public void TestTagsDescription ()
		{
			TimelineEvent evt = CreateTimelineEvent ();
			Assert.AreEqual (evt.TagsDescription (), "test");
			evt.Tags.Add (new Tag ("test2"));
			Assert.AreEqual (evt.TagsDescription (), "test-test2");
			evt.Tags = new ObservableCollection<Tag> ();
			Assert.AreEqual (evt.TagsDescription (), "");
		}

		[Test ()]
		public void TestTimesDescription ()
		{
			TimelineEvent evt = CreateTimelineEvent ();
			Assert.AreEqual (evt.TimesDesription (),
				String.Format ("0:01,000 - 0:02,000 ({0}X)", 2.3));
			evt.Rate = 1;
			Assert.AreEqual (evt.TimesDesription (), "0:01,000 - 0:02,000");
		}

		[Test ()]
		public void TestAddDefaultPositions ()
		{
			TimelineEvent evt = new TimelineEvent ();
			evt.EventType = new EventType ();
			evt.EventType.TagFieldPosition = false;
			evt.EventType.TagHalfFieldPosition = false;
			evt.EventType.TagGoalPosition = false;
			
			Assert.IsNull (evt.FieldPosition);
			Assert.IsNull (evt.HalfFieldPosition);
			Assert.IsNull (evt.GoalPosition);
			evt.AddDefaultPositions ();
			Assert.IsNull (evt.FieldPosition);
			Assert.IsNull (evt.HalfFieldPosition);
			Assert.IsNull (evt.GoalPosition);
			
			evt.EventType.TagFieldPosition = true;
			evt.AddDefaultPositions ();
			Assert.IsNotNull (evt.FieldPosition);
			Assert.IsNull (evt.HalfFieldPosition);
			Assert.IsNull (evt.GoalPosition);
			
			evt.EventType.TagFieldPosition = false;
			evt.EventType.TagHalfFieldPosition = true;
			evt.AddDefaultPositions ();
			Assert.IsNotNull (evt.FieldPosition);
			Assert.IsNotNull (evt.HalfFieldPosition);
			Assert.IsNull (evt.GoalPosition);
			
			evt.EventType.TagFieldPosition = false;
			evt.EventType.TagHalfFieldPosition = false;
			evt.EventType.TagGoalPosition = true;
			evt.AddDefaultPositions ();
			Assert.IsNotNull (evt.FieldPosition);
			Assert.IsNotNull (evt.HalfFieldPosition);
			Assert.IsNotNull (evt.GoalPosition);

			/* Adding the default positions doesn't not overwrite the existing data */
			evt.EventType.FieldPositionIsDistance = true;
			evt.EventType.HalfFieldPositionIsDistance = true;
			evt.FieldPosition.Points [0].X = 100;
			evt.FieldPosition.Points [0].Y = 100;
			evt.HalfFieldPosition.Points [0].X = 100;
			evt.HalfFieldPosition.Points [0].Y = 100;
			evt.AddDefaultPositions ();
			Assert.AreEqual (1, evt.FieldPosition.Points.Count);
			Assert.AreEqual (1, evt.HalfFieldPosition.Points.Count);
			Assert.AreEqual (100, evt.FieldPosition.Points [0].X);
			Assert.AreEqual (100, evt.HalfFieldPosition.Points [0].X);
		}

		[Test ()]
		public void TestCoordinatesInFieldPosition ()
		{
			TimelineEvent evt = CreateTimelineEvent ();
			Assert.AreEqual (evt.CoordinatesInFieldPosition (FieldPositionType.Field),
				evt.FieldPosition);
			Assert.AreEqual (evt.CoordinatesInFieldPosition (FieldPositionType.HalfField),
				evt.HalfFieldPosition);
			Assert.AreEqual (evt.CoordinatesInFieldPosition (FieldPositionType.Goal),
				evt.GoalPosition);
		}

		[Test ()]
		public void TestUpdateCoordinates ()
		{
			TimelineEvent evt = CreateTimelineEvent ();
			evt.UpdateCoordinates (FieldPositionType.Field, new ObservableCollection<Point> { new Point (4, 5) });
			Assert.AreEqual (evt.FieldPosition.Points [0].X, 4);
			Assert.AreEqual (evt.FieldPosition.Points [0].Y, 5);
			
			evt.UpdateCoordinates (FieldPositionType.HalfField,new ObservableCollection<Point> { new Point (4, 5) });
			Assert.AreEqual (evt.HalfFieldPosition.Points [0].X, 4);
			Assert.AreEqual (evt.HalfFieldPosition.Points [0].Y, 5);
			
			evt.UpdateCoordinates (FieldPositionType.Goal, new ObservableCollection<Point> { new Point (4, 5) });
			Assert.AreEqual (evt.GoalPosition.Points [0].X, 4);
			Assert.AreEqual (evt.GoalPosition.Points [0].Y, 5);
		}

		[Test()]
		public void TestIsChanged () {
			TimelineEvent evt = new TimelineEvent ();
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.EventType = new EventType ();
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.Notes = "test";
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.Rate = 2f;
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.Team = TeamType.BOTH;
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.FieldPosition = new Coordinates ();
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.HalfFieldPosition = new Coordinates ();
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.GoalPosition = new Coordinates ();
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.Tags.Add (new Tag ("2"));
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.Tags = null;
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.CamerasConfig.Add (new CameraConfig (2));
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.CamerasConfig = null;
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.Players.Add (new Player ());
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
		}

		[Test ()] 
		public void TestTaggedTeam ()
		{
			Team home = Team.DefaultTemplate (3);
			Team away = Team.DefaultTemplate (3);
			Project p = new Project ();
			p.LocalTeamTemplate = home;
			p.VisitorTeamTemplate = away;
			TimelineEvent evt = new TimelineEvent ();
			evt.Project = p;

			Assert.AreEqual (TeamType.NONE, evt.TaggedTeam);

			evt.Team = TeamType.LOCAL;
			Assert.AreEqual (TeamType.LOCAL, evt.TaggedTeam);

			evt.Team = TeamType.VISITOR;
			Assert.AreEqual (TeamType.VISITOR, evt.TaggedTeam);

			evt.Team = TeamType.BOTH;
			Assert.AreEqual (TeamType.BOTH, evt.TaggedTeam);

			evt.Team = TeamType.NONE;
			evt.Players = new ObservableCollection<Player> { home.List [0] };
			Assert.AreEqual (TeamType.LOCAL, evt.TaggedTeam);
			evt.Team = TeamType.VISITOR;
			Assert.AreEqual (TeamType.BOTH, evt.TaggedTeam);

			evt.Team = TeamType.NONE;
			evt.Players = new ObservableCollection<Player> { away.List [0] };
			Assert.AreEqual (TeamType.VISITOR, evt.TaggedTeam);

			evt.Team = TeamType.LOCAL;
			Assert.AreEqual (TeamType.BOTH, evt.TaggedTeam);

			evt.Team = TeamType.NONE;
			evt.Players = new ObservableCollection<Player> { home.List [0], away.List [0] };
			Assert.AreEqual (TeamType.BOTH, evt.TaggedTeam);
		}

	}
}

