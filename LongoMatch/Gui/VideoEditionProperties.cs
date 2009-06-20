// VideoEditionProperties.cs
//
//  Copyright (C) 2008 Andoni Morales Alastruey
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using Gtk;
using Mono.Unix;
using LongoMatch.Video.Editor;

namespace LongoMatch.Gui.Dialog
{
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class VideoEditionProperties : Gtk.Dialog
	{
		private VideoQuality vq;
		private VideoFormat vf;
		private VideoCodec vcodec;
		private AudioCodec acodec;
		private VideoMuxer muxer;

		
		public VideoEditionProperties()
		{
			this.Build();			
		}
		
		public VideoQuality VideoQuality{
			get{return vq;}
		}
		
		public VideoCodec VideoCodec{
			get{return vcodec;}
		}
		
		public AudioCodec AudioCodec{
			get{return acodec;}
		}
		
		public VideoMuxer VideoMuxer{
			get{return muxer;}
		}
		
		public string Filename{
			get{return fileentry.Text;}
		}
		
		public bool EnableAudio{
			get{return audiocheckbutton.Active;}
		}
		
		public bool TitleOverlay{
			get{return descriptioncheckbutton.Active;}		
		}
		
		public VideoFormat VideoFormat{
			get{return vf;}
		}		

		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			if (qualitycombobox.ActiveText == Catalog.GetString("Low")){
				vq = VideoQuality.Low;
			}
			else if (qualitycombobox.ActiveText == Catalog.GetString("Normal")){
				vq = VideoQuality.Normal;
			}
			else if (qualitycombobox.ActiveText == Catalog.GetString("Good")){
				vq = VideoQuality.Good;
			}
			else if (qualitycombobox.ActiveText == Catalog.GetString("Extra")){
				vq = VideoQuality.Extra;
			}
			
			if (sizecombobox.ActiveText == "TV (4:3 - 720x540)"){
				vf = VideoFormat.TV;
			}			
			else if (sizecombobox.ActiveText == "HD 720p (16:9 - 1280x720)"){
				vf = VideoFormat.HD720p;
			}
			else if (sizecombobox.ActiveText == "Full HD 1080p (16:9 - 1920x1080)"){
				vf = VideoFormat.HD1080p;
			}	
			
			
			if (formatcombobox.ActiveText == "Matroska (H.264)"){
				vcodec = VideoCodec.H264;
				acodec = AudioCodec.AAC;
				muxer = VideoMuxer.MKV;
			}	
			else if (formatcombobox.ActiveText == "Matroska (Theora)"){
				vcodec = VideoCodec.THEORA;
				acodec = AudioCodec.VORBIS;
				muxer = VideoMuxer.MKV;
			}	
			else if (formatcombobox.ActiveText == "Avi (Xvid)"){
				vcodec = VideoCodec.XVID;
				acodec = AudioCodec.MP3;
				muxer = VideoMuxer.AVI;
			}
			else if (formatcombobox.ActiveText == "DVD (MPEG-2)"){
				Console.WriteLine("DVD Muxer");
				vcodec = VideoCodec.MPEG2_VIDEO;
				acodec = AudioCodec.MPEG2_AUDIO;
				muxer = VideoMuxer.DVD;
			}
						
			Hide();
		}


		protected virtual void OnOpenbuttonClicked (object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Save Video As ..."),
			                                                   this,
			                                                   FileChooserAction.Save,
			                                                   "gtk-cancel",ResponseType.Cancel,
			                                                   "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.VideosDir());
			fChooser.CurrentName = "NewVideo.mkv";
			fChooser.DoOverwriteConfirmation = true;
			FileFilter filter = new FileFilter();
			filter.Name = "Matroska File";
			filter.AddPattern("*.mkv");
			fChooser.Filter = filter;
			if (fChooser.Run() == (int)ResponseType.Accept){						
				fileentry.Text = fChooser.Filename;
			}
		
			fChooser.Destroy();
		}
	}
}
