//
//  Copyright (C) 2010 Andoni Morales Alastruey
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;
using Image = LongoMatch.Core.Common.Image;

namespace LongoMatch.Video.Capturer
{
	public class FakeCapturer : ICapturer
	{
		public event ReadyToCaptureHandler ReadyToCapture;
		public event EllpasedTimeHandler EllapsedTime;
		public event ErrorHandler Error;
		public event DeviceChangeHandler DeviceChange;
		public event MediaInfoHandler MediaInfo;

		LiveSourceTimer timer;

		public FakeCapturer ()
		{
			timer = new LiveSourceTimer ();
			timer.EllapsedTime += delegate(Time ellapsedTime) {
				if (EllapsedTime != null)
					EllapsedTime (ellapsedTime);
			};
		}

		public Time CurrentTime {
			get {
				return new Time (timer.CurrentTime);
			}
		}

		public void Configure (CaptureSettings settings, object window_handle)
		{
		}

		public void Dispose ()
		{
			Stop ();
		}

		public void Run ()
		{
		}

		public void Close ()
		{
		}

		public void Start ()
		{
			timer.Start ();
		}

		public void Stop ()
		{
			timer.Stop ();
		}

		public void TogglePause ()
		{
			timer.TogglePause ();
		}

		public uint OutputWidth {
			get {
				return 0;
			}
			set { }
		}

		public uint OutputHeight {
			get {
				return 0;
			}
			set { }
		}

		public string OutputFile {
			get {
				return Catalog.GetString ("Fake live source");
			}
			set { }
		}

		public uint VideoQuality {
			get {
				return 0;
			}
			set { }
		}

		public uint AudioQuality {
			get {
				return 0;
			}
			set { }
		}

		public Image CurrentFrame {
			get {
				return null;
			}
		}

		public string DeviceID {
			get {
				return "";
			}
			set { }
		}

		public bool SetVideoEncoder (VideoEncoderType type)
		{
			return true;
		}

		public bool SetAudioEncoder (AudioEncoderType type)
		{
			return true;
		}

		public bool SetVideoMuxer (VideoMuxerType type)
		{
			return true;
		}

		public bool SetSource (CaptureSourceType type, string sourceElement)
		{
			return true;
		}

		public void Expose ()
		{
		}
	}
}
