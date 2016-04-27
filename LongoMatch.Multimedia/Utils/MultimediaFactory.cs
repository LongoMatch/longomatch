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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Multimedia.Utils;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Editor;
using LongoMatch.Video.Player;
using LongoMatch.Video.Remuxer;
using LongoMatch.Video.Utils;
using VAS.Core.Common;
using VAS.Core.Store;

namespace LongoMatch.Video
{
	public class MultimediaFactory
	{
		Registry registry;

		public MultimediaFactory ()
		{
			registry = new Registry ("Multimedia backend");
			/* Register default elements */
			Register<IPlayer, GstPlayer> (0);
			Register<IFramesCapturer, GstFramesCapturer> (0);
			Register<IVideoEditor, GstVideoSplitter> (0);
			Register<IRemuxer, GstRemuxer> (0);
			Register<ICapturer, GstCameraCapturer> (0);
			Register<IDiscoverer, GstDiscoverer> (0);
		}

		public void Register <I, C> (int priority)
		{
			registry.Register<I, C> (priority);
		}

		public IPlayer GetPlayer ()
		{
			return registry.Retrieve<IPlayer> ();
		}

		public IMultiPlayer GetMultiPlayer ()
		{
			return registry.Retrieve<IMultiPlayer> ();
		}

		public IFramesCapturer GetFramesCapturer ()
		{
			return registry.Retrieve<IFramesCapturer> ();
		}

		public IVideoEditor GetVideoEditor ()
		{
			return registry.Retrieve<IVideoEditor> ();
		}

		public IDiscoverer GetDiscoverer ()
		{
			return registry.Retrieve<IDiscoverer> ();
		}

		public ICapturer GetCapturer ()
		{
			return registry.Retrieve<ICapturer> ("test.avi");
		}

		public IRemuxer GetRemuxer (MediaFile inputFile, string outputFile, VideoMuxerType muxer)
		{
			return registry.Retrieve<IRemuxer> (inputFile.FilePath, outputFile, muxer);
		}

		public MediaFile DiscoverFile (string file, bool takeScreenshot = true)
		{
			IDiscoverer discoverer = GetDiscoverer ();
			MediaFile mfile = discoverer.DiscoverFile (file, takeScreenshot);
			discoverer.Dispose ();
			return mfile;
		}

		public List<Device> VideoDevices {
			get {
				return Devices.ListVideoDevices ();
			}
		}

		public bool FileNeedsRemux (MediaFile file)
		{
			return GStreamer.FileNeedsRemux (file);
		}

		[DllImport ("libgstreamer-0.10.dll")]
		static extern void gst_init (int argc, string argv);

		public static void InitBackend ()
		{
			gst_init (0, "");
		}
	}
}
