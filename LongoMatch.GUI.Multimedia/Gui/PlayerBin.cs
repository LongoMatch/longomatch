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
using LongoMatch.Multimedia.Interfaces;
using LongoMatch.Video;
using LongoMatch.Video.Common;
using LongoMatch.Video.Player;
using LongoMatch.Video.Utils;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]

	public partial class PlayerBin : Gtk.Bin, LongoMatch.Interfaces.GUI.IPlayer
	{

		public event SegmentClosedHandler SegmentClosedEvent;
		public event LongoMatch.Handlers.TickHandler Tick;
		public event LongoMatch.Handlers.ErrorHandler Error;
		public event LongoMatch.Handlers.StateChangeHandler PlayStateChanged;
		public event NextButtonClickedHandler Next;
		public event PrevButtonClickedHandler Prev;
		public event LongoMatch.Handlers.DrawFrameHandler DrawFrame;
		public event SeekEventHandler SeekEvent;
		public event DetachPlayerHandler Detach;

		private const int THUMBNAIL_MAX_WIDTH = 100;
		private LongoMatch.Video.Common.TickHandler tickHandler;
		private LongoMatch.Multimedia.Interfaces.IPlayer player;
		private long length=0;
		private string slength;
		private long segmentStartTime;
		private long segmentStopTime;
		private bool seeking=false;
		private double[] seeksQueue;
		private bool IsPlayingPrevState = false;
		private float rate=1;
		private double previousVLevel = 1;
		private bool muted=false;
		private object[] pendingSeek=null; //{start,stop,rate}
		//the player.mrl is diferent from the filename as it's an uri eg:file:///foo.avi
		private string filename = null;
		protected VolumeWindow vwin;


		#region Constructors
		public PlayerBin()
		{
			this.Build();
			PlayerInit();
			vwin = new VolumeWindow();
			vwin.VolumeChanged += new VolumeChangedHandler(OnVolumeChanged);
			controlsbox.Visible = false;
			UnSensitive();
			timescale.Adjustment.PageIncrement = 0.01;
			timescale.Adjustment.StepIncrement = 0.0001;
			playbutton.CanFocus = false;
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
		}

		#endregion

		#region Properties

		public long AccurateCurrentTime {
			get {
				return player.AccurateCurrentTime;
			}
		}

		public int CurrentTime {
			get {
				return (int) player.CurrentTime;
			}
		}

		public long StreamLength {
			get {
				return player.StreamLength;
			}
		}

		public float Rate {
			get {
				return rate;
			}
			set {
				vscale1.Value = (int)(value*25);
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

		public Image LogoPixbuf {
			set {
				player.LogoPixbuf = value;
			}
		}

		public bool DrawingMode {
			set {
				player.DrawingMode= value;
			}
		}

		public Image DrawingPixbuf {
			set {
				player.DrawingPixbuf=value;
			}
		}

		public bool LogoMode {
			set {
				player.LogoMode = value;
			}
		}

		public bool ExpandLogo {
			get {
				return player.ExpandLogo;
			}
			set {
				player.ExpandLogo = value;
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
		#endregion

		#region Public methods

		public void Open(string mrl) {
			filename = mrl;
			ResetGui();
			CloseActualSegment();
			try {
				player.Open(mrl);
			}
			catch {
				//We handle this error async
			}
			detachbutton.Sensitive = true;
		}

		public void Play() {
			player.Play();
			float val = GetRateFromScale();
			if(segmentStartTime == 0 && segmentStopTime==0)
				player.SetRate(val);
			else
				player.SetRateInSegment(val,segmentStopTime);
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

		public void SetLogo(string filename) {
			player.Logo=filename;
		}

		public void ResetGui() {
			closebutton.Hide();
			SetSensitive();
			timescale.Value=0;
			timelabel.Text="";
			player.CancelProgramedStop();
		}

		public void SetPlayListElement(string fileName,long start, long stop, float rate, bool hasNext) {
			if(hasNext)
				nextbutton.Sensitive = true;
			else
				nextbutton.Sensitive = false;

			if(fileName != filename) {
				Open(fileName);
				//Wait until the pipeline is prerolled and ready to seek
				pendingSeek = new object[3] {start,stop,rate};
			}
			else {
			 player.SegmentSeek(start,stop,rate);
			 player.Play();
			}

			segmentStartTime = start;
			segmentStopTime = stop;
			player.LogoMode = false;
			Rate = rate;
			detachbutton.Sensitive = false;
		}

		public void Close() {
			player.Close();
			filename = null;
			timescale.Value = 0;
			UnSensitive();
		}

		public void SeekTo(long time, bool accurate) {
			player.SeekTime(time,1,accurate);
			if(SeekEvent != null)
				SeekEvent(time);
		}

		public void SeekInSegment(long pos) {
			player.SeekInSegment(pos, GetRateFromScale());
			if(SeekEvent != null)
				SeekEvent(pos);
		}

		public void SeekToNextFrame(bool in_segment) {
			int currentTime = (int)player.CurrentTime;
			if(segmentStopTime==0 | currentTime < segmentStopTime) {
				if(player.Playing)
					player.Pause();
				player.SeekToNextFrame(GetRateFromScale(), in_segment);
				if(SeekEvent != null)
					SeekEvent(currentTime);
			}

		}

		public void SeekToPreviousFrame(bool in_segment) {
			long currentTime = player.CurrentTime;
			if(currentTime> segmentStartTime) {
				if(player.Playing)
					player.Pause();
				player.SeekToPreviousFrame(GetRateFromScale(),in_segment);
				if(SeekEvent != null)
					SeekEvent(currentTime);
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

		public void UpdateSegmentStartTime(long start) {
			segmentStartTime = start;
			player.SegmentStartUpdate(start, GetRateFromScale());
			if(SeekEvent != null)
				SeekEvent(start);
		}

		public void UpdateSegmentStopTime(long stop) {
			segmentStopTime = stop;
			player.SegmentStopUpdate(stop, GetRateFromScale());
			if(SeekEvent != null)
				SeekEvent(stop);
		}

		public void SetStartStop(long start, long stop) {
			segmentStartTime = start;
			segmentStopTime = stop;
			closebutton.Show();
			vscale1.Value = 25;
			player.SegmentSeek(start,stop, GetRateFromScale());
			player.Play();
		}

		public void CloseActualSegment() {
			closebutton.Hide();
			segmentStartTime = 0;
			segmentStopTime = 0;
			vscale1.Value=25;
			//timescale.Sensitive = true;
			slength = TimeString.MSecondsToSecondsString(length);
			SegmentClosedEvent();
			player.CancelProgramedStop();
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

		private float GetRateFromScale() {
			VScale scale= vscale1;
			double val = scale.Value;

			if(val >25) {
				val = val-25 ;
			}
			else if(val <=25) {
				val = val/25;
			}
			return (float)val;
		}

		private bool InSegment() {
			return  !(segmentStopTime == 0 && segmentStartTime ==0) ;
		}

		private void PlayerInit() {
			MultimediaFactory factory;
			Widget playerWidget;

			factory= new MultimediaFactory();
			player = factory.GetPlayer(320,280);

			tickHandler = new LongoMatch.Video.Common.TickHandler(OnTick);
			player.Tick += tickHandler;
			player.StateChange += OnStateChanged;
			player.Eos += OnEndOfStream;
			player.Error += OnError;
			player.ReadyToSeek += OnReadyToSeek;

			playerWidget = (Widget)player;
			playerWidget.ButtonPressEvent += OnVideoboxButtonPressEvent;
			playerWidget.ScrollEvent += OnVideoboxScrollEvent;
			playerWidget.Show();
			videobox.Add(playerWidget);

		}
		
		void Jump(int jump) {
			long pos = Math.Max(CurrentTime + (jump * 1000), 0);
			Log.Debug(String.Format("Stepping {0} seconds from {1} to {2}", jump, CurrentTime, pos));
			if (InSegment())
				SeekInSegment(pos);
			else
				SeekTo(pos, true);
		}

		private void SeekFromTimescale(double pos) {
			if(InSegment()) {
				long seekPos = segmentStartTime + (long)(pos*(segmentStopTime-segmentStartTime));
				player.SeekInSegment(seekPos, GetRateFromScale());
				timelabel.Text= TimeString.MSecondsToSecondsString(seekPos) + "/" +
				                TimeString.MSecondsToSecondsString(segmentStopTime-segmentStartTime);
			}
			else {
				player.Position = pos;
				timelabel.Text= TimeString.MSecondsToSecondsString(player.CurrentTime) + "/" + slength;
				Rate = 1;
			}
		}
		
		void EmitDetach () {
			if (Detach != null)
				Detach(!Detached);
		}
		
		#endregion

		#region Callbacks
		protected virtual void OnStateChanged(object o, StateChangeArgs args) {
			if(args.Playing) {
				playbutton.Hide();
				pausebutton.Show();
			}
			else {
				playbutton.Show();
				pausebutton.Hide();
			}
			if(PlayStateChanged != null)
				PlayStateChanged(this,args.Playing);
		}

		protected void OnReadyToSeek(object o, EventArgs args) {
			if(pendingSeek != null) {
				player.SegmentSeek((long)pendingSeek[0],
				                   (long)pendingSeek[1],
				                   (float)pendingSeek[2]);
				player.Play();
				pendingSeek = null;

			}
		}

		protected virtual void OnTick(object o,TickArgs args) {
			long currentTime = args.CurrentTime;
			float currentposition = args.CurrentPosition;
			long streamLength = args.StreamLength;

			//Console.WriteLine ("Current Time:{0}\n Length:{1}\n",currentTime, streamLength);
			if(length != streamLength) {
				length = streamLength;
				slength = TimeString.MSecondsToSecondsString(length);
			}

			if(InSegment()) {
				currentTime -= segmentStartTime;
				currentposition = (float)currentTime/(float)(segmentStopTime-segmentStartTime);
				slength = TimeString.MSecondsToSecondsString(segmentStopTime-segmentStartTime);
			}

			timelabel.Text = TimeString.MSecondsToSecondsString(currentTime) + "/" + slength;
			timescale.Value = currentposition;
			if(Tick != null)
				Tick(o, args.CurrentTime, args.StreamLength, args.CurrentPosition, args.Seekable);

		}

		protected virtual void OnTimescaleAdjustBounds(object o, Gtk.AdjustBoundsArgs args)
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

		protected virtual void OnTimescaleValueChanged(object sender, System.EventArgs e)
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

		protected virtual void OnPlaybuttonClicked(object sender, System.EventArgs e)
		{
			Play();
		}

		protected virtual void OnStopbuttonClicked(object sender, System.EventArgs e)
		{
			player.SeekTime(segmentStartTime,1,true);
		}

		protected virtual void OnVolumebuttonClicked(object sender, System.EventArgs e)
		{
			vwin.SetLevel(player.Volume);
			vwin.Show();
		}

		protected virtual void OnDestroyEvent(object o, Gtk.DestroyEventArgs args)
		{
			player.Dispose();
		}

		protected virtual void OnVolumeChanged(double level) {
			player.Volume = level;
			if(level == 0)
				muted = true;
			else
				muted = false;
		}

		protected virtual void OnPausebuttonClicked(object sender, System.EventArgs e)
		{
			player.Pause();
		}

		protected virtual void OnEndOfStream(object o, EventArgs args) {
			player.SeekInSegment(0, GetRateFromScale());
			player.Pause();
		}


		protected virtual void OnError(object o, ErrorArgs args) {
			if(Error != null)
				Error(o, args.Message);
		}

		protected virtual void OnClosebuttonClicked(object sender, System.EventArgs e)
		{
			CloseActualSegment();
		}

		protected virtual void OnPrevbuttonClicked(object sender, System.EventArgs e)
		{
			if (InSegment())
				SeekInSegment (segmentStartTime);
			if(Prev != null)
				Prev();
		}

		protected virtual void OnNextbuttonClicked(object sender, System.EventArgs e)
		{
			if(Next != null)
				Next();
		}

		protected virtual void OnVscale1FormatValue(object o, Gtk.FormatValueArgs args)
		{
			double val = args.Value;
			if(val >25) {
				val = val-25 ;
				args.RetVal = val +"X";
			}
			else if(val ==25) {
				args.RetVal = "1X";
			}
			else if(val <25) {
				args.RetVal = "-"+val+"/25"+"X";
			}
		}

		protected virtual void OnVscale1ValueChanged(object sender, System.EventArgs e)
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

			if(InSegment()) {
				player.SetRateInSegment(val,segmentStopTime);
			}
			else
				player.SetRate(val);
			rate = val;
		}

		protected virtual void OnVideoboxButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
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

		protected virtual void OnVideoboxScrollEvent(object o, Gtk.ScrollEventArgs args)
		{
			switch(args.Event.Direction) {
			case ScrollDirection.Down:
				SeekToPreviousFrame(InSegment());
				break;
			case ScrollDirection.Up:
				SeekToNextFrame(InSegment());
				break;
			case ScrollDirection.Left:
				StepBackward();
				break;
			case ScrollDirection.Right:
				StepForward();
				break;
			}
		}

		protected virtual void OnDrawButtonClicked(object sender, System.EventArgs e)
		{
			int currentTime;

			currentTime = (int)AccurateCurrentTime;
			// If the player has reached the end of the segment the current time
			// will be unseekable and it's not possible to get a frame at this
			// instant. If we exceed the segment stop time, decrease in a
			// milisecond the position.
			if(InSegment() && currentTime >= segmentStopTime)
				currentTime -= 1;
			if(DrawFrame != null)
				DrawFrame(currentTime);
		}
		#endregion
	}
}
