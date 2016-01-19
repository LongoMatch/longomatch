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
using System.Collections.Generic;
using System.Linq;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.Drawing.Cairo;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Gui.Menus;
using Mono.Unix;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class Timeline : Gtk.Bin
	{
		const uint TIMEOUT_MS = 100;
		PlaysTimeline timeline;
		Timerule timerule;
		TimelineLabels labels;
		double secondsPerPixel;
		uint timeoutID;
		Time currentTime, nextCurrentTime;
		PlaysMenu menu;
		Project project;
		PeriodsMenu periodsmenu;

		public Timeline ()
		{
			this.Build ();
			this.timerule = new Timerule (new WidgetWrapper (timerulearea));
			timerule.SeekEvent += HandleTimeruleSeek;
			this.timeline = new PlaysTimeline (new WidgetWrapper (timelinearea));
			this.labels = new TimelineLabels (new WidgetWrapper (labelsarea));

			focusbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-dash-center-view", Gtk.IconSize.Menu, 0);

			focusbutton.CanFocus = false;
			focusbutton.Clicked += HandleFocusClicked;
			focusscale.CanFocus = false;
			focusscale.Adjustment.Lower = 0;
			focusscale.Adjustment.Upper = 12;
			focusscale.ValueChanged += HandleValueChanged;
			focusscale.ButtonPressEvent += HandleFocusScaleButtonPress;
			focusscale.ButtonReleaseEvent += HandleFocusScaleButtonRelease;
			timerulearea.HeightRequest = LongoMatch.Drawing.Constants.TIMERULE_HEIGHT;
			leftbox.WidthRequest = StyleConf.TimelineLabelsWidth;
			labelsarea.SizeRequested += (o, args) => {
				leftbox.WidthRequest = args.Requisition.Width;
			};
			hbox1.HeightRequest = LongoMatch.Drawing.Constants.TIMERULE_HEIGHT;
			scrolledwindow1.Vadjustment.ValueChanged += HandleScrollEvent;
			scrolledwindow1.Hadjustment.ValueChanged += HandleScrollEvent;
			timeoutID = 0;

			zoominimage.Pixbuf = LongoMatch.Gui.Helpers.Misc.LoadIcon ("longomatch-zoom-in", 14);
			zoomoutimage.Pixbuf = LongoMatch.Gui.Helpers.Misc.LoadIcon ("longomatch-zoom-out", 14);

			// Synchronize the zoom widget height with scrolledwindow's scrollbar's.
			scrolledwindow1.HScrollbar.SizeAllocated += (object o, SizeAllocatedArgs args) => {
				int spacing = (int)scrolledwindow1.StyleGetProperty ("scrollbar-spacing");
				zoomhbox.HeightRequest = args.Allocation.Height + spacing;
			};

			menu = new PlaysMenu ();
			periodsmenu = new PeriodsMenu ();
		}

		protected override void OnDestroyed ()
		{
			if (timeoutID != 0) {
				GLib.Source.Remove (timeoutID);
				timeoutID = 0;
			}
			timerule.Dispose ();
			timeline.Dispose ();
			labels.Dispose ();
			base.OnDestroyed ();
		}

		public Time CurrentTime {
			set {
				nextCurrentTime = value;
			}
			protected get {
				return currentTime;
			}
		}

		public void Fit ()
		{
			focusbutton.Click ();
		}

		public void ZoomIn ()
		{
			focusscale.Adjustment.Value -= focusscale.Adjustment.StepIncrement;
		}

		public void ZoomOut ()
		{
			focusscale.Adjustment.Value += focusscale.Adjustment.StepIncrement;
		}

		public void SetProject (Project project, EventsFilter filter)
		{
			this.project = project;
			timeline.LoadProject (project, filter);
			labels.LoadProject (project, filter);

			if (project == null) {
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
			// Can throw an exception if there are no files in set
			timerule.Duration = project.Description.FileSet.First ().Duration;
			timeline.ShowMenuEvent += HandleShowMenu;
			timeline.ShowTimersMenuEvent += HandleShowTimersMenu;
			timeline.ShowTimerMenuEvent += HandleShowTimerMenuEvent;
			QueueDraw ();
		}

		public void LoadPlay (TimelineEvent evt)
		{
			timeline.LoadPlay (evt);
		}

		public void AddPlay (TimelineEvent play)
		{
			timeline.AddPlay (play);
			QueueDraw ();
		}

		public void RemovePlays (List<TimelineEvent> plays)
		{
			timeline.RemovePlays (plays);
			QueueDraw ();
		}

		public void AddTimerNode (Timer timer, TimeNode tn)
		{
			timeline.AddTimerNode (timer, tn);
		}

		bool UpdateTime ()
		{
			if (nextCurrentTime != currentTime) {
				currentTime = nextCurrentTime;
				timeline.CurrentTime = currentTime;
				timerule.CurrentTime = currentTime;
			}
			return true;
		}

		void HandleScrollEvent (object sender, System.EventArgs args)
		{
			if (sender == scrolledwindow1.Vadjustment)
				labels.Scroll = scrolledwindow1.Vadjustment.Value;
			else if (sender == scrolledwindow1.Hadjustment)
				timerule.Scroll = scrolledwindow1.Hadjustment.Value;
			QueueDraw ();
		}

		void HandleFocusClicked (object sender, EventArgs e)
		{
			// Align the position to 40% of the scrolled width
			double pos = CurrentTime.TotalSeconds / secondsPerPixel;
			pos -= 0.4 * scrolledwindow1.Allocation.Width;
			double maxPos = timelinearea.Allocation.Width - scrolledwindow1.Allocation.Width;
			
			pos = Math.Min (pos, maxPos);
			scrolledwindow1.Hadjustment.Value = pos;
		}

		[GLib.ConnectBefore]
		void HandleFocusScaleButtonPress (object o, Gtk.ButtonPressEventArgs args)
		{
			if (args.Event.Button == 1) {
				args.Event.SetButton (2);
			} else {
				args.Event.SetButton (1);
			}
		}

		[GLib.ConnectBefore]
		void HandleFocusScaleButtonRelease (object o, Gtk.ButtonReleaseEventArgs args)
		{
			if (args.Event.Button == 1) {
				args.Event.SetButton (2);
			} else {
				args.Event.SetButton (1);
			}
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

			secondsPerPixel = secondsPer100Pixels / 100;
			timerule.SecondsPerPixel = secondsPerPixel;
			timeline.SecondsPerPixel = secondsPerPixel;
			QueueDraw ();
		}

		void HandleShowMenu (List<TimelineEvent> plays, EventType eventType, Time time)
		{
			menu.ShowTimelineMenu (project, plays, eventType, time);
		}

		void HandleShowTimersMenu (List<TimeNode> nodes)
		{
			Menu m = new Menu ();
			MenuItem item = new MenuItem (Catalog.GetString ("Delete"));
			item.Activated += (object sender, EventArgs e) => {
				foreach (Timer t in project.Timers) {
					t.Nodes.RemoveAll (nodes.Contains);
				}
				timeline.RemoveTimers (nodes);
			};
			m.Add (item);
			m.ShowAll ();
			m.Popup ();
		}

		void HandleShowTimerMenuEvent (Timer timer, Time time)
		{
			periodsmenu.ShowMenu (project, timer, time, timeline.PeriodsTimeline, timeline);
		}

		void HandleTimeruleSeek (Time pos, bool accurate, bool synchronous = false, bool throttled = false)
		{
			Config.EventsBroker.EmitLoadEvent (null);
			Config.EventsBroker.EmitSeekEvent (pos, accurate, synchronous, throttled);
		}
	}
}

