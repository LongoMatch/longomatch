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
using LongoMatch.Store.Drawables;
using LongoMatch.Common;

namespace Tests.Core.Drawables
{
	[TestFixture()]
	public class TestRectangle
	{
		[Test()]
		public void TestSerialization ()
		{
			Rectangle r, newr;
			
			r = new Rectangle (new Point (2, 10), 30, 20);
			Utils.CheckSerialization (r);
			newr = Utils.SerializeDeserialize (r);
			Assert.AreEqual (r.TopLeft, newr.TopLeft);
			Assert.AreEqual (r.TopRight, newr.TopRight);
			Assert.AreEqual (r.BottomLeft, newr.BottomLeft);
			Assert.AreEqual (r.BottomRight, newr.BottomRight);
		}
		
		[Test()]
		public void TestAxis ()
		{
			Rectangle r;
			Point p1, p2;
			double width=30, height=20;
			
			p1 = new Point (2, 10);
			r = new Rectangle (p1, width, height);
			Assert.AreEqual (height, r.Height);
			Assert.AreEqual (width, r.Width);
			p2 = new Point (2, 10);
			Assert.AreEqual (p2, r.TopLeft);
			p2.X += width; 
			Assert.AreEqual (p2, r.TopRight);
			p2.Y += height; 
			Assert.AreEqual (p2, r.BottomRight);
			p2.X = 2;
			Assert.AreEqual (p2, r.BottomLeft);
			p2.X = r.TopLeft.X + width / 2;
			p2.Y = r.TopLeft.Y + height / 2;
			Assert.AreEqual (p2, r.Center);
		}
		
		[Test()]
		public void TestGetSelection ()
		{
			Rectangle r;
			Point p1, p2;
			Selection s;
			double width=30, height=20;
			
			p1 = new Point (2, 10);
			r = new Rectangle (p1, width, height);
			p2 = new Point (2, 15);
			s = r.GetSelection (p2, 0);
			Assert.AreEqual (SelectionPosition.Left, s.Position);
			p2 = new Point (15, 30);
			s = r.GetSelection (p2, 0);
			Assert.AreEqual (SelectionPosition.Bottom, s.Position);
			p2 = new Point (32, 12);
			s = r.GetSelection (p2, 0);
			Assert.AreEqual (SelectionPosition.Right, s.Position);
			p2 = new Point (30, 10);
			s = r.GetSelection (p2, 0);
			Assert.AreEqual (SelectionPosition.Top, s.Position);
		}
		
		[Test()]
		public void TestMove ()
		{
			Rectangle r1;
			Point p1, p2;
			double width=10, height=10;
			
			/* Move corners */
			p1 = new Point (0, 0);
			r1 = new Rectangle (p1, width, height);
			
			p2 = new Point (1, 1);
			r1.Move (SelectionPosition.TopLeft, p2, null);
			CompareRect (r1, p2, 9, 9);
			
			p2 = new Point (9, 8);
			r1.Move (SelectionPosition.BottomRight, p2, null);
			CompareRect (r1, new Point (1, 1), 8, 7);
			
			p2 = new Point (11, 2);
			r1.Move (SelectionPosition.TopRight, p2, null);
			CompareRect (r1, new Point (1, 2), 10, 6);
			
			p2 = new Point (2, 9);
			r1.Move (SelectionPosition.BottomLeft, p2, null);
			CompareRect (r1, new Point (2, 2), 9, 7);
			
			/* Move borders */
			p1 = new Point (0, 0);
			r1 = new Rectangle (p1, width, height);
			
			p2 = new Point (3, 5);
			r1.Move (SelectionPosition.Left, p2, null);
			CompareRect (r1, new Point (3, 0), 7, 10);
			
			p2 = new Point (5, 11);
			r1.Move (SelectionPosition.Bottom, p2, null);
			CompareRect (r1, new Point (3, 0), 7, 11);
			
			p2 = new Point (6, 2);
			r1.Move (SelectionPosition.Top, p2, null);
			CompareRect (r1, new Point (3, 2), 7, 9);
			
			p2 = new Point (8, 11);
			r1.Move (SelectionPosition.Right, p2, null);
			CompareRect (r1, new Point (3, 2), 5, 9);
		}
		
		public void CompareRect (Rectangle r, Point p, double width, double height)
		{
			Rectangle r2 = new Rectangle (p, width, height);
			Assert.AreEqual (r2.TopLeft, r.TopLeft);
			Assert.AreEqual (r2.TopRight, r.TopRight);
			Assert.AreEqual (r2.BottomLeft, r.BottomLeft);
			Assert.AreEqual (r2.BottomRight, r.BottomRight);
			
		}
		
	}
}