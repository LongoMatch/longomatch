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
		private VideoQuality vq;
		private VideoFormat vf;
		private VideoEncoderType vcodec;
		private AudioEncoderType acodec;
		private VideoMuxerType muxer;


		#region Constructors
		public VideoEditionProperties()
		{
			this.Build();
			formatcombobox.AppendText(Constants.MP4);
			formatcombobox.AppendText(Constants.AVI);
			if (System.Environment.OSVersion.Platform != PlatformID.Win32NT) {
				formatcombobox.AppendText(Constants.WEBM);
				formatcombobox.AppendText(Constants.OGG);
				formatcombobox.AppendText(Constants.DVD);
			}
			formatcombobox.Active=0;
		}
		#endregion

		#region Properties

		public VideoQuality VideoQuality {
			get {
				return vq;
			}
		}

		public VideoEncoderType VideoEncoderType {
			get {
				return vcodec;
			}
		}

		public AudioEncoderType AudioEncoderType {
			get {
				return acodec;
			}
		}

		public VideoMuxerType VideoMuxer {
			get {
				return muxer;
			}
		}

		public string Filename {
			get {
				return fileentry.Text;
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

		public VideoFormat VideoFormat {
			get {
				return vf;
			}
		}
		#endregion Properties

		#region Private Methods

		private string GetExtension() {
			if (formatcombobox.ActiveText == Constants.MP4)
				return "mp4";
			else if (formatcombobox.ActiveText == Constants.OGG)
				return "ogg";
			else if (formatcombobox.ActiveText == Constants.WEBM)
				return "webm";
			else if (formatcombobox.ActiveText == Constants.AVI)
				return "avi";
			else
				return "mpg";
		}

		#endregion

		protected virtual void OnButtonOkClicked(object sender, System.EventArgs e)
		{
			if (qualitycombobox.ActiveText == Catalog.GetString("Low")) {
				vq = VideoQuality.Low;
			}
			else if (qualitycombobox.ActiveText == Catalog.GetString("Normal")) {
				vq = VideoQuality.Normal;
			}
			else if (qualitycombobox.ActiveText == Catalog.GetString("Good")) {
				vq = VideoQuality.Good;
			}
			else if (qualitycombobox.ActiveText == Catalog.GetString("Extra")) {
				vq = VideoQuality.Extra;
			}

			vf = (VideoFormat)sizecombobox.Active;

			if (formatcombobox.ActiveText == Constants.MP4) {
				vcodec = VideoEncoderType.H264;
				acodec = AudioEncoderType.Aac;
				muxer = VideoMuxerType.Matroska;
			}
			else if (formatcombobox.ActiveText == Constants.OGG) {
				vcodec = VideoEncoderType.Theora;
				acodec = AudioEncoderType.Vorbis;
				muxer = VideoMuxerType.Ogg;
			}
			else if (formatcombobox.ActiveText == Constants.WEBM) {
				vcodec = VideoEncoderType.VP8;
				acodec = AudioEncoderType.Vorbis;
				muxer = VideoMuxerType.WebM;
			}
			else if (formatcombobox.ActiveText == Constants.AVI) {
				vcodec = VideoEncoderType.Xvid;
				acodec = AudioEncoderType.Mp3;
				muxer = VideoMuxerType.Avi;
			}
			else if (formatcombobox.ActiveText == Constants.DVD) {
				vcodec = VideoEncoderType.Mpeg2;
				acodec = AudioEncoderType.Mp3;
				muxer = VideoMuxerType.MpegPS;
			}
			Hide();
		}

		protected virtual void OnOpenbuttonClicked(object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Save Video As ..."),
			                this,
			                FileChooserAction.Save,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.VideosDir());
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
			if (fChooser.Run() == (int)ResponseType.Accept) {
				fileentry.Text = fChooser.Filename;
			}
			fChooser.Destroy();
		}
		protected virtual void OnButtonCancelClicked (object sender, System.EventArgs e)
		{
			this.Destroy();
		}
		
		
	}
}
