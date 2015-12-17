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

namespace LongoMatch.Core.Common
{
	[Serializable]
	public struct EncodingSettings
	{
		public EncodingSettings (VideoStandard videoStandard, EncodingProfile encodingProfile,
		                         EncodingQuality encodingQuality, uint fr_n, uint fr_d,
		                         string outputFile, bool enableAudio, bool enableTitle,
		                         uint titleSize)
		{
			VideoStandard = videoStandard;
			EncodingProfile = encodingProfile;
			EncodingQuality = encodingQuality;
			Framerate_n = fr_n;
			Framerate_d = fr_d;
			OutputFile = outputFile;
			TitleSize = titleSize;
			EnableAudio = enableAudio;
			EnableTitle = enableTitle;
		}

		public VideoStandard VideoStandard;
		public EncodingProfile EncodingProfile;
		public EncodingQuality EncodingQuality;
		public uint Framerate_n;
		public uint Framerate_d;
		public string OutputFile;
		public uint TitleSize;
		public bool EnableAudio;
		public bool EnableTitle;

		
		public static EncodingSettings DefaultRenderingSettings (string outputFilepath)
		{
			return new EncodingSettings (Config.RenderVideoStandard,
				Config.RenderEncodingProfile,
				Config.RenderEncodingQuality,
				Config.FPS_N, Config.FPS_D,
				outputFilepath,
				Config.EnableAudio, Config.OverlayTitle, 20);
		}

		/// <summary>
		/// Return the video bitrate using EncodingQuality.VideoQuality in kbps as a multiplication factor
		/// to retrieve the bitrate relative to the output video size.
		/// </summary>
		public int VideoBitrate {
			get {
				int pixels = (int)(VideoStandard.Width * VideoStandard.Height);
				float xfactor = (float)EncodingQuality.VideoQuality / 1000;
				return (int)(pixels * xfactor);
			}
		}
	}
}

