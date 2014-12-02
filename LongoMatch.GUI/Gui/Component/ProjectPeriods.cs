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
using LongoMatch.Core.Store;
using System.Collections.Generic;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Drawing.Cairo;
using Mono.Unix;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Gui.Menus;
using LongoMatch.Gui.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProjectPeriods : Gtk.Bin
	{
		TimersTimeline timersTimeline;
		Timerule timerule;
		Time duration;
		Project project;
		PeriodsMenu menu;
		Dictionary<Period, Period> periodsDict;
		bool projectHasPeriods;

		public ProjectPeriods ()
		{
			this.Build ();

			zoomoutimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-zoom-more", 20);
			zoominimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-zoom-less", 20);
			arrowimage1.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-down-arrow", 20);
			arrowimage2.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-down-arrow", 20);

			zoominbutton.Clicked += HandleZooomActivated;
			zoomoutbutton.Clicked += HandleZooomActivated;
			playerbin2.Tick += HandleTick;
			playerbin2.ShowControls = false;
			timerule = new Timerule (new WidgetWrapper (drawingarea1));
			timerule.ObjectsCanMove = false;
			timersTimeline = new TimersTimeline (new WidgetWrapper (drawingarea2));
			drawingarea1.HeightRequest = LongoMatch.Drawing.Constants.TIMERULE_HEIGHT;
			drawingarea2.HeightRequest = LongoMatch.Drawing.Constants.TIMER_HEIGHT;
			timersTimeline.TimeNodeChanged += HandleTimeNodeChanged;
			timersTimeline.ShowTimerMenuEvent += HandleShowTimerMenuEvent;
			scrolledwindow2.Hadjustment.ValueChanged += HandleValueChanged;
			synclabel.Markup = String.Format ("<b> {0} </b>",
			                                  Catalog.GetString ("Synchronize the game periods"));
			LongoMatch.Gui.Helpers.Misc.SetFocus (this, false);
			menu = new PeriodsMenu ();
		}

		protected override void OnDestroyed ()
		{
			playerbin2.Destroy ();
			timerule.Dispose ();
			timersTimeline.Dispose ();
			base.OnDestroyed ();
		}

		public void Pause ()
		{
			playerbin2.Pause ();
		}

		public void SaveChanges ()
		{
			if (!projectHasPeriods)
				return;
			foreach (Period p in periodsDict.Keys) {
				Period newp = periodsDict [p];
				TimeNode tn = p.PeriodNode;
				Time diff = newp.PeriodNode.Start - tn.Start;
				foreach (TimelineEvent evt in project.Timeline.Where
				         (e=>e.EventTime > tn.Start && e.EventTime < tn.Stop)) {
					evt.Move (diff);
				}
				foreach (TimeNode t in p.Nodes) {
					t.Move (diff);
				}
			}
		}

		public Project Project {
			set {
				Time start, pDuration;
				List<string> gamePeriods;
				List<Period> periods;
				MediaFile file;
				
				playerbin2.ShowControls = false;
				this.project = value;
				gamePeriods = value.Dashboard.GamePeriods;

				file = value.Description.FileSet.GetAngle (MediaFileAngle.Angle1);
				start = new Time (0);
				duration = file.Duration;
				pDuration = new Time (duration.MSeconds / gamePeriods.Count);
				if (project.Periods == null || project.Periods.Count == 0) {
					periods = new List<Period> ();
					gamePeriods = value.Dashboard.GamePeriods;
					foreach (string s in gamePeriods) {
						Period period = new Period { Name = s };
						period.StartTimer (start);
						period.StopTimer (start + pDuration);
						periods.Add (period);
						start += pDuration;
					}
					value.Periods = periods;
					projectHasPeriods = false;
				} else {
					periodsDict = new Dictionary <Period, Period> ();
					foreach (Period p in project.Periods) {
						Period newp = new Period {Name = p.Name};
						newp.Nodes.Add (p.PeriodNode);
						periodsDict.Add (p, newp);
					}
					projectHasPeriods = true;
					periods = periodsDict.Values.ToList ();
				}
				timersTimeline.LoadPeriods (periods, duration);
				timerule.Duration = duration;
				SetZoom ();
				playerbin2.Open (value.Description.FileSet);
			}
		}

		void SetZoom ()
		{
			if (duration != null) {
				double spp = (double)duration.TotalSeconds / drawingarea1.Allocation.Width;
				int secondsPerPixel = (int)Math.Ceiling (spp);
				timerule.SecondsPerPixel = secondsPerPixel;
				timersTimeline.SecondsPerPixel = secondsPerPixel;
			}
		}

		void HandleTick (Time currentTime)
		{
			timerule.CurrentTime = currentTime;
			timersTimeline.CurrentTime = currentTime;
			drawingarea1.QueueDraw ();
			drawingarea2.QueueDraw ();
		}

		void HandleTimeNodeChanged (TimeNode tNode, object val)
		{
			Time time = val as Time;
			playerbin2.Pause ();
			playerbin2.Seek (time, false);
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
				timersTimeline.SecondsPerPixel ++;
			} else {
				timerule.SecondsPerPixel = Math.Max (1, timerule.SecondsPerPixel - 1);
				timersTimeline.SecondsPerPixel = Math.Max (1, timersTimeline.SecondsPerPixel - 1);
			}
			drawingarea1.QueueDraw ();
			drawingarea2.QueueDraw ();
		}

		void HandleShowTimerMenuEvent (Timer timer, Time time)
		{
			menu.ShowMenu (project, timer, time, timersTimeline.TimerTimeline);
		}
	}
}

