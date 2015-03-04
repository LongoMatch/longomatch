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
	public class TestLine
	{
		[Test()]
		public void TestSerialization ()
		{
			Point p1, p2;
			
			p1 = new Point (0, 0);
			p2 = new Point (5, 5);
			Line line = new Line (p1, p2, LineType.Arrow, LineStyle.Dashed);
				
			Utils.CheckSerialization (line);
			
			Line newLine = Utils.SerializeDeserialize (line);
			Assert.AreEqual (line.Start, newLine.Start);
			Assert.AreEqual (line.Stop, newLine.Stop);
		}
		
		[Test()]
		public void TestCenter ()
		{
			Point p1, p2;

			p1 = new Point (0, 0);
			p2 = new Point (5, 5);
			Line line = new Line (p1, p2, LineType.Arrow, LineStyle.Dashed);
			
			Assert.AreEqual (new Point (2.5, 2.5), line.Center);
		}
		
		[Test()]
		public void TestGetSelection ()
		{
			Point p1, p2, p3;
			Selection s;

			p1 = new Point (0, 5);
			p2 = new Point (5, 5);
			Line line = new Line (p1, p2, LineType.Arrow, LineStyle.Dashed);
			
			/* None */
			p3 = new Point (10, 5);
			s = line.GetSelection (p3, 1);
			Assert.IsNull (s);
			
			/* Start */
			p3 = new Point (0, 5);
			s = line.GetSelection (p3, 1);
			Assert.AreEqual (SelectionPosition.LineStart, s.Position);
			Assert.AreEqual ((double)0, s.Accuracy);
			p3 = new Point (1, 5);
			s = line.GetSelection (p3, 1);
			Assert.AreEqual (SelectionPosition.LineStart, s.Position);
			Assert.AreEqual ((double)1, s.Accuracy);
			
			/* Stop */
			p3 = new Point (5, 5);
			s = line.GetSelection (p3, 1);
			Assert.AreEqual (SelectionPosition.LineStop, s.Position);
			Assert.AreEqual ((double)0, s.Accuracy);
			p3 = new Point (6, 5);
			s = line.GetSelection (p3, 1);
			Assert.AreEqual (SelectionPosition.LineStop, s.Position);
			Assert.AreEqual ((double)1, s.Accuracy);
			
			/* All */
			p3 = new Point (2, 5);
			s = line.GetSelection (p3, 1);
			Assert.AreEqual (SelectionPosition.All, s.Position);
			Assert.AreEqual ((double)0, s.Accuracy);
			p3 = new Point (2, 5.1);
			s = line.GetSelection (p3, 1);
			Assert.AreEqual (SelectionPosition.All, s.Position);
			Assert.AreEqual (0.1, Math.Round (s.Accuracy, 3));
			
			
			p1 = new Point (0, 0);
			p2 = new Point (5, 5);
			line = new Line (p1, p2, LineType.Arrow, LineStyle.Dashed);

			p3 = new Point (0, 3);
			s = line.GetSelection (p3, 1);
			Assert.IsNull (s);
			
			p3 = new Point (2.5, 2.5);
			s = line.GetSelection (p3, 1);
			Assert.AreEqual (SelectionPosition.All, s.Position);
			Assert.AreEqual ((double)0, s.Accuracy);
		}
		
		[Test()]
		public void TestMove ()
		{
			Point p1, p2, p3, p4;

			p1 = new Point (5, 5);
			p2 = new Point (7, 10);
			Line line = new Line (p1, p2, LineType.Arrow, LineStyle.Dashed);
			p3 = new Point (8, 20);
			line.Move (SelectionPosition.LineStart, p3, null);
			Assert.AreEqual (line.Start, p3);
			p3 = new Point (10, 10);
			line.Move (SelectionPosition.LineStop, p3, null);
			Assert.AreEqual (line.Stop, p3);
			
			line.Start = new Point (5, 5);
			line.Stop = new Point (7, 10);
			p3 = new Point (10, 10);
			p4 = new Point (2, 5);
			line.Move (SelectionPosition.All, p3, p4);
			Assert.AreEqual (line.Start.X, 5 + 8);
			Assert.AreEqual (line.Start.Y, 5 + 5);
			Assert.AreEqual (line.Stop.X, 7 + 8);
			Assert.AreEqual (line.Stop.Y, 10 + 5);
		}
	}
}

