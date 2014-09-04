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
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Core.Common;

namespace Tests.Core.Drawables
{
	[TestFixture()]
	public class TestQuadrilateral
	{
		[Test()]
		public void TestSerialization ()
		{
			Point tl, tr, br, bl;
			Quadrilateral q, nq;
			
			bl = new Point (1, 1);
			br = new Point (10, 4);
			tl = new Point (3, 20);
			tr = new Point (12, 15);
			q = new Quadrilateral (tl, tr, bl, br);
			Utils.CheckSerialization (q);
			nq = Utils.SerializeDeserialize (q);
			Assert.AreEqual (q.BottomLeft, nq.BottomLeft);
			Assert.AreEqual (q.BottomRight, nq.BottomRight);
			Assert.AreEqual (q.TopLeft, nq.TopLeft);
			Assert.AreEqual (q.TopRight, nq.TopRight);
		}
		
		[Test()]
		public void TestGetSelection ()
		{
			Point tl, tr, br, bl;
			Quadrilateral q;
			Selection s;
			
			tl = new Point (1, 1);
			tr = new Point (10, 4);
			bl = new Point (3, 20);
			br = new Point (12, 15);
			q = new Quadrilateral (tl, tr, bl, br);
			
			s = q.GetSelection (bl, 1);
			Assert.AreEqual (SelectionPosition.BottomLeft, s.Position);
			s = q.GetSelection (br, 1);
			Assert.AreEqual (SelectionPosition.BottomRight, s.Position);
			s = q.GetSelection (tl, 1);
			Assert.AreEqual (SelectionPosition.TopLeft, s.Position);
			s = q.GetSelection (tr, 1);
			Assert.AreEqual (SelectionPosition.TopRight, s.Position);
			
			/* rectangle
			 *  1,20   12,20
			 *  1,1    12, 1
			 */ 
			s = q.GetSelection (new Point (0, 1), 0.5);
			Assert.IsNull (s);
			s = q.GetSelection (new Point (1, 21), 0.5);
			Assert.IsNull (s);
			s = q.GetSelection (new Point (13, 5), 0.5);
			Assert.IsNull (s);
			s = q.GetSelection (new Point (4, 0), 0.5);
			Assert.IsNull (s);
			
			s = q.GetSelection (new Point (4, 5), 0);
			Assert.AreEqual (SelectionPosition.All, s.Position);
		}
		
		[Test()]
		public void TestMove ()
		{
			Point tl, tr, br, bl, src, dst;
			Quadrilateral q;
			
			bl = new Point (1, 1);
			br = new Point (10, 4);
			tl = new Point (3, 20);
			tr = new Point (12, 15);
			q = new Quadrilateral (tl, tr, bl, br);
			
			src = new Point (4, 5);
			dst = new Point (7, 3);
			q.Move (SelectionPosition.All, dst, src);
			Assert.AreEqual (1 + 3, bl.X);
			Assert.AreEqual (10 + 3, br.X);
			Assert.AreEqual (3 + 3, tl.X);
			Assert.AreEqual (12 + 3, tr.X);
			
			Assert.AreEqual (1 - 2, bl.Y);
			Assert.AreEqual (4 - 2, br.Y);
			Assert.AreEqual (20 - 2, tl.Y);
			Assert.AreEqual (15 - 2, tr.Y);
			
			bl = new Point (1, 1);
			q.Move (SelectionPosition.BottomLeft, bl, null);
			Assert.AreEqual (bl, q.BottomLeft);
			br = new Point (10, 4);
			q.Move (SelectionPosition.BottomRight, br, null);
			Assert.AreEqual (br, q.BottomRight);
			tl = new Point (3, 20);
			q.Move (SelectionPosition.TopLeft, tl, null);
			Assert.AreEqual (tl, q.TopLeft);
			tr = new Point (12, 15);
			q.Move (SelectionPosition.TopRight, tr, null);
			Assert.AreEqual (tr, q.TopRight);
		}
	}
}

