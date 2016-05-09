//
//  Copyright (C) 2015 Andoni Morales Alastruey
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
using LongoMatch.Core.Store;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestDashboardButton
	{
		[Test ()]
		public void TestSerialization ()
		{
			DashboardButton db = new DashboardButton ();
			Utils.CheckSerialization (db);
			db = new PenaltyCardButton ();
			Utils.CheckSerialization (db);
			db = new PenaltyCardButton ();
			Utils.CheckSerialization (db);
			db = new ScoreButton ();
			Utils.CheckSerialization (db);
		}

		[Test ()]
		public void TestTimerButton ()
		{
			TimerButtonLongoMatch tm = new TimerButtonLongoMatch ();
			Assert.IsNull (tm.Name);
			tm.Timer = new TimerLongoMatch { Name = "test" };
			Assert.AreEqual (tm.Name, "test");
			tm.Name = "test2";
			Assert.AreEqual (tm.Timer.Name, "test2");
		}

		[Test ()]
		public void TestPenaltyCardButton ()
		{
			PenaltyCardButton pb = new PenaltyCardButton ();
			Assert.IsNull (pb.BackgroundColor);
			Assert.IsNull (pb.Name);
			pb.PenaltyCard = new PenaltyCard ("test", Color.Red, CardShape.Circle);
			Assert.AreEqual (pb.Name, "test");
			Assert.AreEqual (pb.BackgroundColor, Color.Red);
			Assert.AreEqual (pb.PenaltyCardEventType, pb.EventType);
		}

		[Test ()]
		public void TestScoreButton ()
		{
			ScoreButton sb = new ScoreButton ();
			Assert.IsNull (sb.Score);
			sb.Score = new Score ("test", 2);
			Assert.AreEqual (sb.Name, "test");
			Assert.AreEqual (sb.BackgroundColor, sb.EventType.Color);
			Assert.AreEqual (sb.ScoreEventType, sb.EventType);
		}

		[Test ()]
		public void TestIsChanged ()
		{
			var tib = new TimerButtonLongoMatch ();
			Assert.IsTrue (tib.IsChanged);
			tib.IsChanged = false;
			tib.Timer = new TimerLongoMatch ();
			Assert.IsTrue (tib.IsChanged);
			tib.IsChanged = false;
		}
	}
}

