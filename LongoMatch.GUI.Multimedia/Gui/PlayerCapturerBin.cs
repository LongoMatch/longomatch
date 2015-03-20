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
using LongoMatch.Gui.Helpers;
using LongoMatch.Core.Interfaces;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlayerCapturerBin : Gtk.Bin
	{
		/* Player Events */
		public event StateChangeHandler PlayStateChanged;

		public enum PlayerOperationMode
		{
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
			ConnectSignals ();
			replayhbox.HeightRequest = livebox.HeightRequest = StyleConf.PlayerCapturerControlsHeight;
			replayimage.Pixbuf = Misc.LoadIcon ("longomatch-replay", StyleConf.PlayerCapturerIconSize);
			liveimage.Pixbuf = Misc.LoadIcon ("longomatch-live", StyleConf.PlayerCapturerIconSize);
			livelabel.ModifyFg (Gtk.StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteActive));
			replaylabel.ModifyFg (Gtk.StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteActive));
			livebox.Visible = replayhbox.Visible = false;
			playerbin.CloseEvent += HandleCloseClicked;
			playerbin.PrepareLoadEvent += HandlePrepareLoadEvent;
		}

		protected override void OnDestroyed ()
		{
			playerbin.Destroy ();
			capturerbin.Destroy ();
			base.OnDestroyed ();
		}

		public IPlayerController Player {
			get {
				return playerbin;
			}
		}

		public ICapturerBin Capturer {
			get {
				return capturerbin;
			}
		}

		public PlayerOperationMode Mode {
			set {
				mode = value;
				if (mode == PlayerOperationMode.Player) {
					ShowPlayer ();
					playerbin.Compact = false;
					playerbin.CloseAlwaysVisible = false;
				} else {
					ShowCapturer ();
					playerbin.CloseAlwaysVisible = true;
					playerbin.Compact = true;
				}
				Log.Debug ("CapturerPlayer setting mode " + value);
				backLoaded = false;
			}
		}

		public void ShowPlayer ()
		{
			playerbox.Visible = true;
			replayhbox.Visible = false;
			if (mode == PlayerOperationMode.PreviewCapturer && Config.ReviewPlaysInSameWindow)
				capturerbox.Visible = true;
			else
				capturerbox.Visible = false;
		}

		public void ShowCapturer ()
		{
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

		public void Close ()
		{
			playerbin.Close ();
			capturerbin.Close ();
		}

		#endregion

		void HandlePrepareLoadEvent (MediaFileSet fileSet)
		{
			if (mode == PlayerOperationMode.PreviewCapturer) {
				ShowPlayer ();
				LoadBackgroundPlayer (fileSet);
				livebox.Visible = replayhbox.Visible = true;
			}
		}

		protected void OnBacktolivebuttonClicked (object sender, System.EventArgs e)
		{
			playerbin.Pause ();
			ShowCapturer ();
		}

		void ConnectSignals ()
		{
			playerbin.PlayStateChanged += delegate (bool playing) {
				if (PlayStateChanged != null)
					PlayStateChanged (playing);
			};
		}

		void LoadBackgroundPlayer (MediaFileSet file)
		{
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

