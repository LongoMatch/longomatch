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
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;

namespace Tests.Core.Store
{
	[TestFixture()]
	public class TestPoint
	{
		[Test()]
		public void TestSerialization ()
		{
			Point p = new Point (3, 4);
			
			Utils.CheckSerialization (p);
			Point newp = Utils.SerializeDeserialize (p);
			Assert.AreEqual (p.X, newp.X);
			Assert.AreEqual (p.Y, newp.Y);
		}
		
		[Test()]
		public void TestEqual ()
		{
			Point p1 = new Point (1, 2);
			Point p2 = new Point (1, 2);
			Assert.AreEqual (p1, p2);
		}
		
		[Test()]
		public void TestDistance ()
		{
			Point p1 = new Point (5, 5);
			Point p2 = new Point (0, 0);
			
			Assert.AreEqual (Math.Sqrt (50), p1.Distance (p2));
			
			p2 = new Point (5, 10);
			Assert.AreEqual (5, p1.Distance (p2));
			p1 = new Point (2, 10); 
			Assert.AreEqual (3, p1.Distance (p2));
		}
		
		[Test()]
		public void TestNormalize ()
		{
			Point p1 = new Point (3, 5);
			Point p2 = p1.Normalize (100, 100);
			Assert.AreEqual ((double)p1.X / 100, p2.X);
			Assert.AreEqual ((double)p1.Y / 100, p2.Y);
		}
		
		[Test()]
		public void TestDenormalize ()
		{
			Point p1 = new Point (0.2, 0.5);
			Point p2 = p1.Denormalize (100, 100);
			Assert.AreEqual ((double)p1.X * 100, p2.X);
			Assert.AreEqual ((double)p1.Y * 100, p2.Y);
		}

		[Test()]
		public void TestOperators ()
		{
			Point p1 = new Point (2, 4);
			Point p2 = new Point (4, 5);
			Point p3 = p1 + p2;
			Assert.AreEqual (p3.X, 6);
			Assert.AreEqual (p3.Y, 9);
			p3 = p1 - p2;
			Assert.AreEqual (p3.X, -2);
			Assert.AreEqual (p3.Y, -1);
		}
	}
}

