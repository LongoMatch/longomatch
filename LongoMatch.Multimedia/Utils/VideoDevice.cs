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
using System;
using System.Collections.Generic;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Video.Common;
using LongoMatch.Video.Capturer;

namespace LongoMatch.Multimedia.Utils
{
	public class VideoDevice
	{
		
		static public List<Device> ListVideoDevices() {
			List<Device> devicesList  = new List<Device>();

			/* Generate the list of devices, on the supported platforms
			 * and extra DV device for the dv1394src element and the default
			 * OS source for all of them */
			foreach(string devName in GstCameraCapturer.VideoDevices) {
				CaptureSourceType source;

				if(Environment.OSVersion.Platform == PlatformID.Unix)
					source = CaptureSourceType.DV;
				else
					source = CaptureSourceType.System;

				devicesList.Add(new Device {
					ID = devName,
					DeviceType = source,
				});
			}
			if(Environment.OSVersion.Platform != PlatformID.Win32NT) {
				devicesList.Add(new Device {
					ID = Catalog.GetString("Default device"),
					DeviceType = CaptureSourceType.System
				});
			}
			return devicesList;
		}
	}
}

