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

using LongoMatch.Common;
using LongoMatch.Interfaces.Drawing;

namespace LongoMatch.Store.Drawables
{
	[Serializable]
	public abstract class Drawable: IBlackboardObject
	{
		public Drawable ()
		{
		}
		
		public Color StrokeColor {
			get;
			set;
		}
		
		public int LineWidth {
			get;
			set;
		}
		
		public Color FillColor {
			get;
			set;
		}
		
		public bool Selected {
			get;
			set;
		}
		
		public LineStyle Style{
			get;
			set;
		}
		
		public virtual Area Area {
			get;
			protected set;
		}
		
		public virtual void Reorder () {
		}
		
		public virtual Selection GetSelection (Point point, double pr) {
			Point[] vertices;
			double d;
			
			if (Area == null) {
				return null;
			}

			if ((point.X < Area.Start.X - pr) ||
			    (point.X > Area.Start.X + Area.Width + pr) ||
			    (point.Y < Area.Start.Y - pr) ||
			    (point.Y > Area.Start.Y + Area.Height + pr)) {
				return null;
			} 
			
			/* Check vertices */
			vertices = Area.Vertices;
			if (MatchPoint (vertices[0], point, pr, out d)) {
				return new Selection (this, SelectionPosition.TopLeft, d);
			} else if (MatchPoint (vertices[1], point, pr, out d)) {
				return new Selection (this, SelectionPosition.TopRight, d);
			} else if (MatchPoint (vertices[2], point, pr, out d)) {
				return new Selection (this, SelectionPosition.BottomRight, d);
			} else if (MatchPoint (vertices[3], point, pr, out d)) {
				return new Selection (this, SelectionPosition.BottomLeft, d);
			}
			
			vertices = Area.VerticesCenter;
			if (MatchPoint (vertices[0], point, pr, out d)) {
				return new Selection (this, SelectionPosition.Top, d);
			} else if (MatchPoint (vertices[1], point, pr, out d)) {
				return new Selection (this, SelectionPosition.Right, d);
			} else if (MatchPoint (vertices[2], point, pr, out d)) {
				return new Selection (this, SelectionPosition.Bottom, d);
			} else if (MatchPoint (vertices[3], point, pr, out d)) {
				return new Selection (this, SelectionPosition.Left, d);
			}
			
			return new Selection (this, SelectionPosition.All, point.Distance (Area.Center));
		}

		public abstract void Move (Selection s, Point dst, Point start);
		
		public  void Move (SelectionPosition s, Point dst, Point start) {
			Move (new Selection (null, s, 0), dst, start);
		}

		public static bool MatchPoint (Point p1, Point p2, double precision, out double accuracy) {
			accuracy = p1.Distance (p2);
			return accuracy <= precision;
		}
		
		public static bool MatchAxis (double c1, double c2, double precision, out double accuracy) {
				accuracy = Math.Abs (c1 - c2);
				return accuracy <= precision;
		}
	}
}

