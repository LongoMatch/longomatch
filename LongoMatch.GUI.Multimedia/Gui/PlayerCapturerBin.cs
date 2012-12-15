// 
//  Copyright (C) 2012 Andoni Morales Alastruey
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

using LongoMatch.Handlers;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Common;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayerCapturerBin : Gtk.Bin, IPlayer, ICapturer
	{	
		/* Common events */
		public event ErrorHandler Error;
		
		/* Capturer events */
		public event EventHandler CaptureFinished;
		
		/* Player Events */
		public event SegmentClosedHandler SegmentClosedEvent;
		public event TickHandler Tick;
		public event StateChangeHandler PlayStateChanged;
		public event NextButtonClickedHandler Next;
		public event PrevButtonClickedHandler Prev;
		public event DrawFrameHandler DrawFrame;
		public event SeekEventHandler SeekEvent;
		public event DetachPlayerHandler Detach;
		
		public enum PlayerOperationMode {
			Player,
			Capturer,
			PreviewCapturer,
		}
		
		PlayerOperationMode mode;
		CaptureSettings captureSettings;
		bool backLoaded = false;
		
		public PlayerCapturerBin ()
		{
			this.Build ();
			ConnectSignals();
		}
		
		public PlayerOperationMode Mode {
			set {
				mode = value;
				if (mode == PlayerOperationMode.Player) {
					playerbin.ExpandLogo = true;
					ShowPlayer();
				} else {
					ShowCapturer();
				}
				backtolivebutton.Visible = false;
				Log.Debug ("CapturerPlayer setting mode " + value);
				backLoaded = false;
			}
		}
		
		public void ShowPlayer () {
			playerbin.Visible = true;
			capturerbin.Visible = false;
		}
		
		public void ShowCapturer () {
			playerbin.Visible = false;
			capturerbin.Visible = true;
		}
		
#region Common
		public int CurrentTime {
			get {
				if (mode == PlayerOperationMode.Player)
					return playerbin.CurrentTime;
				else
					return capturerbin.CurrentTime;
			}
		}
		
		public Image CurrentMiniatureFrame {
			get {
				if (mode == PlayerOperationMode.Player)
					return playerbin.CurrentMiniatureFrame;
				else
					return capturerbin.CurrentMiniatureFrame;
			}
		}
		
		public void Close () {
			playerbin.Close ();
			capturerbin.Close ();
		}
		
#endregion

#region Capturer
		public CapturerType Type {
			set {
				capturerbin.Type = value;
			}
		}
		
		public string Logo {
			set {
				capturerbin.Logo = value;
			}
		}
		
		public bool Capturing {
			get {
				return capturerbin.Capturing;
			}
		}
		
		public CaptureSettings CaptureProperties {
			set {
				captureSettings = value;
				capturerbin.CaptureProperties = value;
			}
		}
		
		public void Start () {
			capturerbin.Start ();
		}
		
		public void TogglePause () {
			capturerbin.TogglePause ();
		}
		
		public void Stop () {
			capturerbin.Stop ();
		}
		
		public void Run () {
			capturerbin.Run ();
		}
#endregion
		
		
#region Player

		public bool SeekingEnabled {
			set {
				playerbin.SeekingEnabled = value;
			}
		}
		
		public bool Detached {
			set {
				playerbin.Detached = value;
			}
			get {
				return playerbin.Detached;
			}
		}
		
		public long AccurateCurrentTime {
			get {
				return playerbin.AccurateCurrentTime;
			}
		}
		
		public long StreamLength {
			get {
				return playerbin.StreamLength;
			}
		}
		
		public Image CurrentFrame {
			get {
				return playerbin.CurrentFrame;
			}
		}
		
		public Image LogoPixbuf {
			set {
				playerbin.LogoPixbuf = value;
			}
		}
		
		public Image DrawingPixbuf {
			set {
				playerbin.DrawingPixbuf = value;
			}
		}
		
		public bool DrawingMode {
			set {
				playerbin.DrawingMode = value;
			}
		}
		
		public bool LogoMode {
			set {
				playerbin.LogoMode = value;
			}
		}
		
		public bool ExpandLogo {
			set {
				playerbin.ExpandLogo = value;
			}
			
			get {
				return playerbin.ExpandLogo;
			}
		}
		
		public bool Opened {
			get {
				return playerbin.Opened;
			}
		}
		
		public bool FullScreen {
			set {
				playerbin.FullScreen = value;
			}
		}
		
		public float Rate {
			get {
				return playerbin.Rate;
			}
			set {
				playerbin.Rate = value;
			}
		}

		public void Open (string mrl) {
			playerbin.Open (mrl);
		}
		
		public void Play () {
			playerbin.Play ();
		}
		
		public void Pause () {
			playerbin.Pause ();
		}
		
		public void TogglePlay () {
			playerbin.TogglePlay ();
		}
		
		public void SetLogo (string filename) {
			playerbin.SetLogo (filename);
		}
		
		public void ResetGui () {
			playerbin.ResetGui ();
		}
		
		public void SetPlayListElement (string fileName,long start, long stop, float rate, bool hasNext) {
			playerbin.SetPlayListElement (fileName, start, stop, rate, hasNext);
		}
		
		public void SeekTo (long time, bool accurate) {
			playerbin.SeekTo (time, accurate);
		}
		
		public void SeekInSegment (long pos) {
			playerbin.SeekInSegment (pos);
		}
		
		public void SeekToNextFrame (bool in_segment) {
			playerbin.SeekToNextFrame (in_segment);
		}
		
		public void SeekToPreviousFrame (bool in_segment) {
			playerbin.SeekToPreviousFrame (in_segment);
		}
		
		public void StepForward () {
			playerbin.StepForward ();
		}
		
		public void StepBackward () {
			playerbin.StepBackward ();
		}
		
		public void FramerateUp () {
			playerbin.FramerateUp ();
		}
		
		public void FramerateDown () {
			playerbin.FramerateDown ();
		}
		
		public void UpdateSegmentStartTime (long start) {
			playerbin.UpdateSegmentStartTime (start);
		}
		
		public void UpdateSegmentStopTime (long stop) {
			playerbin.UpdateSegmentStopTime (stop);
		}
		
		public void SetStartStop (long start, long stop) {
			if (mode == PlayerOperationMode.PreviewCapturer) {
				backtolivebutton.Visible = true;
				LoadBackgroundPlayer();
				ShowPlayer ();
			}
			playerbin.SetStartStop (start, stop);
		}
		
		public void CloseActualSegment () {
			playerbin.CloseActualSegment ();
		}
		
		public void SetSensitive () {
			playerbin.SetSensitive ();
		}
		
		public void UnSensitive () {
			playerbin.UnSensitive ();
		}
#endregion

		protected void OnBacktolivebuttonClicked (object sender, System.EventArgs e)
		{
			backtolivebutton.Visible = false;
			playerbin.Pause();
			ShowCapturer ();
		}
		
		void ConnectSignals () {
			capturerbin.CaptureFinished += delegate(object sender, EventArgs e) {
				if (CaptureFinished != null)
					CaptureFinished (sender, e);
			};
			
			capturerbin.Error += delegate(object sender, string message) {
				if (Error != null)
					Error (sender, message);
			};
			
			playerbin.Error += delegate(object sender, string message) {
				if (Error != null)
					Error (sender, message);
			};
			
			playerbin.SegmentClosedEvent += delegate () {
				if (SegmentClosedEvent != null)
					SegmentClosedEvent ();
			};
			
			playerbin.Tick += delegate (object o, long currentTime, long streamLength,
			                            float currentPosition, bool seekable) {
				if (Tick != null)
					Tick (o, currentTime, streamLength, currentPosition, seekable);
			};
			
			playerbin.PlayStateChanged += delegate (object sender, bool playing) {
				if (PlayStateChanged != null)
					PlayStateChanged (sender, playing);
			};
			
			playerbin.Next += delegate () {
				if (Next != null)
					Next ();
			};
			
			playerbin.Prev += delegate () {
				if (Prev != null)
					Prev ();
			};
			
			playerbin.DrawFrame += delegate (int time) {
				if (DrawFrame != null)
					DrawFrame (time);
			};
			
			playerbin.SeekEvent += delegate (long pos) {
				if (SeekEvent != null)
					SeekEvent (pos);
			};
			
			playerbin.Detach += delegate (bool detach) {
				if (Detach != null)
					Detach (detach);
			};
		}
		
		void LoadBackgroundPlayer () {
			if (backLoaded)
				return;
				
			if (mode == PlayerOperationMode.PreviewCapturer) {
				/* The output video file is now created, it's time to 
				 * load it in the player */
				playerbin.SetSensitive ();
				playerbin.Open (captureSettings.EncodingSettings.OutputFile);
				playerbin.LogoMode = false;
				playerbin.SeekingEnabled = false;
				Log.Debug ("Loading encoded file in the backround player");
				backLoaded = true;
			}
		}
	}
}

