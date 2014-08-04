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
using LongoMatch.Store;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Interfaces;
using LongoMatch.Common;
using LongoMatch.Store.Drawables;

namespace LongoMatch.Drawing.CanvasObject
{
	public class TimeNodeObject: CanvasObject, ICanvasSelectableObject
	{
		const int MAX_TIME_SPAN = 1000;

		public TimeNodeObject (TimeNode node)
		{
			TimeNode = node;
			SelectWhole = true;
		}

		public TimeNode TimeNode {
			get;
			set;
		}

		public bool SelectWhole {
			get;
			set;
		}

		public Time MaxTime {
			set;
			protected get;
		}

		public double OffsetY {
			get;
			set;
		}

		public double SecondsPerPixel {
			set;
			protected get;
		}

		protected double StartX {
			get {
				return Utils.TimeToPos (TimeNode.Start, SecondsPerPixel);
			}
		}

		protected double StopX {
			get {
				return Utils.TimeToPos (TimeNode.Stop, SecondsPerPixel);
			}
		}

		protected double CenterX {
			get {
				return Utils.TimeToPos (TimeNode.Start + TimeNode.Duration / 2,
				                        SecondsPerPixel);
			}
		}

		public Selection GetSelection (Point point, double precision)
		{
			double accuracy;
			if (point.Y >= OffsetY && point.Y < OffsetY + Constants.CATEGORY_HEIGHT) {
				if (Drawable.MatchAxis (point.X, StartX, precision, out accuracy)) {
					return new Selection (this, SelectionPosition.Left, accuracy);
				} else if (Drawable.MatchAxis (point.X, StopX, precision, out accuracy)) {
					return new Selection (this, SelectionPosition.Right, accuracy);
				} else if (SelectWhole && point.X > StartX && point.X < StopX) {
					return new Selection (this, SelectionPosition.All,
					                      Math.Abs (CenterX - point.X));
				}
			}
			return null;
		}

		public void Move (Selection sel, Point p, Point start)
		{
			Time newTime = Utils.PosToTime (p, SecondsPerPixel);

			if (p.X < 0) {
				p.X = 0;
			} else if (newTime > MaxTime) {
				p.X = Utils.TimeToPos (MaxTime, SecondsPerPixel);
			}
			newTime = Utils.PosToTime (p, SecondsPerPixel);

			switch (sel.Position) {
			case SelectionPosition.Left:
				{
					if (newTime.MSeconds + MAX_TIME_SPAN > TimeNode.Stop.MSeconds) {
						TimeNode.Start.MSeconds = TimeNode.Stop.MSeconds - MAX_TIME_SPAN;
					} else {
						TimeNode.Start = newTime;
					}
					break;
				}
			case SelectionPosition.Right:
				{
					if (newTime.MSeconds - MAX_TIME_SPAN < TimeNode.Start.MSeconds) {
						TimeNode.Stop.MSeconds = TimeNode.Start.MSeconds + MAX_TIME_SPAN;
					} else {
						TimeNode.Stop = newTime;
					}
					break;
				}
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double mid, bottom, stop;
			Color c;

			tk.Begin ();
			if (Selected) {
				c = Constants.TIMER_SELECTED_COLOR;
			} else {
				c = Constants.TIMER_UNSELECTED_COLOR;
			}
			tk.FillColor = c;
			tk.StrokeColor = c;
			tk.LineWidth = 4;
			
			mid = OffsetY + Constants.CATEGORY_HEIGHT / 2;
			bottom = OffsetY + Constants.CATEGORY_HEIGHT;
			stop = Utils.TimeToPos (TimeNode.Stop, SecondsPerPixel);
			
			tk.DrawLine (new Point (StartX, OffsetY),
			             new Point (StartX, bottom));
			tk.DrawLine (new Point (StartX, bottom),
			             new Point (stop, bottom));
			tk.DrawLine (new Point (stop, OffsetY),
			             new Point (stop, bottom));
			tk.FontSize = 20;
			tk.DrawText (new Point (StartX, OffsetY), stop - StartX,
			             Constants.CATEGORY_HEIGHT - 4, TimeNode.Name);
			             
			tk.End ();
		}
	}
}
