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
using System.Linq;
using System.Runtime.InteropServices;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;
using LongoMatch.Multimedia.Utils;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Converter;
using LongoMatch.Video.Editor;
using LongoMatch.Video.Player;
using LongoMatch.Video.Remuxer;
using LongoMatch.Video.Utils;

namespace LongoMatch.Video
{
	public class MultimediaFactory
	{
		Dictionary<Type, List<BackendElement>> elements;

		public MultimediaFactory ()
		{
			elements = new Dictionary<Type, List<BackendElement>> ();
			/* Register default elements */
			Register (0, typeof(IPlayer), typeof(GstPlayer));
			Register (0, typeof(IFramesCapturer), typeof(GstFramesCapturer));
			Register (0, typeof(IVideoConverter), typeof(GstVideoConverter));
			Register (0, typeof(IVideoEditor), typeof(GstVideoSplitter));
			Register (0, typeof(IRemuxer), typeof(GstRemuxer));
			Register (0, typeof(ICapturer), typeof(GstCameraCapturer));
			Register (0, typeof(IDiscoverer), typeof(GstDiscoverer));
		}

		public void Register (int priority, Type interfac, Type elementType)
		{
			if (!elements.ContainsKey (interfac)) {
				elements [interfac] = new List<BackendElement> ();
			}
			elements [interfac].Add (new BackendElement (elementType, priority));
		}

		public IPlayer GetPlayer ()
		{
			return GetDefaultElement<IPlayer> (typeof(IPlayer));
		}

		public IFramesCapturer GetFramesCapturer ()
		{
			return GetDefaultElement<IFramesCapturer> (typeof(IFramesCapturer));
		}

		public IVideoEditor GetVideoEditor ()
		{
			return GetDefaultElement<IVideoEditor> (typeof(IVideoEditor));
		}

		public IVideoConverter GetVideoConverter (string filename)
		{
			return GetDefaultElement<IVideoConverter> (typeof(IVideoConverter), filename);
		}

		public IDiscoverer GetDiscoverer ()
		{
			return GetDefaultElement<IDiscoverer> (typeof(IDiscoverer));
		}

		public ICapturer GetCapturer (CapturerType type)
		{
			switch (type) {
			case CapturerType.Live:
				return GetDefaultElement<ICapturer> (typeof(ICapturer), "test.avi");
			default:
				return new FakeCapturer ();
			}
		}

		public IRemuxer GetRemuxer (MediaFile inputFile, string outputFile, VideoMuxerType muxer)
		{
			if (inputFile.Container == GStreamer.MPEG1_PS ||
				inputFile.Container == GStreamer.MPEG2_PS ||
				inputFile.Container == GStreamer.MPEG2_TS) {
				return new MpegRemuxer (inputFile.FilePath, outputFile);
			} else {
				return GetDefaultElement<IRemuxer> (typeof(IRemuxer),
				                                    inputFile.FilePath,
				                                    outputFile, muxer);
			}
		}

		public MediaFile DiscoverFile (string file, bool takeScreenshot = true)
		{
			return GetDiscoverer ().DiscoverFile (file, takeScreenshot);
		}

		public List<Device> VideoDevices {
			get {
				return VideoDevice.ListVideoDevices ();
			}
		}

		public bool FileNeedsRemux (MediaFile file)
		{
			return GStreamer.FileNeedsRemux (file);
		}

		[DllImport("libgstreamer-0.10.dll")]
		static extern void gst_init (int argc, string argv);

		public static void InitBackend ()
		{
			gst_init (0, "");
		}

		T GetDefaultElement<T> (Type interfac, params object[] args)
		{
			Type elementType;
			
			if (!elements.ContainsKey (interfac)) {
				throw new Exception (String.Format ("No {0} available in the multimedia backend", interfac));
			}
			elementType = elements [interfac].OrderByDescending (e => e.priority).First ().type;
			return (T)Activator.CreateInstance (elementType, args);
		}

		internal class BackendElement
		{
			public Type type;
			public int priority;

			public BackendElement (Type type, int priority)
			{
				this.type = type;
				this.priority = priority;
			}
		}
	}
}
