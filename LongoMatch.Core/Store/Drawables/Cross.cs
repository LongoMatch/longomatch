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
using Newtonsoft.Json;
using LongoMatch.Common;

namespace LongoMatch.Store.Drawables
{
	public class Cross: Line
	{
		public Cross ()
		{
		}
		
		public Cross (Point start, Point stop, LineType type, LineStyle style):
			base (start, stop, type, style)
		{
		}
		
		[JsonIgnore]
		public Point StartI {
			get {
				return new Point (Stop.X, Start.Y);
			}
		}
		
		[JsonIgnore]
		public Point StopI {
			get {
				return new Point (Start.X, Stop.Y);
			}
		}
		
		public override Selection GetSelection (Point p, double pr=0.05) {
			double d;
			
			if (MatchPoint (Start, p, pr, out d)) {
				return new Selection (this, SelectionPosition.TopLeft, d);
			} else if (MatchPoint (StartI, p, pr, out d)) {
				return new Selection (this, SelectionPosition.TopRight, d);
			} else if (MatchPoint (Stop, p, pr, out d)) {
				return new Selection (this, SelectionPosition.BottomRight, d);
			} else if (MatchPoint (StopI, p, pr, out d)) {
				return new Selection (this, SelectionPosition.BottomLeft, d);
			} else {
				double slope = (Start.Y - Stop.Y) / (Start.X - Stop.Y);
				double yi = Start.Y / (slope * Start.X);
				d = Math.Abs (p.Y / (slope * p.X) - yi);
				if (d < pr) {
					return new Selection (this, SelectionPosition.All, d);
				} else {
					return new Selection (this, SelectionPosition.None, d);
				}
			}
		}
		
		public override void Move (Selection sel, Point p, Point moveStart) {
			switch (sel.Position) {
			case SelectionPosition.TopLeft:
				Start = p;
				break;
			case SelectionPosition.BottomRight:
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

