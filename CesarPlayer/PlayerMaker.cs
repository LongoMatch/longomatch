// PlayerMaker.cs created with MonoDevelop
// User: ando at 3:13 25/11/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace CesarPlayer
{
	
	
	public class PlayerMaker
	{
		
		OperatingSystem oS;
		
		public PlayerMaker()
		{
			oS = Environment.OSVersion;	
		
		}
		
		public IPlayer getPlayer(int width, int height){
			
			switch (oS.Platform){
			 case PlatformID.Unix:
				return new GstPlayer(width,height,BvwUseType.BVW_USE_TYPE_VIDEO);
				
			case PlatformID.Win32NT:
				return new GstPlayer(width,height,BvwUseType.BVW_USE_TYPE_VIDEO);
				

				
			 default:
				return new GstPlayer(width,height,BvwUseType.BVW_USE_TYPE_VIDEO);
				
				
			}
				                     
		
		}
	}
}
