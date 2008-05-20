// IPlayer.cs
//
//  Copyright (C) 2007 [name of author]
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
	
	public delegate void StateChangedHandler (bool playing);
	public delegate void TickEventHandler (long currentTime, long streamLength, float position, bool seekable);
	public delegate void EndOfStreamEventHandler ();
	public delegate void SegmentDoneHandler();
	public delegate void InvalidVideoFileHandler (string videoFile);


	public interface IPlayer
	{
	

		// Events

		event         StateChangedHandler StateChanged;
		event         TickEventHandler TickEvent;
		event         EndOfStreamEventHandler EndOfStreamEvent;
		event         InvalidVideoFileHandler InvalidVideoFile;
		event         SegmentDoneHandler SegmentDoneEvent;

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
		
		double Rate{
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
		
		void CancelProgramedStop();
		
		void SetLogo(string fileName);
		
		Pixbuf GetCurrentFrame();
		
	}
}
