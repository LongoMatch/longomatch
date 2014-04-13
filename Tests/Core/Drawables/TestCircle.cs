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
using LongoMatch.Store.Drawables;

namespace Tests.Core.Drawables
{
	[TestFixture()]
	public class TestCircle
	{
		[Test()]
		public void TestSerialization ()
		{
			Point p = new Point (10, 10);
			Circle c = new Circle (p, 5);
			Utils.CheckSerialization (c);
			Circle nc = Utils.SerializeDeserialize (c);
			Assert.AreEqual (nc.Center, c.Center);
			Assert.AreEqual (nc.Radius, c.Radius);
		}
		
		[Test()]
		public void TestMove ()
		{
			Point p = new Point (10, 10);
			Circle c = new Circle (p, 5);
			Point p2 = new Point (12, 10);
			c.Move (SelectionPosition.All, p2, p);
			Assert.AreEqual (p2, c.Center);
			
			p2 = new Point (12, 16);
			c.Move (SelectionPosition.CircleBorder, p2, p);
			Assert.AreEqual (6, c.Radius);
			
			p2 = new Point (14, 10);
			c.Move (SelectionPosition.CircleBorder, p2, p);
			Assert.AreEqual (2, c.Radius);
		}
		
		[Test()]
		public void TestSelection ()
		{
			Selection s;
			Point p1, p2;
			Circle c;
			
			p1 = new Point (10, 10);
			c = new Circle (p1, 5);
			p2 = new Point (16, 10);
			s = c.GetSelection (p2, 0.9);
			Assert.AreEqual (SelectionPosition.None, s.Position);
			p2 = new Point (15.9, 10);
			s = c.GetSelection (p2, 1);
			Assert.AreEqual (SelectionPosition.CircleBorder, s.Position);
			p2 = new Point (14.1, 10);
			s = c.GetSelection (p2, 1);
			Assert.AreEqual (SelectionPosition.CircleBorder, s.Position);
			p2 = new Point (13, 10);
			s = c.GetSelection (p2, 1);
			Assert.AreEqual (SelectionPosition.All, s.Position);
		}
	}
}

