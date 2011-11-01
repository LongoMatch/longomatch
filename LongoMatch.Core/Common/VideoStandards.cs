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
using Mono.Unix;

namespace LongoMatch.Common
{
	[Serializable]
	public struct VideoStandard
	{
		public VideoStandard(string name, uint height, uint width) {
			Name = name;
			Height = height;
			Width = width;
		}
		
		public string Name;
		public uint Height;
		public uint Width;
	}
	
	public class VideoStandards {
		public static VideoStandard Original = new VideoStandard(Catalog.GetString("Keep original size"), 0, 0);
		public static VideoStandard P240 = new VideoStandard("240p", 240, 0);
		public static VideoStandard P480 = new VideoStandard("480p", 480, 0);
		public static VideoStandard P720 = new VideoStandard("720p", 720, 0);
		public static VideoStandard P1080 = new VideoStandard("1080p", 1080, 0);
		public static VideoStandard P240_4_3 = new VideoStandard("240p (4:3)", 240, 320);
		public static VideoStandard P240_16_9 = new VideoStandard("240p (16:9)", 240, 426);
		public static VideoStandard P480_4_3 = new VideoStandard("480p (4:3)", 480, 640);
		public static VideoStandard P480_16_9 = new VideoStandard("480p (16:9)", 480, 854);
		public static VideoStandard P720_4_3 = new VideoStandard("720p (4:3)", 720, 960);
		public static VideoStandard P720_16_9 = new VideoStandard("720p (16:9)", 720, 1280);
		public static VideoStandard P1080_4_3 = new VideoStandard("1080p (4:3)", 1080, 1440);
		public static VideoStandard P1080_16_9 = new VideoStandard("1080p (16:9)", 1080, 1920);
	}
	
}

