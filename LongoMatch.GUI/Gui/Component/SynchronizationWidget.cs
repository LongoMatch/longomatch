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
using System.ComponentModel;
using Gtk;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Gui.Menus;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Resources.Styles;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Drawing.Cairo;
using VAS.Drawing.Widgets;
using VAS.Services.State;
using VAS.Services.ViewModel;
using Helpers = VAS.UI.Helpers;
using Timer = VAS.Core.Store.Timer;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class SynchronizationWidget : Gtk.Bin, IView<CameraSynchronizationVM>
	{
		CamerasLabelsView camerasLabels;
		CamerasTimelineView camerasTimeline;
		Timerule timerule;
		PeriodsMenu menu;
		double maxSecondsPerPixel;
		VideoPlayerVM videoPlayerVM;
		ProjectVM projectVM;
		CameraSynchronizationVM camSyncVM;
		IVideoPlayerView videoPlayerView;

		public SynchronizationWidget ()
		{
			this.Build ();

			zoomscale.CanFocus = false;
			zoomscale.Adjustment.Lower = 0;
			zoomscale.Adjustment.Upper = 100;

			zoomoutimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-zoom-out", 14);
			zoominimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-zoom-in", 14);

			timerule = new Timerule (new WidgetWrapper (timerulearea));
			timerule.AutoUpdate = true;
			camerasTimeline = new CamerasTimelineView (new WidgetWrapper (timelinearea));
			camerasLabels = new CamerasLabelsView (new WidgetWrapper (labelsarea));

			// Set some sane defaults
			labels_vbox.WidthRequest = Sizes.TimelineLabelsWidth;
			// We need to align the timerule and the beginning of labels list
			timerulearea.HeightRequest = Sizes.TimelineCameraHeight;

			menu = new PeriodsMenu ();

			videoPlayerView = App.Current.ViewLocator.Retrieve ("VideoPlayerView") as IVideoPlayerView;
			videoplayerbox.PackStart (videoPlayerView as Widget, true, true, 0);
			videoplayerbox.ShowAll ();

			Helpers.Misc.SetFocus (this, false);
			Bind ();
		}

		protected override void OnDestroyed ()
		{
			timerule.Dispose ();
			camerasLabels.Dispose ();
			camerasTimeline.Dispose ();
			base.OnDestroyed ();
		}

		public CameraSynchronizationVM ViewModel {
			get {
				return camSyncVM;
			}
			set {
				camSyncVM = value;
				camerasTimeline.ViewModel = camSyncVM;
				if (camSyncVM != null) {
					projectVM = camSyncVM.Project;
					videoPlayerVM = camSyncVM.VideoPlayer;

					videoPlayerVM.ViewMode = PlayerViewOperationMode.Synchronization;
					timerule.ViewModel = videoPlayerVM;
					camerasLabels.ViewModel = projectVM.FileSet;
					(videoPlayerView as IView<VideoPlayerVM>).SetViewModel (videoPlayerVM);
					UpdateMaxSecondsPerPixel ();
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (CameraSynchronizationVM)viewModel;
		}

		void Bind ()
		{
			zoomscale.ValueChanged += HandleZoomChanged;

			// Handle dragging of periods
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
			// Synchronize the zoom widget height with scrolledwindow's scrollbar's and
			// the whole timeline widget height.
			scrolledwindow2.HScrollbar.SizeAllocated += (object o, SizeAllocatedArgs args) => {
				int spacing = (int)scrolledwindow2.StyleGetProperty ("scrollbar-spacing");
				zoomhbox.HeightRequest = args.Allocation.Height + spacing;
				hbox5.HeightRequest = labelsarea.HeightRequest + args.Allocation.Height + spacing;
			};
		}

		/// <summary>
		/// Calculates the maximum number of seconds per pixel to accomodate the complete duration in available space.
		/// </summary>
		void UpdateMaxSecondsPerPixel ()
		{
			if (projectVM?.FileSet.Duration != null) {
				// With 20 pixels of margin to properly see the whole segment
				maxSecondsPerPixel = (double)projectVM.FileSet.Duration.TotalSeconds /
					(scrolledwindow2.Allocation.Width - 20);
				HandleZoomChanged (zoomscale, new EventArgs ());
			}
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

		void HandleShowTimerMenuEvent (Timer timer, Time time)
		{
			if (!ViewModel.FixedPeriods)
				menu.ShowMenu (projectVM.Model, timer, time, camerasTimeline);
		}

		void HandleZoomChanged (object sender, EventArgs e)
		{
			// We zoom from our Maximum number of seconds per pixel to the minimum using the 0 to 100 scale value
			double secondsPerPixel = 0, minSecondsPerPixel = 0.01;
			double diff = maxSecondsPerPixel - minSecondsPerPixel;

			secondsPerPixel = maxSecondsPerPixel - (diff * zoomscale.Value / 100);

			timerule.SecondsPerPixel = secondsPerPixel;
			camerasTimeline.SecondsPerPixel = secondsPerPixel;
			QueueDraw ();
		}
	}
}
