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
	public class DrawableImp: Drawable
	{
		public override Selection GetSelection (Point point, double precision)
		{
			throw new System.NotImplementedException ();
		}
		
		public override void Move (Selection sel, Point p, Point start)
		{
			throw new System.NotImplementedException ();
		}
	}
	
	[TestFixture()]
	public class TestDrawable
	{
		[Test()]
		public void TestSerialization ()
		{
			Color c1, c2;
			c1 = new Color (1, 2, 3, 2);
			c2 = new Color (1, 5, 6, 3);
			DrawableImp d = new DrawableImp {StrokeColor=c1, LineWidth=30, FillColor=c2};
			
			Utils.CheckSerialization (d);
			Drawable newD = Utils.SerializeDeserialize (d);
			
			Assert.AreEqual (d.FillColor, newD.FillColor);
			Assert.AreEqual (d.LineWidth, newD.LineWidth);
			Assert.AreEqual (d.StrokeColor, newD.StrokeColor);
		}
	}
}

