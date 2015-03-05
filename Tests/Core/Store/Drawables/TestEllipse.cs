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

namespace Tests.Core.Store.Drawables
{
	[TestFixture()]
	public class TestEllipse
	{
		[Test()]
		public void TestSerialization ()
		{
			Point c = new Point (10, 10);
			Ellipse e = new Ellipse (c, 3, 2);
			Utils.CheckSerialization (e);
			Ellipse ne = Utils.SerializeDeserialize (e);
			Assert.AreEqual (ne.Center, e.Center);
			Assert.AreEqual (ne.AxisX, e.AxisX);
			Assert.AreEqual (ne.AxisY, e.AxisY);
		}
		
		[Test()]
		public void TestAxis ()
		{
			Point c = new Point (10, 10);
			Ellipse e = new Ellipse (c, 3, 2);
			Assert.AreEqual (new Point (13, 10), e.Right);
			Assert.AreEqual (new Point (7, 10), e.Left);
			Assert.AreEqual (new Point (10, 12), e.Top);
			Assert.AreEqual (new Point (10, 8), e.Bottom);
		}
		
		[Test()]
		public void TestMove ()
		{
			Point c = new Point (10, 10);
			Ellipse e = new Ellipse (c, 3, 2);

			/* Move axis */
			Point p = new Point (8, 9);
			e.Move (SelectionPosition.Left, p, null);
			Assert.AreEqual (10, e.Center.X);
			Assert.AreEqual (10, e.Center.Y);
			Assert.AreEqual (2, e.AxisX);
			Assert.AreEqual (2, e.AxisY);
			
			p = new Point (15, 5);
			e.Move (SelectionPosition.Left, p, null);
			Assert.AreEqual (10, e.Center.X);
			Assert.AreEqual (10, e.Center.Y);
			Assert.AreEqual (5, e.AxisX);
			Assert.AreEqual (2, e.AxisY);
			
			p = new Point (15, 5);
			e.Move (SelectionPosition.Bottom, p, null);
			Assert.AreEqual (10, e.Center.X);
			Assert.AreEqual (10, e.Center.Y);
			Assert.AreEqual (5, e.AxisX);
			Assert.AreEqual (5, e.AxisY);
			
			p = new Point (15, 12);
			e.Move (SelectionPosition.Top, p, null);
			Assert.AreEqual (10, e.Center.X);
			Assert.AreEqual (10, e.Center.Y);
			Assert.AreEqual (5, e.AxisX);
			Assert.AreEqual (2, e.AxisY);
			
			/* Move all */
			p = new Point (15, 12);
			e.Move (SelectionPosition.All, p, new Point (10, 10));
			Assert.AreEqual (15, e.Center.X);
			Assert.AreEqual (12, e.Center.Y);
			Assert.AreEqual (5, e.AxisX);
			Assert.AreEqual (2, e.AxisY);
		}
	}
}

