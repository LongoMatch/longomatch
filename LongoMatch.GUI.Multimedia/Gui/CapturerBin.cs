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
using Gtk;
using LongoMatch.Common;
using LongoMatch.Gui.Helpers;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Interfaces.Multimedia;
using LongoMatch.Multimedia.Utils;
using LongoMatch.Store;
using Mono.Unix;
using Image = LongoMatch.Common.Image;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category("CesarPlayer")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CapturerBin : Gtk.Bin, ICapturerBin
	{
		CaptureSettings settings;
		CapturerType type;
		bool delayStart;
		ICapturer capturer;
		int periodIndex;
		Period currentPeriod;
		Time ellapsedTime;
		List<string> periods;

		public CapturerBin ()
		{
			this.Build ();
			recbutton.Visible = false;
			stopbutton.Visible = false;
			finishbutton.Visible = false;
			cancelbutton.Visible = true;
			LongoMatch.Gui.Helpers.Misc.DisableFocus (vbox1);
			videodrawingarea.CanFocus = true;
			ConnectSignals ();
			ellapsedTime = new Time (0);
		}

		public CapturerType Mode {
			set {
				type = value;
				videodrawingarea.Visible = value == CapturerType.Live;
			}
			
		}

		public bool Capturing {
			get;
			protected set;
		}

		public Time CurrentTime {
			get {
				if (capturer == null)
					return new Time (-1);
				return capturer.CurrentTime;
			}
		}

		public CaptureSettings CaptureSettings {
			get {
				return settings;
			}
		}

		public List<string> PeriodsNames {
			set {
				periods = value;
				UpdateLabel (value [0]);
			}
			get {
				return periods;
			}
		}

		public List<Period> Periods {
			set;
			get;
		}

		public void StartPeriod ()
		{
			if (capturer == null)
				return;
				
			if (currentPeriod != null) {
				throw new Exception ("Period already started");
			}

			currentPeriod = new Period { Name = periods[periodIndex] };
			currentPeriod.StartTimer (ellapsedTime);
			Log.Information (String.Format ("Start new period {0} at {1}",
			                                currentPeriod.Name, ellapsedTime.ToSecondsString ()));
			Capturing = true;
			recbutton.Visible = false;
			stopbutton.Visible = true;
			finishbutton.Visible = true;
			if (Periods.Count == 0) {
				capturer.Start ();
			} else {
				capturer.TogglePause ();
			}
			if (periodIndex + 1 == periods.Count) {
				stopbutton.Visible = false;
			}
			UpdateLabel (currentPeriod.Name);
			Periods.Add (currentPeriod);
		}

		public void StopPeriod ()
		{
			string msg;
			
			msg = Catalog.GetString ("Do you want to stop the current period?");
			
			if (!MessagesHelpers.QuestionMessage (this, msg)) {
				return;
			}
			if (currentPeriod == null) {
				throw new Exception ("Period not started");
			}
			
			Log.Information (String.Format ("Stop period {0} at {1}",
			                                currentPeriod.Name, ellapsedTime.ToSecondsString ()));
			periodIndex ++;
			Capturing = false;
			capturer.TogglePause ();
			currentPeriod.StopTimer (ellapsedTime);
			UpdateLabel (periods [periodIndex]);
			currentPeriod = null;
			recbutton.Visible = true;
			stopbutton.Visible = false;
		}

		public void Stop ()
		{
			if (currentPeriod != null) {
				Log.Information (String.Format ("Stop period {0} at {1}",
				                                currentPeriod.Name, ellapsedTime.ToSecondsString ()));
				currentPeriod.StopTimer (ellapsedTime);
			}
			Log.Information ("Stop capture");
			capturer.Stop ();
		}

		public void Run (CaptureSettings settings)
		{
			/* Close any previous instance of the capturer */
			Close ();

			capturer = Config.MultimediaToolkit.GetCapturer (type);
			capturer.EllapsedTime += OnTick;
			this.settings = settings;
			if (type != CapturerType.Live) {
				capturer.Error += OnError;
				capturer.DeviceChange += OnDeviceChange;
				videodrawingarea.DoubleBuffered = true;
			} else {
				videodrawingarea.DoubleBuffered = false;
			}
			if (type == CapturerType.Fake || videodrawingarea.IsRealized) {
				Configure ();
				capturer.Run ();
			} else {
				delayStart = true;
			}
			Periods = new List<Period> ();
		}

		public void Close ()
		{
			bool stop = Capturing;

			/* resetting common properties */
			stopbutton.Visible = false;
			finishbutton.Visible = false;
			recbutton.Visible = false;
			Capturing = false;

			if (capturer == null)
				return;

			/* stopping and closing capturer */
			try {
				if (Capturing) {
					capturer.Stop ();
				}
				capturer.Close ();
				if (type == CapturerType.Live) {
					/* release and dispose live capturer */
					capturer.Error -= OnError;
					capturer.DeviceChange -= OnDeviceChange;
					capturer.Dispose ();
				}
			} catch (Exception ex) {
				Log.Exception (ex);
			}
			capturer = null;
		}

		public Image CurrentMiniatureFrame {
			get {
				if (capturer == null)
					return null;

				Image image = capturer.CurrentFrame;

				if (image.Value == null)
					return null;
				image.ScaleInplace (Constants.MAX_THUMBNAIL_SIZE, Constants.MAX_THUMBNAIL_SIZE);
				return image;
			}
		}

		void ConnectSignals ()
		{
			recbutton.Clicked += (sender, e) => StartPeriod ();
			stopbutton.Clicked += (sender, e) => StopPeriod ();
			finishbutton.Clicked += (sender, e) => {
				string msg = Catalog.GetString ("Do you want to finish the current capture?");
				if (!MessagesHelpers.QuestionMessage (this, msg)) {
					return;
				}
				Config.EventsBroker.EmitCaptureFinished (false);
			};
			cancelbutton.Clicked += (sender, e) => Config.EventsBroker.EmitCaptureFinished (true);
			videodrawingarea.Realized += (sender, e) => {
				if (delayStart) {
					Configure ();
					capturer.Run ();
				}
			};
		}

		void UpdateLabel (string name)
		{
			frame1.Label = Catalog.GetString ("Period") + " " + name;
		}

		string FormatTime (Period period, Time time)
		{
			return String.Format ("{0} {1}: {2}  ", Catalog.GetString ("Period"),
			                      period.Name, time.ToSecondsString ());
		}

		void Configure ()
		{
			VideoMuxerType muxer;
			IntPtr windowHandle = IntPtr.Zero;
			
			if (capturer == null)
				return;
			
			recbutton.Visible = true;
			/* We need to use Matroska for live replay and remux when the capture is done */
			muxer = settings.EncodingSettings.EncodingProfile.Muxer;
			if (muxer == VideoMuxerType.Avi || muxer == VideoMuxerType.Mp4) {
				settings.EncodingSettings.EncodingProfile.Muxer = VideoMuxerType.Matroska;
			}
				
			if (type == CapturerType.Live) {
				windowHandle = WindowHandle.GetWindowHandle (videodrawingarea.GdkWindow);
			}
			capturer.Configure (settings, windowHandle); 
			delayStart = false;
		}

		void DeviceChanged (int deviceID)
		{
			string msg;
			/* device disconnected, pause capture */
			if (deviceID == -1) {
				if (Capturing)
					capturer.TogglePause ();
				recbutton.Sensitive = false;
				msg = Catalog.GetString ("Device disconnected. " + "The capture will be paused");
				MessagesHelpers.WarningMessage (this, msg);
			} else {
				recbutton.Sensitive = true;
				msg = Catalog.GetString ("Device reconnected." + "Do you want to restart the capture?");
				if (MessagesHelpers.QuestionMessage (this, msg, null)) {
					capturer.TogglePause ();
				}
			}
		}

		void OnTick (Time ellapsedTime)
		{
			string text = "";
			Time duration = new Time (0);

			this.ellapsedTime = ellapsedTime;
			
			foreach (Period period in Periods) {
				TimeNode tn = period.PeriodNode;
				if (tn.Stop != null) {
					text += FormatTime (period, tn.Duration);
					duration += tn.Duration;
				} else {
					text += FormatTime (period, ellapsedTime - duration);
					break;
				}
			}
			timelabel.Markup = String.Format ("<span font=\"30px bold\">{0}</span> ", text);
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
	}
}
