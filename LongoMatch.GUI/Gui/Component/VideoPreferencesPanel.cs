//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using Gtk;
using Misc = VAS.UI.Helpers.Misc;
using VAS.Core.Common;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class VideoPreferencesPanel : Gtk.Bin
	{
		CheckButton overlayTitle, enableSound, addWatermark;

		public VideoPreferencesPanel ()
		{
			this.Build ();

			if (App.Current.Config.FPS_N == 30) {
				fpscombobox.Active = 1;
			} else if (App.Current.Config.FPS_N == 50) {
				fpscombobox.Active = 2;
			} else if (App.Current.Config.FPS_N == 60) {
				fpscombobox.Active = 3;
			} else {
				fpscombobox.Active = 0;
			}
			fpscombobox.Changed += HandleFPSChanged;
			Misc.FillImageFormat (renderimagecombo, VideoStandards.Rendering, App.Current.Config.RenderVideoStandard);
			Misc.FillEncodingFormat (renderenccombo, App.Current.Config.RenderEncodingProfile);
			Misc.FillQuality (renderqualcombo, App.Current.Config.RenderEncodingQuality);

			Misc.FillImageFormat (captureimagecombo, VideoStandards.Capture, App.Current.Config.CaptureVideoStandard);
			Misc.FillEncodingFormat (captureenccombo, App.Current.Config.CaptureEncodingProfile);
			Misc.FillQuality (capturequalcombo, App.Current.Config.CaptureEncodingQuality);

			renderimagecombo.Changed += HandleImageChanged;
			captureimagecombo.Changed += HandleImageChanged;

			renderenccombo.Changed += HandleEncodingChanged;
			captureenccombo.Changed += HandleEncodingChanged;

			renderqualcombo.Changed += HandleQualityChanged;
			capturequalcombo.Changed += HandleQualityChanged;

			enableSound = new CheckButton ();
			rendertable.Attach (enableSound, 1, 2, 3, 4,
				AttachOptions.Fill,
				AttachOptions.Fill, 0, 0);
			enableSound.CanFocus = false;
			enableSound.Show ();
			enableSound.Active = App.Current.Config.EnableAudio;
			enableSound.Toggled += (sender, e) => {
				App.Current.Config.EnableAudio = enableSound.Active;
			};

			overlayTitle = new CheckButton ();
			rendertable.Attach (overlayTitle, 1, 2, 4, 5,
				AttachOptions.Fill,
				AttachOptions.Fill, 0, 0);
			overlayTitle.CanFocus = false;
			overlayTitle.Show ();
			overlayTitle.Active = App.Current.Config.OverlayTitle;
			overlayTitle.Toggled += (sender, e) => {
				App.Current.Config.OverlayTitle = overlayTitle.Active;
			};

			addWatermark = new CheckButton ();
			rendertable.Attach (addWatermark, 1, 2, 5, 6,
				AttachOptions.Fill,
				AttachOptions.Fill, 0, 0);
			addWatermark.CanFocus = false;
			addWatermark.Show ();

			if (App.Current.LicenseManager != null) {
				bool canRemoveWatermark = App.Current.LicenseLimitationsService.CanExecute (VASFeature.Watermark.ToString ());
				addWatermark.Sensitive = canRemoveWatermark;
				if (!canRemoveWatermark) {
					addWatermark.Active = true;
				} else {
					addWatermark.Active = App.Current.Config.AddWatermark;
				}
			}

			addWatermark.Toggled += (sender, e) => {
				App.Current.Config.AddWatermark = addWatermark.Active;
			};

			SizeGroup sgroup = new SizeGroup (SizeGroupMode.Horizontal);
			SizeGroup sgroup2 = new SizeGroup (SizeGroupMode.Horizontal);
			foreach (Widget w in generaltable) {
				if (w is Label) {
					sgroup.AddWidget (w);
				} else {
					sgroup2.AddWidget (w);
				}
			}
			foreach (Widget w in capturetable) {
				if (w is Label) {
					sgroup.AddWidget (w);
				} else {
					sgroup2.AddWidget (w);
				}
			}
			foreach (Widget w in rendertable) {
				if (w is Label) {
					sgroup.AddWidget (w);
				} else {
					sgroup2.AddWidget (w);
				}
			}
		}

		void HandleFPSChanged (object sender, EventArgs e)
		{
			App.Current.Config.FPS_D = 1;
			if (fpscombobox.Active == 0) {
				App.Current.Config.FPS_N = 25;
			} else if (fpscombobox.Active == 1) {
				App.Current.Config.FPS_N = 30;
			} else if (fpscombobox.Active == 2) {
				App.Current.Config.FPS_N = 50;
			} else if (fpscombobox.Active == 3) {
				App.Current.Config.FPS_N = 60;
			}
		}

		void HandleQualityChanged (object sender, EventArgs e)
		{
			EncodingQuality qual;
			ListStore store;
			TreeIter iter;
			ComboBox combo = sender as ComboBox;

			combo.GetActiveIter (out iter);
			store = combo.Model as ListStore;
			qual = (EncodingQuality)store.GetValue (iter, 1);

			if (combo == renderqualcombo)
				App.Current.Config.RenderEncodingQuality = qual;
			else
				App.Current.Config.CaptureEncodingQuality = qual;
		}

		void HandleImageChanged (object sender, EventArgs e)
		{
			VideoStandard std;
			ListStore store;
			TreeIter iter;
			ComboBox combo = sender as ComboBox;

			combo.GetActiveIter (out iter);
			store = combo.Model as ListStore;
			std = (VideoStandard)store.GetValue (iter, 1);

			if (combo == renderimagecombo)
				App.Current.Config.RenderVideoStandard = std;
			else
				App.Current.Config.CaptureVideoStandard = std;

		}

		void HandleEncodingChanged (object sender, EventArgs e)
		{
			EncodingProfile enc;
			ListStore store;
			TreeIter iter;
			ComboBox combo = sender as ComboBox;

			combo.GetActiveIter (out iter);
			store = combo.Model as ListStore;
			enc = (EncodingProfile)store.GetValue (iter, 1);

			if (combo == renderenccombo)
				App.Current.Config.RenderEncodingProfile = enc;
			else
				App.Current.Config.CaptureEncodingProfile = enc;

		}
	}
}

