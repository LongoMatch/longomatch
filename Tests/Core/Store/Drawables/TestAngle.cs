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
using LongoMatch.Core.Store.Drawables;

namespace Tests.Core.Drawables
{
	[TestFixture()]
	public class TestAngle
	{
		[Test()]
		public void TestSerialization ()
		{
			Point start, center, stop;
			
			start = new Point (0, 10);
			stop = new Point (10, 0);
			center = new Point (0, 0);
			Angle a = new Angle (start, center, stop);
			Utils.CheckSerialization (a);
			Angle na = Utils.SerializeDeserialize (a);
			Assert.AreEqual (na.Center, a.Center);
			Assert.AreEqual (na.Start, a.Start);
			Assert.AreEqual (na.Stop, a.Stop);
		}
		
		[Test()]
		public void TestSelection ()
		{
			Point start, center, stop, p;
			Selection s;
			
			start = new Point (0, 10);
			stop = new Point (10, 0);
			center = new Point (0, 0);
			Angle a = new Angle (start, center, stop);
			
			p = new Point (0.1, 0.3);
			s = a.GetSelection (p, 0.5);
			Assert.AreEqual (SelectionPosition.AngleCenter, s.Position);
			Assert.AreEqual (p.Distance (center), s.Accuracy);
			
			p = new Point (9.8, 0.3);
			s = a.GetSelection (p, 0.5);
			Assert.AreEqual (SelectionPosition.AngleStop, s.Position);
			Assert.AreEqual (p.Distance (stop), s.Accuracy);
			
			p = new Point (0.2, 9.9);
			s = a.GetSelection (p, 0.5);
			Assert.AreEqual (SelectionPosition.AngleStart, s.Position);
			Assert.AreEqual (p.Distance (start), s.Accuracy);
			
			p = new Point (5, 5);
			s = a.GetSelection (p, 0.5);
			Assert.IsNull (s);
		}
		
		[Test()]
		public void TestMove ()
		{
			Point start, center, stop, p;
			
			start = new Point (0, 10);
			stop = new Point (10, 0);
			center = new Point (0, 0);
			Angle a = new Angle (start, center, stop);
			p = new Point (0, 9);
			a.Move (SelectionPosition.AngleStart, p, null);
			Assert.AreEqual (p, a.Start);
			p = new Point (11, 2);
			a.Move (SelectionPosition.AngleStop, p, null);
			Assert.AreEqual (p, a.Stop);
			p = new Point (2, 2);
			a.Move (SelectionPosition.AngleCenter, p, null);
			Assert.AreEqual (p, a.Center);
		}
	}
}

