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
using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Store;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class TimerObject: TaggerObject
	{
		Time currentTime;

		public TimerObject (Timer timer): base (timer)
		{
			Timer = timer;
			Toggle = true;
			CurrentTime = new Time (0);
		}

		public Timer Timer {
			get;
			set;
		}

		public Time CurrentTime {
			set {
				if (CurrentTimeNode != null) {
					if (value < CurrentTimeNode.Start) {
						Timer.CancelTimer ();
						CurrentTimeNode = null;
					}
				}
				currentTime = value;
			}
			get {
				return currentTime;
			}
		}

		TimeNode CurrentTimeNode {
			get;
			set;
		}

		Time PartialTime {
			get {
				if (CurrentTimeNode == null) {
					return new Time (0);
				} else {
					return CurrentTime - CurrentTimeNode.Start;
				}
			}
		}

		public override void ClickReleased ()
		{
			TimeNode tn;

			base.ClickReleased ();
			tn = CurrentTimeNode;
			if (tn == null) {
				Log.Debug ("Start timer at " + CurrentTime.ToMSecondsString ());
				CurrentTimeNode = Timer.StartTimer (CurrentTime);
			} else {
				Log.Debug ("Stop timer at " + CurrentTime.ToMSecondsString ());
				Timer.StopTimer (CurrentTime);
				tn.Stop = CurrentTime;
				CurrentTimeNode = null;
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double h;

			if (CurrentTimeNode == null || Mode == TagMode.Edit) {
				h = Timer.Height;
			} else {
				h = Timer.Height / 2;
			}
			
			tk.Begin ();

			/* Draw Rectangle */
			DrawButton (tk);
			
			/* Draw header */
			tk.LineWidth = 2;
			tk.StrokeColor = Timer.TextColor;
			tk.FillColor = Timer.TextColor;
			tk.FontWeight = FontWeight.Bold;
			tk.DrawText (Position, Timer.Width, h, Timer.Name);
			if (CurrentTimeNode != null && Mode != TagMode.Edit) {
				tk.DrawText (new Point (Position.X, Position.Y + h), Timer.Width, h,
				             PartialTime.ToSecondsString ());
			}
			DrawSelectionArea (tk);
			tk.End ();
		}
	}
}

