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

namespace LongoMatch.Common
{

	public struct CaptureSettings
	{
		public CaptureSourceType CaptureSourceType;
		public string DeviceID;
		public EncodingSettings EncodingSettings;

		public static CaptureSettings DefaultSettings() {
			CaptureSettings settings = new CaptureSettings();
			settings.CaptureSourceType = CaptureSourceType.System;
			settings.EncodingSettings = new EncodingSettings(VideoStandards.P480_4_3,
			                                                 EncodingProfiles.MP4,
			                                                 25, 1, 1000, 128, "", 20);
			return settings;
		}
	}
	
}
