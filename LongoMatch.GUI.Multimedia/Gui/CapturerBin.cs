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

using Image = LongoMatch.Common.Image;
using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Interfaces.Multimedia;
using LongoMatch.Gui.Helpers;
using LongoMatch.Video;
using LongoMatch.Video.Common;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Utils;
using Mono.Unix;
using LongoMatch.Store;
using LongoMatch.Multimedia.Utils;

namespace LongoMatch.Gui
{


	[System.ComponentModel.Category("CesarPlayer")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CapturerBin : Gtk.Bin, ICapturerBin
	{
		public event EventHandler CaptureFinished;
		public event ErrorHandler Error;

		Image logopix;
		CaptureSettings settings;
		CapturerType type;
		bool captureStarted, capturing, delayStart;
		ICapturer capturer;

		public CapturerBin()
		{
			this.Build();
			recbutton.Clicked += OnRecbuttonClicked;
			pausebutton.Clicked += OnPausebuttonClicked;
			stopbutton.Clicked += OnStopbuttonClicked;
			videodrawingarea.CanFocus = false;
			
			videodrawingarea.Realized += (sender, e) => {
				if (delayStart) {
					Configure ();
					capturer.Run ();
				}
			};
		}

		public string Logo {
			set {
				try {
					this.logopix = new Image(new Gdk.Pixbuf(value));
				} catch {
					/* FIXME: Add log */
				}
			}
		}

		public Time CurrentTime {
			get {
				if(capturer == null)
					return new Time (-1);
				return capturer.CurrentTime;
			}
		}

		public bool Capturing {
			get {
				return capturing;
			}
		}
		
		public CaptureSettings CaptureSettings {
			get {
				return settings;
			}
		}

		public void Start() {
			if(capturer == null)
				return;

			capturing = true;
			captureStarted = true;
			recbutton.Visible = false;
			pausebutton.Visible = true;
			stopbutton.Visible = true;
			capturer.Start();
		}

		public void TogglePause() {
			if(capturer == null)
				return;

			if (capturing) {
				string msg = Catalog.GetString("Do you want to pause the recording?");
				if (!MessagesHelpers.QuestionMessage (this, msg)) {
					return;
				}
			}				
			capturing = !capturing;
			recbutton.Visible = !capturing;
			pausebutton.Visible = capturing;
			capturer.TogglePause();
		}

		public void Run (CapturerType type, CaptureSettings settings) {
			/* Close any previous instance of the capturer */
			Close ();

			MultimediaToolkit factory = new MultimediaToolkit();
			capturer = factory.GetCapturer(type);
			capturer.EllapsedTime += OnTick;
			this.settings = settings;
			if (type != CapturerType.Live) {
				capturer.Error += OnError;
				capturer.DeviceChange += OnDeviceChange;
				videodrawingarea.DoubleBuffered = true;
			} else {
				videodrawingarea.DoubleBuffered = false;
			}
			if (videodrawingarea.IsRealized) {
				Configure();
				capturer.Run();
			} else {
				delayStart = true;
			}
		}
		
		public void Close() {
			/* resetting common properties */
			pausebutton.Visible = false;
			stopbutton.Visible = false;
			recbutton.Visible = true;
			captureStarted = false;
			capturing = false;
			OnTick(new Time (0));

			if(capturer == null)
				return;

			/* stopping and closing capturer */
			try {
				capturer.Stop();
				capturer.Close();
				if (type == CapturerType.Live) {
					/* release and dispose live capturer */
					capturer.Error -= OnError;
					capturer.DeviceChange -= OnDeviceChange;
					capturer.Dispose();
				}
			} catch(Exception ex) {
				Log.Exception (ex);
			}
			capturer = null;
		}

		public Image CurrentMiniatureFrame {
			get {
				if(capturer == null)
					return null;

				Image image = capturer.CurrentFrame;

				if(image.Value == null)
					return null;
				image.Scale (Constants.MAX_THUMBNAIL_SIZE, Constants.MAX_THUMBNAIL_SIZE);
				return image;
			}
		}

		void Configure () {
			VideoMuxerType muxer;
			IntPtr windowHandle;
			
			if(capturer == null)
				return;
			
			/* We need to use Matroska for live replay and remux when the capture is done */
			muxer = settings.EncodingSettings.EncodingProfile.Muxer;
			if (muxer == VideoMuxerType.Avi || muxer == VideoMuxerType.Mp4) {
				settings.EncodingSettings.EncodingProfile.Muxer = VideoMuxerType.Matroska;
			}
				
			windowHandle = GtkHelpers.GetWindowHandle (videodrawingarea.GdkWindow);
			capturer.Configure (settings, windowHandle); 
			delayStart = false;
		}

		protected virtual void OnRecbuttonClicked(object sender, System.EventArgs e)
		{
			if(capturer == null)
				return;

			if(captureStarted == true) {
				if(capturing)
					return;
				TogglePause();
			}
			else
				Start();
		}

		protected virtual void OnPausebuttonClicked(object sender, System.EventArgs e)
		{
			if(capturer != null && capturing)
				TogglePause();
		}

		protected virtual void OnStopbuttonClicked(object sender, System.EventArgs e)
		{
			string msg;

			if(capturer == null)
				return;

			msg = Catalog.GetString("Do you want to stop and finish the current capture?");
			if (MessagesHelpers.QuestionMessage (this, msg, null)) {
				Close();
				recbutton.Visible = true;
				pausebutton.Visible = false;
				stopbutton.Visible = false;
			
				if(CaptureFinished != null)
					CaptureFinished(this, new EventArgs());
			}
		}
		
		protected virtual void OnTick(Time ellapsedTime) {
			timelabel.Markup = String.Format("<span font=\"20px bold\">Time --> {0}</span> ", 
			                                 CurrentTime.ToSecondsString());
		}

		protected virtual void OnError(string message)
		{
			if(Error != null)
				Error(message);
			Close();
		}

		protected virtual void OnDeviceChange(int deviceID)
		{
			string msg;
			/* device disconnected, pause capture */
			if(deviceID == -1) {
				if(capturing)
					TogglePause();

				recbutton.Sensitive = false;
				msg = Catalog.GetString("Device disconnected. " +
				                        "The capture will be paused");
				MessagesHelpers.WarningMessage (this, msg);
			} else {
				recbutton.Sensitive = true;
				msg = Catalog.GetString("Device reconnected." +
				                        "Do you want to restart the capture?");
				if (MessagesHelpers.QuestionMessage (this, msg, null)) {
					TogglePause ();
				}
			}
		}

		protected virtual void OnLogodrawingareaExposeEvent(object o, Gtk.ExposeEventArgs args)
		{
			Gdk.Window win;
			Gdk.Pixbuf logo, frame;
			int width, height, allocWidth, allocHeight, logoX, logoY;
			float ratio;

			if(logopix == null)
				return;
			
			logo = logopix.Value;

			win = videodrawingarea.GdkWindow;
			width = logo.Width;
			height = logo.Height;
			allocWidth = videodrawingarea.Allocation.Width;
			allocHeight = videodrawingarea.Allocation.Height;

			/* Checking if allocated space is smaller than our logo */
			if((float) allocWidth / width > (float) allocHeight / height) {
				ratio = (float) allocHeight / height;
			} else {
				ratio = (float) allocWidth / width;
			}
			width = (int)(width * ratio);
			height = (int)(height * ratio);

			logoX = (allocWidth / 2) - (width / 2);
			logoY = (allocHeight / 2) - (height / 2);

			/* Drawing our frame */
			frame = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, allocWidth, allocHeight);
			logo.Composite(frame, 0, 0, allocWidth, allocHeight, logoX, logoY,
			                  ratio, ratio, Gdk.InterpType.Bilinear, 255);

			win.DrawPixbuf(this.Style.BlackGC, frame, 0, 0,
			               0, 0, allocWidth, allocHeight,
			               Gdk.RgbDither.Normal, 0, 0);
			frame.Dispose();
			return;
		}
	}
}
