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
using LongoMatch.Common;
using LongoMatch.Store;

namespace Tests.Core
{
	[TestFixture()]
	public class TestPlay
	{
		Category cat1;
		
		public Play CreatePlay () {
			Play play = new Play();
			cat1 = new Category {Name="Cat1"};
			
			play.Category = cat1;
			play.Notes = "notes";
			play.Selected = true;
			play.Team = LongoMatch.Common.Team.LOCAL;
			play.FieldPosition = new Coordinates();
			play.FieldPosition.Points.Add (new Point (1, 2));
			play.HalfFieldPosition = new Coordinates ();
			play.HalfFieldPosition.Points.Add (new Point (4,5));
			play.GoalPosition = new Coordinates ();
			play.GoalPosition.Points.Add (new Point (6, 7));
			play.Rate = 1.5f;
			play.Name = "Play";
			play.Start = new Time(1000);
			play.Stop = new Time(2000);
			play.Rate = 2.3f;
			
			play.Tags.Add(new Tag ("test"));
			return play;
		}
		
		[Test()]
		public void TestCase ()
		{
			Play p = new Play ();
			Utils.CheckSerialization (p);
			
			p = CreatePlay ();
			var newp = Utils.SerializeDeserialize (p);
			
			Assert.AreEqual (p.Category.ID, newp.Category.ID);
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
	}
}

