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
using Gdk;
using GLib;
using LongoMatch.Video;
using LongoMatch.Video.Common;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Utils;
using Mono.Unix;

namespace LongoMatch.Gui
{
	
	
	[System.ComponentModel.Category("CesarPlayer")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CapturerBin : Gtk.Bin
	{
		public event EventHandler CaptureFinished;
		public event ErrorHandler Error;
		
		private Pixbuf logopix;
		private CapturePropertiesStruct captureProps;
		private bool captureStarted;
		private bool capturing;
		private const int THUMBNAIL_MAX_WIDTH = 100;		
		
		ICapturer capturer;
		
		public CapturerBin()
		{
			this.Build();
			captureProps = new CapturePropertiesStruct();
			captureProps.Width = 320;
			captureProps.Height = 240;
			captureProps.VideoBitrate = 1000;
			captureProps.AudioBitrate = 128;
			captureProps.VideoEncoder = VideoEncoderType.H264;
			captureProps.AudioEncoder = AudioEncoderType.Aac;
			captureProps.Muxer = VideoMuxerType.Mp4;
			captureProps.OutputFile = "";
			captureProps.CaptureSourceType = CaptureSourceType.Raw;
			Type = CapturerType.Live;
		}		
		
		public CapturerType Type {
			set {
				if (capturer != null) {
					capturer.Error -= OnError;
					capturer.Stop();
					capturerhbox.Remove(capturer as Gtk.Widget);
				}
				MultimediaFactory factory = new MultimediaFactory();
				capturer = factory.getCapturer(value);	
				capturer.EllapsedTime += OnTick;
				capturer.Error += OnError;
				if (value != CapturerType.Fake){
					capturerhbox.Add((Widget)capturer);
					(capturer as Widget).Visible = true;
					capturerhbox.Visible = true;
					logodrawingarea.Visible = false;
				}
				else{
					logodrawingarea.Visible = true;
					capturerhbox.Visible = false;
				}
				captureStarted = false;
				capturing = false;
				SetProperties();
				pausebutton.Visible = false;
				stopbutton.Visible = false;
			}

		}
		
		public string Logo{
			set{
				try{
					this.logopix = new Pixbuf(value);
				}catch{
					/* FIXME: Add log */
				}
			}
		}
		
		public int CurrentTime {
			get {
				return capturer.CurrentTime;
			}
		}
		
		public bool Capturing{
			get{
				return capturing;
			}
		}
		
		public CapturePropertiesStruct CaptureProperties{
			set{
				captureProps = value;
			}
		}
		
		public void Start(){
			capturing = true;
			captureStarted = true;
			recbutton.Visible = false;
			pausebutton.Visible = true;
			stopbutton.Visible = true;
			capturer.Start();
		}
		
		public void TogglePause(){
			capturing = !capturing;
			recbutton.Visible = !capturing;
			pausebutton.Visible = capturing;
			capturer.TogglePause();
		}
		
		public void Stop() {
			capturing = false;
			capturer.Stop();
		}
		
		public void Run(){
			capturer.Run();
		}

		public void Close(){
			capturer.Close();
			capturing = false;
		}
		
		public Pixbuf CurrentMiniatureFrame {
			get {
				int h, w;
				double rate;
				Pixbuf scaled_pix;
				Pixbuf pix = capturer.CurrentFrame;
				
				if (pix == null)
					return null;
				
				w = pix.Width;
				h = pix.Height;
				rate = (double)w / (double)h;
				
				if (h > w) {
					w = (int)(THUMBNAIL_MAX_WIDTH * rate);
					h = THUMBNAIL_MAX_WIDTH;
				} else {
					h = (int)(THUMBNAIL_MAX_WIDTH / rate);
					w = THUMBNAIL_MAX_WIDTH;
				}
				scaled_pix = pix.ScaleSimple (w, h, Gdk.InterpType.Bilinear);
				pix.Dispose();
					
				return scaled_pix;				                       
			}
		}
		
		private void SetProperties(){
			capturer.OutputFile = captureProps.OutputFile;
			capturer.OutputHeight = captureProps.Height;
			capturer.OutputWidth = captureProps.Width;
			capturer.SetVideoEncoder(captureProps.VideoEncoder);
			capturer.SetAudioEncoder(captureProps.AudioEncoder);
			capturer.SetVideoMuxer(captureProps.Muxer);	
			capturer.SetSource(captureProps.CaptureSourceType);
			capturer.DeviceID = captureProps.DeviceID;
			capturer.VideoBitrate = captureProps.VideoBitrate;
			capturer.AudioBitrate = captureProps.AudioBitrate;
		}

		protected virtual void OnRecbuttonClicked (object sender, System.EventArgs e)
		{
			if (captureStarted == true){
				if (capturing)
					return;
				TogglePause();
			}
			else
				Start();	
		}

		protected virtual void OnPausebuttonClicked (object sender, System.EventArgs e)
		{
			if (capturing)
				TogglePause();						
		}

		protected virtual void OnStopbuttonClicked (object sender, System.EventArgs e)
		{
			int res;
			
			MessageDialog md = new MessageDialog((Gtk.Window)this.Toplevel, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo,
			                                     Catalog.GetString("You are going to stop and finish the current capture."+"\n"+
			                                                       "Do you want to proceed?"));
			res = md.Run();
			md.Destroy();
			if (res == (int)ResponseType.Yes){
				md = new MessageDialog((Gtk.Window)this.Toplevel, DialogFlags.Modal, MessageType.Info, ButtonsType.None,
				                                     Catalog.GetString("Finalizing file. This can take a while"));
				md.Show();
				Stop();
				md.Destroy();
				recbutton.Visible = true;
				pausebutton.Visible = false;
				stopbutton.Visible = false;
				if (CaptureFinished != null)
					CaptureFinished(this, new EventArgs());
			}
		}				
		
		protected virtual void OnTick (int ellapsedTime){
			timelabel.Text = "Time: " + TimeString.MSecondsToSecondsString(CurrentTime);
		}
		
		protected virtual void OnError (object o, ErrorArgs args)
		{
			if (Error != null)
				Error (o, args);
			/* On error, the capturer is not usable anymore. We reset everything
			 * setting the fake capturer */
			if (capturer != null){
				capturer.Dispose();
				capturer = null;
			}
			Type = CapturerType.Fake;
		}
		
		protected virtual void OnLogodrawingareaExposeEvent (object o, Gtk.ExposeEventArgs args)
		{	
			Gdk.Window win;
			Rectangle area;
			Pixbuf frame;
			Pixbuf drawing;
			int width, height, allocWidth, allocHeight, logoX, logoY;
			float ratio;
			
			if (logopix == null)
				return;

			win = logodrawingarea.GdkWindow;
			width = logopix.Width;
			height = logopix.Height;
			allocWidth = logodrawingarea.Allocation.Width;
			allocHeight = logodrawingarea.Allocation.Height;
			area = args.Event.Area;
			
			/* Checking if allocated space is smaller than our logo */
			if ((float) allocWidth / width > (float) allocHeight / height) {
				ratio = (float) allocHeight / height;
			} else {
				ratio = (float) allocWidth / width;
			}
			width = (int) (width * ratio);
			height = (int) (height * ratio);
			
			logoX = (allocWidth / 2) - (width / 2);
			logoY = (allocHeight / 2) - (height / 2);

			/* Drawing our frame */
			frame = new Pixbuf(Colorspace.Rgb, false, 8, allocWidth, allocHeight);
			logopix.Composite(frame, 0, 0, allocWidth, allocHeight, logoX, logoY, 
			                  ratio, ratio, InterpType.Bilinear, 255);
			
			win.DrawPixbuf (this.Style.BlackGC, frame, 0, 0,
			                0, 0, allocWidth, allocHeight,
			                RgbDither.Normal, 0, 0);
			frame.Dispose();
			return;
		}
	}
}
