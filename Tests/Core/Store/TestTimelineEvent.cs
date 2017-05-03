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
using System.Collections.ObjectModel;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestTimelineEvent
	{
		EventType evtType1;

		public LMTimelineEvent CreateTimelineEvent ()
		{
			LMTimelineEvent evt = new LMTimelineEvent ();
			evtType1 = new EventType { Name = "Cat1" };

			evt.EventType = evtType1;
			evt.Notes = "notes";
			evt.Playing = true;
			evt.Teams.Add (new LMTeam ());
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
			LMTimelineEvent p = new LMTimelineEvent ();
			Utils.CheckSerialization (p);

			p = CreateTimelineEvent ();
			var newp = Utils.SerializeDeserialize (p);

			Assert.AreEqual (p.EventType.ID, newp.EventType.ID);
			Assert.AreEqual (p.Notes, newp.Notes);
			Assert.AreEqual (p.Teams, newp.Teams);
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

		[Test ()]
		public void TestEquals ()
		{
			LMTimelineEvent evt1 = new LMTimelineEvent ();
			LMTimelineEvent evt2 = new LMTimelineEvent ();
			Assert.AreNotEqual (evt1, evt2);
			evt2.ID = evt1.ID;
			Assert.AreEqual (evt1, evt2);
			evt2.ID = Guid.Parse (evt1.ID.ToString ());
			Assert.AreEqual (evt1, evt2);
		}

		[Test ()]
		public void TestProperties ()
		{
			LMTimelineEvent evt = CreateTimelineEvent ();
			Assert.AreEqual (evt.HasDrawings, false);
			Assert.AreEqual (evt.Color, evt.EventType.Color);
			Assert.AreEqual (evt.Description,
				String.Format ("Play\ntest\n0:01,000 - 0:02,000 ({0}X)", 2.3));
		}

		[Test ()]
		public void TestTagsDescription ()
		{
			LMTimelineEvent evt = CreateTimelineEvent ();
			Assert.AreEqual (evt.TagsDescription (), "test");
			evt.Tags.Add (new Tag ("test2"));
			Assert.AreEqual (evt.TagsDescription (), "test-test2");
			evt.Tags.Replace (new ObservableCollection<Tag> ());
			Assert.AreEqual (evt.TagsDescription (), "");
		}

		[Test ()]
		public void TestTimesDescription ()
		{
			LMTimelineEvent evt = CreateTimelineEvent ();
			Assert.AreEqual (evt.TimesDesription (),
				String.Format ("0:01,000 - 0:02,000 ({0}X)", 2.3));
			evt.Rate = 1;
			Assert.AreEqual (evt.TimesDesription (), "0:01,000 - 0:02,000");
		}

		[Test ()]
		public void TestAddDefaultPositions ()
		{
			LMTimelineEvent evt = new LMTimelineEvent ();
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
			LMTimelineEvent evt = CreateTimelineEvent ();
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
			LMTimelineEvent evt = CreateTimelineEvent ();
			evt.UpdateCoordinates (FieldPositionType.Field, new RangeObservableCollection<Point> { new Point (4, 5) });
			Assert.AreEqual (evt.FieldPosition.Points [0].X, 4);
			Assert.AreEqual (evt.FieldPosition.Points [0].Y, 5);

			evt.UpdateCoordinates (FieldPositionType.HalfField, new RangeObservableCollection<Point> { new Point (4, 5) });
			Assert.AreEqual (evt.HalfFieldPosition.Points [0].X, 4);
			Assert.AreEqual (evt.HalfFieldPosition.Points [0].Y, 5);

			evt.UpdateCoordinates (FieldPositionType.Goal, new RangeObservableCollection<Point> { new Point (4, 5) });
			Assert.AreEqual (evt.GoalPosition.Points [0].X, 4);
			Assert.AreEqual (evt.GoalPosition.Points [0].Y, 5);
		}

		[Test ()]
		public void TestIsChanged ()
		{
			LMTimelineEvent evt = new LMTimelineEvent ();
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
			evt.Teams.Add (new LMTeam ());
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
			evt.CamerasConfig.Add (new CameraConfig (2));
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.CamerasConfig = null;
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
			evt.Players.Add (new LMPlayer ());
			Assert.IsTrue (evt.IsChanged);
			evt.IsChanged = false;
		}

		[Test ()]
		public void TestCamerasConfigNotHavingNulls ()
		{
			TimelineEvent evt = CreateTimelineEvent ();
			var cc = new ObservableCollection<CameraConfig> ();
			cc.Add (null);
			cc.Add (new CameraConfig (0));
			evt.CamerasConfig = cc;

			Assert.NotNull (evt.CamerasConfig [0]);
			Assert.AreEqual (evt.CamerasConfig [0].Index, new CameraConfig (1).Index);
		}
	}
}

