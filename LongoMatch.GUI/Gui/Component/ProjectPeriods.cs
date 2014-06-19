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
using System.Collections.Generic;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Drawing.Cairo;
using Mono.Unix;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProjectPeriods : Gtk.Bin
	{
		TimersTimeline timersTimenline;
		Timerule timerule;
		
		
		public ProjectPeriods ()
		{
			this.Build ();
			playerbin2.Tick += HandleTick;
			playerbin2.ShowControls = false;
			timerule = new Timerule (new WidgetWrapper (drawingarea1));
			timersTimenline = new TimersTimeline (new WidgetWrapper (drawingarea2));
			drawingarea1.HeightRequest = LongoMatch.Drawing.Common.TIMERULE_HEIGHT;
			drawingarea2.HeightRequest = LongoMatch.Drawing.Common.TIMER_HEIGHT;
			timersTimenline.TimeNodeChanged += HandleTimeNodeChanged;
			scrolledwindow2.Hadjustment.ValueChanged += HandleValueChanged;
			synclabel.Markup = String.Format ("{0} {1} {2}", "<b>⬇  ",
			                                Catalog.GetString ("Synchronize the game periods"),
			                                "  ⬇</b>");
		}

		public Project Project {
			set {
				Time start, duration, pDuration;
				List<string> gamePeriods;
				
				playerbin2.ShowControls = false;
				
				gamePeriods = value.Categories.GamePeriods;

				start = new Time (0);
				duration = value.Description.File.Duration;
				pDuration = new Time (duration.MSeconds / gamePeriods.Count);
				List<Timer> timers = new List<Timer> ();
				gamePeriods = value.Categories.GamePeriods;
				
				timerule.Duration = duration;
				playerbin2.Open (value.Description.File.FilePath);

				foreach (string s in gamePeriods) {
					Timer timer = new Timer {Name = s};
					timer.Start (start);
					timer.Stop (start + pDuration);
					timers.Add (timer);
					start += pDuration;
				}
				timersTimenline.LoadTimers (timers, duration, false);
			}
		}
		
		void HandleTick (Time currentTime, Time streamLength, double currentPosition)
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

	}
}

