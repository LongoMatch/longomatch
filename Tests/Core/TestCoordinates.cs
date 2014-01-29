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
	public class TestCoordinates
	{
		[Test()]
		public void TestSerialization ()
		{
			Coordinates coords = new Coordinates();
			Point p1 = new Point (1, 2);
			Point p2 = new Point (3, 4);
			coords.Add(p1);
			coords.Add(p2);
			
			Utils.CheckSerialization(coords);
			Coordinates newcoords = Utils.SerializeDeserialize(coords);
			
			Assert.AreEqual (coords.Count, newcoords.Count);
			Assert.AreEqual (coords[0].X, newcoords[0].X);
			Assert.AreEqual (coords[1].X, newcoords[1].X);
			Assert.AreEqual (coords[0].Y, newcoords[0].Y);
			Assert.AreEqual (coords[1].Y, newcoords[1].Y);
		}
		
		[Test()]
		public void TestEqual ()
		{
			Coordinates coords = new Coordinates();
			coords.Add(new Point (1, 2));
			coords.Add(new Point (3, 4));
			
			Coordinates coords2 = new Coordinates();
			coords2.Add (new Point (1, 2));
			coords2.Add (new Point (3, 4));
			
			Assert.AreEqual (coords, coords2);
			
			/* Different number of elements */
			coords2.Add (new Point (1, 2));
			Assert.AreNotEqual (coords, coords2);
			
			/* Same number of elements but different points */
			coords2 = new Coordinates();
			coords2.Add (new Point (1, 1));
			coords2.Add (new Point (3, 4));
			Assert.AreNotEqual (coords, coords2);
		}
	}
}

