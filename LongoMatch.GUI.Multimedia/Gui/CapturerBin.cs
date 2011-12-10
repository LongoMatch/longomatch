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
using LongoMatch.Video;
using LongoMatch.Video.Common;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Utils;
using Mono.Unix;

namespace LongoMatch.Gui
{


	[System.ComponentModel.Category("CesarPlayer")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CapturerBin : Gtk.Bin, ICapturer
	{
		public event EventHandler CaptureFinished;
		public event LongoMatch.Handlers.ErrorHandler Error;

		private Image logopix;
		private CaptureSettings captureProps;
		private CapturerType capturerType;
		private bool captureStarted;
		private bool capturing;
		private const int THUMBNAIL_MAX_WIDTH = 100;

		LongoMatch.Multimedia.Interfaces.ICapturer capturer;

		public CapturerBin()
		{
			this.Build();
			captureProps = CaptureSettings.DefaultSettings();
			Type = CapturerType.Fake;
		}

		public CapturerType Type {
			set {
				/* Close any previous instance of the capturer */
				Close();

				MultimediaFactory factory = new MultimediaFactory();
				capturer = factory.GetCapturer(value);
				capturer.EllapsedTime += OnTick;
				if(value != CapturerType.Fake) {
					capturer.Error += OnError;
					capturer.DeviceChange += OnDeviceChange;
					capturerhbox.Add((Widget)capturer);
					(capturer as Widget).Visible = true;
					capturerhbox.Visible = true;
					logodrawingarea.Visible = false;
				}
				else {
					logodrawingarea.Visible = true;
					capturerhbox.Visible = false;
				}
				SetProperties();
				capturerType = value;
			}

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

		public int CurrentTime {
			get {
				if(capturer == null)
					return -1;
				return capturer.CurrentTime;
			}
		}

		public bool Capturing {
			get {
				return capturing;
			}
		}

		public CaptureSettings CaptureProperties {
			set {
				captureProps = value;
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

			capturing = !capturing;
			recbutton.Visible = !capturing;
			pausebutton.Visible = capturing;
			capturer.TogglePause();
		}

		public void Stop() {
			if(capturer != null) {
				capturing = false;
				capturer.Stop();
			}
		}

		public void Run() {
			if(capturer != null)
				capturer.Run();
		}

		public void Close() {
			/* resetting common properties */
			pausebutton.Visible = false;
			stopbutton.Visible = false;
			recbutton.Visible = true;
			captureStarted = false;
			capturing = false;
			OnTick(0);

			if(capturer == null)
				return;

			/* stopping and closing capturer */
			try {
				capturer.Stop();
				capturer.Close();
				if(capturerType == CapturerType.Live) {
					/* release and dispose live capturer */
					capturer.Error -= OnError;
					capturer.DeviceChange += OnDeviceChange;
					capturerhbox.Remove(capturer as Gtk.Widget);
					capturer.Dispose();
				}
			} catch(Exception) {}
			capturer = null;
		}

		public Image CurrentMiniatureFrame {
			get {
				if(capturer == null)
					return null;

				Image image = new Image(capturer.CurrentFrame);

				if(image.Value == null)
					return null;
				image.Scale(THUMBNAIL_MAX_WIDTH, THUMBNAIL_MAX_WIDTH);
				return image;
			}
		}

		private void SetProperties() {
			if(capturer == null)
				return;

			capturer.DeviceID = captureProps.DeviceID;
			capturer.OutputFile = captureProps.EncodingSettings.OutputFile;
			capturer.OutputHeight = captureProps.EncodingSettings.VideoStandard.Height;
			capturer.OutputWidth = captureProps.EncodingSettings.VideoStandard.Width;
			capturer.SetVideoEncoder(captureProps.EncodingSettings.EncodingProfile.VideoEncoder);
			capturer.SetAudioEncoder(captureProps.EncodingSettings.EncodingProfile.AudioEncoder);
			capturer.SetVideoMuxer(captureProps.EncodingSettings.EncodingProfile.Muxer);
			capturer.SetSource(captureProps.CaptureSourceType);
			capturer.VideoBitrate = captureProps.EncodingSettings.VideoBitrate;
			capturer.AudioBitrate = captureProps.EncodingSettings.AudioBitrate;
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
			int res;

			if(capturer == null)
				return;

			MessageDialog md = new MessageDialog((Gtk.Window)this.Toplevel, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo,
			                                     Catalog.GetString("You are going to stop and finish the current capture."+"\n"+
			                                                     "Do you want to proceed?"));
			res = md.Run();
			md.Destroy();
			if(res == (int)ResponseType.Yes) {
				md = new MessageDialog((Gtk.Window)this.Toplevel, DialogFlags.Modal, MessageType.Info, ButtonsType.None,
				                       Catalog.GetString("Finalizing file. This can take a while"));
				md.Show();
				Stop();
				md.Destroy();
				recbutton.Visible = true;
				pausebutton.Visible = false;
				stopbutton.Visible = false;
				if(CaptureFinished != null)
					CaptureFinished(this, new EventArgs());
			}
		}

		protected virtual void OnTick(int ellapsedTime) {
			timelabel.Text = "Time: " + TimeString.MSecondsToSecondsString(CurrentTime);
		}

		protected virtual void OnError(object o, ErrorArgs args)
		{
			if(Error != null)
				Error(o, args.Message);

			Close();
		}

		protected virtual void OnDeviceChange(object o, DeviceChangeArgs args)
		{
			/* device disconnected, pause capture */
			if(args.DeviceChange == -1) {
				if(capturing)
					TogglePause();

				recbutton.Sensitive = false;

				MessageDialog md = new MessageDialog((Gtk.Window)this.Toplevel, DialogFlags.Modal,
				                                     MessageType.Question, ButtonsType.Ok,
				                                     Catalog.GetString("Device disconnected. " +
				                                                     "The capture will be paused"));
				md.Icon=Stetic.IconLoader.LoadIcon(md, "longomatch", Gtk.IconSize.Dialog);
				md.Run();
				md.Destroy();
			} else {
				recbutton.Sensitive = true;
				MessageDialog md = new MessageDialog((Gtk.Window)this.Toplevel, DialogFlags.Modal,
				                                     MessageType.Question, ButtonsType.YesNo,
				                                     Catalog.GetString("Device reconnected." +
				                                                     "Do you want to restart the capture?"));
				md.Icon=Stetic.IconLoader.LoadIcon(md, "longomatch", Gtk.IconSize.Dialog);
				if(md.Run() == (int)ResponseType.Yes) {
					Console.WriteLine("Accepted to toggle pause");
					TogglePause();
				}
				md.Destroy();
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

			win = logodrawingarea.GdkWindow;
			width = logo.Width;
			height = logo.Height;
			allocWidth = logodrawingarea.Allocation.Width;
			allocHeight = logodrawingarea.Allocation.Height;

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
