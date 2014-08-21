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
using System.Collections.Generic;
using LongoMatch.Interfaces;
using LongoMatch.Store.Playlists;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayerCapturerBin : Gtk.Bin, IPlayerBin, ICapturerBin
	{	
		/* Player Events */
		public event StateChangeHandler PlayStateChanged;
		
		public enum PlayerOperationMode {
			Player,
			Capturer,
			FakeCapturer,
			PreviewCapturer,
		}
		
		PlayerOperationMode mode;
		bool backLoaded = false;
		
		public PlayerCapturerBin ()
		{
			this.Build ();
			ConnectSignals();
		}
		
		protected override void OnDestroyed ()
		{
			playerbin.Destroy ();
			capturerbin.Destroy ();
			base.OnDestroyed ();
		}
		
		public PlayerOperationMode Mode {
			set {
				mode = value;
				if (mode == PlayerOperationMode.Player) {
					ShowPlayer();
				} else {
					if (value == PlayerOperationMode.FakeCapturer) {
						capturerbin.Mode = CapturerType.Fake;
					} else {
						capturerbin.Mode = CapturerType.Live;
					}
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
		
		public List<string> PeriodsNames {
			set {
				capturerbin.PeriodsNames = value;
			}
		}
		
		public List<Period> PeriodsTimers {
			set {
				capturerbin.Periods = value;
			}
		}

		public void StartPeriod () {
			capturerbin.StartPeriod ();
		}
		
		public void StopPeriod () {
			capturerbin.StopPeriod ();
		}
		
		public void Stop () {
			capturerbin.Stop ();
		}
		
		public void Run (CaptureSettings settings) {
			capturerbin.Run (settings);
		}
#endregion
		
		
#region Player

		public bool SeekingEnabled {
			set {
				playerbin.SeekingEnabled = value;
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
		
		public void Open (MediaFile file) {
			playerbin.Open (file);
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
		
		public void LoadPlayListPlay (Playlist playlist, IPlaylistElement play) {
			playerbin.LoadPlayListPlay (playlist, play);
		}
		
		public void LoadPlay (MediaFile file, Play play, Time seekTime, bool playing) {
			if (mode == PlayerOperationMode.PreviewCapturer) {
				backtolivebutton.Visible = true;
				ShowPlayer ();
				LoadBackgroundPlayer(file);
			}
			playerbin.LoadPlay (file, play, seekTime, playing);
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
			playerbin.PlayStateChanged += delegate (bool playing) {
				if (PlayStateChanged != null)
					PlayStateChanged (playing);
			};
		}
		
		void LoadBackgroundPlayer (MediaFile file) {
			if (backLoaded)
				return;
				
			/* The output video file is now created, it's time to 
				 * load it in the player */
			playerbin.Open (file);
			playerbin.SeekingEnabled = false;
			Log.Debug ("Loading encoded file in the backround player");
			backLoaded = true;
		}
	}
}

