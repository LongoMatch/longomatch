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
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.UI.Helpers;
using VASUi = VAS.UI;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlayerCapturerBin : VAS.UI.PlayerCapturerBin
	{
		//IPlayerView playerview;
		//PlayerViewOperationMode mode;

		public PlayerCapturerBin ()
		{
			base.Build ();
			/*
			this.Build ();
			replayhbox.HeightRequest = livebox.HeightRequest = StyleConf.PlayerCapturerControlsHeight;
			replayimage.Pixbuf = Misc.LoadIcon ("longomatch-replay", StyleConf.PlayerCapturerIconSize);
			liveimage.Pixbuf = Misc.LoadIcon ("longomatch-live", StyleConf.PlayerCapturerIconSize);
			livelabel.ModifyFg (Gtk.StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteActive));
			replaylabel.ModifyFg (Gtk.StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteActive));
			livebox.Visible = replayhbox.Visible = true;
			playerview = Config.GUIToolkit.GetPlayerView ();
			playerbox.PackEnd (playerview as Gtk.Widget);
			(playerview as Gtk.Widget).ShowAll ();
			Player = playerview.Player;
			*/
		}

		/*
		protected override void OnDestroyed ()
		{
			(playerview as Gtk.Widget).Destroy ();
			capturerbin.Destroy ();
			base.OnDestroyed ();
		}

		public IPlayerController Player {
			set {
				Player.ElementLoadedEvent += HandleElementLoadedEvent;
				Player.PrepareViewEvent += HandlePrepareViewEvent;
			}

			get {
				return playerview.Player;
			}
		}

		public ICapturerBin Capturer {
			get {
				return capturerbin;
			}
		}

		public PlayerViewOperationMode Mode {
			set {
				mode = value;
				if (mode == PlayerViewOperationMode.Analysis) {
					ShowPlayer ();
				} else {
					ShowCapturer ();
				}
				playerview.Mode = value;
				Log.Debug ("CapturerPlayer setting mode " + value);
			}
		}

		public void ShowPlayer ()
		{
			playerbox.Visible = true;
			replayhbox.Visible = false;
			if (mode == PlayerViewOperationMode.LiveAnalysisReview && Config.ReviewPlaysInSameWindow)
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

		public void AttachPlayer (bool attached)
		{
			playerview.PlayerAttached = attached;
		}

		void HandlePrepareViewEvent ()
		{
			ShowPlayer ();
		}
		*/

		protected override void HandleElementLoadedEvent (object element, bool hasNext)
		{
			if (element == null) {
				if (mode == PlayerViewOperationMode.Analysis) {
					return;
				}
				livebox.Visible = replayhbox.Visible = false;
				Player.Pause ();
				ShowCapturer ();
			} else {
				if (element is TimelineEventLongoMatch && mode == PlayerViewOperationMode.LiveAnalysisReview) {
					ShowPlayer ();
					livebox.Visible = replayhbox.Visible = true;
				}
			}
		}

		/*
		protected void OnBacktolivebuttonClicked (object sender, System.EventArgs e)
		{
			Player.Pause ();
			ShowCapturer ();
		}
		*/
	}
}

