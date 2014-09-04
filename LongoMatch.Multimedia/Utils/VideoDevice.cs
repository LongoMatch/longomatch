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
using LongoMatch.Core.Common;
using LongoMatch.Video.Capturer;

namespace LongoMatch.Multimedia.Utils
{
	public class VideoDevice
	{
		#if OSTYPE_OS_X
		static string[] devices = new string[1] {"osxvideosrc"};
		
		#elif OSTYPE_WINDOWS
				static string[] devices = new string[2] {"dshowvideosrc", "ksvideosrc"};
		
		#else
		static string[] devices = new string[2] { "v4l2src", "dv1394src" };
		#endif
		static public List<Device> ListVideoDevices ()
		{
			List<Device> devicesList = new List<Device> ();

			foreach (string source in devices) {
				foreach (string devname in GstCameraCapturer.ListVideoDevices (source)) {
					devicesList.Add (new Device {
						ID = devname,
						DeviceType = CaptureSourceType.System,
						SourceElement = source,
					});
				}
			}
			return devicesList;
		}
	}
}

