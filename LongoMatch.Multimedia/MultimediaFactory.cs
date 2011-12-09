// PlayerMaker.cs
//
//  Copyright(C) 2007-2009 Andoni Morales Alastruey
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
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using LongoMatch.Common;
using LongoMatch.Interfaces.Multimedia;
using LongoMatch.Multimedia.Interfaces;
using LongoMatch.Multimedia.Utils;
using LongoMatch.Store;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Player;
using LongoMatch.Video.Editor;
using LongoMatch.Video.Utils;
using LongoMatch.Video.Common;

namespace LongoMatch.Video
{


	public class MultimediaFactory: IMultimediaToolkit
	{

		OperatingSystem oS;

		public MultimediaFactory()
		{
			oS = Environment.OSVersion;
		}

		public IPlayer GetPlayer(int width, int height) {
			switch(oS.Platform) {
			case PlatformID.Unix:
				return new GstPlayer(width,height,PlayerUseType.Video);

			case PlatformID.Win32NT:
				return new GstPlayer(width,height,PlayerUseType.Video);

			default:
				return new GstPlayer(width,height,PlayerUseType.Video);
			}
		}

		public IMetadataReader GetMetadataReader() {

			switch(oS.Platform) {
			case PlatformID.Unix:
				return new GstPlayer(1,1,PlayerUseType.Metadata);

			case PlatformID.Win32NT:
				return new GstPlayer(1,1,PlayerUseType.Metadata);

			default:
				return new GstPlayer(1,1,PlayerUseType.Metadata);
			}
		}

		public IFramesCapturer GetFramesCapturer() {
			switch(oS.Platform) {
			case PlatformID.Unix:
				return new GstPlayer(1,1,PlayerUseType.Capture);

			case PlatformID.Win32NT:
				return new GstPlayer(1,1,PlayerUseType.Capture);

			default:
				return new GstPlayer(1,1,PlayerUseType.Capture);
			}
		}

		public IVideoEditor GetVideoEditor() {
			switch(oS.Platform) {
			case PlatformID.Unix:
				return new GstVideoSplitter();

			case PlatformID.Win32NT:
				return new GstVideoSplitter();

			default:
				return new GstVideoSplitter();
			}
		}

		public ICapturer GetCapturer(CapturerType type) {
			switch(type) {
			case CapturerType.Fake:
				return new FakeCapturer();

			case CapturerType.Live:
				return new GstCameraCapturer("test.avi");

			default:
				return new FakeCapturer();
			}
		}
		
		public MediaFile DiscoverFile (string file) {
			return PreviewMediaFile.DiscoverFile(file);
		}	
		
		public List<Device> VideoDevices {
			get {
				return VideoDevice.ListVideoDevices();
			}
		}
		
		[DllImport("libcesarplayer.dll")]
		static extern void gst_init (int argc, string argv);
		public static void InitBackend() {
			gst_init(0, "");
		}
	}
}