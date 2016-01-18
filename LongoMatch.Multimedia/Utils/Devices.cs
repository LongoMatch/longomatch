//
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LongoMatch.Core.Common;
using LongoMatch.Video.Capturer;
using System;
using System.Runtime.InteropServices;

namespace LongoMatch.Multimedia.Utils
{
	public class Devices
	{
		[DllImport ("libcesarplayer.dll")]
		static extern IntPtr lgm_device_enum_video_devices (string source);

		[DllImport ("libcesarplayer.dll")]
		static extern IntPtr lgm_device_get_formats (IntPtr raw);

		[DllImport ("libcesarplayer.dll")]
		static extern IntPtr lgm_device_get_device_name (IntPtr raw);

		[DllImport ("libcesarplayer.dll")]
		static extern void lgm_device_free (IntPtr raw);

		[DllImport ("libcesarplayer.dll")]
		static extern IntPtr lgm_device_video_format_get_info (IntPtr raw, out int width, out int height,
		                                                       out int fps_n, out int fps_d);

		static readonly string[] devices_osx = new string[2] { "avfvideosrc", "decklinkvideosrc" };
		static readonly string[] devices_win = new string[2] { "ksvideosrc", "dshowvideosrc" };
		static readonly string[] devices_lin = new string[2] { "v4l2src", "dv1394src" };

		static public List<Device> ListVideoDevices ()
		{
			string[] devices;
			if (LongoMatch.Core.Common.Utils.OS == OperatingSystemID.OSX)
				devices = devices_osx;
			else if (LongoMatch.Core.Common.Utils.OS == OperatingSystemID.Windows)
				devices = devices_win;
			else
				devices = devices_lin;

			List<Device> devicesList = new List<Device> ();

			foreach (string source in devices) {
				GLib.List devices_raw = new GLib.List (lgm_device_enum_video_devices (source),
					                        typeof(IntPtr), true, false);

				foreach (IntPtr device_raw in devices_raw) {
					/* The Direct Show GStreamer element seems to have problems with the
					 * BlackMagic DeckLink cards, so filter them out. They are also
					 * available through the ksvideosrc element. */
					if (source == "dshowvideosrc" &&
					    Regex.Match (source, ".*blackmagic.*|.*decklink.*", RegexOptions.IgnoreCase).Success) {
						continue;
					}

					Device device = new Device ();
					device.DeviceType = CaptureSourceType.System;
					device.SourceElement = source;
					device.ID = GLib.Marshaller.PtrToStringGFree (lgm_device_get_device_name (device_raw));

					GLib.List formats_raw = new GLib.List (lgm_device_get_formats (device_raw),
						                        typeof(IntPtr), false, false);
					foreach (IntPtr format_raw in formats_raw) {
						DeviceVideoFormat format = new DeviceVideoFormat ();
						lgm_device_video_format_get_info (format_raw, out format.width, out format.height,
							out format.fps_n, out format.fps_d);
						device.Formats.Add (format);
					}
					devicesList.Add (device);
					lgm_device_free (device_raw);
				}
			}
			return devicesList;
		}
	}
}

