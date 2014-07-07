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
using LongoMatch.Store;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayerCapturerBin : Gtk.Bin, IPlayerBin, ICapturerBin
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
		public event PlaybackRateChangedHandler PlaybackRateChanged;
		
		public enum PlayerOperationMode {
			Player,
			Capturer,
			PreviewCapturer,
		}
		
		PlayerOperationMode mode;
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
			if (mode == PlayerOperationMode.PreviewCapturer && Config.ReviewPlaysInSameWindow)
				capturerbin.Visible = true;
			else
				capturerbin.Visible = false;
		}
		
		public void ShowCapturer () {
			playerbin.Visible = false;
			capturerbin.Visible = true;
		}
		
#region Common
		public Time CurrentTime {
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
		public string Logo {
			set {
				capturerbin.Logo = value;
			}
		}
		
		public CaptureSettings CaptureSettings {
			get {
				return capturerbin.CaptureSettings;
			}
		}
		
		public bool Capturing {
			get {
				return capturerbin.Capturing;
			}
		}
		
		public void Start () {
			capturerbin.Start ();
		}
		
		public void TogglePause () {
			capturerbin.TogglePause ();
		}
		
		public void Run (CapturerType type, CaptureSettings settings) {
			capturerbin.Run (type, settings);
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
		
		public Time StreamLength {
			get {
				return playerbin.StreamLength;
			}
		}
		
		public Image CurrentFrame {
			get {
				return playerbin.CurrentFrame;
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
		
		public void ResetGui () {
			playerbin.ResetGui ();
		}
		
		public void LoadPlayListPlay (PlayListPlay play, bool hasNext) {
			playerbin.LoadPlayListPlay (play, hasNext);
		}
		
		public void LoadPlay (string filename, Play play) {
			if (mode == PlayerOperationMode.PreviewCapturer) {
				backtolivebutton.Visible = true;
				ShowPlayer ();
				LoadBackgroundPlayer(filename);
			}
			playerbin.LoadPlay (filename, play);
		}
		
		public void Seek (Time time, bool accurate) {
			playerbin.Seek (time, accurate);
		}
		
		public void SeekToNextFrame () {
			playerbin.SeekToNextFrame ();
		}
		
		public void SeekToPreviousFrame () {
			playerbin.SeekToPreviousFrame ();
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
		
		public void CloseSegment () {
			playerbin.CloseSegment ();
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
			
			capturerbin.Error += delegate(string message) {
				if (Error != null)
					Error (message);
			};
			
			playerbin.Error += delegate(string message) {
				if (Error != null)
					Error (message);
			};
			
			playerbin.SegmentClosedEvent += delegate () {
				if (SegmentClosedEvent != null)
					SegmentClosedEvent ();
			};
			
			playerbin.Tick += delegate (Time t, Time s, double p) {
				if (Tick != null)
					Tick (t, s, p);
			};
			
			playerbin.PlayStateChanged += delegate (bool playing) {
				if (PlayStateChanged != null)
					PlayStateChanged (playing);
			};
			
			playerbin.Next += delegate () {
				if (Next != null)
					Next ();
			};
			
			playerbin.Prev += delegate () {
				if (Prev != null)
					Prev ();
			};
			
			playerbin.DrawFrame += delegate (Time time) {
				if (DrawFrame != null)
					DrawFrame (time);
			};
			
			playerbin.SeekEvent += delegate (Time pos) {
				if (SeekEvent != null)
					SeekEvent (pos);
			};
			
			playerbin.Detach += delegate (bool detach) {
				if (Detach != null)
					Detach (detach);
			};
			
			playerbin.PlaybackRateChanged += (rate) => {
				if (PlaybackRateChanged != null)
					PlaybackRateChanged (rate);
			};
		}
		
		void LoadBackgroundPlayer (string filename) {
			if (backLoaded)
				return;
				
			/* The output video file is now created, it's time to 
				 * load it in the player */
			playerbin.Open (filename);
			playerbin.SeekingEnabled = false;
			Log.Debug ("Loading encoded file in the backround player");
			backLoaded = true;
		}
	}
}

