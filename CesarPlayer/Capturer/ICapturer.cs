// ICapturer.cs
//
//  Copyright (C) 2008 Andoni Morales Alastruey
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

namespace LongoMatch.Video.Capturer
{
	
	
	public interface ICapturer
	{
		
		
		uint EncodeWidth {
			get ;
			set ;
		}

		uint EncodeHeight {
			get;
			set ;
		}
		
		 string OutputFile {
			get ;
			set ;
		}

				
		uint VideoBitrate {
			get;
			set ;
		}
		
		uint AudioBitrate {
			get ;
			set ;
		}
		bool SetVideoEncoder(LongoMatch.Video.Capturer.GccVideoEncoderType type);
		bool SetAudioEncoder(LongoMatch.Video.Capturer.GccAudioEncoderType type);
		bool SetVideoMuxer(LongoMatch.Video.Capturer.GccVideoMuxerType type);
		
		void TogglePause();
		
		void Start();
		
		void Stop();
		
		void Run();
	}
}
