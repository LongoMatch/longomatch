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
using Pango;
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

			zoomoutimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-zoom-out", 14);
			zoominimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-zoom-in", 14);

			// Only main cam has audio for now
			main_cam_audio_button_image.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-volume-hi", IconSize.Button);
			sec_cam_audio_button_image.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-volume-off", IconSize.Button);
			main_cam_audio_button.Active = true;
			sec_cam_audio_button.Active = false;

			// We control visibility of those widgets
			sec_cam_vbox.NoShowAll = true;
			sec_cam_vbox.Visible = false;
			sec_cam_didactic_label.NoShowAll = true;
			sec_cam_didactic_label.Visible = true;
			sec_cam_didactic_label.Text = Catalog.GetString ("Drag the bars in the timeline to synchronize secondary video files with the main video");

			timerule = new Timerule (new WidgetWrapper (timerulearea));
			camerasTimeline = new CamerasTimeline (new WidgetWrapper (timelinearea));
			camerasLabels = new CamerasLabels (new WidgetWrapper (labelsarea));

			// Set some sane defaults
			labels_vbox.WidthRequest = StyleConf.TimelineLabelsWidth;
			// We need to align the timerule and the beginning of labels list
			timerulearea.HeightRequest = StyleConf.TimelineCameraHeight;

			main_cam_label.ModifyFont (FontDescription.FromString (Config.Style.Font + " bold 14"));
			sec_cam_label.ModifyFont (FontDescription.FromString (Config.Style.Font + " bold 14"));

			ConnectSignals ();

			LongoMatch.Gui.Helpers.Misc.SetFocus (this, false);

			menu = new PeriodsMenu ();
		}

		void ConnectSignals ()
		{
			zoomscale.ValueChanged += HandleZoomChanged;
			main_cam_audio_button.Toggled += HandleAudioToggled;
			sec_cam_audio_button.Toggled += HandleAudioToggled;

			main_cam_playerbin.Tick += HandleTick;
			main_cam_playerbin.PlayStateChanged += HandleStateChanged;

			// Listen for seek events from the timerule
			Config.EventsBroker.SeekEvent += HandleSeekEvent;
			// Handle dragging of cameras and periods
			camerasTimeline.CameraDragged += HandleCameraDragged;
			camerasTimeline.TimeNodeChanged += HandleTimeNodeChanged;
			camerasTimeline.ShowTimerMenuEvent += HandleShowTimerMenuEvent;

			// Synchronize scrollbars with timerule and labels
			scrolledwindow2.Vadjustment.ValueChanged += HandleScrollEvent;
			scrolledwindow2.Hadjustment.ValueChanged += HandleScrollEvent;

			/* FIXME: Links Label size to the container */
			labelsarea.SizeRequested += (o, args) => {
				labels_vbox.WidthRequest = args.Requisition.Width;
			};

			// Adjust our zoom factors when the window is resized
			scrolledwindow2.SizeAllocated += (o, args) =>  {
				UpdateMaxSecondsPerPixel ();
			};
			// Synchronize the zoom widget height with scrolledwindow's scrollbar's.
			scrolledwindow2.HScrollbar.SizeAllocated += (object o, SizeAllocatedArgs args) => {
				int spacing = (int)scrolledwindow2.StyleGetProperty ("scrollbar-spacing");
				zoomhbox.HeightRequest = args.Allocation.Height + spacing;
			};
		}

		protected override void OnDestroyed ()
		{
			Config.EventsBroker.SeekEvent -= HandleSeekEvent;

			main_cam_playerbin.Destroy ();
			sec_cam_playerbin.Destroy ();

			timerule.Dispose ();
			camerasLabels.Dispose ();
			camerasTimeline.Dispose ();

			base.OnDestroyed ();
		}

		public void Pause ()
		{
			main_cam_playerbin.Pause ();
			sec_cam_playerbin.Pause ();
		}

		public void Seek (Time time, bool accurate)
		{
			if (main_cam_playerbin.Opened) {
				main_cam_playerbin.Seek (time, accurate);
			}
			if (sec_cam_playerbin.Opened) {
				sec_cam_playerbin.Seek (time, accurate);
			}
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
				main_cam_label.Text = fileSet.First ().Name;
				main_cam_playerbin.ShowControls = false;
				main_cam_playerbin.Open (fileSet);
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

		/// <summary>
		/// Try to slave the secondary player to the first 
		/// </summary>
		/// <param name="playing">If set to <c>true</c> playing.</param>
		void HandleStateChanged (bool playing)
		{
			if (playing) {
				sec_cam_playerbin.Play ();
			} else {
				sec_cam_playerbin.Pause ();
			}
		}

		void HandleAudioToggled (object sender, EventArgs args)
		{
			if (sender == main_cam_audio_button) {
				main_cam_playerbin.Volume = main_cam_audio_button.Active ? 1 : 0;
				main_cam_audio_button_image.Pixbuf = Helpers.Misc.LoadIcon (main_cam_audio_button.Active ? "longomatch-control-volume-hi" : "longomatch-control-volume-off", IconSize.Button);
			} else if (sender == sec_cam_audio_button) {
				sec_cam_playerbin.Volume = sec_cam_audio_button.Active ? 1 : 0;
				sec_cam_audio_button_image.Pixbuf = Helpers.Misc.LoadIcon (sec_cam_audio_button.Active ? "longomatch-control-volume-hi" : "longomatch-control-volume-off", IconSize.Button);
			}
		}

		void HandleCameraDragged (MediaFile mediafile, TimeNode timenode)
		{
			// Start by pausing players
			main_cam_playerbin.Pause ();
			sec_cam_playerbin.Pause ();

			// Check if the CurrentTime of the time rule is in that node
			if (timenode.Start <= timerule.CurrentTime && timerule.CurrentTime <= timenode.Stop) {
				// Check if we need to show the player
				if (!sec_cam_vbox.Visible) {
					sec_cam_didactic_label.Hide ();
					sec_cam_vbox.Show ();
				}
				// Open this media file if needed
				if (!sec_cam_playerbin.Opened ||
					sec_cam_playerbin.MediaFileSet.FirstOrDefault () != mediafile) {
					MediaFileSet fileSet = new MediaFileSet ();
					fileSet.Add (mediafile);

					// Reload player with new cam
					sec_cam_label.Text = mediafile.Name;
					sec_cam_playerbin.ShowControls = false;
					sec_cam_playerbin.Open (fileSet);

					// Configure audio
					HandleAudioToggled (sec_cam_audio_button, new EventArgs ());
				}
				// Seek to position 
				sec_cam_playerbin.Seek (timerule.CurrentTime, true);
			} else {
				// Camera is out of scope, show didactic message
				sec_cam_vbox.Hide ();
				sec_cam_didactic_label.Text = Catalog.GetString ("Camera out of scope");
				sec_cam_didactic_label.Show ();
			}
		}

		/// <summary>
		/// Periods segments have moved, adjust main camera position to segment boundaries
		/// </summary>
		void HandleTimeNodeChanged (TimeNode tNode, object val)
		{
			Time time = val as Time;

			main_cam_playerbin.Pause ();
			if (sec_cam_playerbin.Opened) {
				sec_cam_playerbin.Pause ();
			}
			Seek (time, false);
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
			double diff = maxSecondsPerPixels - minSecondsPerPixels;

			secondsPerPixel = maxSecondsPerPixels - (diff * zoomscale.Value / 100);

			timerule.SecondsPerPixel = secondsPerPixel;
			camerasTimeline.SecondsPerPixel = secondsPerPixel;
			QueueDraw ();
		}

		void HandleSeekEvent (Time time, bool accurate) {
			Seek (time, false);
		}

		void HandleShowTimerMenuEvent (Timer timer, Time time)
		{
			menu.ShowMenu (project, timer, time, camerasTimeline.PeriodsTimeline);
		}
	}
}

