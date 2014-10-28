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
using System.Collections.Generic;
using System.Linq;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Helpers;
using LongoMatch.Multimedia.Utils;
using Mono.Unix;
using Image = LongoMatch.Core.Common.Image;
using Misc = LongoMatch.Gui.Helpers.Misc;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category("CesarPlayer")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CapturerBin : Gtk.Bin, ICapturerBin
	{
		CapturerType type;
		CaptureSettings settings;
		bool delayStart;
		Period currentPeriod;
		uint timeoutID;
		TimeNode currentTimeNode;
		Time accumTime;
		DateTime currentPeriodStart;
		List<string> gamePeriods;

		public CapturerBin ()
		{
			this.Build ();
			LongoMatch.Gui.Helpers.Misc.SetFocus (vbox1, false);
			videowindow.ReadyEvent += HandleReady;
			videowindow.ExposeEvent += HandleExposeEvent;
			videowindow.CanFocus = true;
			recbutton.Clicked += (sender, e) => StartPeriod ();
			stopbutton.Clicked += (sender, e) => StopPeriod ();
			pausebutton.Clicked += (sender, e) => PausePeriod ();
			resumebutton.Clicked += (sender, e) => ResumePeriod ();
			savebutton.Clicked += HandleSaveClicked;
			cancelbutton.Clicked += HandleCloseClicked;
			recimage.Pixbuf = Misc.LoadIcon ("longomatch-record",
			                                 StyleConf.PlayerCapturerIconSize);
			stopimage.Pixbuf = Misc.LoadIcon ("longomatch-stop",
			                                  StyleConf.PlayerCapturerIconSize);
			pauseimage.Pixbuf = Misc.LoadIcon ("longomatch-pause-clock",
			                                   StyleConf.PlayerCapturerIconSize);
			pauseimage.Pixbuf = Misc.LoadIcon ("longomatch-resume-clock",
			                                   StyleConf.PlayerCapturerIconSize);
			saveimage.Pixbuf = Misc.LoadIcon ("longomatch-save",
			                                  StyleConf.PlayerCapturerIconSize);
			resumeimage.Pixbuf = Misc.LoadIcon ("longomatch-pause-clock",
			                                    StyleConf.PlayerCapturerIconSize);
			cancelimage.Pixbuf = Misc.LoadIcon ("longomatch-cancel-rec",
			                                    StyleConf.PlayerCapturerIconSize);
			Periods = new List<Period> ();
			Reset ();
			Mode = CapturerType.Live;
		}

		protected override void OnDestroyed ()
		{
			if (timeoutID != 0) {
				GLib.Source.Remove (timeoutID);
			}
			base.OnDestroyed ();
		}

		public CapturerType Mode {
			set {
				type = value;
				videowindow.Visible = value == CapturerType.Live;
				if (type == CapturerType.Fake) {
					SetStyle (StyleConf.PlayerCapturerControlsHeight * 2, 24 * 2, 40 * 2);
				} else {
					SetStyle (StyleConf.PlayerCapturerControlsHeight, 24, 40);
				}
			}
		}

		public bool Capturing {
			get;
			set;
		}

		public CaptureSettings CaptureSettings {
			get {
				return settings;
			}
		}

		public List<string> PeriodsNames {
			set {
				gamePeriods = value;
				if (gamePeriods != null && gamePeriods.Count > 0) {
					periodlabel.Markup = gamePeriods [0];
				} else {
					periodlabel.Markup = "1";
				}
			}
			get {
				return gamePeriods;
			}
		}

		public ICapturer Capturer {
			get;
			set;
		}

		public List<Period> Periods {
			set;
			get;
		}

		public Time CurrentTime {
			get {
				int timeDiff;
				
				timeDiff = (int)(DateTime.UtcNow - currentPeriodStart).TotalMilliseconds; 
				return new Time (accumTime.MSeconds + timeDiff);
			}
		}

		Time EllapsedTime {
			get {
				if (currentPeriod != null) {
					return currentPeriod.TotalTime;
				} else {
					return new Time (0);
				}
				
			}
		}

		public void StartPeriod ()
		{
			string periodName;
			
			recbutton.Visible = false;
			pausebutton.Visible = savebutton.Visible = stopbutton.Visible = true;
			
			if (PeriodsNames != null && PeriodsNames.Count > Periods.Count) {
				periodName = PeriodsNames [Periods.Count];
			} else {
				periodName = (Periods.Count + 1).ToString ();
			}
			currentPeriod = new Period { Name = periodName };
			
			currentTimeNode = currentPeriod.StartTimer (accumTime, periodName);
			currentTimeNode.Stop = currentTimeNode.Start;
			currentPeriodStart = DateTime.UtcNow;
			timeoutID = GLib.Timeout.Add (20, UpdateTime);
			if (Capturer != null) {
				if (Periods.Count == 0) {
					Capturer.Start ();
				} else {
					Capturer.TogglePause ();
				}
			}
			periodlabel.Markup = currentPeriod.Name;
			Capturing = true;
			Periods.Add (currentPeriod);
			Log.Debug ("Start new period start=", currentTimeNode.Start.ToMSecondsString ());
		}

		public void StopPeriod ()
		{
			GLib.Source.Remove (timeoutID);
			if (currentPeriod != null) {
				currentPeriod.StopTimer (CurrentTime);
				accumTime = CurrentTime;
				Log.Debug ("Stop period stop=", accumTime.ToMSecondsString ());
			}
			currentTimeNode = null;
			currentPeriod = null;
			
			recbutton.Visible = true;
			pausebutton.Visible = resumebutton.Visible = stopbutton.Visible = false;
			if (Capturer != null && Capturing) {
				Capturer.TogglePause ();
			}
			Capturing = false;
		}

		public void PausePeriod ()
		{
			if (currentPeriod != null) {
				Log.Debug ("Pause period at currentTime=", CurrentTime.ToMSecondsString ());
				currentPeriod.PauseTimer (CurrentTime);
			}
			currentTimeNode = null;
			pausebutton.Visible = false;
			resumebutton.Visible = true;
			Capturing = false;
		}

		public void ResumePeriod ()
		{
			Log.Debug ("Resume period at currentTime=", CurrentTime.ToMSecondsString ());
			currentTimeNode = currentPeriod.Resume (CurrentTime);
			pausebutton.Visible = true;
			resumebutton.Visible = false;
			Capturing = true;
		}

		void SetStyle (int height, int fontSize, int timeWidth)
		{
			string font = String.Format ("Ubuntu {0}px", fontSize);
			Pango.FontDescription desc = Pango.FontDescription.FromString (font);

			controllerbox.HeightRequest = height;
			hourseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundDark));
			hourlabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			hourlabel.ModifyFont (desc);
			hourseventbox.WidthRequest = timeWidth;
			minuteseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundDark));
			minuteslabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			minuteslabel.ModifyFont (desc);
			minuteseventbox.WidthRequest = timeWidth;
			secondseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundDark));
			secondslabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			secondslabel.ModifyFont (desc);
			secondseventbox.WidthRequest = timeWidth;
			label1.ModifyFont (desc);
			label1.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			label2.ModifyFont (desc);
			label2.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			periodlabel.ModifyFont (desc);
			periodlabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));

		}
		
		bool UpdateTime ()
		{
			if (currentTimeNode != null) {
				currentTimeNode.Stop = CurrentTime;
			}
			hourlabel.Markup = EllapsedTime.Hours.ToString ("d2");
			minuteslabel.Markup = EllapsedTime.Minutes.ToString ("d2");
			secondslabel.Markup = EllapsedTime.Seconds.ToString ("d2");
			Config.EventsBroker.EmitCapturerTick (CurrentTime);
			return true;
		}

		void HandleSaveClicked (object sender, EventArgs e)
		{
			string msg = Catalog.GetString ("Do you want to finish the current capture?");
			if (MessagesHelpers.QuestionMessage (this, msg)) {
				Config.EventsBroker.EmitCaptureFinished (false);
			}
		}

		void HandleCloseClicked (object sender, EventArgs e)
		{
			string msg = Catalog.GetString ("Do you want to close and cancell the current capture?");
			if (MessagesHelpers.QuestionMessage (this, msg)) {
				Config.EventsBroker.EmitCaptureFinished (true);
			}
		}

		public void Run (CaptureSettings settings)
		{
			Reset ();
			if (type == CapturerType.Live) {
				Capturer = Config.MultimediaToolkit.GetCapturer ();
				this.settings = settings;
				videowindow.Ratio = (float)settings.EncodingSettings.VideoStandard.Width /
					settings.EncodingSettings.VideoStandard.Height;
				Capturer.Error += OnError;
				Capturer.DeviceChange += OnDeviceChange;
				Periods = new List<Period> ();
				if (videowindow.Ready) {
					Configure ();
				} else {
					delayStart = true;
				}
			}
		}

		public void Close ()
		{
			StopPeriod ();
			/* stopping and closing capturer */
			if (Capturer != null) {
				try {
					Capturer.Close ();
					Capturer.Error -= OnError;
					Capturer.DeviceChange -= OnDeviceChange;
					Capturer.Dispose ();
				} catch (Exception ex) {
					Log.Exception (ex);
				}
			}
			Capturer = null;
		}

		public Image CurrentMiniatureFrame {
			get {
				if (Capturer == null)
					return null;

				Image image = Capturer.CurrentFrame;

				if (image.Value == null)
					return null;
				image.ScaleInplace (Constants.MAX_THUMBNAIL_SIZE,
				                    Constants.MAX_THUMBNAIL_SIZE);
				return image;
			}
		}

		void Reset ()
		{
			currentPeriod = null;
			currentTimeNode = null;
			currentPeriodStart = DateTime.UtcNow;
			accumTime = new Time (0);
			Capturing = false;
			Capturer = null;
			recbutton.Visible = true;
			stopbutton.Visible = false;
			pausebutton.Visible = false;
			savebutton.Visible = false;
			cancelbutton.Visible = true;
			resumebutton.Visible = false;
		}

		void Configure ()
		{
			VideoMuxerType muxer;
			IntPtr windowHandle = IntPtr.Zero;
			
			if (Capturer == null) {
				videowindow.Visible = false;
				return;
			}
			
			/* We need to use Matroska for live replay and remux when the capture is done */
			muxer = settings.EncodingSettings.EncodingProfile.Muxer;
			if (muxer == VideoMuxerType.Avi || muxer == VideoMuxerType.Mp4) {
				settings.EncodingSettings.EncodingProfile.Muxer = VideoMuxerType.Matroska;
			}
			windowHandle = WindowHandle.GetWindowHandle (videowindow.Window.GdkWindow);
			Capturer.Configure (settings, windowHandle); 
			settings.EncodingSettings.EncodingProfile.Muxer = muxer;
			delayStart = false;
			Capturer.Run ();
		}

		void HandleReady (object sender, EventArgs e)
		{
			if (delayStart) {
				Configure ();
			}
		}

		void DeviceChanged (int deviceID)
		{
			string msg;
			/* device disconnected, pause capture */
			if (deviceID == -1) {
				PausePeriod ();
				msg = Catalog.GetString ("Device disconnected. " + "The capture will be paused");
				MessagesHelpers.WarningMessage (this, msg);
			} else {
				msg = Catalog.GetString ("Device reconnected." + "Do you want to restart the capture?");
				if (MessagesHelpers.QuestionMessage (this, msg, null)) {
					ResumePeriod ();
				}
			}
		}

		void OnError (string message)
		{
			Application.Invoke (delegate {
				Config.EventsBroker.EmitCaptureError (message);
			});
		}

		void OnDeviceChange (int deviceID)
		{
			Application.Invoke (delegate {
				DeviceChanged (deviceID);
			});
		}

		void HandleExposeEvent (object o, ExposeEventArgs args)
		{
			if (Capturer != null) {
				Capturer.Expose ();
			}
		}
	}
}
