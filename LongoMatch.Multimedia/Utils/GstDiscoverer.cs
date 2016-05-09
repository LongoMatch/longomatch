//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using System.Runtime.InteropServices;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Video;
using VAS.Core.Store;
using VAS.Core.Common;
using VAS.Core;

namespace LongoMatch.Video.Utils
{
	public class GstDiscoverer: IDiscoverer
	{
		const int THUMBNAIL_MAX_HEIGHT = 72;
		const int THUMBNAIL_MAX_WIDTH = 96;

		[DllImport ("libcesarplayer.dll")]
		static extern unsafe uint lgm_discover_uri (string uri, out long duration,
		                                            out uint width, out uint height,
		                                            out uint fps_n, out uint fps_d,
		                                            out uint par_n, out uint par_d,
		                                            out IntPtr container,
		                                            out IntPtr video_codec,
		                                            out IntPtr audio_codec,
		                                            out IntPtr err);

		public MediaFile DiscoverFile (string filePath, bool takeScreenshot = true)
		{
			long duration = 0;
			uint width, height, fps_n, fps_d, par_n, par_d, ret, fps = 0;
			string container, audio_codec, video_codec;
			bool has_audio, has_video;
			float par = 0;
			IntPtr container_ptr, audio_codec_ptr, video_codec_ptr;
			IntPtr error = IntPtr.Zero;
			Image preview = null;
			MultimediaFactory factory;
			IFramesCapturer thumbnailer;

			ret = lgm_discover_uri (filePath, out duration, out width, out height, out fps_n,
				out fps_d, out par_n, out par_d, out container_ptr,
				out video_codec_ptr, out audio_codec_ptr, out error);
			if (error != IntPtr.Zero)
				throw new GLib.GException (error);
			if (ret != 0) {
				throw new Exception (Catalog.GetString ("Could not parse file:") + filePath);
			}
			
			has_audio = audio_codec_ptr != IntPtr.Zero;
			has_video = video_codec_ptr != IntPtr.Zero;
			container = GLib.Marshaller.PtrToStringGFree (container_ptr);
			audio_codec = GLib.Marshaller.PtrToStringGFree (audio_codec_ptr);
			video_codec = GLib.Marshaller.PtrToStringGFree (video_codec_ptr);
			/* From nanoseconds to milliseconds */
			duration = duration / (1000 * 1000);
			
			if (has_video) {
				fps = fps_n / fps_d;
				par = (float)par_n / par_d;
				if (takeScreenshot) {
					factory = new MultimediaFactory ();
					thumbnailer = factory.GetFramesCapturer ();
					thumbnailer.Open (filePath);
					preview = thumbnailer.GetFrame (new Time { TotalSeconds = 2 }, false,
						THUMBNAIL_MAX_WIDTH, THUMBNAIL_MAX_HEIGHT);
					thumbnailer.Dispose ();
				}
			}
			
			return new MediaFile (filePath, duration, (ushort)fps, has_audio, has_video,
				container, video_codec, audio_codec, width, height,
				par, preview, null);
		}

		public void Dispose ()
		{
		}
	}
}
