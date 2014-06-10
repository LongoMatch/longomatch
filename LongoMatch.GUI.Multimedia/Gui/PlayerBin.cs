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

		public event SegmentClosedHandler SegmentClosedEvent;
		public event LongoMatch.Handlers.TickHandler Tick;
		public event LongoMatch.Handlers.ErrorHandler Error;
		public event LongoMatch.Handlers.StateChangeHandler PlayStateChanged;
		public event NextButtonClickedHandler Next;
		public event PrevButtonClickedHandler Prev;
		public event LongoMatch.Handlers.DrawFrameHandler DrawFrame;
		public event SeekEventHandler SeekEvent;
		public event DetachPlayerHandler Detach;
		public event PlaybackRateChangedHandler PlaybackRateChanged;

		const int THUMBNAIL_MAX_WIDTH = 100;
		const int SCALE_FPS = 25;
		TickHandler tickHandler;
		IPlayer player;
		Time length;
		bool seeking, IsPlayingPrevState, muted, emitRateScale, readyToSeek;
		string filename;
		double previousVLevel = 1;
		double[] seeksQueue;
		object[] pendingSeek; //{seekTime, rate, playing}
		protected VolumeWindow vwin;
		Seeker seeker;
		Segment segment;


		#region Constructors
		public PlayerBin()
		{
			this.Build();
			vwin = new VolumeWindow();
			ConnectSignals ();
			tickHandler = new TickHandler(OnTick);
			controlsbox.Visible = false;
			UnSensitive();
			timescale.Adjustment.PageIncrement = 0.01;
			timescale.Adjustment.StepIncrement = 0.0001;
			playbutton.CanFocus = false;
			pausebutton.CanFocus = false;
			prevbutton.CanFocus = false;
			nextbutton.CanFocus = false;
			jumpspinbutton.CanFocus = false;
			detachbutton.CanFocus = false;
			volumebutton.CanFocus = false;
			timescale.CanFocus = false;
			vscale1.CanFocus = false;
			drawbutton.CanFocus = false;
			seeksQueue = new double[2];
			seeksQueue [0] = -1;
			seeksQueue [1] = -1;
			detachbutton.Clicked += (sender, e) => EmitDetach();
			seeker = new Seeker();
			seeker.SeekEvent += HandleSeekEvent;
			segment.Start = new Time(-1);
			segment.Stop = new Time(int.MaxValue);
			
			CreatePlayer ();
		}

		#endregion

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
		
		public bool Detached {
			get;
			set;
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
			LoadSegment (filename, play.Start, play.Stop, seekTime, playing, play.Rate);
		}
		
		public void Close() {
			player.Tick -= tickHandler;
			player.Close();
			filename = null;
			timescale.Value = 0;
			UnSensitive();
		}

		public void Seek (Time time, bool accurate) {
			player.Seek (time, accurate);
			if(SeekEvent != null)
				SeekEvent (time);
		}

		public void SeekToNextFrame () {
			if (player.CurrentTime < segment.Stop) {
				player.SeekToNextFrame ();
				if(SeekEvent != null)
					SeekEvent (player.CurrentTime);
			}
		}

		public void SeekToPreviousFrame () {
			if (player.CurrentTime > segment.Start) {
				seeker.Seek (SeekType.StepDown);
			}
		}

		public void StepForward() {
			Jump((int)jumpspinbutton.Value);
		}

		public void StepBackward() {
			Jump(-(int)jumpspinbutton.Value);
		}

		public void FramerateUp() {
			vscale1.Adjustment.Value += vscale1.Adjustment.StepIncrement;
		}

		public void FramerateDown() {
			vscale1.Adjustment.Value -= vscale1.Adjustment.StepIncrement;
		}

		public void CloseSegment() {
			closebutton.Hide();
			segment.Start = new Time (-1);
			segment.Stop = new Time (int.MaxValue);
			SetScaleValue (SCALE_FPS);
			//timescale.Sensitive = true;
			if (SegmentClosedEvent != null)
				SegmentClosedEvent();
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
			timescale.ValueChanged += OnTimescaleValueChanged;
			timescale.AdjustBounds += OnTimescaleAdjustBounds;
			vscale1.FormatValue += OnVscale1FormatValue;
			vscale1.ValueChanged += OnVscale1ValueChanged;
			drawbutton.Clicked += OnDrawButtonClicked;
			volumebutton.Clicked += OnVolumebuttonClicked;

		}
		
		void LoadSegment (string filename, Time start, Time stop, Time seekTime,
		                  bool playing, float rate = 1) {
			Log.Debug (String.Format ("Update player segment {0} {1} {2}",
			                          start.ToMSecondsString(),
			                          stop.ToMSecondsString(), rate));
			if (filename != this.filename) {
				Open (filename, false);
			}
			player.Pause();
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
					player.Play ();
				}
			} else {
				Log.Debug ("Delaying seek until player is ready");
				pendingSeek = new object[3] {seekTime, rate, playing};
			}
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
			Seek (pos, true);
		}

		void SeekFromTimescale (double pos) {
			if(SegmentLoaded) {
				Time duration = segment.Stop - segment.Start;
				Time seekPos = segment.Start + duration * pos;
				seeker.Seek (SeekType.Keyframe, seekPos);
				timelabel.Text = seekPos.ToMSecondsString() + "/" + duration.ToMSecondsString();
			}
			else {
				seeker.Seek (SeekType.Keyframe, length * pos);
				timelabel.Text = player.CurrentTime.ToMSecondsString () + "/" +
					length.ToMSecondsString();
			}
		}
		
		void EmitDetach () {
			if (Detach != null)
				Detach(!Detached);
		}
		
		void CreatePlayer ()
		{
			videodrawingarea.DoubleBuffered = false;
			player = Config.MultimediaToolkit.GetPlayer ();

			player.Tick += OnTick;
			player.StateChange += OnStateChanged;
			player.Eos += OnEndOfStream;
			player.Error += OnError;
			player.ReadyToSeek += OnReadyToSeek;

			videoeventbox.ButtonPressEvent += OnVideoboxButtonPressEvent;
			videoeventbox.ScrollEvent += OnVideoboxScrollEvent;
			videodrawingarea.Realized += HandleRealized;
			videodrawingarea.ExposeEvent += HandleExposeEvent;
			videodrawingarea.CanFocus = false;
		}

		#endregion

		#region Callbacks
		void HandleExposeEvent (object sender, ExposeEventArgs args)
		{
			player.Expose();
		}
		
		void OnStateChanged(bool playing) {
			if(playing) {
				playbutton.Hide();
				pausebutton.Show();
			}
			else {
				playbutton.Show();
				pausebutton.Hide();
			}
			if(PlayStateChanged != null)
				PlayStateChanged(playing);
		}

		void OnReadyToSeek() {
			readyToSeek = true;
			if(pendingSeek != null) {
				player.Rate = (float) pendingSeek [1];
				player.Seek ((Time)pendingSeek[0], true);
				if ((bool)pendingSeek[2]) {
					player.Play();
				}
				pendingSeek = null;
			}
		}

		void OnTick (Time currentTime, Time streamLength, double currentPosition) {
			string slength;

			if (length != streamLength) {
				length = streamLength;
			}

			if (SegmentLoaded) {
				Time dur, ct;
				double cp;

				dur = segment.Stop - segment.Start;
				if (currentTime > segment.Stop) {
					player.Pause ();
				}
				ct = currentTime - segment.Start;
				cp = (float)ct.MSeconds/(float)(dur.MSeconds);
				slength = dur.ToMSecondsString();
				timelabel.Text = ct.ToMSecondsString() + "/" + slength;
				timescale.Value = cp;
			} else {
				slength = length.ToMSecondsString ();
				timelabel.Text = currentTime.ToMSecondsString() + "/" + slength;
				timescale.Value = currentPosition;
			}

			if (Tick != null)
				Tick (currentTime, streamLength, currentPosition);

		}

		void OnTimescaleAdjustBounds(object o, Gtk.AdjustBoundsArgs args)
		{
			double pos;

			if(!seeking) {
				seeking = true;
				IsPlayingPrevState = player.Playing;
				player.Tick -= tickHandler;
				player.Pause();
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
				player.Tick += tickHandler;
				if(IsPlayingPrevState)
					player.Play();
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

		void OnDestroyEvent(object o, Gtk.DestroyEventArgs args)
		{
			player.Dispose();
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
			player.Pause();
		}

		void OnEndOfStream(object o, EventArgs args) {
			player.Seek (new Time (0), true);
			player.Pause();
		}

		void OnError(string message) {
			if(Error != null)
				Error(message);
		}

		void OnClosebuttonClicked(object sender, System.EventArgs e)
		{
			CloseSegment();
			player.Play ();
		}

		void OnPrevbuttonClicked(object sender, System.EventArgs e)
		{
			if (segment.Start.MSeconds > 0)
				Seek (segment.Start, true);
			if(Prev != null)
				Prev();
		}

		void OnNextbuttonClicked(object sender, System.EventArgs e)
		{
			if(Next != null)
				Next();
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
			if (PlaybackRateChanged != null && emitRateScale) {
				PlaybackRateChanged (val);
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
			if(DrawFrame != null)
				DrawFrame (CurrentTime);
		}
		
		void HandleRealized (object sender, EventArgs e)
		{
			player.WindowHandle = GtkHelpers.GetWindowHandle (videodrawingarea.GdkWindow);
		}
		
		void HandleSeekEvent (SeekType type, Time start, float rate)
		{
			/* We only use it for backwards framestepping for now */
			if (type == SeekType.StepDown || type == SeekType.StepUp) {
				if(player.Playing)
					player.Pause ();
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
