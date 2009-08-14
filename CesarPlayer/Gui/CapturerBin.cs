// CapturerBin.cs
//
//  Copyright (C) 2008-2009 Andoni Morales Alastruey
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
using LongoMatch.Video.Capturer;
using LongoMatch.Video;
using Gtk;

namespace LongoMatch.Gui
{
	
	
	[System.ComponentModel.Category("CesarPlayer")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CapturerBin : Gtk.Bin
	{
		ICapturer capturer;
		
		public CapturerBin()
		{
			this.Build();
			MultimediaFactory factory = new MultimediaFactory();
			capturer = factory.getCapturer();			
			this.capturerhbox.Add((Widget)capturer);
			((Widget)capturer).Show();
		}
		 
		public string OutputFile {
			set{
				this.capturer.OutputFile= value;
			}
			
		}
		
				
		public uint VideoBitrate {
			set{this.capturer.VideoBitrate=value;}
		}
		
		public uint AudioBitrate {
			set{this.capturer.AudioBitrate=value;}
		}
		
		public void TogglePause(){
			this.capturer.TogglePause();
		}
		
		public void Start(){
			this.capturer.Start();
		}
		
		public void Stop(){
			this.capturer.Stop();
		}
		
		public void Run(){
			this.capturer.Run();
		}
				
		public void SetVideoEncoder(LongoMatch.Video.Capturer.GccVideoEncoderType type){
			this.capturer.SetVideoEncoder(type);
		}
		
		public void SetAudioEncoder(LongoMatch.Video.Capturer.GccAudioEncoderType type){
			this.capturer.SetAudioEncoder(type);
		}
		
		public void SetVideoMuxer(LongoMatch.Video.Capturer.GccVideoMuxerType type){
			this.capturer.SetVideoMuxer(type);
		}

		protected virtual void OnRecbuttonClicked (object sender, System.EventArgs e)
		{
			this.Start();
		}

		protected virtual void OnPausebuttonClicked (object sender, System.EventArgs e)
		{
			this.TogglePause();
		}

		protected virtual void OnStopbuttonClicked (object sender, System.EventArgs e)
		{
			this.Stop();
		}
		
		
	}
}
