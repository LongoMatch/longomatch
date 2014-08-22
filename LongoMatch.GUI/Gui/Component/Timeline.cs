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
using LongoMatch.Drawing.Widgets;
using LongoMatch.Store;
using LongoMatch.Handlers;
using LongoMatch.Common;
using System.Collections.Generic;
using LongoMatch.Interfaces;
using LongoMatch.Drawing.Cairo;
using LongoMatch.Drawing;
using Gtk;
using Mono.Unix;
using LongoMatch.Gui.Menus;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class Timeline : Gtk.Bin
	{
		const uint TIMEOUT_MS = 100;
		
		PlaysTimeline timeline;
		Timerule timerule;
		CategoriesLabels labels;
		MediaFile projectFile;
		double secondsPerPixel;
		uint timeoutID;
		Time currentTime, nextCurrentTime;
		PlaysMenu menu;
		Project project;

		public Timeline ()
		{
			this.Build ();
			this.timerule = new Timerule (new WidgetWrapper (timerulearea));
			this.timeline = new PlaysTimeline (new WidgetWrapper(timelinearea));
			this.labels = new CategoriesLabels (new WidgetWrapper (labelsarea));
			focusbutton.CanFocus = false;
			focusbutton.Clicked += HandleFocusClicked;
			focusscale.CanFocus = false;
			focusscale.Adjustment.Lower = 0;
			focusscale.Adjustment.Upper = 12;
			focusscale.ValueChanged += HandleValueChanged;
			timerulearea.HeightRequest = LongoMatch.Drawing.Constants.TIMERULE_HEIGHT;
			labelsarea.WidthRequest = StyleConf.TimelineCategoryHeight;
			hbox1.HeightRequest = LongoMatch.Drawing.Constants.TIMERULE_HEIGHT;
			scrolledwindow1.Vadjustment.ValueChanged += HandleScrollEvent;
			scrolledwindow1.Hadjustment.ValueChanged += HandleScrollEvent;
			timeoutID = 0;
			menu = new PlaysMenu ();
		}
		
		protected override void OnDestroyed ()
		{
			timerule.Dispose ();
			timeline.Dispose ();
			labels.Dispose ();
			base.OnDestroyed ();
		}
		
		public TimeNode SelectedTimeNode {
			set {
			}
		}
		
		public Time CurrentTime {
			set {
				nextCurrentTime = value;
			}
			protected get {
				return currentTime;
			}
		}
		
		public void SetProject (Project project, PlaysFilter filter) {
			this.project = project;
			timeline.LoadProject (project, filter);
			labels.LoadProject (project, filter);

			if(project == null) {
				if (timeoutID != 0) {
					GLib.Source.Remove (timeoutID);
					timeoutID = 0;
				}
				return;
			}
			
			if (timeoutID == 0) {
				timeoutID = GLib.Timeout.Add (TIMEOUT_MS, UpdateTime);
			}
			focusscale.Value = 6;
			projectFile = project.Description.File;
			timerule.Duration = project.Description.File.Duration;
			timeline.ShowMenuEvent += HandleShowMenu;
			QueueDraw ();
		}

		public void AddPlay(Play play) {
			timeline.AddPlay (play);
			QueueDraw ();
		}

		public void RemovePlays(List<Play> plays) {
			timeline.RemovePlays (plays);
			QueueDraw ();
		}
		
		bool UpdateTime () {
			if (nextCurrentTime != currentTime) {
				currentTime = nextCurrentTime;
				timeline.CurrentTime = currentTime;
				timerule.CurrentTime = currentTime;
				QueueDraw ();
			}
			return true;
		}
		
		void HandleScrollEvent(object sender, System.EventArgs args)
		{
			if(sender == scrolledwindow1.Vadjustment)
				labels.Scroll = scrolledwindow1.Vadjustment.Value;
			else if(sender == scrolledwindow1.Hadjustment)
				timerule.Scroll = scrolledwindow1.Hadjustment.Value;
			QueueDraw ();
		}

		void HandleFocusClicked (object sender, EventArgs e)
		{
			double pos = CurrentTime.Seconds / secondsPerPixel;
			double maxPos = timelinearea.Allocation.Width - scrolledwindow1.Allocation.Width;
			
			pos = Math.Min (pos, maxPos);
			scrolledwindow1.Hadjustment.Value = pos;
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			double secondsPer100Pixels, value;
			
			value = Math.Round (focusscale.Value);
			if (value == 0) {
				secondsPer100Pixels = 1;
			} else if (value <= 6) {
				secondsPer100Pixels = value * 10;
			} else {
				secondsPer100Pixels = (value - 5) * 60;
			}

			secondsPerPixel = secondsPer100Pixels / 100 ;
			timerule.SecondsPerPixel = secondsPerPixel;
			timeline.SecondsPerPixel = secondsPerPixel;
			QueueDraw ();
		}
		
		void HandleShowMenu (List<Play> plays, Category cat, Time time)
		{
			menu.ShowTimelineMenu (project, plays, cat, time);
		}
	}
}

