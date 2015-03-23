// PlayerBin.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using System.Linq;
using Gdk;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Drawing.Cairo;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Multimedia.Utils;
using LongoMatch.Services;
using Mono.Unix;
using Pango;
using Image = LongoMatch.Core.Common.Image;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]

	public partial class PlayerBin : Gtk.Bin
	{
		const int SCALE_FPS = 25;
		IPlayerController player;
		bool seeking, IsPlayingPrevState, muted, ignoreRate, ignoreVolume;
		double previousVLevel = 1;
		protected VolumeWindow vwin;
		Blackboard blackboard;
		PlayerViewOperationMode mode;

		#region Constructors

		public PlayerBin ()
		{
			this.Build ();

			closebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-cancel-rec",
				StyleConf.PlayerCapturerIconSize);
			drawbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-draw",
				StyleConf.PlayerCapturerIconSize);
			playbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-play",
				StyleConf.PlayerCapturerIconSize);
			pausebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-pause",
				StyleConf.PlayerCapturerIconSize);
			prevbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-rw",
				StyleConf.PlayerCapturerIconSize);
			nextbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-ff",
				StyleConf.PlayerCapturerIconSize);
			volumebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-volume-hi",
				StyleConf.PlayerCapturerIconSize);
			detachbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-detach",
				StyleConf.PlayerCapturerIconSize);

			// Force tooltips to be translatable as there seems to be a bug in stetic 
			// code generation for translatable tooltips.
			vscale1.TooltipMarkup = Catalog.GetString ("Playback speed");
			closebutton.TooltipMarkup = Catalog.GetString ("Close loaded event");
			drawbutton.TooltipMarkup = Catalog.GetString ("Draw frame");
			playbutton.TooltipMarkup = Catalog.GetString ("Play");
			pausebutton.TooltipMarkup = Catalog.GetString ("Pause");
			prevbutton.TooltipMarkup = Catalog.GetString ("Previous");
			nextbutton.TooltipMarkup = Catalog.GetString ("Next");
			jumpspinbutton.TooltipMarkup = Catalog.GetString ("Jump in seconds. Hold the Shift key with the direction keys to activate it.");
			volumebutton.TooltipMarkup = Catalog.GetString ("Volume");
			detachbutton.TooltipMarkup = Catalog.GetString ("Detach window");

			vwin = new VolumeWindow ();
			ConnectSignals ();
			blackboard = new Blackboard (new WidgetWrapper (blackboarddrawingarea));
			vbox3.NoShowAll = true;
			timescale.Adjustment.PageIncrement = 0.01;
			timescale.Adjustment.StepIncrement = 0.0001;
			LongoMatch.Gui.Helpers.Misc.SetFocus (vbox3, false);
			videowindow.CanFocus = true;
			detachbutton.Clicked += (sender, e) => Config.EventsBroker.EmitDetach ();
			vscale1.ModifyFont (FontDescription.FromString (Config.Style.Font + " 8"));
			controlsbox.HeightRequest = StyleConf.PlayerCapturerControlsHeight;

			Player = new PlayerController ();
			Player.CamerasVisible = new List<int> { 0 };
			Mode = PlayerViewOperationMode.Analysis;
			CreateWindows ();
			ResetGui ();
		}

		#endregion

		protected override void OnDestroyed ()
		{
			blackboard.Dispose ();
			player.Dispose ();
			base.OnDestroyed ();
		}

		#region Properties

		public IPlayerController Player {
			get {
				return player;
			}
			set {
				player = value;
				player.ElementLoadedEvent += HandleElementLoadedEvent;
				player.LoadDrawingsEvent += HandleLoadDrawingsEvent;
				player.PARChangedEvent += HandlePARChangedEvent;
				player.PlaybackRateChangedEvent += HandlePlaybackRateChangedEvent;
				player.PlaybackStateChangedEvent += HandlePlaybackStateChangedEvent;
				player.TimeChangedEvent += HandleTimeChangedEvent;
				player.VolumeChangedEvent += HandleVolumeChangedEvent;
			}
		}

		public PlayerViewOperationMode Mode {
			set {
				mode = value;
				switch (mode) {
				case PlayerViewOperationMode.Analysis:
					ShowControls = true;
					Compact = false;
					break;
				case PlayerViewOperationMode.LiveAnalysisReview:
					ShowControls = true;
					Compact = true;
					break;
				case PlayerViewOperationMode.Synchronization:
					ShowControls = false;
					Compact = false;
					break;
				}
			}
			get {
				return mode;
			}
		}

		public object CamerasLayout {
			get {
				return 0;
			}
			set {
			}
		}

		public List<int> CamerasVisible {
			get {
				return new List<int> { 0 };
			}
			set {
			}
		}

		#endregion

		#region Private methods

		bool ControlsSensitive {
			set {
				controlsbox.Sensitive = value;
				vscale1.Sensitive = value;
			}
		}

		bool ShowControls {
			set {
				controlsbox.Visible = value;
				vscale1.Visible = value;
			}
		}

		bool Compact {
			set {
				prevbutton.Visible = nextbutton.Visible = jumplabel.Visible =
					jumpspinbutton.Visible = tlabel.Visible = timelabel.Visible =
						detachbutton.Visible = vscale1.Visible = !value;
			}
		}

		bool DrawingsVisible {
			set {
				videowindow.Visible = !value;
				blackboarddrawingarea.Visible = value;
			}
		}

		void ResetGui ()
		{
			if (mode != PlayerViewOperationMode.LiveAnalysisReview) {
				closebutton.Visible = false;
			}
			ControlsSensitive = true;
			DrawingsVisible = false;
			timescale.Value = 0;
			timelabel.Text = "";
			seeking = false;
			IsPlayingPrevState = false;
			muted = false;
			ignoreRate = false;
			ignoreVolume = false;
			videowindow.Visible = true;
		}

		void ConnectSignals ()
		{
			vwin.VolumeChanged += new VolumeChangedHandler (OnVolumeChanged);
			closebutton.Clicked += OnClosebuttonClicked;
			prevbutton.Clicked += OnPrevbuttonClicked;
			nextbutton.Clicked += OnNextbuttonClicked;
			playbutton.Clicked += OnPlaybuttonClicked;
			pausebutton.Clicked += OnPausebuttonClicked;
			drawbutton.Clicked += OnDrawButtonClicked;
			volumebutton.Clicked += OnVolumebuttonClicked;
			timescale.ValueChanged += OnTimescaleValueChanged;
			timescale.ButtonPressEvent += OnTimescaleButtonPress;
			timescale.ButtonReleaseEvent += OnTimescaleButtonRelease;
			vscale1.FormatValue += OnVscale1FormatValue;
			vscale1.ValueChanged += OnVscale1ValueChanged;

		}

		void LoadImage (Image image, FrameDrawing drawing)
		{
			if (image == null) {
				DrawingsVisible = false;
				return;
			}
			blackboard.Background = image;
			blackboard.Drawing = drawing;
			DrawingsVisible = true;
			blackboarddrawingarea.QueueDraw ();
		}

		float GetRateFromScale ()
		{
			VScale scale = vscale1;
			double val = scale.Value;

			if (val > SCALE_FPS) {
				val = val + 1 - SCALE_FPS;
			} else if (val <= SCALE_FPS) {
				val = val / SCALE_FPS;
			}
			return (float)val;
		}

		void CreateWindows ()
		{
			videowindow.ButtonPressEvent += OnVideoboxButtonPressEvent;
			videowindow.ScrollEvent += OnVideoboxScrollEvent;
			videowindow.ReadyEvent += HandleReady;
			videowindow.ExposeEvent += HandleExposeEvent;
			videowindow.CanFocus = true;
		}

		void SetVolumeIcon (string name)
		{
			volumebuttonimage.Pixbuf = Helpers.Misc.LoadIcon (name, IconSize.Button, 0);
		}

		#endregion

		#region ControllerCallbacks

		void HandleVolumeChangedEvent (double level)
		{

		}

		void HandleTimeChangedEvent (Time currentTime, Time duration, bool seekable)
		{
			timelabel.Text = currentTime.ToMSecondsString (true) + "/" + duration.ToMSecondsString ();
			if (duration.MSeconds == 0) {
				timescale.Value = 0;
			} else {
				timescale.Value = (double)currentTime.MSeconds / duration.MSeconds;
			}
			timescale.Sensitive = seekable;
		}

		void HandlePlaybackStateChangedEvent (bool playing)
		{
			if (playing) {
				playbutton.Hide ();
				pausebutton.Show ();
			} else {
				playbutton.Show ();
				pausebutton.Hide ();
			}
		}

		void HandlePlaybackRateChangedEvent (float rate)
		{
			ignoreRate = true;
			if (rate > 1) {
				vscale1.Value = rate - 1 + SCALE_FPS;
			} else {
				vscale1.Value = rate * SCALE_FPS;
			}
			ignoreRate = true;
		}

		void HandlePARChangedEvent (IntPtr windowHandle, float par)
		{
			videowindow.Ratio = par;
		}

		void HandleLoadDrawingsEvent (FrameDrawing frameDrawing)
		{
			if (frameDrawing != null) {
				LoadImage (Player.CurrentFrame, frameDrawing);
			} else {
				DrawingsVisible = false;
			}
		}

		void HandleElementLoadedEvent (object element, bool hasNext)
		{
			if (element == null) {
				if (Mode != PlayerViewOperationMode.LiveAnalysisReview) {
					closebutton.Visible = false;
				}
			} else {
				nextbutton.Sensitive = hasNext;
				closebutton.Visible = true;
				if (element is PlaylistDrawing) {
					PlaylistDrawing drawing = element as PlaylistDrawing;
					LoadImage (null, drawing.Drawing);
				} else if (element is PlaylistImage) {
					PlaylistImage image = element as PlaylistImage;
					LoadImage (image.Image, null);
				}
			}
		}

		#endregion

		#region UI Callbacks

		void HandleExposeEvent (object sender, ExposeEventArgs args)
		{
			Player.Expose ();
			/* The player draws over the eventbox when it's resized
			 * so make sure that we queue a draw in the event box after
			 * the expose */
			lightbackgroundeventbox.QueueDraw ();
		}

		[GLib.ConnectBefore]
		void OnTimescaleButtonPress (object o, Gtk.ButtonPressEventArgs args)
		{
			if (args.Event.Button == 1) {
				GtkGlue.EventButtonSetButton (args.Event, 2);
			} else {
				GtkGlue.EventButtonSetButton (args.Event, 1);
			}

			if (!seeking) {
				seeking = true;
				IsPlayingPrevState = Player.Playing;
				Player.IgnoreTicks = true;
				Player.Pause ();
			}
		}

		[GLib.ConnectBefore]
		void OnTimescaleButtonRelease (object o, Gtk.ButtonReleaseEventArgs args)
		{
			if (args.Event.Button == 1) {
				GtkGlue.EventButtonSetButton (args.Event, 2);
			} else {
				GtkGlue.EventButtonSetButton (args.Event, 1);
			}

			if (seeking) {
				seeking = false;
				Player.IgnoreTicks = false;
				if (IsPlayingPrevState)
					Player.Play ();
			}
		}

		void OnTimescaleValueChanged (object sender, System.EventArgs e)
		{
			if (seeking) {
				Player.SeekRelative (timescale.Value);
			}
		}

		void OnPlaybuttonClicked (object sender, System.EventArgs e)
		{
			Player.Play ();
		}

		void OnVolumebuttonClicked (object sender, System.EventArgs e)
		{
			vwin.SetLevel (Player.Volume);
			vwin.Show ();
		}

		void OnVolumeChanged (double level)
		{
			double prevLevel;

			prevLevel = Player.Volume;
			if (prevLevel > 0 && level == 0) {
				SetVolumeIcon ("longomatch-control-volume-off");
			} else if (prevLevel > 0.5 && level <= 0.5) {
				SetVolumeIcon ("longomatch-control-volume-low");
			} else if (prevLevel <= 0.5 && level > 0.5) {
				SetVolumeIcon ("longomatch-control-volume-med");
			} else if (prevLevel < 1 && level == 1) {
				SetVolumeIcon ("longomatch-control-volume-hi");
			}
			Player.Volume = level;
			if (level == 0)
				muted = true;
			else
				muted = false;
		}

		void OnPausebuttonClicked (object sender, System.EventArgs e)
		{
			Player.Pause ();
		}

		void OnClosebuttonClicked (object sender, System.EventArgs e)
		{
			Player.UnloadCurrentEvent ();
		}

		void OnPrevbuttonClicked (object sender, System.EventArgs e)
		{
			Player.Previous ();
		}

		void OnNextbuttonClicked (object sender, System.EventArgs e)
		{
			Player.Next ();
		}

		void OnVscale1FormatValue (object o, Gtk.FormatValueArgs args)
		{
			double val = args.Value;
			if (val >= SCALE_FPS) {
				val = val + 1 - SCALE_FPS;
				args.RetVal = val + "X";
			} else if (val < SCALE_FPS) {
				args.RetVal = val + "/" + SCALE_FPS + "X";
			}
		}

		void OnVscale1ValueChanged (object sender, System.EventArgs e)
		{
			float val = GetRateFromScale ();

			// Mute for rate != 1
			if (val != 1 && Player.Volume != 0) {
				previousVLevel = Player.Volume;
				Player.Volume = 0;
			} else if (val != 1 && muted)
				previousVLevel = 0;
			else if (val == 1)
				Player.Volume = previousVLevel;

			if (!ignoreRate) {
				Player.Rate = val;
				Config.EventsBroker.EmitPlaybackRateChanged (val);
			}
		}

		void OnVideoboxButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			/* FIXME: The pointer is grabbed when the event box is clicked.
			 * Make sure to ungrab it in order to avoid clicks outisde the window
			 * triggering this callback. This should be fixed properly.*/
			Pointer.Ungrab (Gtk.Global.CurrentEventTime);
			Player.TogglePlay ();
		}

		void OnVideoboxScrollEvent (object o, Gtk.ScrollEventArgs args)
		{
			switch (args.Event.Direction) {
			case ScrollDirection.Down:
				Player.SeekToPreviousFrame ();
				break;
			case ScrollDirection.Up:
				Player.SeekToNextFrame ();
				break;
			case ScrollDirection.Left:
				Player.StepBackward ();
				break;
			case ScrollDirection.Right:
				Player.StepForward ();
				break;
			}
		}

		void OnDrawButtonClicked (object sender, System.EventArgs e)
		{
			Config.EventsBroker.EmitDrawFrame (null, -1, CamerasVisible [0], true);
		}

		void HandleReady (object sender, EventArgs e)
		{
			IntPtr handle = WindowHandle.GetWindowHandle (videowindow.Window.GdkWindow);
			Player.WindowHandle = handle;
			Player.Ready ();
		}

		#endregion
	}
}
