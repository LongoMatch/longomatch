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
using LongoMatch.Common;
using Misc = LongoMatch.Gui.Helpers.Misc;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class VideoPreferencesPanel : Gtk.Bin
	{
		public VideoPreferencesPanel ()
		{
			this.Build ();
			Misc.FillImageFormat (renderimagecombo, Config.RenderVideoStandard);
			Misc.FillEncodingFormat (renderenccombo, Config.RenderEncodingProfile);
			Misc.FillQuality (renderqualcombo, Config.RenderEncodingQuality);
			
			Misc.FillImageFormat (captureimagecombo, Config.CaptureVideoStandard);
			Misc.FillEncodingFormat (captureenccombo, Config.CaptureEncodingProfile);
			Misc.FillQuality (capturequalcombo, Config.CaptureEncodingQuality);
			
			renderimagecombo.Changed += HandleImageChanged;
			captureimagecombo.Changed += HandleImageChanged;
			
			renderenccombo.Changed += HandleEncodingChanged;  
			captureenccombo.Changed += HandleEncodingChanged;  
			
			renderqualcombo.Changed += HandleQualityChanged;
			captureimagecombo.Changed += HandleImageChanged;
		}

		void HandleQualityChanged (object sender, EventArgs e)
		{
			EncodingQuality qual;
			ListStore store;
			TreeIter iter;
			ComboBox combo = sender as ComboBox;
			
			combo.GetActiveIter (out iter);
			store = combo.Model as ListStore;
			qual = (EncodingQuality) store.GetValue(iter, 1);
			
			if (combo == renderqualcombo)
				Config.RenderEncodingQuality = qual;
			else
				Config.CaptureEncodingQuality = qual;
		}

		void HandleImageChanged (object sender, EventArgs e)
		{
			VideoStandard std;
			ListStore store;
			TreeIter iter;
			ComboBox combo = sender as ComboBox;
			
			combo.GetActiveIter (out iter);
			store = combo.Model as ListStore;
			std = (VideoStandard) store.GetValue(iter, 1);
			
			if (combo == renderimagecombo)
				Config.RenderVideoStandard = std;
			else
				Config.CaptureVideoStandard = std;
			
		}

		void HandleEncodingChanged (object sender, EventArgs e)
		{
			EncodingProfile enc;
			ListStore store;
			TreeIter iter;
			ComboBox combo = sender as ComboBox;
			
			combo.GetActiveIter (out iter);
			store = combo.Model as ListStore;
			enc = (EncodingProfile) store.GetValue(iter, 1);
			
			if (combo == renderenccombo)
				Config.RenderEncodingProfile = enc;
			else
				Config.CaptureEncodingProfile = enc;
			
		}
	}
}

