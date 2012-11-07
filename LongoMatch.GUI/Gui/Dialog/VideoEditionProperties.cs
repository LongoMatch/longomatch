// VideoEditionProperties.cs
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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using Gtk;
using Mono.Unix;
using LongoMatch.Video.Editor;
using LongoMatch.Video.Common;
using LongoMatch.Common;

namespace LongoMatch.Gui.Dialog
{

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class VideoEditionProperties : Gtk.Dialog
	{
		EncodingSettings encSettings;
		ListStore stdStore;
		ListStore encStore;

		#region Constructors
		public VideoEditionProperties()
		{
			this.Build();
			encSettings = new EncodingSettings();
			FillVideoStandards();
			FillEncodingProfiles();
		}
		#endregion

		#region Properties

		public EncodingSettings EncodingSettings{
			get {
				return encSettings;
			}
		}

		public bool EnableAudio {
			get {
				return audiocheckbutton.Active;
			}
		}

		public bool TitleOverlay {
			get {
				return descriptioncheckbutton.Active;
			}
		}

		#endregion Properties

		#region Private Methods

		private string GetExtension() {
			TreeIter iter;
			formatcombobox.GetActiveIter(out iter);
			return ((EncodingProfile) encStore.GetValue(iter, 1)).Extension;
		}

		#endregion

		private void FillVideoStandards() {
			stdStore = new ListStore(typeof(string), typeof (VideoStandard));
			stdStore.AppendValues(VideoStandards.P240_4_3.Name, VideoStandards.P240_4_3);
			stdStore.AppendValues(VideoStandards.P240_16_9.Name, VideoStandards.P240_16_9);
			stdStore.AppendValues(VideoStandards.P480_4_3.Name, VideoStandards.P480_4_3);
			stdStore.AppendValues(VideoStandards.P480_16_9.Name, VideoStandards.P480_16_9);
			stdStore.AppendValues(VideoStandards.P720_4_3.Name, VideoStandards.P720_4_3);
			stdStore.AppendValues(VideoStandards.P720_16_9.Name, VideoStandards.P720_16_9);
			stdStore.AppendValues(VideoStandards.P1080_4_3.Name, VideoStandards.P1080_4_3);
			stdStore.AppendValues(VideoStandards.P1080_16_9.Name, VideoStandards.P1080_16_9);
			sizecombobox.Model = stdStore;
			sizecombobox.Active = 2;
		}

		private void FillEncodingProfiles() {
			encStore = new ListStore(typeof(string), typeof (EncodingProfile));
			encStore.AppendValues(EncodingProfiles.MP4.Name, EncodingProfiles.MP4);
			encStore.AppendValues(EncodingProfiles.Avi.Name, EncodingProfiles.Avi);
			encStore.AppendValues(EncodingProfiles.WebM.Name, EncodingProfiles.WebM);
			formatcombobox.Model = encStore;
			formatcombobox.Active = 0;
		}
		
		protected virtual void OnButtonOkClicked(object sender, System.EventArgs e)
		{
			TreeIter iter;
			
			if(qualitycombobox.ActiveText == Catalog.GetString("Low")) {
				encSettings.VideoBitrate = (uint) VideoQuality.Low;
				encSettings.AudioBitrate = (uint) AudioQuality.Low;
			}
			else if(qualitycombobox.ActiveText == Catalog.GetString("Normal")) {
				encSettings.VideoBitrate = (uint) VideoQuality.Normal;
				encSettings.AudioBitrate =(uint)  AudioQuality.Normal;
			}
			else if(qualitycombobox.ActiveText == Catalog.GetString("Good")) {
				encSettings.VideoBitrate =(uint)  VideoQuality.Good;
				encSettings.AudioBitrate =(uint)  AudioQuality.Good;
			}
			else if(qualitycombobox.ActiveText == Catalog.GetString("Extra")) {
				encSettings.VideoBitrate =(uint)  VideoQuality.Extra;
				encSettings.AudioBitrate =(uint)  AudioQuality.Extra;
			}

			/* Get size info */
			sizecombobox.GetActiveIter(out iter);
			encSettings.VideoStandard = (VideoStandard) stdStore.GetValue(iter, 1);
			
			/* Get encoding profile info */
			formatcombobox.GetActiveIter(out iter);
			encSettings.EncodingProfile = (EncodingProfile) encStore.GetValue(iter, 1);
			
			encSettings.OutputFile = filelabel.Text;
			
			/* FIXME: Configure with the UI */
			encSettings.Framerate_n = 25;
			encSettings.Framerate_d = 1;
			
			encSettings.TitleSize = 20; 
			
			Hide();
		}

		protected virtual void OnOpenbuttonClicked(object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Save Video As ..."),
			                this,
			                FileChooserAction.Save,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(Config.VideosDir());
			fChooser.CurrentName = "NewVideo."+GetExtension();
			fChooser.DoOverwriteConfirmation = true;
			FileFilter filter = new FileFilter();
			filter.Name = "Multimedia Files";
			filter.AddPattern("*.mkv");
			filter.AddPattern("*.mp4");
			filter.AddPattern("*.ogg");
			filter.AddPattern("*.avi");
			filter.AddPattern("*.mpg");
			filter.AddPattern("*.vob");
			fChooser.Filter = filter;
			if(fChooser.Run() == (int)ResponseType.Accept) {
				filelabel.Text = fChooser.Filename;
			}
			fChooser.Destroy();
		}
		protected virtual void OnButtonCancelClicked(object sender, System.EventArgs e)
		{
			this.Destroy();
		}


	}
}
