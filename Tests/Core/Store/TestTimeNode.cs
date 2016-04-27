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
using VAS.Core.Store;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestTimeNode
	{
		[Test ()]
		public void TestSerialization ()
		{
			TimeNode tn = new TimeNode ();
			
			Utils.CheckSerialization (tn);
			
			tn.Start = new Time (1000);
			tn.Stop = new Time (2000);
			tn.Name = "Test";
			tn.Rate = 2.0f;
			
			Utils.CheckSerialization (tn);
			
			TimeNode newtn = Utils.SerializeDeserialize (tn);
			Assert.AreEqual (tn.Start, newtn.Start);
			Assert.AreEqual (tn.Stop, newtn.Stop);
			Assert.AreEqual (tn.Name, newtn.Name);
			Assert.AreEqual (tn.Rate, newtn.Rate);
		}

		[Test ()]
		public void TestDuration ()
		{
			TimeNode tn = new TimeNode ();
			tn.Start = new Time (1000);
			tn.Stop = new Time (2000);
			Assert.AreEqual (tn.Duration, tn.Stop - tn.Start);
		}

		[Test ()]
		public void TestUpdateEventTime ()
		{
			TimeNode tn = new TimeNode ();
			tn.Start = new Time (1000);
			tn.Stop = new Time (2000);
			Assert.AreEqual (tn.EventTime, tn.Start);
			tn.EventTime = new Time (1500);
			Assert.AreEqual (tn.EventTime.MSeconds, 1500);
			/* EventTime is updated to match the time node boundaries */
			tn.Stop = new Time (1400);
			Assert.AreEqual (tn.EventTime, tn.Stop);
			tn.Start = new Time (1405);
			Assert.AreEqual (tn.EventTime, tn.Start);
		}

		[Test ()]
		public void TestMove ()
		{
			TimeNode tn = new TimeNode ();
			tn.Start = new Time (1000);
			tn.EventTime = new Time (1500);
			tn.Stop = new Time (2000);
			tn.Move (new Time (100));
			Assert.AreEqual (tn.Start.MSeconds, 1100);
			Assert.AreEqual (tn.EventTime.MSeconds, 1600);
			Assert.AreEqual (tn.Stop.MSeconds, 2100);
		}

		[Test ()]
		public void TestJoin ()
		{
			TimeNode tn;
			TimeNode tn1 = new TimeNode ();
			TimeNode tn2 = new TimeNode ();
			tn1.Start = new Time (1000);
			tn1.Stop = new Time (2000);

			/* Lower outbound join */
			tn2.Start = new Time (0);
			tn2.Stop = new Time (900);
			Assert.IsNull (tn1.Join (tn2));
			
			/* Upper limit join */
			tn2.Start = new Time (2100);
			tn2.Stop = new Time (3000);
			Assert.IsNull (tn1.Join (tn2));

			/* Lower limit join */
			tn2.Start = new Time (0);
			tn2.Stop = new Time (1000);
			tn = tn1.Join (tn2);
			Assert.AreEqual (tn.Start, tn2.Start);
			Assert.AreEqual (tn.Stop, tn1.Stop);
			
			/* Upper limit join */
			tn2.Start = new Time (2000);
			tn2.Stop = new Time (2100);
			tn = tn1.Join (tn2);
			Assert.AreEqual (tn.Start, tn1.Start);
			Assert.AreEqual (tn.Stop, tn2.Stop);
			
			/* Upper Join */
			tn2.Start = new Time (1900);
			tn = tn1.Join (tn2);
			Assert.AreEqual (tn.Start, tn1.Start);
			Assert.AreEqual (tn.Stop, tn2.Stop);
			
			/* Lower Join */
			tn2.Start = new Time (500);
			tn2.Stop = new Time (1500);
			tn = tn1.Join (tn2);
			Assert.AreEqual (tn.Start, tn2.Start);
			Assert.AreEqual (tn.Stop, tn1.Stop);
			
			/* Whole Join */
			tn2.Start = new Time (500);
			tn2.Stop = new Time (2500);
			tn = tn1.Join (tn2);
			Assert.AreEqual (tn.Start, tn2.Start);
			Assert.AreEqual (tn.Stop, tn2.Stop);
		}

		[Test ()]
		public void TestIntersect ()
		{
			TimeNode tn;
			TimeNode tn1 = new TimeNode ();
			TimeNode tn2 = new TimeNode ();
			tn1.Start = new Time (1000);
			tn1.Stop = new Time (2000);
			
			/* Lower out bounds */
			tn2.Start = new Time (0);
			tn2.Stop = new Time (1000);
			Assert.IsNull (tn1.Intersect (tn2));
			
			/* Upper out bounds */
			tn2.Start = new Time (2000);
			tn2.Stop = new Time (2100);
			Assert.IsNull (tn1.Intersect (tn2));

			/* Intersection */
			tn2.Start = new Time (1500);
			tn2.Stop = new Time (2400);
			TimeNode tn3 = tn1.Intersect (tn2);
			Assert.AreEqual (1500, tn3.Start.MSeconds);
			Assert.AreEqual (2000, tn3.Stop.MSeconds);
		}
	}
}

