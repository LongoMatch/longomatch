// CapturerBin.cs
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
using LongoMatch.Video;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Utils;

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
			Type = CapturerType.FAKE;		
		}
		
		public CapturerType Type {
			set{
				if (capturer != null){
					capturer.Stop();
					capturerhbox.Remove(capturer as Gtk.Widget);
				}
				MultimediaFactory factory = new MultimediaFactory();
				capturer = factory.getCapturer(value);	
				capturer.EllapsedTime += OnTick;
				capturerhbox.Add((Widget)capturer);
				((Widget)capturer).Show();
			}
		}
		 
		public string OutputFile {
			set{
				capturer.OutputFile= value;
			}			
		}		
				
		public uint VideoBitrate {
			set{capturer.VideoBitrate=value;}			
		}
		
		public uint AudioBitrate {
			set{capturer.AudioBitrate=value;}
		}
		
		public int CurrentTime {
			get{
				return capturer.CurrentTime;
			}
		}
		public void TogglePause(){
			capturer.TogglePause();
		}
		
		public void Start(){
			capturer.Start();
		}
		
		public void Stop(){
			capturer.Stop();
		}
		
		public void SetVideoEncoder(LongoMatch.Video.Capturer.GccVideoEncoderType type){
			capturer.SetVideoEncoder(type);
		}
		
		public void SetAudioEncoder(LongoMatch.Video.Capturer.GccAudioEncoderType type){
			capturer.SetAudioEncoder(type);
		}
		
		public void SetVideoMuxer(LongoMatch.Video.Capturer.GccVideoMuxerType type){
			capturer.SetVideoMuxer(type);
		}

		protected virtual void OnRecbuttonClicked (object sender, System.EventArgs e)
		{
			Start();
			recbutton.Visible = false;
			pausebutton.Visible = true;
			stopbutton.Visible = true;
		}

		protected virtual void OnPausebuttonClicked (object sender, System.EventArgs e)
		{
			TogglePause();
			recbutton.Visible = true;
			pausebutton.Visible = false;			
		}

		protected virtual void OnStopbuttonClicked (object sender, System.EventArgs e)
		{
			Stop();
			recbutton.Visible = true;
			pausebutton.Visible = false;
			stopbutton.Visible = false;
		}				
		
		protected virtual void OnTick (int ellapsedTime){
			timelabel.Text = "Time: " + TimeString.MSecondsToSecondsString(CurrentTime);
		}
	}
}
