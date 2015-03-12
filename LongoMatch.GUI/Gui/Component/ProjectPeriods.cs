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
		double maxSecondsPerPixels;

		public ProjectPeriods ()
		{
			this.Build ();

			zoomscale.CanFocus = false;
			zoomscale.Adjustment.Lower = 0;
			zoomscale.Adjustment.Upper = 100;
			zoomscale.ValueChanged += HandleZoomChanged;

			zoomoutimage.Pixbuf = LongoMatch.Gui.Helpers.Misc.LoadIcon ("longomatch-zoom-out", 14);
			zoominimage.Pixbuf = LongoMatch.Gui.Helpers.Misc.LoadIcon ("longomatch-zoom-in", 14);

			main_cam_playerbin.Tick += HandleTick;
			main_cam_playerbin.ShowControls = false;
			//sec_cam_playerbin.ShowControls = false;

			timerule = new Timerule (new WidgetWrapper (timerulearea)) { ObjectsCanMove = false };
			camerasTimeline = new CamerasTimeline (new WidgetWrapper (timelinearea));
			camerasLabels = new CamerasLabels (new WidgetWrapper (labelsarea));

			/* FIXME: Links Label size to the container */
			labelsarea.SizeRequested += (o, args) => {
				labels_vbox.WidthRequest = args.Requisition.Width;
			};

			// Set some sane defaults
			labels_vbox.WidthRequest = StyleConf.TimelineLabelsWidth;
			// We need to align the timerule and the beginning of labels list
			timerulearea.HeightRequest = StyleConf.TimelineCameraHeight;

			camerasTimeline.TimeNodeChanged += HandleTimeNodeChanged;
			camerasTimeline.ShowTimerMenuEvent += HandleShowTimerMenuEvent;

			// Synchronize scrollbars with timerule and labels
			scrolledwindow2.Vadjustment.ValueChanged += HandleScrollEvent;
			scrolledwindow2.Hadjustment.ValueChanged += HandleScrollEvent;

			// Adjust our zoom factors when the window is resized
			scrolledwindow2.SizeAllocated += (o, args) =>  {
				UpdateMaxSecondsPerPixel ();
			};
			// Synchronize the zoom widget height with scrolledwindow's scrollbar's.
			scrolledwindow2.HScrollbar.SizeAllocated += (object o, SizeAllocatedArgs args) => {
				int spacing = (int)scrolledwindow2.StyleGetProperty ("scrollbar-spacing");
				zoomhbox.HeightRequest = args.Allocation.Height + spacing;
			};

			LongoMatch.Gui.Helpers.Misc.SetFocus (this, false);

			menu = new PeriodsMenu ();
		}

		protected override void OnDestroyed ()
		{
			main_cam_playerbin.Destroy ();
			//sec_cam_playerbin.Destroy ();
			timerule.Dispose ();
			camerasLabels.Dispose ();
			camerasTimeline.Dispose ();
			base.OnDestroyed ();
		}

		public void Pause ()
		{
			main_cam_playerbin.Pause ();
			//sec_cam_playerbin.Pause ();
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

				MediaFileSet fileSet = project.Description.FileSet;
				file = fileSet.FirstOrDefault ();
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

				camerasLabels.Load (fileSet);
				camerasTimeline.Load (periods, fileSet, duration);

				UpdateMaxSecondsPerPixel ();
				UpdateTimeLineSize (fileSet);

				timerule.Duration = duration;

				// Open media file
				main_cam_playerbin.Open (value.Description.FileSet);
			}
		}

		/// <summary>
		/// Calculates the maximum number of seconds per pixel to accomodate the complete duration in available space.
		/// </summary>
		void UpdateMaxSecondsPerPixel () {
			if (duration != null) {
				// With 20 pixels of margin to properly see the whole segment
				maxSecondsPerPixels = (double)duration.TotalSeconds / (scrolledwindow2.Allocation.Width - 20);
				HandleZoomChanged (zoomscale, new EventArgs ());
			}
		}

		/// <summary>
		/// Adjusts the VPaned position to accomodate up to 8 cameras.
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
			// We zoom from our Maximum number of seconds per pixel to the minimum using the 0 to 100 scale value
			double secondsPerPixel = 0, minSecondsPerPixels = 0.01;
			double value = Math.Round (zoomscale.Value);
			double diff = maxSecondsPerPixels - minSecondsPerPixels;

			secondsPerPixel = maxSecondsPerPixels - (diff * zoomscale.Value / 100);

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

