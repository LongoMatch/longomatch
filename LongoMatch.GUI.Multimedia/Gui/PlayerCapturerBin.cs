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

using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using System.Collections.Generic;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Gui.Helpers;

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
			replayhbox.HeightRequest = livebox.HeightRequest = StyleConf.PlayerCapturerControlsHeight;
			replayimage.Pixbuf = Misc.LoadIcon ("longomatch-replay", StyleConf.PlayerCapturerIconSize);
			liveimage.Pixbuf = Misc.LoadIcon ("longomatch-live", StyleConf.PlayerCapturerIconSize);
			livelabel.ModifyFg (Gtk.StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteActive));
			replaylabel.ModifyFg (Gtk.StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteActive));
			livebox.Visible = replayhbox.Visible = false;
			playerbin.CloseEvent += HandleCloseClicked;
		}

		protected override void OnDestroyed ()
		{
			playerbin.Destroy ();
			capturerbin.Destroy ();
			base.OnDestroyed ();
		}
		
		public IPlayerBin Player {
			get {
				return playerbin;
			}
		}
		
		public PlayerOperationMode Mode {
			set {
				mode = value;
				if (mode == PlayerOperationMode.Player) {
					ShowPlayer();
					playerbin.Compact = false;
					playerbin.CloseAlwaysVisible = false;
				} else {
					ShowCapturer();
					playerbin.CloseAlwaysVisible = true;
					playerbin.Compact = true;
				}
				Log.Debug ("CapturerPlayer setting mode " + value);
				backLoaded = false;
			}
		}
		
		public void ShowPlayer () {
			playerbox.Visible = true;
			replayhbox.Visible = false;
			if (mode == PlayerOperationMode.PreviewCapturer && Config.ReviewPlaysInSameWindow)
				capturerbox.Visible = true;
			else
				capturerbox.Visible = false;
		}
		
		public void ShowCapturer () {
			playerbox.Visible = false;
			livebox.Visible = false;
			capturerbox.Visible = true;
		}

		void HandleCloseClicked (object sender, EventArgs e)
		{
			if (mode == PlayerOperationMode.Player) {
				return;
			}
			livebox.Visible = replayhbox.Visible = false;
			playerbin.Pause ();
			ShowCapturer ();
		}
		
#region Common
		public void Close () {
			playerbin.Close ();
			capturerbin.Close ();
		}
		
#endregion

#region Capturer

		public Time CurrentCaptureTime {
			get {
				return capturerbin.CurrentCaptureTime;
			}
		}
		
		public Image CurrentCaptureFrame {
			get {
				return capturerbin.CurrentCaptureFrame;
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
		
		public List<string> PeriodsNames {
			set {
				capturerbin.PeriodsNames = value;
			}
		}
		
		public List<Period> Periods {
			get {
				return capturerbin.Periods;
			}
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
		
		public void Run (CaptureSettings settings) {
			capturerbin.Run (settings);
		}
		
		public void PausePeriod ()
		{
			capturerbin.PausePeriod ();
		}

		public void ResumePeriod ()
		{
			capturerbin.ResumePeriod ();
		}

#endregion
		
		
#region Player

		public Time CurrentTime {
			get {
				return playerbin.CurrentTime;
			}
		}
		
		public bool Playing {
			get {
				return playerbin.Playing;
			}
		}
		
		public MediaFileAngle ActiveAngle {
			get {
				return playerbin.ActiveAngle;
			}
		}
		
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
		
		public Image CurrentMiniatureFrame {
			get {
				return playerbin.CurrentMiniatureFrame;
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
		
		public void Open (MediaFileSet fileSet) {
			playerbin.Open (fileSet);
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
		
		public void LoadPlay (MediaFileSet fileSet, TimelineEvent play, Time seekTime, bool playing) {
			if (mode == PlayerOperationMode.PreviewCapturer) {
				ShowPlayer ();
				LoadBackgroundPlayer(fileSet);
				livebox.Visible = replayhbox.Visible = true;
			}
			playerbin.LoadPlay (fileSet, play, seekTime, playing);
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
			playerbin.Pause();
			ShowCapturer ();
		}
		
		void ConnectSignals () {
			playerbin.PlayStateChanged += delegate (bool playing) {
				if (PlayStateChanged != null)
					PlayStateChanged (playing);
			};
		}
		
		void LoadBackgroundPlayer (MediaFileSet file) {
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

