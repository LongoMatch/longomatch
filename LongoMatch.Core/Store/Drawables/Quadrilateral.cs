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
using LongoMatch.Core.Common;

namespace LongoMatch.Core.Store.Drawables
{
	[Serializable]
	public class Quadrilateral: Drawable
	{
		public Quadrilateral ()
		{
		}
		
		public Quadrilateral (Point tl, Point tr, Point bl, Point br)
		{
			TopLeft = tl;
			TopRight = tr;
			BottomLeft = bl;
			BottomRight = br;
		}
		
		public Point TopLeft {
			get;
			set;
		}
		
		public Point TopRight {
			get;
			set;
		}
		
		public Point BottomLeft  {
			get;
			set;
		}
		
		public Point BottomRight  {
			get;
			set;
		}
		
		public override Area Area {
			get {
				double xmin, xmax, ymin, ymax;
			
				/* Create a rectangle that wraps the quadrilateral */
				xmin = Math.Min (BottomLeft.X, TopLeft.X);
				xmax = Math.Max (BottomRight.X, TopRight.X);
				ymin = Math.Min (TopLeft.Y, TopRight.Y);
				ymax = Math.Max (BottomLeft.Y, BottomRight.Y);
				return new Area (new Point (xmin, ymin), xmax - xmin, ymax - ymin);
			}
		}

		public override Selection GetSelection (Point p, double pr, bool inMotion=false) {
			double xmin, xmax, ymin, ymax;
			double d;
			
			/* Create a rectangle that wraps the quadrilateral */
			xmin = Math.Min (BottomLeft.X, TopLeft.X) - pr;
			xmax = Math.Max (BottomRight.X, TopRight.X) + pr;
			ymin = Math.Min (TopLeft.Y, TopRight.Y) - pr;
			ymax = Math.Max (BottomLeft.Y, BottomRight.Y) + pr;
			
			if ((p.X > xmax || p.X < xmin || p.Y < ymin || p.Y > ymax)) {
				return null;
			} else if (MatchPoint (TopLeft, p, pr, out d)) {
				return new Selection (this, SelectionPosition.TopLeft, d);
			} else if (MatchPoint (TopRight, p, pr, out d)) {
				return new Selection (this, SelectionPosition.TopRight, d);
			} else if (MatchPoint (BottomLeft, p, pr, out d)) {
				return new Selection (this, SelectionPosition.BottomLeft, d);
			} else if (MatchPoint (BottomRight, p, pr, out d)) {
				return new Selection (this, SelectionPosition.BottomRight, d);
			} else {
				Point center = new Point ((TopRight.X - TopLeft.X) / 2,
				                          (BottomLeft.Y - TopLeft.Y) / 2);
				return new Selection (this, SelectionPosition.All, center.Distance (p));
			}
		}
		
		public override void Move (Selection sel,  Point p, Point moveStart) {
			switch (sel.Position) {
			case SelectionPosition.TopLeft:
				TopLeft = p;
				break;
			case SelectionPosition.TopRight:
				TopRight = p;
				break;
			case SelectionPosition.BottomLeft:
				BottomLeft = p;
				break;
			case SelectionPosition.BottomRight:
				BottomRight = p;
				break;
			case SelectionPosition.All: {
				double xdiff, ydiff;
				
				xdiff = p.X - moveStart.X;
				ydiff = p.Y - moveStart.Y;
				TopLeft.X += xdiff;
				TopLeft.Y += ydiff;
				TopRight.X += xdiff;
				TopRight.Y += ydiff;
				BottomRight.X += xdiff;
				BottomRight.Y += ydiff;
				BottomLeft.X += xdiff;
				BottomLeft.Y += ydiff;
				break;
			}
			default:
				throw new Exception ("Unsupported move for quadrilateral:  " + sel.Position);
			}
		}
	}
}

