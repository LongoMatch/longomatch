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
using NUnit.Framework;
using LongoMatch.Common;
using LongoMatch.Store.Drawables;

namespace Tests.Core.Drawables
{
	[TestFixture()]
	public class TestMultipoints
	{
		[Test()]
		public void TestSerialization ()
		{
			Point p1, p2, p3;
			List<Point> l;
			
			p1 = new Point (2, 2);
			p2 = new Point (3, 5);
			p3 = new Point (6, 8);
			l = new List<Point> ();
			l.Add (p1);
			l.Add (p2);
			l.Add (p3);
			
			MultiPoints m = new MultiPoints (l);
			Utils.CheckSerialization (m);
			MultiPoints nm = Utils.SerializeDeserialize(m);
			Assert.AreEqual (p1, nm.Points[0]);
			Assert.AreEqual (p2, nm.Points[1]);
			Assert.AreEqual (p3, nm.Points[2]);
		}
		
		[Test()]
		public void TestAxis ()
		{
			double xmin, xmax, ymin, ymax;
			Point p1, p2, p3;
			List<Point> l;
			
			p1 = new Point (2, 2);
			p2 = new Point (3, 5);
			p3 = new Point (6, 8);
			l = new List<Point> ();
			l.Add (p1);
			l.Add (p2);
			l.Add (p3);
			xmin = 2;
			xmax = 6;
			ymin = 2;
			ymax = 8;
			
			MultiPoints m = new MultiPoints (l);
			Assert.AreEqual (new Point (xmin, ymax), m.TopLeft);
			Assert.AreEqual (new Point (xmax, ymax), m.TopRight);
			Assert.AreEqual (new Point (xmax, ymin), m.BottomRight);
			Assert.AreEqual (new Point (xmin, ymin), m.BottomLeft);
		}
		
		[Test()]
		public void TestSelection ()
		{
			Point p1, p2, p3;
			List<Point> l;
			Selection s;
			
			p1 = new Point (2, 2);
			p2 = new Point (3, 5);
			p3 = new Point (6, 8);
			l = new List<Point> ();
			l.Add (p1);
			l.Add (p2);
			l.Add (p3);
			
			MultiPoints m = new MultiPoints (l);
			s = m.GetSelection (new Point (3, 5), 1);
			Assert.AreEqual (SelectionPosition.All, s.Position);
			
			s = m.GetSelection (new Point (0, 5), 1);
			Assert.IsNull (s);
			s = m.GetSelection (new Point (5, 12), 1);
			Assert.IsNull (s);
		}
	}
}

