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
using LongoMatch.Core.Store;
using NUnit.Framework;

namespace Tests.Core.Store
{
	[TestFixture()]
	public class TestTime
	{
		[Test()]
		public void TestSerialization ()
		{
			Time t = new Time (1000);
			Utils.CheckSerialization (t);
		}
		
		[Test()]
		public void TestProperties ()
		{
			Time t = new Time (12323456);
			Assert.AreEqual (t.NSeconds, 12323456000000);
			Assert.AreEqual (t.TotalSeconds, 12323);
			Assert.AreEqual (t.Hours, 3);
			Assert.AreEqual (t.Minutes, 25);
			Assert.AreEqual (t.Seconds, 23);
		}
		
		[Test()]
		public void TestStrings ()
		{
			Time t = new Time (2323456);
			Assert.AreEqual (t.ToMSecondsString (false), "38:43,456");
			Assert.AreEqual (t.ToMSecondsString (true), "0:38:43,456");
			Assert.AreEqual (t.ToSecondsString (false), "38:43");
			Assert.AreEqual (t.ToSecondsString (true), "0:38:43");
		}
		
		[Test()]
		public void TestOperators ()
		{
			Time t1 = new Time (2000);
			Time t2 = new Time (1000);
			
			Assert.IsTrue (t1 > t2);
			Assert.IsTrue (t2 < t1);
			t2.MSeconds = 2000;
			Assert.IsTrue (t1 >= t2);
			Assert.IsTrue (t1 <= t2);
			Assert.IsTrue (t1 == t2);
			t2.MSeconds = 1000;
			Assert.IsFalse (t1 == t2);
			t2.MSeconds = 0;
			Assert.IsTrue (t1 != t2);
			t2.MSeconds = 1000;
			Assert.AreEqual ((t1 + 100).MSeconds, 2100); 
			Assert.AreEqual ((t1 - 100).MSeconds, 1900); 
			Assert.AreEqual ((t1 + t2).MSeconds, 3000); 
			Assert.AreEqual ((t1 - t2).MSeconds, 1000); 
			Assert.AreEqual ((t1 * 2).MSeconds, 4000);
			Assert.AreEqual ((t1 / 2).MSeconds, 1000);
		}
	}
}

