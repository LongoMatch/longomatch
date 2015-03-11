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
		CamerasLabels camerasLabels;
		CamerasTimeline camerasTimeline;
		Timerule timerule;
		Time duration;
		Project project;
		PeriodsMenu menu;
		Dictionary<Period, Period> periodsDict;
		bool projectHasPeriods;

		public ProjectPeriods ()
		{
			this.Build ();

			zoomscale.CanFocus = false;
			zoomscale.Adjustment.Lower = 0;
			zoomscale.Adjustment.Upper = 12;
			zoomscale.ValueChanged += HandleZoomChanged;

			zoominimage.Pixbuf = LongoMatch.Gui.Helpers.Misc.LoadIcon ("longomatch-zoom-in", 14);
			zoomoutimage.Pixbuf = LongoMatch.Gui.Helpers.Misc.LoadIcon ("longomatch-zoom-out", 14);

			// Synchronize the zoom widget height with scrolledwindow's scrollbar's.
			scrolledwindow2.HScrollbar.SizeAllocated += (object o, SizeAllocatedArgs args) => {
				int spacing = (int)scrolledwindow2.StyleGetProperty ("scrollbar-spacing");
				zoomhbox.HeightRequest = args.Allocation.Height + spacing;
			};

			main_cam_playerbin.Tick += HandleTick;
			main_cam_playerbin.ShowControls = false;
			//secondary_cam_playerbin.ShowControls = false;

			timerule = new Timerule (new WidgetWrapper (timerulearea)) { ObjectsCanMove = false };
			camerasTimeline = new CamerasTimeline (new WidgetWrapper (timelinearea));
			camerasLabels = new CamerasLabels (new WidgetWrapper (labelsarea));

			/* Links Label size to the container */
			labelsarea.SizeRequested += (o, args) => {
				labels_vbox.WidthRequest = args.Requisition.Width;
			};

			// Set some sane defaults
			labels_vbox.WidthRequest = StyleConf.TimelineLabelsWidth;
			// We need to aligne the timerule and the beginning of labels list
			timerulearea.HeightRequest = StyleConf.TimelineCameraHeight;

			camerasTimeline.TimeNodeChanged += HandleTimeNodeChanged;
			camerasTimeline.ShowTimerMenuEvent += HandleShowTimerMenuEvent;

			// Synchronize scrollbars with timerule and labels
			scrolledwindow2.Vadjustment.ValueChanged += HandleScrollEvent;
			scrolledwindow2.Hadjustment.ValueChanged += HandleScrollEvent;

			LongoMatch.Gui.Helpers.Misc.SetFocus (this, false);

			menu = new PeriodsMenu ();
		}

		protected override void OnDestroyed ()
		{
			main_cam_playerbin.Destroy ();
			timerule.Dispose ();
			camerasLabels.Dispose ();
			camerasTimeline.Dispose ();
			base.OnDestroyed ();
		}

		public void Pause ()
		{
			main_cam_playerbin.Pause ();
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
				
				main_cam_playerbin.ShowControls = false;
				this.project = value;
				gamePeriods = value.Dashboard.GamePeriods;

				file = value.Description.FileSet.FirstOrDefault ();
				start = new Time (0);
				duration = file.Duration;
				pDuration = new Time (duration.MSeconds / gamePeriods.Count);
				if (project.Periods == null || project.Periods.Count == 0) {
					periods = new List<Period> ();
					gamePeriods = value.Dashboard.GamePeriods;
					foreach (string s in gamePeriods) {
						Period period = new Period { Name = s };
						period.Start (start);
						period.Stop (start + pDuration);
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

				MediaFileSet fileSet = project.Description.FileSet;

				camerasLabels.Load (fileSet);
				camerasTimeline.Load (periods, fileSet, duration);
				UpdateTimeLineSize (fileSet);

				timerule.Duration = duration;
				zoomscale.Value = 6;

				// Open media file
				main_cam_playerbin.Open (value.Description.FileSet);
			}
		}

		/// <summary>
		/// Adjusts the VPaned position to accomodate up to 8 cameras
		/// </summary>
		/// <param name="fileSet">File set.</param>
		void UpdateTimeLineSize (MediaFileSet fileSet)
		{
			// Number of media files plus period sync line
			int visibleItems = Math.Min (StyleConf.TimelineCameraMaxLines, project.Description.FileSet.Count + 1);
			int height = scrolledwindow2.HScrollbar.Requisition.Height * 2;
			height += visibleItems * StyleConf.TimelineCameraHeight;
			vpaned2.Position = vpaned2.Allocation.Height - height;
		}

		/// <summary>
		/// Handles the tick from media player to update Current Time in timelines.
		/// </summary>
		/// <param name="currentTime">Current time.</param>
		void HandleTick (Time currentTime)
		{
			timerule.CurrentTime = currentTime;
			camerasTimeline.CurrentTime = currentTime;
			QueueDraw ();
		}

		void HandleTimeNodeChanged (TimeNode tNode, object val)
		{
			Time time = val as Time;
			main_cam_playerbin.Pause ();
			main_cam_playerbin.Seek (time, false);
			// FIXME: Reflect change in the MediaFile's offset.
		}

		/// <summary>
		/// Handles the scroll event from the scrolled window and synchronise the timelines labels and timerule with it.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		void HandleScrollEvent (object sender, System.EventArgs args)
		{
			if (sender == scrolledwindow2.Vadjustment)
				camerasLabels.Scroll = scrolledwindow2.Vadjustment.Value;
			else if (sender == scrolledwindow2.Hadjustment) {
				timerule.Scroll = scrolledwindow2.Hadjustment.Value;
			}
			QueueDraw ();
		}

		void HandleZoomChanged (object sender, EventArgs e)
		{
			double secondsPer100Pixels, value;

			value = Math.Round (zoomscale.Value);
			if (value == 0) {
				secondsPer100Pixels = 1;
			} else if (value <= 6) {
				secondsPer100Pixels = value * 10;
			} else {
				secondsPer100Pixels = (value - 5) * 60;
			}

			double secondsPerPixel = secondsPer100Pixels / 100;
			timerule.SecondsPerPixel = secondsPerPixel;
			camerasTimeline.SecondsPerPixel = secondsPerPixel;
			QueueDraw ();
		}

		void HandleShowTimerMenuEvent (Timer timer, Time time)
		{
			menu.ShowMenu (project, timer, time, camerasTimeline.PeriodsTimeline);
		}
	}
}

