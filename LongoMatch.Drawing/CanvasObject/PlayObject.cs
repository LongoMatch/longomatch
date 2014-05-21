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
	public class PlayObject: BaseCanvasObject, ICanvasSelectableObject
	{
		const int MAX_TIME_SPAN=1000;
		
		public PlayObject (Play play)
		{
			Play = play;
		}
		
		public Play Play {
			get;
			set;
		}
		
		public double OffsetY {
			get;
			set;
		}
		
		public double SecondsPerPixel {
			set;
			protected get;
		}
		
		public bool Selected {
			set;
			get;
		}
		
		double StartX {
			get {
				return Common.TimeToPos (Play.Start, SecondsPerPixel);
			}
		}
		
		double StopX {
			get {
				return Common.TimeToPos (Play.Stop, SecondsPerPixel);
			}
		}
		
		double CenterX {
			get {
				return Common.TimeToPos (Play.Start + Play.Duration / 2,
				                         SecondsPerPixel);
			}
		}
		
		public override void Draw (IDrawingToolkit tk, Area area) {
			Color c = Play.Category.Color;
			tk.Begin ();
			tk.FillColor = new Color (c.R, c.G, c.B, (ushort) (0.8 * ushort.MaxValue));
			if (Selected) {
				tk.StrokeColor = Common.PLAY_OBJECT_SELECTED_COLOR;
			} else {
				tk.StrokeColor = Play.Category.Color;
			}
			tk.LineWidth = 2;
			tk.DrawRoundedRectangle (new Point (StartX, OffsetY),
			                  Common.TimeToPos (Play.Duration, SecondsPerPixel),
			                  Common.CATEGORY_HEIGHT, 2);
			tk.End ();
		}
		
		public Selection GetSelection (Point point, double precision) {
			double accuracy;
			if (point.Y >= OffsetY && point.Y < OffsetY + Common.CATEGORY_HEIGHT) {
				if (Drawable.MatchAxis (point.X, StartX, precision, out accuracy)) {
					return new Selection (this, SelectionPosition.Left, accuracy);
				} else if (Drawable.MatchAxis (point.X, StopX, precision, out accuracy)) {
					return new Selection (this, SelectionPosition.Right, accuracy);
				} else if (point.X > StartX && point.X < StopX) {
					return new Selection (this, SelectionPosition.All,
					                      Math.Abs (CenterX - point.X));
				}
			}
			return null;
		}
		
		public void Move (Selection sel, Point p, Point start) {
			Time newTime = Common.PosToTime (p, SecondsPerPixel);
			
			switch (sel.Position) {
			case SelectionPosition.Left: {
				if (newTime.MSeconds + MAX_TIME_SPAN > Play.Stop.MSeconds) {
					Play.Start.MSeconds = Play.Stop.MSeconds - MAX_TIME_SPAN;
				} else {
					Play.Start = newTime;
				}
				break;
			}
			case SelectionPosition.Right: {
				if (newTime.MSeconds - MAX_TIME_SPAN < Play.Start.MSeconds) {
					Play.Stop.MSeconds = Play.Start.MSeconds + MAX_TIME_SPAN;
				} else {
					Play.Stop = newTime;
				}
				break;
			}
			}
		}
	}
}

