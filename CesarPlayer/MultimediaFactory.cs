﻿// PlayerMaker.cs 
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

using System;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Player;
using LongoMatch.Video.Editor;
using LongoMatch.Video.Utils;

namespace LongoMatch.Video
{
	
	
	public class MultimediaFactory
	{
		
		OperatingSystem oS;
		
		public MultimediaFactory()
		{
			oS = Environment.OSVersion;	
		}
		
		public IPlayer getPlayer(int width, int height){
			switch (oS.Platform) { 
				case PlatformID.Unix:
					return new GstPlayer(width,height,GstUseType.Video);
					
				case PlatformID.Win32NT:
					return new GstPlayer(width,height,GstUseType.Video);
				
				default:
					return new GstPlayer(width,height,GstUseType.Video);
			}		
		}
		
		public IMetadataReader getMetadataReader(){
			
			switch (oS.Platform) { 
				case PlatformID.Unix:
					return new GstPlayer(1,1,GstUseType.Metadata);
					
				case PlatformID.Win32NT:
					return new GstPlayer(1,1,GstUseType.Metadata);
					
				default:
					return new GstPlayer(1,1,GstUseType.Metadata);
			}
		}
		
		public IFramesCapturer getFramesCapturer(){
			switch (oS.Platform) { 
				case PlatformID.Unix:
					return new GstPlayer(1,1,GstUseType.Capture);
					
				case PlatformID.Win32NT:
					return new GstPlayer(1,1,GstUseType.Capture);
					
				default:
					return new GstPlayer(1,1,GstUseType.Capture);
			}
		}
		
		public IVideoEditor getVideoEditor(){
			switch (oS.Platform) { 
				case PlatformID.Unix:
					return new GstVideoSplitter();
					
				case PlatformID.Win32NT:
					return new GstVideoSplitter();	
					
				default:
					return new GstVideoSplitter();
			}
		}	
		
		public ICapturer getCapturer(CapturerType type){
			switch (type) { 
				case CapturerType.FAKE:
					return new FakeCapturer();
					
				case CapturerType.DVCAM:
					return new GstCameraCapturer("test.avi");
				
				case CapturerType.WEBCAM:
					return new FakeCapturer();
									
				default:
					return new FakeCapturer();
			}			
		}
	}
}