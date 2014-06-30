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
using System.Linq;
using Newtonsoft.Json;
using LongoMatch.Common;

namespace LongoMatch.Store.Drawables
{
	[Serializable]
	public class Cross: Drawable
	{
		public Cross ()
		{
		}
		
		public Cross (Point start, Point stop, LineStyle style)
		{
			Start = start;
			Stop = stop;
			Style = style;
		}
		
		public Point Start {
			set;
			get;
		}
		
		public Point Stop {
			set;
			get;
		}
		
		[JsonIgnore]
		public override Area Area {
			get {
				Point tl = new Point (Math.Min (Start.X, Stop.X),
				                      Math.Min (Start.Y, Stop.Y));
				return new Area (tl, Math.Abs (Start.X - Stop.X),
				                 Math.Abs (Start.Y - Stop.Y));
			}
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
		
		public override void Reorder ()
		{
			Point [] array = new Point[] {Start, Stop, StartI, StopI};
			
			array = array.OrderBy (p=> p.X).ThenBy (p=> p.Y).ToArray();
			Start = array[0];
			Stop = array[3];
		}
		
		public override Selection GetSelection (Point p, double pr=0.05) {
			double d;
			Selection sel;
			
			if (Selected) {
				return base.GetSelection (p, pr);
			}
			
			if (MatchPoint (Start, p, pr, out d)) {
				return new Selection (this, SelectionPosition.TopLeft, d);
			} else if (MatchPoint (StopI, p, pr, out d)) {
				return new Selection (this, SelectionPosition.BottomRight, d);
			} else if (MatchPoint (StartI, p, pr, out d)) {
				return new Selection (this, SelectionPosition.TopRight, d);
			} else if (MatchPoint (StopI, p, pr, out d)) {
				return new Selection (this, SelectionPosition.BottomLeft, d);
			} else {
				Line aline = new Line {Start = Start, Stop = Stop};
				sel = aline.GetSelection (p, pr);
				if (sel == null) {
					Line bline = new Line {Start = StartI, Stop = StopI};
					sel = bline.GetSelection (p, pr);
				}
				if (sel != null) {
					sel.Drawable = this;
				}
				return sel;
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
			case SelectionPosition.TopRight:
				Start.Y = p.Y;
				Stop.X = p.X;
				break;
			case SelectionPosition.BottomLeft:
				Start.X = p.X;
				Stop.Y = p.Y;
				break;
			case SelectionPosition.Top:
				Start.Y = p.Y;
				break;
			case SelectionPosition.Bottom:
				Stop.Y = p.Y;
				break;
			case SelectionPosition.Left:
				Start.X = p.X;
				break;
			case SelectionPosition.Right:
				Stop.X = p.X;
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

