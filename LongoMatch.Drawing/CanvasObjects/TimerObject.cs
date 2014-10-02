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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class TimerObject: TaggerObject
	{
		Time currentTime;

		public TimerObject (TimerButton timer): base (timer)
		{
			Button = timer;
			Toggle = true;
			CurrentTime = new Time (0);
		}

		public TimerButton Button {
			get;
			set;
		}

		public Time CurrentTime {
			set {
				bool update = false;

				if (CurrentTimeNode != null) {
					if (value < CurrentTimeNode.Start) {
						Button.Timer.CancelTimer ();
						CurrentTimeNode = null;
					}
				}
				if (value != null && currentTime != null &&
				    currentTime.Seconds != value.Seconds) {
					update = true;
				}
				currentTime = value;
				if (update && CurrentTimeNode != null) {
					EmitRedrawEvent (this, DrawArea);
				}
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
				CurrentTimeNode = Button.Timer.StartTimer (CurrentTime);
			} else {
				Log.Debug ("Stop timer at " + CurrentTime.ToMSecondsString ());
				Button.Timer.StopTimer (CurrentTime);
				tn.Stop = CurrentTime;
				CurrentTimeNode = null;
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double h;

			if (CurrentTimeNode == null || Mode == TagMode.Edit) {
				h = Button.Height;
			} else {
				h = Button.Height / 2;
			}
			
			if (!UpdateDrawArea (tk, area, new Area (Position, Width, Height))) {
				return;
			};

			tk.Begin ();
			/* Draw Rectangle */
			DrawButton (tk);
			
			/* Draw header */
			tk.LineWidth = 2;
			tk.StrokeColor = Button.TextColor;
			tk.FillColor = Button.TextColor;
			tk.FontWeight = FontWeight.Bold;
			tk.DrawText (DrawPosition, Button.Width, h, Button.Timer.Name);
			if (CurrentTimeNode != null && Mode != TagMode.Edit) {
				tk.DrawText (new Point (DrawPosition.X, Position.Y + h), Button.Width, h,
				             PartialTime.ToSecondsString ());
			}
			DrawSelectionArea (tk);
			tk.End ();
		}
	}
}

