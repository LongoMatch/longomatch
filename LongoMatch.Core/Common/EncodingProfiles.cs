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
namespace LongoMatch.Common
{
	[Serializable]
	public struct EncodingProfile
	{
		public EncodingProfile(string name, string extension,
		                       VideoEncoderType videoEncoder,
		                       AudioEncoderType audioEncoder,
		                       VideoMuxerType muxer) {
		    Name = name;
		    Extension = extension;
			VideoEncoder = videoEncoder;
			AudioEncoder = audioEncoder;
			Muxer = muxer;
		}
		
		public string Name;
		public string Extension;
		public VideoEncoderType VideoEncoder;
		public AudioEncoderType AudioEncoder;
		public VideoMuxerType Muxer;
	}
	
	public class EncodingProfiles {
		public static EncodingProfile WebM = new EncodingProfile("WebM (VP8 + Vorbis)", "webm",
		                                                         VideoEncoderType.VP8,
		                                                         AudioEncoderType.Vorbis,
		                                                         VideoMuxerType.WebM);
		                                                                     
		public static EncodingProfile Avi = new EncodingProfile("AVI (Mpeg4 + MP3)", "avi",
		                                                        VideoEncoderType.Mpeg4,
		                                                        AudioEncoderType.Mp3,
		                                                        VideoMuxerType.Avi);

		public static EncodingProfile MP4 = new EncodingProfile("MP4 (H264 + AAC)", "mp4",
		                                                        VideoEncoderType.H264,
		                                                        AudioEncoderType.Aac,
		                                                        VideoMuxerType.Mp4);
		                                                        
		public static EncodingProfile MatroskaMpeg4 = new EncodingProfile("Matroska (Mpeg4 + Vorbis)", "avi",
		                                                        VideoEncoderType.Mpeg4,
		                                                        AudioEncoderType.Vorbis,
		                                                        VideoMuxerType.Matroska);

		public static EncodingProfile MatroskaH264 = new EncodingProfile("Matroska (H264 + AAC)", "mp4",
		                                                        VideoEncoderType.H264,
		                                                        AudioEncoderType.Aac,
		                                                        VideoMuxerType.Matroska);
	}
	
}
