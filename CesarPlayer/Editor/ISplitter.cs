// 
//  Copyright (C) 2009 Andoni Morales Alastruey 2009
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
	
	
	public interface IVideoSplitter
	{
		event LongoMatch.Video.Handlers.ProgressHandler Progress;	
		
		bool EnableAudio{
			set;
			get;
		}		
		
		bool EnableTitle{
			set;
			get;
		}
		
		int VideoBitrate {
			set;
			get;
		}
		
		int AudioBitrate {
			set;
			get;
		}
		
		int Width {
			get ;
			set;
		}
		
		int Height {
			get ;
			set ;
		}
		
		string OutputFile {
			get ;
			set;
		}
		
		AudioCodec AudioCodec{
			set;
		}
		
		VideoCodec VideoCodec{
			set;
		}
		
		VideoMuxer VideoMuxer{
			set;
		}		
		
		void SetSegment(string filePath, long start, long duration, double rate, string title);
		
		void Start();
		
		void Cancel();
	}
}
