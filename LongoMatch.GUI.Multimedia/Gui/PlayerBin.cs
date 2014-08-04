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
using Gtk;
using Gdk;
using Mono.Unix;
using System.Runtime.InteropServices;

using Image = LongoMatch.Common.Image;
using LongoMatch.Handlers;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Interfaces.Multimedia;
using LongoMatch.Video;
using LongoMatch.Video.Common;
using LongoMatch.Video.Player;
using LongoMatch.Video.Utils;
using LongoMatch.Store;
using LongoMatch.Multimedia.Utils;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Drawing.Cairo;


namespace LongoMatch.Gui
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]

	public partial class PlayerBin : Gtk.Bin, LongoMatch.Interfaces.GUI.IPlayerBin
	{
		struct Segment {
			public Time Start;
			public Time Stop;
		}	


		public event TickHandler Tick;
		public event LongoMatch.Handlers.StateChangeHandler PlayStateChanged;
		public event SeekEventHandler SeekEvent;

		const int THUMBNAIL_MAX_WIDTH = 100;
		const int SCALE_FPS = 25;
		IPlayer player;
		Play loadedPlay;
		Time length, lastTime;
		bool seeking, IsPlayingPrevState, muted, emitRateScale, readyToSeek;
		string filename;
		double previousVLevel = 1;
		double[] seeksQueue;
		object[] pendingSeek; //{seekTime, rate, playing}
		protected VolumeWindow vwin;
		Seeker seeker;
		Segment segment;
		Blackboard blackboard;
		uint timeout;
		bool ignoreTick;


		#region Constructors
		public PlayerBin()
		{
			this.Build();
			vwin = new VolumeWindow();
			ConnectSignals ();
			blackboard = new Blackboard (new WidgetWrapper (blackboarddrawingarea));
			controlsbox.Visible = false;
			UnSensitive();
			timescale.Adjustment.PageIncrement = 0.01;
			timescale.Adjustment.StepIncrement = 0.0001;
			LongoMatch.Gui.Helpers.Misc.DisableFocus (vbox3);
			videodrawingarea.CanFocus = true;
			seeksQueue = new double[2];
			seeksQueue [0] = -1;
			seeksQueue [1] = -1;
			detachbutton.Clicked += (sender, e) => Config.EventsBroker.EmitDetach ();
			seeker = new Seeker();
			seeker.SeekEvent += HandleSeekEvent;
			segment.Start = new Time(-1);
			segment.Stop = new Time(int.MaxValue);
			lastTime = new Time (0);
			length = new Time (0);
			
			CreatePlayer ();
		}

		#endregion
		protected override void OnDestroyed ()
		{
			ReconfigureTimeout (0);
			player.Dispose ();
			blackboard.Dispose ();
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
				if(value)
					GdkWindow.Fullscreen();
				else
					GdkWindow.Unfullscreen();
			}
		}

		public Image CurrentMiniatureFrame {
			get {
				return player.GetCurrentFrame(THUMBNAIL_MAX_WIDTH, THUMBNAIL_MAX_WIDTH);
			}
		}

		public Image CurrentFrame {
			get {
				return player.GetCurrentFrame();
			}
		}

		public bool Opened {
			get {
				return filename != null;
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
		#endregion

		#region Public methods

		public void Open (string filename) {
			Open (filename, true);
		}

		public void Play() {
			DrawingsVisible = false;
			player.Play();
		}

		public void Pause() {
			player.Pause();
		}

		public void TogglePlay() {
			if(player.Playing)
				Pause();
			else
				Play();
		}

		public void ResetGui() {
			closebutton.Hide();
			SetSensitive();
			timescale.Value=0;
			timelabel.Text="";
			SeekingEnabled = true;
			seeking=false;
			IsPlayingPrevState = false;
			muted=false;
			emitRateScale = true;
			videodrawingarea.Visible = true;
			blackboarddrawingarea.Visible = false;
		}

		public void LoadPlayListPlay (PlayListPlay play, bool hasNext) {
			if(hasNext)
				nextbutton.Sensitive = true;
			else
				nextbutton.Sensitive = false;

			LoadSegment (play.MediaFile.FilePath, play.Start, play.Stop,
			             play.Start, true, play.Rate);
		}
		
		public void LoadPlay (string filename, Play play, Time seekTime, bool playing) {
			loadedPlay = play;
			LoadSegment (filename, play.Start, play.Stop, seekTime, playing, play.Rate);
		}
		
		public void Close() {
			ReconfigureTimeout (0);
			player.Close();
			filename = null;
			timescale.Value = 0;
			UnSensitive();
		}

		public void Seek (Time time, bool accurate) {
			DrawingsVisible = false;
			player.Seek (time, accurate);
			if(SeekEvent != null)
				SeekEvent (time);
		}

		public void SeekToNextFrame () {
			DrawingsVisible = false;
			if (player.CurrentTime < segment.Stop) {
				player.SeekToNextFrame ();
				if(SeekEvent != null)
					SeekEvent (player.CurrentTime);
			}
		}

		public void SeekToPreviousFrame () {
			DrawingsVisible = false;
			if (player.CurrentTime > segment.Start) {
				seeker.Seek (SeekType.StepDown);
			}
		}

		public void StepForward() {
			DrawingsVisible = false;
			Jump((int)jumpspinbutton.Value);
		}

		public void StepBackward() {
			DrawingsVisible = false;
			Jump(-(int)jumpspinbutton.Value);
		}
		
		public void FramerateUp() {
			DrawingsVisible = false;
			vscale1.Adjustment.Value += vscale1.Adjustment.StepIncrement;
		}

		public void FramerateDown() {
			DrawingsVisible = false;
			vscale1.Adjustment.Value -= vscale1.Adjustment.StepIncrement;
		}

		public void CloseSegment() {
			closebutton.Hide();
			segment.Start = new Time (-1);
			segment.Stop = new Time (int.MaxValue);
			SetScaleValue (SCALE_FPS);
			//timescale.Sensitive = true;
			loadedPlay = null;
			Config.EventsBroker.EmitPlaySelected (null);
		}

		public void SetSensitive() {
			controlsbox.Sensitive = true;
			vscale1.Sensitive = true;
		}

		public void UnSensitive() {
			controlsbox.Sensitive = false;
			vscale1.Sensitive = false;
		}

		#endregion

		#region Private methods

		bool DrawingsVisible {
			set {
				videodrawingarea.Visible = !value;
				blackboarddrawingarea.Visible = value;
			}
		}
		
		void Open(string filename, bool seek) {
			ResetGui();
			CloseSegment();
			if (filename != this.filename) {
				readyToSeek = false;
				this.filename = filename;
				try {
					Log.Debug ("Openning new file " + filename);
					player.Open(filename);
				}
				catch (Exception ex) {
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
		
		void ConnectSignals () {
			vwin.VolumeChanged += new VolumeChangedHandler(OnVolumeChanged);
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
		
		void LoadSegment (string filename, Time start, Time stop, Time seekTime,
		                  bool playing, float rate = 1) {
			Log.Debug (String.Format ("Update player segment {0} {1} {2}",
			                          start.ToMSecondsString(),
			                          stop.ToMSecondsString(), rate));
			if (filename != this.filename) {
				Open (filename, false);
			}
			Pause ();
			segment.Start = start;
			segment.Stop = stop;
			rate = rate == 0 ? 1 : rate;
			closebutton.Show();
			if (readyToSeek) {
				Log.Debug ("Player is ready to seek, seeking to " +
				           start.ToMSecondsString());
				SetScaleValue ((int) (rate * SCALE_FPS));
				player.Rate = (double) rate;
				player.Seek (seekTime, true);
				if (playing) {
					Play ();
				}
			} else {
				Log.Debug ("Delaying seek until player is ready");
				pendingSeek = new object[3] {seekTime, rate, playing};
			}
		}

		void LoadDrawing (FrameDrawing drawing) {
			Pause ();
			ignoreTick = true;
			player.Seek (drawing.Render, true);
			ignoreTick = false;
			blackboard.Background = player.GetCurrentFrame () ;
			blackboard.Drawing = drawing;
			DrawingsVisible = true;
			blackboarddrawingarea.QueueDraw ();
			videodrawingarea.Visible = false;
		}

		void SetScaleValue (int value) {
			emitRateScale = false;
			vscale1.Value = value;
			emitRateScale = true;
		}
		
		float GetRateFromScale() {
			VScale scale= vscale1;
			double val = scale.Value;

			if(val > SCALE_FPS) {
				val = val + 1 - SCALE_FPS ;
			}
			else if(val <= SCALE_FPS) {
				val = val / SCALE_FPS;
			}
			return (float)val;
		}

		void Jump (int jump) {
			Time pos = CurrentTime + (jump * 1000);
			if (pos.MSeconds < 0)
				pos.MSeconds = 0;
			Log.Debug (String.Format("Stepping {0} seconds from {1} to {2}", jump, CurrentTime, pos));
			DrawingsVisible = false;
			Seek (pos, true);
		}

		void SeekFromTimescale (double pos) {
			Time seekPos, duration;
			SeekType seekType;

			if(SegmentLoaded) {
				duration = segment.Stop - segment.Start;
				seekPos = segment.Start + duration * pos;
				seekType = SeekType.Accurate;
			}
			else {
				duration = length;
				seekPos = length * pos;
				seekType = SeekType.Keyframe;
			}
			seeker.Seek (seekType, seekPos);
			timelabel.Text = seekPos.ToMSecondsString() + "/" + duration.ToMSecondsString();
		}
		
		void CreatePlayer ()
		{
			videodrawingarea.DoubleBuffered = false;
			player = Config.MultimediaToolkit.GetPlayer ();

			player.Error += OnError;
			player.StateChange += OnStateChanged;
			player.Eos += OnEndOfStream;
			player.ReadyToSeek += OnReadyToSeek;
			videoeventbox.ButtonPressEvent += OnVideoboxButtonPressEvent;
			videoeventbox.ScrollEvent += OnVideoboxScrollEvent;
			videodrawingarea.Realized += HandleRealized;
			videodrawingarea.ExposeEvent += HandleExposeEvent;
			videodrawingarea.CanFocus = false;
		}
		
		void ReconfigureTimeout (uint mseconds) {
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
			}
			else {
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
			player.Expose();
		}
		
		void OnStateChanged(bool playing) {
			Application.Invoke (delegate {
				DoStateChanged (playing);});
		}

		void OnReadyToSeek() {
			Application.Invoke (delegate {
				ReadyToSeek ();});
		}

		bool OnTick () {
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
					Pause();
				}
				ct = currentTime - segment.Start;
				cp = (float)ct.MSeconds/(float)(dur.MSeconds);
				slength = dur.ToMSecondsString();
				timelabel.Text = ct.ToMSecondsString() + "/" + slength;
				timescale.Value = cp;
				if (loadedPlay != null && loadedPlay.Drawings.Count > 0) {
					FrameDrawing fd = loadedPlay.Drawings.FirstOrDefault (f => f.Render > lastTime &&  f.Render <= currentTime);
					if (fd != null) {
						LoadDrawing (fd);
					}
				}
			} else {
				slength = length.ToMSecondsString ();
				timelabel.Text = currentTime.ToMSecondsString() + "/" + slength;
				if (timescale.Visible) {
					timescale.Value = (double) currentTime.MSeconds / length.MSeconds;
				}
			}
			lastTime = currentTime;

			if (Tick != null) {
				Tick (currentTime);
			}
			
			Config.EventsBroker.EmitTick (currentTime);
			return true;
		}

		void OnTimescaleAdjustBounds(object o, Gtk.AdjustBoundsArgs args)
		{
			double pos;

			if(!seeking) {
				seeking = true;
				IsPlayingPrevState = player.Playing;
				ignoreTick = true;
				Pause ();
				seeksQueue [0] = -1;
				seeksQueue [1] = -1;
			}

			pos = timescale.Value;
			seeksQueue[0] = seeksQueue[1];
			seeksQueue[1] = pos;

			SeekFromTimescale(pos);
		}

		void OnTimescaleValueChanged(object sender, System.EventArgs e)
		{
			if(seeking) {
				/* Releasing the timescale always report value different from the real one.
				 * We need to cache previous position and seek again to the this position */
				SeekFromTimescale(seeksQueue[0] != -1 ? seeksQueue[0] : seeksQueue[1]);
				seeking=false;
				ignoreTick = false;
				if(IsPlayingPrevState)
					Play ();
			}
		}

		void OnPlaybuttonClicked(object sender, System.EventArgs e)
		{
			Play();
		}

		void OnVolumebuttonClicked(object sender, System.EventArgs e)
		{
			vwin.SetLevel(player.Volume);
			vwin.Show();
		}

		void OnVolumeChanged(double level) {
			player.Volume = level;
			if(level == 0)
				muted = true;
			else
				muted = false;
		}

		void OnPausebuttonClicked(object sender, System.EventArgs e)
		{
			Pause ();
		}

		void OnEndOfStream (object o, EventArgs args) {
			Application.Invoke (delegate {
				player.Seek (new Time (0), true);
				Pause ();
			});
		}

		void OnError(string message) {
			Application.Invoke (delegate {
				Config.EventsBroker.EmitMultimediaError (message);
			});
		}

		void OnClosebuttonClicked(object sender, System.EventArgs e)
		{
			CloseSegment();
			Play ();
		}

		void OnPrevbuttonClicked(object sender, System.EventArgs e)
		{
			if (segment.Start.MSeconds > 0)
				Seek (segment.Start, true);
			Config.EventsBroker.EmitPrev();
		}

		void OnNextbuttonClicked(object sender, System.EventArgs e)
		{
			Config.EventsBroker.EmitNext();
		}

		void OnVscale1FormatValue(object o, Gtk.FormatValueArgs args)
		{
			double val = args.Value;
			if(val >= SCALE_FPS) {
				val = val + 1 - SCALE_FPS ;
				args.RetVal = val +"X";
			} else if(val < SCALE_FPS) {
				args.RetVal = "-"+val+"/"+SCALE_FPS+"X";
			}
		}

		void OnVscale1ValueChanged(object sender, System.EventArgs e)
		{
			float val = GetRateFromScale();

			// Mute for rate != 1
			if(val != 1 && player.Volume != 0) {
				previousVLevel = player.Volume;
				player.Volume=0;
			}
			else if(val != 1 && muted)
				previousVLevel = 0;
			else if(val ==1)
				player.Volume = previousVLevel;

			player.Rate = val;
			if (emitRateScale) {
				Config.EventsBroker.EmitPlaybackRateChanged (val);
			}
		}

		void OnVideoboxButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
		{
			if(filename == null)
				return;
			/* FIXME: The pointer is grabbed when the event box is clicked.
			 * Make sure to ungrab it in order to avoid clicks outisde the window
			 * triggering this callback. This should be fixed properly.*/
			Pointer.Ungrab(Gtk.Global.CurrentEventTime);
			if(!player.Playing)
				Play();
			else
				Pause();
		}

		void OnVideoboxScrollEvent(object o, Gtk.ScrollEventArgs args)
		{
			switch(args.Event.Direction) {
			case ScrollDirection.Down:
				SeekToPreviousFrame ();
				break;
			case ScrollDirection.Up:
				SeekToNextFrame ();
				break;
			case ScrollDirection.Left:
				StepBackward();
				break;
			case ScrollDirection.Right:
				StepForward();
				break;
			}
		}

		void OnDrawButtonClicked(object sender, System.EventArgs e)
		{
			Config.EventsBroker.EmitDrawFrame (null, -1);
		}
		
		void HandleRealized (object sender, EventArgs e)
		{
			player.WindowHandle = WindowHandle.GetWindowHandle (videodrawingarea.GdkWindow);
		}
		
		void HandleSeekEvent (SeekType type, Time start, float rate)
		{
			DrawingsVisible = false;
			/* We only use it for backwards framestepping for now */
			if (type == SeekType.StepDown || type == SeekType.StepUp) {
				if(player.Playing)
					Pause ();
				if (type == SeekType.StepDown)
					player.SeekToPreviousFrame ();
				else
					player.SeekToNextFrame ();
				if (SeekEvent != null)
					SeekEvent (CurrentTime);
			}
			if (type == SeekType.Accurate || type == SeekType.Keyframe) {
				player.Rate = (double) rate;
				player.Seek (start, type == SeekType.Accurate);
				if (SeekEvent != null)
					SeekEvent (start);
			}
		}

		#endregion
	}
}
