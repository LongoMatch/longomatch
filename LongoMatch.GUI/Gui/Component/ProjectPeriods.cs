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
using LongoMatch.Core.Store;
using System.Collections.Generic;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Drawing.Cairo;
using Mono.Unix;
using LongoMatch.Gui.Helpers;
using LongoMatch.Core.Common;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProjectPeriods : Gtk.Bin
	{
		TimersTimeline timersTimenline;
		Timerule timerule;
		Time duration;
		
		public ProjectPeriods ()
		{
			this.Build ();
			zoominbutton.Clicked += HandleZooomActivated;
			zoomoutbutton.Clicked += HandleZooomActivated;
			playerbin2.Tick += HandleTick;
			playerbin2.ShowControls = false;
			timerule = new Timerule (new WidgetWrapper (drawingarea1));
			timersTimenline = new TimersTimeline (new WidgetWrapper (drawingarea2));
			drawingarea1.HeightRequest = LongoMatch.Drawing.Constants.TIMERULE_HEIGHT;
			drawingarea2.HeightRequest = LongoMatch.Drawing.Constants.TIMER_HEIGHT;
			timersTimenline.TimeNodeChanged += HandleTimeNodeChanged;
			scrolledwindow2.Hadjustment.ValueChanged += HandleValueChanged;
			synclabel.Markup = String.Format ("{0} {1} {2}", "<b>⬇  ",
			                                Catalog.GetString ("Synchronize the game periods"),
			                                "  ⬇</b>");
			Misc.SetFocus (this, false);
		}
		
		protected override void OnDestroyed ()
		{
			playerbin2.Destroy ();
			timerule.Dispose ();
			timersTimenline.Dispose ();
			base.OnDestroyed ();
		}

		public Project Project {
			set {
				Time start, pDuration;
				List<string> gamePeriods;
				MediaFile file;
				
				playerbin2.ShowControls = false;
				
				gamePeriods = value.Dashboard.GamePeriods;

				file = value.Description.FileSet.GetAngle (MediaFileAngle.Angle1);
				start = new Time (0);
				duration = file.Duration;
				pDuration = new Time (duration.MSeconds / gamePeriods.Count);
				List<Period> periods = new List<Period> ();
				gamePeriods = value.Dashboard.GamePeriods;
				
				timerule.Duration = duration;
				SetZoom ();
				playerbin2.Open (value.Description.FileSet);
				
				foreach (string s in gamePeriods) {
					Period period = new Period {Name = s};
					period.StartTimer (start);
					period.StopTimer (start + pDuration);
					periods.Add (period);
					start += pDuration;
				}
				value.Periods = periods;
				timersTimenline.LoadPeriods (periods, duration);
			}
		}
		
		void SetZoom () {
			if (duration != null) {
				double spp = (double) duration.Seconds / drawingarea1.Allocation.Width;
				int secondsPerPixel = (int) Math.Ceiling (spp);
				timerule.SecondsPerPixel = secondsPerPixel;
				timersTimenline.SecondsPerPixel = secondsPerPixel;
			}
		}
		
		void HandleTick (Time currentTime)
		{
			timerule.CurrentTime = currentTime;
			timersTimenline.CurrentTime = currentTime;
			drawingarea1.QueueDraw ();
			drawingarea2.QueueDraw ();
		}

		void HandleTimeNodeChanged (TimeNode tNode, object val)
		{
			Time time = val as Time;
			playerbin2.Pause ();
			playerbin2.Seek (time, true);
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			timerule.Scroll = scrolledwindow2.Hadjustment.Value;
			drawingarea1.QueueDraw ();
		}
		
		void HandleZooomActivated (object sender, EventArgs e)
		{
			if (sender == zoomoutbutton) {
				timerule.SecondsPerPixel ++;
				timersTimenline.SecondsPerPixel ++;
			} else {
				timerule.SecondsPerPixel = Math.Max (1, timerule.SecondsPerPixel - 1);
				timersTimenline.SecondsPerPixel = Math.Max (1, timersTimenline.SecondsPerPixel - 1);
			}
			drawingarea1.QueueDraw();
			drawingarea2.QueueDraw();
		}
	}
}

