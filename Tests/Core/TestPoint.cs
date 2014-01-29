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
using LongoMatch.Store;
using LongoMatch.Common;

namespace Tests.Core
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

	}
}

