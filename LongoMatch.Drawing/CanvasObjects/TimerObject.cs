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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class TimerObject: TaggerObject
	{
		Time currentTime;
		static Image iconImage;

		public TimerObject (TimerButton timer): base (timer)
		{
			Button = timer;
			Toggle = true;
			CurrentTime = new Time (0);
			if (iconImage == null) {
				iconImage = new Image (System.IO.Path.Combine (Config.ImagesDir,
				                                               StyleConf.ButtonTimerIcon));
			}
			MinWidth = StyleConf.ButtonMinWidth;
			MinHeight = StyleConf.ButtonHeaderHeight + StyleConf.ButtonTimerFontSize;
		}

		public TimerButton Button {
			get;
			set;
		}

		public override Image Icon {
			get {
				return iconImage;
			}
		}

		public Time CurrentTime {
			set {
				bool update = false;

				if (CurrentTimeNode != null) {
					if (value < CurrentTimeNode.Start) {
						Button.Timer.CancelTimer ();
						Active = false;
						CurrentTimeNode = null;
					}
				}
				if (value != null && currentTime != null &&
					currentTime.Seconds != value.Seconds) {
					update = true;
				}
				currentTime = value;
				if (update && CurrentTimeNode != null) {
					ReDraw ();
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

		public Image TeamImage {
			get;
			set;
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
			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			base.Draw (tk, area);

			tk.Begin ();
			
			if (Active && Mode != TagMode.Edit) {
				tk.LineWidth = 2;
				tk.StrokeColor = Button.BackgroundColor;
				tk.FillColor = Button.BackgroundColor;
				tk.FontWeight = FontWeight.Normal;
				tk.FontSize = StyleConf.ButtonHeaderFontSize;
				tk.FontAlignment = FontAlignment.Left;
				tk.DrawText (new Point (Position.X + StyleConf.ButtonHeaderWidth, Position.Y),
				             Button.Width - StyleConf.ButtonHeaderWidth,
				             StyleConf.ButtonHeaderHeight, Button.Timer.Name);
				tk.FontWeight = FontWeight.Bold;
				tk.FontSize = StyleConf.ButtonTimerFontSize;
				tk.FontAlignment = FontAlignment.Center;
				tk.DrawText (new Point (Position.X, Position.Y + StyleConf.ButtonHeaderHeight),
				             Button.Width, Button.Height - StyleConf.ButtonHeaderHeight,
				             PartialTime.ToSecondsString ());
			} else {
				Text = Button.Timer.Name;
				DrawText (tk);
				Text = null;
			}
			
			if (TeamImage != null) {
				tk.DrawImage (new Point (Position.X + Width - 40, Position.Y + 5), 40,
				              iconImage.Height, TeamImage, true);
			}

			DrawSelectionArea (tk);
			tk.End ();
		}
	}
}

