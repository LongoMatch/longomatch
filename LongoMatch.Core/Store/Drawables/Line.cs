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
using Newtonsoft.Json;

namespace LongoMatch.Store.Drawables
{
	public class Line: Drawable
	{
		public Line ()
		{
		}

		public Line (Point start, Point stop, LineType type, LineStyle style)
		{
			Start = start;
			Stop = stop;
			Type = type;
			Style = style;
		}
		
		public Point Start {
			get;
			set;
		}
		
		public Point Stop {
			get;
			set;
		}
		
		public LineType Type{
			get;
			set;
		}
		
		[JsonIgnore]
		public Point Center {
			get {
				return new Point (Start.X + (Stop.X - Start.X) / 2,
				                  Start.Y + (Stop.Y -Start.Y) / 2);
			}
		}
		
		public override Selection GetSelection (Point p, double pr=0.05) {
			double d;
		
			if (MatchPoint (Start, p, pr, out d)) {
				return new Selection (this, SelectionPosition.LineStart, d);
			} else if (MatchPoint (Stop, p, pr, out d)) {
				return new Selection (this, SelectionPosition.LineStop, d);
			} else {
				double minx, maxx, miny, maxy;
				
				minx = Math.Min (Start.X, Stop.X) - pr;
				maxx = Math.Max (Start.X, Stop.X) + pr;
				miny = Math.Min (Start.Y, Stop.Y) - pr;
				maxy = Math.Max (Start.Y, Stop.Y) + pr;
				if (p.X < minx || p.X > maxx || p.Y < miny || p.Y > maxy) {
					return null;
				}
				if (Start.X == Stop.X) {
					d = p.Distance (new Point (Start.X, p.Y));
				} else if (Start.Y == Stop.Y) {
					d = p.Distance (new Point (p.X, Start.Y));
				} else {
					double yi, slope;
					
					slope = (Start.Y - Stop.Y) / (Start.X - Stop.X);
					yi = Start.Y - (slope * Start.X);
					d = Math.Abs ((slope * p.X) + yi - p.Y);
				}
				
				if (d  < pr) {
					return new Selection (this, SelectionPosition.All, d);
				} else {
					return null;
				}
			}
		}
		
		public override void Move (Selection sel, Point p, Point moveStart) {
						switch (sel.Position) {
			case SelectionPosition.LineStart:
				Start = p;
				break;
			case SelectionPosition.LineStop:
				Stop = p;
				break;
			case SelectionPosition.All:
				Start.X += p.X - moveStart.X;
				Start.Y += p.Y - moveStart.Y;
				Stop.X += p.X - moveStart.X;
				Stop.Y += p.Y - moveStart.Y;
				break;
			default:
				throw new Exception ("Unsupported move for line:  " + sel.Position);
			}
		}
	}
}

