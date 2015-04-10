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
using LongoMatch.Drawing.CanvasObjects;
using Mono.Unix;
using Gtk;
using Pango;
using LongoMatch.Core.Common;
using LongoMatch.Gui.Menus;
using LongoMatch.Gui.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class SynchronizationWidget : Gtk.Bin
	{
		CamerasLabels camerasLabels;
		CamerasTimeline camerasTimeline;
		Timerule timerule;
		Time duration;
		Project project;
		PeriodsMenu menu;
		Dictionary<Period, Period> periodsDict;
		double maxSecondsPerPixels;

		enum DidacticMessage
		{
			Initial,
			CameraOutOfScope,
		}

		public SynchronizationWidget ()
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

			// We control visibility of those widgets, they are hidden at startup
			sec_cam_vbox.NoShowAll = true;
			sec_cam_didactic_label.NoShowAll = true;

			timerule = new Timerule (new WidgetWrapper (timerulearea));
			camerasTimeline = new CamerasTimeline (new WidgetWrapper (timelinearea));
			camerasLabels = new CamerasLabels (new WidgetWrapper (labelsarea));

			// Set some sane defaults
			labels_vbox.WidthRequest = StyleConf.TimelineLabelsWidth;
			// We need to align the timerule and the beginning of labels list
			timerulearea.HeightRequest = StyleConf.TimelineCameraHeight;

			main_cam_label.ModifyFont (FontDescription.FromString (Config.Style.Font + " bold 14"));
			sec_cam_label.ModifyFont (FontDescription.FromString (Config.Style.Font + " bold 14"));

			main_cam_playerbin.Mode = PlayerViewOperationMode.Synchronization;
			sec_cam_playerbin.Mode = PlayerViewOperationMode.Synchronization;

			ConnectSignals ();

			LongoMatch.Gui.Helpers.Misc.SetFocus (this, false);

			menu = new PeriodsMenu ();
		}

		void ConnectSignals ()
		{
			zoomscale.ValueChanged += HandleZoomChanged;
			main_cam_audio_button.Toggled += HandleAudioToggled;
			sec_cam_audio_button.Toggled += HandleAudioToggled;

			main_cam_playerbin.Player.TimeChangedEvent += HandleTick;
			main_cam_playerbin.Player.PlaybackStateChangedEvent += HandleStateChanged;

			// Listen for seek events from the timerule
			Config.EventsBroker.SeekEvent += Seek;
			Config.EventsBroker.TogglePlayEvent += HandleTogglePlayEvent;
			Config.EventsBroker.KeyPressed += HandleKeyPressed;
			// Handle dragging of periods
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
			scrolledwindow2.SizeAllocated += (o, args) => {
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
			Config.EventsBroker.SeekEvent -= Seek;

			main_cam_playerbin.Destroy ();
			sec_cam_playerbin.Destroy ();

			timerule.Dispose ();
			camerasLabels.Dispose ();
			camerasTimeline.Dispose ();

			base.OnDestroyed ();
		}

		public void Pause ()
		{
			if (main_cam_playerbin.Player.Opened) {
				main_cam_playerbin.Player.Pause ();
			}
			if (sec_cam_playerbin.Player.Opened) {
				sec_cam_playerbin.Player.Pause ();
			}
		}

		public void Seek (Time time, bool accurate)
		{
			if (main_cam_playerbin.Player.Opened) {
				main_cam_playerbin.Player.Seek (time, accurate);
			}
			if (sec_cam_playerbin.Player.Opened) {
				sec_cam_playerbin.Player.Seek (time, accurate);
			}
		}

		public void SaveChanges (bool resyncEvents)
		{
			foreach (TimelineEvent evt in project.Timeline) {
				int cc = evt.CamerasVisible.Count;
				int fc = project.Description.FileSet.Count;

				if (cc < fc) {
					for (int i = cc; i < fc; i++) {
						evt.CamerasVisible.Add (i);
					}
				}
			}

			if (!resyncEvents)
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
				start = new Time (0);
				// FIXME: What should we do if the fileset is empty ?
				file = fileSet.FirstOrDefault ();
				duration = file.Duration;
				pDuration = new Time (duration.MSeconds / gamePeriods.Count);
				periodsDict = new Dictionary <Period, Period> ();
				// If no periods are provided create from dashboard
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
				} else {
					foreach (Period p in project.Periods) {
						Period newp = new Period { Name = p.Name };
						newp.Nodes.Add (p.PeriodNode);
						periodsDict.Add (p, newp);
					}
					periods = periodsDict.Values.ToList ();
				}

				camerasLabels.Load (fileSet);
				camerasTimeline.Load (periods, fileSet, duration);

				UpdateMaxSecondsPerPixel ();
				UpdateTimeLineSize (fileSet);

				timerule.Duration = duration;

				// Open media file
				main_cam_label.Text = fileSet.First ().Name;
				main_cam_playerbin.Player.Open (fileSet);

				if (fileSet.Count > 1) {
					// Start with initial didactic message
					ShowDidactic (DidacticMessage.Initial);
					// Connect secondary camera event handlers
					camerasTimeline.CameraDragged += HandleCameraDragged;
					camerasTimeline.SelectedCameraChanged += HandleSelectedCameraChanged;
				} else {
					// Disconnect secondary camera event handlers
					camerasTimeline.CameraDragged -= HandleCameraDragged;
					camerasTimeline.SelectedCameraChanged -= HandleSelectedCameraChanged;
					// Just in case it was previously visible, a mediafile might still be loaded if 
					// the user is going back and forth adding/removing files to the set.
					HideSecondaryPlayer ();
					HideDidactic ();
				}
			}
		}

		/// <summary>
		/// Calculates the maximum number of seconds per pixel to accomodate the complete duration in available space.
		/// </summary>
		void UpdateMaxSecondsPerPixel ()
		{
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
			int visibleItems = Math.Min (StyleConf.TimelineCameraMaxLines, fileSet.Count + 1);
			int height = scrolledwindow2.HScrollbar.Requisition.Height * 2;
			height += visibleItems * StyleConf.TimelineCameraHeight;
			vpaned2.Position = vpaned2.Allocation.Height - height;
		}

		/// <summary>
		/// Handles the tick from media player to update Current Time in timelines.
		/// </summary>
		/// <param name="currentTime">Current time.</param>
		void HandleTick (Time currentTime, Time duration, bool seekable)
		{
			timerule.CurrentTime = currentTime;
			camerasTimeline.CurrentTime = currentTime;

			CameraObject camera = camerasTimeline.SelectedCamera;
			// Detect when secondary camera goes in and out of scope while main camera is playing.
			if (camera != null) {
				if (IsInScope (camera)) {
					if (ShowSecondaryPlayer ()) {
						// If the player was shown, resync.
						SyncSecondaryPlayer ();
					}
				} else {
					ShowDidactic (DidacticMessage.CameraOutOfScope);
				}
			}
			QueueDraw ();
		}

		/// <summary>
		/// Try to slave the secondary player to the first 
		/// </summary>
		/// <param name="playing">If set to <c>true</c> playing.</param>
		void HandleStateChanged (object sender, bool playing)
		{
			if (playing) {
				if (sec_cam_playerbin.Player.Opened) {
					sec_cam_playerbin.Player.Play ();
				}
			} else {
				if (sec_cam_playerbin.Player.Opened) {
					sec_cam_playerbin.Player.Pause ();
				}
			}
		}

		void HandleKeyPressed (object sender, HotKey key)
		{
			KeyAction action;

			try {
				action = Config.Hotkeys.ActionsHotkeys.GetKeyByValue (key);
			} catch (Exception ex) {
				/* The dictionary contains 2 equal values for different keys */
				Log.Exception (ex);
				return;
			}

			if (action == KeyAction.None) {
				return;
			}

			switch (action) {
			case KeyAction.FrameUp:
			case KeyAction.FrameDown:
				CameraObject camera = camerasTimeline.SelectedCamera;
				if (camera != null) {
					Pause ();
					Time before = sec_cam_playerbin.Player.CurrentTime;
					if (action == KeyAction.FrameUp)
						sec_cam_playerbin.Player.SeekToNextFrame ();
					else
						sec_cam_playerbin.Player.SeekToPreviousFrame ();
					Time diff = sec_cam_playerbin.Player.CurrentTime - before;

					// Reflect change in offset
					camera.MediaFile.Offset += diff.MSeconds;
					UpdateLabels ();
					// TODO: Reflect the change in the timeline position without triggering a seek.
				}
				return;
			}
		}

		/// <summary>
		/// Configure players' audio volume based on toggle buttons.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		void HandleAudioToggled (object sender, EventArgs args)
		{
			if (sender == main_cam_audio_button) {
				main_cam_playerbin.Player.Volume = main_cam_audio_button.Active ? 1 : 0;
				main_cam_audio_button_image.Pixbuf = Helpers.Misc.LoadIcon (main_cam_audio_button.Active ?
					"longomatch-control-volume-hi" : "longomatch-control-volume-off", IconSize.Button);
			} else if (sender == sec_cam_audio_button) {
				sec_cam_playerbin.Player.Volume = sec_cam_audio_button.Active ? 1 : 0;
				sec_cam_audio_button_image.Pixbuf = Helpers.Misc.LoadIcon (sec_cam_audio_button.Active ?
					"longomatch-control-volume-hi" : "longomatch-control-volume-off", IconSize.Button);
			}
		}

		/// <summary>
		/// Hides the secondary player, pausing it if necessary.
		/// </summary>
		void HideSecondaryPlayer ()
		{
			if (sec_cam_playerbin.Player.Opened && sec_cam_playerbin.Player.Playing) {
				sec_cam_playerbin.Player.Pause ();
			}
			sec_cam_vbox.Hide ();
		}

		/// <summary>
		/// Shows the secondary player, hiding didactic message.
		/// </summary>
		/// <returns><c>true</c>, if secondary player was shown, <c>false</c> otherwise.</returns>
		bool ShowSecondaryPlayer ()
		{
			// Only show secondary player if we have a camera selected
			if (!sec_cam_vbox.Visible && sec_cam_playerbin.Player.Opened) {
				HideDidactic ();
				sec_cam_vbox.Show ();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Synchronises the secondary player position and playing state with the main one.
		/// </summary>
		/// <returns><c>true</c>, if secondary player was synced, <c>false</c> otherwise.</returns>
		bool SyncSecondaryPlayer ()
		{
			if (main_cam_playerbin.Player.Opened && sec_cam_playerbin.Player.Opened) {
				sec_cam_playerbin.Player.Seek (main_cam_playerbin.Player.CurrentTime, true);
				if (main_cam_playerbin.Player.Playing) {
					sec_cam_playerbin.Player.Play ();
				}
				return true;
			}
			return false;
		}

		void HideDidactic ()
		{
			sec_cam_didactic_label.Hide ();
		}

		/// <summary>
		/// Handles the case where the secondary video gets out of scope compared to current time of main video.
		/// </summary>
		void ShowDidactic (DidacticMessage message)
		{
			// Show didactic message, hide secondary player
			HideSecondaryPlayer ();
			switch (message) {
			case DidacticMessage.Initial:
				sec_cam_didactic_label.Text = Catalog.GetString ("Drag the bars in the timeline "
				+ "to synchronize secondary video files with the main video");
				break;
			case DidacticMessage.CameraOutOfScope:
				sec_cam_didactic_label.Text = Catalog.GetString ("Camera out of scope");
				break;
			}
			sec_cam_didactic_label.Show ();
		}

		/// <summary>
		/// Determines whether the provided camera object is in scope of the current time.
		/// </summary>
		/// <returns><c>true</c> if this camera is in scope of the current time; otherwise, <c>false</c>.</returns>
		/// <param name="camera">Camera.</param>
		bool IsInScope (CameraObject camera)
		{
			if (camera == null) {
				return false;
			}
			if (camera.TimeNode.Start <= timerule.CurrentTime &&
			    timerule.CurrentTime <= camera.TimeNode.Stop) {
				return true;
			} else {
				return false;
			}
		}

		void UpdateLabels ()
		{
			CameraObject camera = camerasTimeline.SelectedCamera;

			if (camera != null) {
				sec_cam_label.Markup = String.Format (
					"<b>{0}</b> - <span foreground=\"{1}\" size=\"smaller\">{2}: {3}</span>",
					camera.MediaFile.Name, Config.Style.PaletteActive.ToRGBString (false),
					Catalog.GetString ("Offset"), camera.MediaFile.Offset.ToMSecondsString ());
			}
		}

		void HandleCameraUpdate (CameraObject camera)
		{
			UpdateLabels ();
			// If we are in scope, show player. Didactic message otherwise
			if (IsInScope (camera)) {
				ShowSecondaryPlayer ();
				// And resync
				SyncSecondaryPlayer ();
			} else {
				ShowDidactic (DidacticMessage.CameraOutOfScope);
			}
		}

		void HandleSelectedCameraChanged (object sender, EventArgs args)
		{
			CameraObject camera = camerasTimeline.SelectedCamera;
			if (camera != null) {
				// Check if we need to reopen the player
				if (!sec_cam_playerbin.Player.Opened ||
				    sec_cam_playerbin.Player.FileSet.FirstOrDefault () != camera.MediaFile) {
					MediaFileSet fileSet = new MediaFileSet ();
					fileSet.Add (camera.MediaFile);

					sec_cam_playerbin.Player.Open (fileSet);

					// Configure audio
					HandleAudioToggled (sec_cam_audio_button, new EventArgs ());
				}
				// And update
				HandleCameraUpdate (camera);
			} else {
				// When no camera is selected show the initial didactic message.
				ShowDidactic (DidacticMessage.Initial);
			}
		}

		void HandleCameraDragged (MediaFile mediafile, TimeNode timenode)
		{
			// Start by pausing players
			Pause ();
			// And update
			HandleCameraUpdate (camerasTimeline.SelectedCamera);
		}

		/// <summary>
		/// Periods segments have moved, adjust main camera position to segment boundaries
		/// </summary>
		void HandleTimeNodeChanged (TimeNode tNode, object val)
		{
			Time time = val as Time;

			Pause ();
			// Don't try to be accurate here. We are looking for period starts
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

		void HandleShowTimerMenuEvent (Timer timer, Time time)
		{
			menu.ShowMenu (project, timer, time, camerasTimeline.PeriodsTimeline);
		}

		void HandleTogglePlayEvent (bool playing)
		{
			if (playing) {
				main_cam_playerbin.Player.Play ();
			} else {
				Pause ();
			}
		}

	}
}

