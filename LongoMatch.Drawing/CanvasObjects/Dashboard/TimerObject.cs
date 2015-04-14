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

namespace LongoMatch.Drawing.CanvasObjects.Dashboard
{
	public class TimerObject: DashboardButtonObject
	{
		Time currentTime;
		static Image iconImage;

		public TimerObject (TimerButton timer) : base (timer)
		{
			Button = timer;
			Toggle = true;
			CurrentTime = new Time (0);
			if (iconImage == null) {
				iconImage = new Image (System.IO.Path.Combine (Config.ImagesDir,
					StyleConf.ButtonTimerIcon));
			}
			MinWidth = StyleConf.ButtonMinWidth;
			MinHeight = iconImage.Height + StyleConf.ButtonTimerFontSize;
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

				if (Button.StartTime != null) {
					if (value < Button.StartTime) {
						Button.Cancel ();
						Active = false;
					}
				}
				if (value != null && currentTime != null &&
				    currentTime.TotalSeconds != value.TotalSeconds) {
					update = true;
				}
				currentTime = value;
				if (update && Button.StartTime != null) {
					// It is possible that the button is activated but not thtough a click
					Active = true;
					ReDraw ();
				}
			}
			get {
				return currentTime;
			}
		}

		Time PartialTime {
			get {
				if (Button.StartTime == null) {
					return new Time (0);
				} else {
					return CurrentTime - Button.StartTime;
				}
			}
		}

		public Image TeamImage {
			get;
			set;
		}

		public override void ClickReleased ()
		{
			base.ClickReleased ();
			if (Button.StartTime == null) {
				Log.Debug ("Start timer at " + CurrentTime.ToMSecondsString ());
				Button.Start (CurrentTime);
			} else {
				Log.Debug ("Stop timer at " + CurrentTime.ToMSecondsString ());
				if (Button.StartTime.MSeconds != CurrentTime.MSeconds) {
					Button.Stop (CurrentTime);
				} else {
					Button.Cancel ();
				}
			}
		}

		int TextHeaderX {
			get {
				return iconImage.Width + 5 * 2;
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			base.Draw (tk, area);

			tk.Begin ();
			
			if (Active && Mode != DashboardMode.Edit) {
				tk.LineWidth = StyleConf.ButtonLineWidth;
				tk.StrokeColor = Button.BackgroundColor;
				tk.FillColor = Button.BackgroundColor;
				tk.FontWeight = FontWeight.Normal;
				tk.FontSize = StyleConf.ButtonHeaderFontSize;
				tk.FontAlignment = FontAlignment.Left;
				tk.DrawText (new Point (Position.X + TextHeaderX, Position.Y),
					Button.Width - TextHeaderX, iconImage.Height, Button.Timer.Name);
				tk.FontWeight = FontWeight.Bold;
				tk.FontSize = StyleConf.ButtonTimerFontSize;
				tk.FontAlignment = FontAlignment.Center;
				tk.DrawText (new Point (Position.X, Position.Y + iconImage.Height),
					Button.Width, Button.Height - iconImage.Height,
					PartialTime.ToSecondsString (), false, true);
			} else {
				Text = Button.Name;
				DrawText (tk);
				Text = null;
			}
			
			if (TeamImage != null) {
				tk.DrawImage (new Point (Position.X + Width - 40, Position.Y + 5), 40,
					iconImage.Height, TeamImage, true);
			}
			tk.End ();
		}
	}
}

