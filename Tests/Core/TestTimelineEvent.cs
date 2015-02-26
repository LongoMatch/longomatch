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
using System.Collections.Generic;

namespace Tests.Core
{
	[TestFixture()]
	public class TestTimelineEvent
	{
		EventType evtType1;
		
		public TimelineEvent CreateTimelineEvent () {
			TimelineEvent evt = new TimelineEvent();
			evtType1 = new EventType {Name="Cat1"};
			
			evt.EventType = evtType1;
			evt.Notes = "notes";
			evt.Selected = true;
			evt.Team = Team.LOCAL;
			evt.FieldPosition = new Coordinates();
			evt.FieldPosition.Points.Add (new Point (1, 2));
			evt.HalfFieldPosition = new Coordinates ();
			evt.HalfFieldPosition.Points.Add (new Point (4,5));
			evt.GoalPosition = new Coordinates ();
			evt.GoalPosition.Points.Add (new Point (6, 7));
			evt.Rate = 1.5f;
			evt.Name = "Play";
			evt.Start = new Time(1000);
			evt.EventTime = new Time(1500);
			evt.Stop = new Time(2000);
			evt.Rate = 2.3f;
			
			evt.Tags.Add(new Tag ("test"));
			return evt;
		}
		
		[Test()]
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
		}
		
		[Test()]
		public void TestProperties ()
		{
			TimelineEvent evt = CreateTimelineEvent ();
			Assert.AreEqual (evt.HasDrawings, false);
			Assert.AreEqual (evt.Color, evt.EventType.Color);
			Assert.AreEqual (evt.Description, "Play\ntest\n0:01,000 - 0:02,000 (2,3X)");
		}
		
		[Test()]
		public void TestTagsDescription ()
		{
			TimelineEvent evt = CreateTimelineEvent ();
			Assert.AreEqual (evt.TagsDescription (), "test");
			evt.Tags.Add (new Tag ("test2"));
			Assert.AreEqual (evt.TagsDescription (), "test-test2");
			evt.Tags = new List<Tag> ();
			Assert.AreEqual (evt.TagsDescription (), "");
		}

		[Test()]
		public void TestTimesDescription ()
		{
			TimelineEvent evt = CreateTimelineEvent ();
			Assert.AreEqual (evt.TimesDesription (), "0:01,000 - 0:02,000 (2,3X)");
			evt.Rate = 1;
			Assert.AreEqual (evt.TimesDesription (), "0:01,000 - 0:02,000");
		}

		[Test()]
		public void TestAddDefaultPositions ()
		{
			TimelineEvent evt = new TimelineEvent();
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
		}
		
		[Test()]
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
		
		[Test()]
		public void TestUpdateCoordinates ()
		{
			TimelineEvent evt = CreateTimelineEvent ();
			evt.UpdateCoordinates (FieldPositionType.Field, new List<Point> {new Point (4, 5)});
			Assert.AreEqual (evt.FieldPosition.Points[0].X, 4);
			Assert.AreEqual (evt.FieldPosition.Points[0].Y, 5);
			
			evt.UpdateCoordinates (FieldPositionType.HalfField, new List<Point> {new Point (4, 5)});
			Assert.AreEqual (evt.HalfFieldPosition.Points[0].X, 4);
			Assert.AreEqual (evt.HalfFieldPosition.Points[0].Y, 5);
			
			evt.UpdateCoordinates (FieldPositionType.Goal, new List<Point> {new Point (4, 5)});
			Assert.AreEqual (evt.GoalPosition.Points[0].X, 4);
			Assert.AreEqual (evt.GoalPosition.Points[0].Y, 5);
		}
	}
}

