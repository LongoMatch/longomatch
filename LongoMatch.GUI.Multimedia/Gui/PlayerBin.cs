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
using System.Linq;
using Gdk;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Drawing.Cairo;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Multimedia.Utils;
using LongoMatch.Video.Common;
using LongoMatch.Video.Utils;
using Image = LongoMatch.Core.Common.Image;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]

	public partial class PlayerBin : Gtk.Bin, IPlayerBin
	{
		struct Segment
		{
			public Time Start;
			public Time Stop;
		}

		public event TickHandler Tick;
		public event StateChangeHandler PlayStateChanged;

		const int THUMBNAIL_MAX_WIDTH = 100;
		const int SCALE_FPS = 25;
		IPlayer player;
		TimelineEvent loadedPlay;
		IPlaylistElement loadedPlaylistElement;
		Playlist loadedPlaylist;
		Time length, lastTime;
		bool seeking, IsPlayingPrevState, muted, emitRateScale, readyToSeek;
		bool ignoreTick, stillimageLoaded, delayedOpen;
		MediaFile file;
		double previousVLevel = 1;
		double[] seeksQueue;
		object[] pendingSeek;
		//{seekTime, rate, playing}
		protected VolumeWindow vwin;
		Seeker seeker;
		Segment segment;
		Blackboard blackboard;
		uint timeout;

		#region Constructors
		public PlayerBin ()
		{
			this.Build ();

			closebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-back", IconSize.Button, 0);
			drawbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-draw", IconSize.Button, 0);
			playbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-play", IconSize.Button, 0);
			pausebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-pause", IconSize.Button, 0);
			prevbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-rw", IconSize.Button, 0);
			nextbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-ff", IconSize.Button, 0);
			volumebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-volume-hi", IconSize.Button, 0);
			detachbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-control-detach", IconSize.Button, 0);

			vwin = new VolumeWindow ();
			ConnectSignals ();
			blackboard = new Blackboard (new WidgetWrapper (blackboarddrawingarea));
			controlsbox.Visible = false;
			UnSensitive ();
			timescale.Adjustment.PageIncrement = 0.01;
			timescale.Adjustment.StepIncrement = 0.0001;
			LongoMatch.Gui.Helpers.Misc.DisableFocus (vbox3);
			videowindow.CanFocus = true;
			seeksQueue = new double[2];
			seeksQueue [0] = -1;
			seeksQueue [1] = -1;
			detachbutton.Clicked += (sender, e) => Config.EventsBroker.EmitDetach ();
			seeker = new Seeker ();
			seeker.SeekEvent += HandleSeekEvent;
			segment.Start = new Time (-1);
			segment.Stop = new Time (int.MaxValue);
			lastTime = new Time (0);
			length = new Time (0);
			
			CreatePlayer ();
		}
		#endregion
		protected override void OnDestroyed ()
		{
			Close ();
			base.OnDestroyed ();
		}
		#region Properties
		public Time CurrentTime {
			get {
				return player.CurrentTime;
			}
		}

		public Time StreamLength {
			get {
				return player.StreamLength;
			}
		}

		public bool SeekingEnabled {
			set {
				timescale.Sensitive = value;
			}
		}

		public bool FullScreen {
			set {
				if (value)
					GdkWindow.Fullscreen ();
				else
					GdkWindow.Unfullscreen ();
			}
		}

		public IFramesCapturer FramesCapturer {
			get;
			set;
		}
		
		public Image CurrentMiniatureFrame {
			get {
				return player.GetCurrentFrame (THUMBNAIL_MAX_WIDTH, THUMBNAIL_MAX_WIDTH);
			}
		}

		public Image CurrentFrame {
			get {
				return player.GetCurrentFrame ();
			}
		}

		public bool Opened {
			get {
				return file != null;
			}
		}

		public Widget VideoWidget {
			get {
				return ((Gtk.EventBox)player);
			}
		}

		public bool ShowControls {
			set {
				controlsbox.Visible = value;
				vscale1.Visible = value;
			}
		}
		
		public bool Playing {
			get {
				return player != null ? player.Playing : false;
			}
		}
		
		#endregion
		#region Public methods
		public void Open (MediaFile file)
		{
			if (videowindow.Ready) {
				Open (file, true);
			} else {
				this.file = file;
				delayedOpen = true;
			}
		}

		public void Play ()
		{
			DrawingsVisible = false;
			player.Play ();
		}

		public void Pause ()
		{
			player.Pause ();
		}

		public void TogglePlay ()
		{
			if (player.Playing)
				Pause ();
			else
				Play ();
		}

		public void ResetGui ()
		{
			closebutton.Hide ();
			SetSensitive ();
			timescale.Value = 0;
			timelabel.Text = "";
			SeekingEnabled = true;
			seeking = false;
			IsPlayingPrevState = false;
			muted = false;
			emitRateScale = true;
			videowindow.Visible = true;
			blackboarddrawingarea.Visible = false;
		}

		public void LoadPlayListPlay (Playlist playlist, IPlaylistElement element)
		{
			if (playlist.HasNext ())
				nextbutton.Sensitive = true;
			else
				nextbutton.Sensitive = false;

			loadedPlay = null;
			loadedPlaylist = playlist;
			loadedPlaylistElement = element;

			if (element is PlaylistPlayElement) {
				PlaylistPlayElement ple = element as PlaylistPlayElement;
				TimelineEvent play = ple.Play;
				LoadSegment (ple.File, play.Start, play.Stop, play.Start, true, play.Rate);
			} else if (element is PlaylistImage) {
				//LoadStillImage (element as PlaylistImage);
			} else if (element is PlaylistDrawing) {
				//LoadFrameDrawing (element as PlaylistDrawing);
			}
		}

		public void LoadPlay (MediaFile file, TimelineEvent play, Time seekTime, bool playing)
		{
			loadedPlaylist = null;
			loadedPlaylistElement = null;
			loadedPlay = play;
			LoadSegment (file, play.Start, play.Stop, seekTime, playing, play.Rate);
		}

		public void Close ()
		{
			player.Error -= OnError;
			player.StateChange -= OnStateChanged;
			player.Eos -= OnEndOfStream;
			player.ReadyToSeek -= OnReadyToSeek;
			ReconfigureTimeout (0);
			player.Dispose ();
			blackboard.Dispose ();
		}

		public void Seek (Time time, bool accurate)
		{
			DrawingsVisible = false;
			player.Seek (time, accurate);
			OnTick ();
		}

		public void SeekToNextFrame ()
		{
			DrawingsVisible = false;
			if (player.CurrentTime < segment.Stop) {
				player.SeekToNextFrame ();
				OnTick ();
			}
		}

		public void SeekToPreviousFrame ()
		{
			DrawingsVisible = false;
			if (player.CurrentTime > segment.Start) {
				seeker.Seek (SeekType.StepDown);
			}
		}

		public void StepForward ()
		{
			DrawingsVisible = false;
			Jump ((int)jumpspinbutton.Value);
		}

		public void StepBackward ()
		{
			DrawingsVisible = false;
			Jump (-(int)jumpspinbutton.Value);
		}

		public void FramerateUp ()
		{
			DrawingsVisible = false;
			vscale1.Adjustment.Value += vscale1.Adjustment.StepIncrement;
		}

		public void FramerateDown ()
		{
			DrawingsVisible = false;
			vscale1.Adjustment.Value -= vscale1.Adjustment.StepIncrement;
		}

		public void CloseSegment ()
		{
			ImageLoaded = false;
			closebutton.Hide ();
			segment.Start = new Time (-1);
			segment.Stop = new Time (int.MaxValue);
			SetScaleValue (SCALE_FPS);
			//timescale.Sensitive = true;
			loadedPlay = null;
			ImageLoaded = false;
			Config.EventsBroker.EmitLoadEvent (null);
		}

		public void SetSensitive ()
		{
			controlsbox.Sensitive = true;
			vscale1.Sensitive = true;
		}

		public void UnSensitive ()
		{
			controlsbox.Sensitive = false;
			vscale1.Sensitive = false;
		}
		#endregion
		#region Private methods
		bool DrawingsVisible {
			set {
				videowindow.Visible = !value;
				blackboarddrawingarea.Visible = value;
			}
		}

		void Open (MediaFile file, bool seek, bool force=false)
		{
			ResetGui ();
			CloseSegment ();
			videowindow.Ratio = (float) (file.VideoWidth * file.Par / file.VideoHeight);
			if (file != this.file || force) {
				readyToSeek = false;
				this.file = file;
				try {
					Log.Debug ("Opening new file " + file.FilePath);
					player.Open (file.FilePath);
				} catch (Exception ex) {
					Log.Exception (ex);
					//We handle this error async
				}
			} else if (seek) {
				player.Seek (new Time (0), true);
			}
			detachbutton.Sensitive = true;
		}

		bool SegmentLoaded {
			get {
				return segment.Start.MSeconds != -1;
			}
		}

		bool ImageLoaded {
			set {
				stillimageLoaded = value;
				drawbutton.Sensitive = !stillimageLoaded;
				playbutton.Sensitive = !stillimageLoaded;
				pausebutton.Sensitive = !stillimageLoaded;
				jumpspinbutton.Sensitive = !stillimageLoaded;
				timescale.Sensitive = !stillimageLoaded;
				vscale1.Sensitive = !stillimageLoaded;
			}
			get {
				return stillimageLoaded;
			}
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
			timescale.AdjustBounds += OnTimescaleAdjustBounds;			
			vscale1.FormatValue += OnVscale1FormatValue;
			vscale1.ValueChanged += OnVscale1ValueChanged;

		}

		void LoadSegment (MediaFile file, Time start, Time stop, Time seekTime,
		                  bool playing, float rate = 1)
		{
			Log.Debug (String.Format ("Update player segment {0} {1} {2}",
			                          start.ToMSecondsString (),
			                          stop.ToMSecondsString (), rate));
			if (file != this.file) {
				Open (file, false);
			}
			Pause ();
			segment.Start = start;
			segment.Stop = stop;
			rate = rate == 0 ? 1 : rate;
			closebutton.Show ();
			ImageLoaded = false;
			if (readyToSeek) {
				Log.Debug ("Player is ready to seek, seeking to " +
					seekTime.ToMSecondsString ());
				SetScaleValue ((int)(rate * SCALE_FPS));
				player.Rate = (double)rate;
				Seek (seekTime, true);
				if (playing) {
					Play ();
				}
			} else {
				Log.Debug ("Delaying seek until player is ready");
				pendingSeek = new object[3] { seekTime, rate, playing };
			}
		}

		void LoadImage (Image image, FrameDrawing drawing)
		{
			blackboard.Background = image;
			blackboard.Drawing = drawing;
			DrawingsVisible = true;
			blackboarddrawingarea.QueueDraw ();
			videowindow.Visible = false;
		}

		void LoadStillImage (Image image)
		{
			ImageLoaded = true;
			LoadImage (image, null);
		}

		void LoadFrameDrawing (FrameDrawing drawing)
		{
			ImageLoaded = true;
			LoadImage (null, drawing);
		}

		void LoadPlayDrawing (FrameDrawing drawing)
		{
			Pause ();
			if (FramesCapturer != null) {
				LoadImage (FramesCapturer.GetFrame (drawing.Render, true), drawing);
			} else {
				ignoreTick = true;
				player.Seek (drawing.Render, true);
				ignoreTick = false;
				LoadImage (player.GetCurrentFrame (), drawing);
			}
		}

		void SetScaleValue (int value)
		{
			emitRateScale = false;
			vscale1.Value = value;
			emitRateScale = true;
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

		void Jump (int jump)
		{
			Time pos = CurrentTime + (jump * 1000);
			if (pos.MSeconds < 0)
				pos.MSeconds = 0;
			Log.Debug (String.Format ("Stepping {0} seconds from {1} to {2}", jump, CurrentTime, pos));
			DrawingsVisible = false;
			Seek (pos, true);
		}

		void SeekFromTimescale (double pos)
		{
			Time seekPos, duration;
			SeekType seekType;

			if (SegmentLoaded) {
				duration = segment.Stop - segment.Start;
				seekPos = segment.Start + duration * pos;
				seekType = SeekType.Accurate;
			} else {
				duration = length;
				seekPos = length * pos;
				seekType = SeekType.Keyframe;
			}
			seeker.Seek (seekType, seekPos);
			timelabel.Text = seekPos.ToMSecondsString (true) + "/" + duration.ToMSecondsString (true);
		}

		void CreatePlayer ()
		{
			player = Config.MultimediaToolkit.GetPlayer ();

			player.Error += OnError;
			player.StateChange += OnStateChanged;
			player.Eos += OnEndOfStream;
			player.ReadyToSeek += OnReadyToSeek;
			videowindow.ButtonPressEvent += OnVideoboxButtonPressEvent;
			videowindow.ScrollEvent += OnVideoboxScrollEvent;
			videowindow.ReadyEvent += HandleReady;
			videowindow.ExposeEvent += HandleExposeEvent;
			videowindow.CanFocus = true;
		}

		void ReconfigureTimeout (uint mseconds)
		{
			if (timeout != 0) {
				GLib.Source.Remove (timeout);
				timeout = 0;
			}
			if (mseconds != 0) {
				timeout = GLib.Timeout.Add (mseconds, OnTick);
			}
		}

		void DoStateChanged (bool playing)
		{
			if (playing) {
				ReconfigureTimeout (20);
				playbutton.Hide ();
				pausebutton.Show ();
			} else {
				ReconfigureTimeout (0);
				playbutton.Show ();
				pausebutton.Hide ();
			}
			if (PlayStateChanged != null)
				PlayStateChanged (playing);
		}

		void ReadyToSeek ()
		{
			readyToSeek = true;
			length = player.StreamLength;
			if (pendingSeek != null) {
				player.Rate = (float)pendingSeek [1];
				player.Seek ((Time)pendingSeek [0], true);
				if ((bool)pendingSeek [2]) {
					Play ();
				}
				pendingSeek = null;
			}
			OnTick ();
		}
		#endregion
		#region Callbacks
		void HandleExposeEvent (object sender, ExposeEventArgs args)
		{
			player.Expose ();
			/* The player draws over the eventbox when it's resized
			 * so make sure that we queue a draw in the event box after
			 * the expose */
			lightbackgroundeventbox.QueueDraw ();
		}

		void OnStateChanged (bool playing)
		{
			Application.Invoke (delegate {
				DoStateChanged (playing);
			});
		}

		void OnReadyToSeek ()
		{
			Application.Invoke (delegate {
				ReadyToSeek ();
			});
		}

		bool OnTick ()
		{
			string slength;
			Time currentTime;

			if (ignoreTick) {
				return true;
			}

			currentTime = CurrentTime;
			if (SegmentLoaded) {
				Time dur, ct;
				double cp;

				dur = segment.Stop - segment.Start;
				if (currentTime > segment.Stop) {
					Pause ();
					Config.EventsBroker.EmitNextPlaylistElement (loadedPlaylist);
				}
				ct = currentTime - segment.Start;
				cp = (float)ct.MSeconds / (float)(dur.MSeconds);
				slength = dur.ToMSecondsString (true);
				timelabel.Text = ct.ToMSecondsString (true) + "/" + slength;
				timescale.Value = cp;
				if (loadedPlay != null && loadedPlay.Drawings.Count > 0) {
					FrameDrawing fd = loadedPlay.Drawings.FirstOrDefault (f => f.Render > lastTime && f.Render <= currentTime);
					if (fd != null) {
						LoadPlayDrawing (fd);
					}
				}
			} else {
				slength = length.ToMSecondsString (true);
				timelabel.Text = currentTime.ToMSecondsString (true) + "/" + slength;
				if (timescale.Visible) {
					timescale.Value = (double)currentTime.MSeconds / length.MSeconds;
				}
			}
			lastTime = currentTime;

			if (Tick != null) {
				Tick (currentTime);
			}
			
			Config.EventsBroker.EmitPlayerTick (currentTime);
			return true;
		}

		void OnTimescaleAdjustBounds (object o, Gtk.AdjustBoundsArgs args)
		{
			double pos;

			if (!seeking) {
				seeking = true;
				IsPlayingPrevState = player.Playing;
				ignoreTick = true;
				Pause ();
				seeksQueue [0] = -1;
				seeksQueue [1] = -1;
			}

			pos = timescale.Value;
			seeksQueue [0] = seeksQueue [1];
			seeksQueue [1] = pos;

			SeekFromTimescale (pos);
		}

		void OnTimescaleValueChanged (object sender, System.EventArgs e)
		{
			if (seeking) {
				/* Releasing the timescale always report value different from the real one.
				 * We need to cache previous position and seek again to the this position */
				SeekFromTimescale (seeksQueue [0] != -1 ? seeksQueue [0] : seeksQueue [1]);
				seeking = false;
				ignoreTick = false;
				if (IsPlayingPrevState)
					Play ();
			}
		}

		void OnPlaybuttonClicked (object sender, System.EventArgs e)
		{
			Play ();
		}

		void OnVolumebuttonClicked (object sender, System.EventArgs e)
		{
			vwin.SetLevel (player.Volume);
			vwin.Show ();
		}

		void SetVolumeIcon (string name)
		{
			volumebuttonimage.Pixbuf = Helpers.Misc.LoadIcon (name, IconSize.Button, 0);
		}
		
		void OnVolumeChanged (double level)
		{
			double prevLevel;
		
			prevLevel = player.Volume;
			if (prevLevel > 0 && level == 0) {
				SetVolumeIcon ("longomatch-control-volume-off");
			} else if (prevLevel > 0.5 && level <= 0.5) {
				SetVolumeIcon ("longomatch-control-volume-low");
			} else if (prevLevel <= 0.5 && level > 0.5) {
				SetVolumeIcon ("longomatch-control-volume-med");
			} else if (prevLevel < 1 && level == 1) {
				SetVolumeIcon ("longomatch-control-volume-hi");
			}
			player.Volume = level;
			if (level == 0)
				muted = true;
			else
				muted = false;
		}

		void OnPausebuttonClicked (object sender, System.EventArgs e)
		{
			Pause ();
		}

		void OnEndOfStream ()
		{
			Application.Invoke (delegate {
				player.Seek (new Time (0), true);
				Pause ();
			});
		}

		void OnError (string message)
		{
			Application.Invoke (delegate {
				Config.EventsBroker.EmitMultimediaError (message);
			});
		}

		void OnClosebuttonClicked (object sender, System.EventArgs e)
		{
			CloseSegment ();
			Play ();
		}

		void OnPrevbuttonClicked (object sender, System.EventArgs e)
		{
			Config.EventsBroker.EmitPreviousPlaylistElement (loadedPlaylist);
		}

		void OnNextbuttonClicked (object sender, System.EventArgs e)
		{
			Config.EventsBroker.EmitNextPlaylistElement (loadedPlaylist);
		}

		void OnVscale1FormatValue (object o, Gtk.FormatValueArgs args)
		{
			double val = args.Value;
			if (val >= SCALE_FPS) {
				val = val + 1 - SCALE_FPS;
				args.RetVal = val + "X";
			} else if (val < SCALE_FPS) {
				args.RetVal = "-" + val + "/" + SCALE_FPS + "X";
			}
		}

		void OnVscale1ValueChanged (object sender, System.EventArgs e)
		{
			float val = GetRateFromScale ();

			// Mute for rate != 1
			if (val != 1 && player.Volume != 0) {
				previousVLevel = player.Volume;
				player.Volume = 0;
			} else if (val != 1 && muted)
				previousVLevel = 0;
			else if (val == 1)
				player.Volume = previousVLevel;

			player.Rate = val;
			if (emitRateScale) {
				Config.EventsBroker.EmitPlaybackRateChanged (val);
			}
		}

		void OnVideoboxButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			if (file == null)
				return;
			/* FIXME: The pointer is grabbed when the event box is clicked.
			 * Make sure to ungrab it in order to avoid clicks outisde the window
			 * triggering this callback. This should be fixed properly.*/
			Pointer.Ungrab (Gtk.Global.CurrentEventTime);
			if (!player.Playing)
				Play ();
			else
				Pause ();
		}

		void OnVideoboxScrollEvent (object o, Gtk.ScrollEventArgs args)
		{
			switch (args.Event.Direction) {
			case ScrollDirection.Down:
				SeekToPreviousFrame ();
				break;
			case ScrollDirection.Up:
				SeekToNextFrame ();
				break;
			case ScrollDirection.Left:
				StepBackward ();
				break;
			case ScrollDirection.Right:
				StepForward ();
				break;
			}
		}

		void OnDrawButtonClicked (object sender, System.EventArgs e)
		{
			Config.EventsBroker.EmitDrawFrame (null, -1);
		}

		void HandleReady (object sender, EventArgs e)
		{
			IntPtr handle = WindowHandle.GetWindowHandle (videowindow.Window.GdkWindow);
			player.WindowHandle = handle;
			if (delayedOpen) {
				Open (file, true, true);
				delayedOpen = false;
				player.Expose ();
			}
		}

		void HandleSeekEvent (SeekType type, Time start, float rate)
		{
			DrawingsVisible = false;
			/* We only use it for backwards framestepping for now */
			if (type == SeekType.StepDown || type == SeekType.StepUp) {
				if (player.Playing)
					Pause ();
				if (type == SeekType.StepDown)
					player.SeekToPreviousFrame ();
				else
					player.SeekToNextFrame ();
				OnTick ();
			}
			if (type == SeekType.Accurate || type == SeekType.Keyframe) {
				player.Rate = (double)rate;
				player.Seek (start, type == SeekType.Accurate);
				OnTick ();
			}
		}
		#endregion
	}
}
