// IPlayer.cs
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
using Gtk;
using Gdk;

namespace CesarPlayer
{
	
	


	public interface IPlayer
	{
	

		// Events

		event         StateChangedHandler StateChanged;
		event         TickEventHandler TickEvent;
		event         EndOfStreamEventHandler EndOfStreamEvent;
		event         InvalidVideoFileHandler InvalidVideoFile;
		event         SegmentDoneHandler SegmentDoneEvent;
		event         ErrorEventHandler ErrorEvent;

		string FilePath{
			get;
			set;
		}
		
		long Length{
			get;
		}
		
		
		
		long CurrentTime{
			get;
			
		}
		
		float Position{
			get;
			set;
			
		}
		
		bool LogoMode {
			get;
			set;
		}
		
		int Volume{
			get;
			set;
		}
		
		float Rate{
			get;
			set;
		}
		
		Widget Window{
			get;
		}
		
		bool Playing {
			get;
		}
		
		void Seek(long time, bool accurate);
		
		void Play();
		
		void TogglePlay();
		
		void Pause();
		
		void Stop();
				
		void Close();
		
		
		void Dispose();
		
		void SegmentSeek(long start, long stop);
		
		void SeekInSegment(long pos);
			
		void UpdateSegmentStartTime(long start);
		
		void UpdateSegmentStopTime(long stop);
		
		long GetAccurateCurrentTime();
		
		void CancelProgramedStop();
		
		void SetLogo(string fileName);
		
		Pixbuf GetCurrentFrame();
		
		Pixbuf GetCurrentThumbnail();
		
	}
}
