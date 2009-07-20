// 
//  Copyright (C) 2009 Andoni Morales Alastruey
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
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 

using System;

namespace LongoMatch.Video.Editor
{
	
	
	public class AvidemuxMerger:GenericMerger
	{
		private const string UNPACK=" --force-unpack ";
		private const string H264_ALT = " --force-alt-h264 ";
		private const string LOAD = " --load ";
		private const string APPEND = " --append ";
		private const string OGG = " OGM ";
		private const string AVI = " AVI ";
		private const string MATROSKA = " MKV ";
		private const string DVD = " MPEG-PS ";
		
		
		public AvidemuxMerger():base("avidemux2_cli")
		{
			OutputMuxer = VideoMuxer.OGG;
		}
		
		protected override string CreateMergeCommandLineArgs(){
			bool first = true;
			string args = "";
			string outputFormat = OGG;
						
			foreach (String path in filesToMergeList){
				args+= UNPACK;
				if (inputFilesVideoCodec == VideoCodec.H264)
					args+= H264_ALT;
				if (first){
					first = false;
					args+= LOAD + path;
				}
				else 
					args+= APPEND + path;				
			}
			
			switch (outputMuxer){
			case VideoMuxer.AVI:
				outputFormat = AVI;
				break;
			case VideoMuxer.OGG:
				outputFormat = OGG;
				break;	
			case VideoMuxer.MKV:
				outputFormat = MATROSKA;
				break;
			case VideoMuxer.DVD:
				outputFormat = DVD;
				break;				
			}
			
			args+= String.Format(" --video-codec COPY --audio-codec COPY --rebuild-index --output-format {0} --force-smart --save {1}",outputFormat,outputFile);
			Console.WriteLine(args);
			return args;
			
		}
	}
}
