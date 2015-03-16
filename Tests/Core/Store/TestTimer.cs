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
using NUnit.Framework;
using System;
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestTimer
	{
		[Test ()]
		public void TestSerialization ()
		{
			Timer timer = new Timer ();
			Utils.CheckSerialization (timer);

			timer.Name = "test";
			timer.Team = LongoMatch.Core.Common.TeamType.LOCAL;
			Timer timer2 = Utils.SerializeDeserialize (timer);
			Assert.AreEqual (timer.Name, timer2.Name);
			Assert.AreEqual (timer.Nodes, timer2.Nodes);
			Assert.AreEqual (timer.Team, timer2.Team);
		}

		[Test ()]
		public void TestTotalTime ()
		{
			Timer timer = new Timer { Name = "Test" };
			timer.Start (new Time (1000));
			timer.Stop (new Time (2000));
			Assert.AreEqual (1000, timer.TotalTime.MSeconds);
			timer.Start (new Time (3000));
			Assert.AreEqual (1000, timer.TotalTime.MSeconds);
			timer.Stop (new Time (4000));
			Assert.AreEqual (2000, timer.TotalTime.MSeconds);
		}

		[Test ()]
		public void TestStartTimer ()
		{
			Timer timer = new Timer { Name = "test" };

			timer.Start (new Time (1000));
			Assert.AreEqual (1, timer.Nodes.Count);
			Assert.AreEqual ("test", timer.Nodes [0].Name);
			Assert.AreEqual (1000, timer.Nodes [0].Start.MSeconds);
			Assert.IsNull (timer.Nodes [0].Stop);

			timer.Start (new Time (5000), "new");
			Assert.AreEqual (2, timer.Nodes.Count);
			/* Starting a time should stop the previous period */
			Assert.AreEqual (5000, timer.Nodes [0].Stop.MSeconds);
			Assert.AreEqual ("new", timer.Nodes [1].Name);
			Assert.AreEqual (5000, timer.Nodes [1].Start.MSeconds);
		}

		[Test ()]
		public void TestStopTimer ()
		{
			Timer timer = new Timer { Name = "Test" };
			timer.Start (new Time (1000));
			Assert.IsNull (timer.Nodes [0].Stop);
			timer.Stop (new Time (1200));
			Assert.AreEqual (1200, timer.Nodes [0].Stop.MSeconds);
		}

		[Test ()]
		public void TestCancelTimer ()
		{
			Timer timer = new Timer { Name = "Test" };
			timer.Start (new Time (1000));
			timer.Stop (new Time (2000));
			timer.CancelCurrent ();
			Assert.AreEqual (1, timer.Nodes.Count);
			timer.Start (new Time (3000));
			Assert.AreEqual (2, timer.Nodes.Count);
			timer.CancelCurrent ();
			Assert.AreEqual (1, timer.Nodes.Count);
		}
	}
}

