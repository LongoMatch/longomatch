// PlayerMaker.cs 
//
//  Copyright (C) 2007 Andoni Morales Alastruey
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Player;
using LongoMatch.Video.Utils;

namespace LongoMatch.Video
{
	
	
	public class PlayerMaker
	{
		
		OperatingSystem oS;
		
		public PlayerMaker()
		{
			oS = Environment.OSVersion;	
		
		}
		
		public IPlayer getPlayer(int width, int height){
			
			switch (oS.Platform) { 
			 case PlatformID.Unix:
				return new GstPlayer(width,height,UseType.Video);
				
				
			case PlatformID.Win32NT:
				return new GstPlayer(width,height,UseType.Video);
				//return new DSPlayer(UseType.Video);
				
				
			 default:
				return new GstPlayer(width,height,UseType.Video);
				
			}
		
		}
		
		public IMetadataReader getMetadataReader(){
			
			switch (oS.Platform) { 
			 case PlatformID.Unix:
				return new GstPlayer(1,1,UseType.Metadata);
				
			case PlatformID.Win32NT:
				return new GstPlayer(1,1,UseType.Metadata);
				//return new DSPlayer(UseType.Metadata);
				
			 default:
				return new GstPlayer(1,1,UseType.Metadata);
			}
		}
		
		public IFramesCapturer getFramesCapturer(){
			
			switch (oS.Platform) { 
			 case PlatformID.Unix:
				return new GstPlayer(1,1,UseType.Metadata);
				
			case PlatformID.Win32NT:
				return new GstPlayer(1,1,UseType.Metadata);
				//return new DSPlayer(UseType.Metadata);
				
			 default:
				return new GstPlayer(1,1,UseType.Metadata);
			}
		}
		
		
		public ICapturer getCapturer(){
			switch (oS.Platform) { 
				
			 case PlatformID.Unix:
				return new GstCameraCapturer("test.avi");
				
			case PlatformID.Win32NT:
				return new GstCameraCapturer("test.avi");	
				
			 default:
				return new GstCameraCapturer("test.avi");
			}
			
		}
		
	}
}
