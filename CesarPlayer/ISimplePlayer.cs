// ISimplePlayer.cs
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
using CesarPlayer;
using Gdk;

namespace CesarPlayer
{
	
	
	public interface ISimplePlayer
	{
		event         CesarPlayer.TickHandler Tick;
		
		
		
		
		
		
		long AccurateCurrentTime{
			get;
		}
		
		long CurrentTime{
			get;
			
		}
		
		long StreamLength{
			get;
		}
		
		bool LogoMode {
			get;
			set;
		}
		
		
		Pixbuf CurrentFrame{
			get;
		}
		
		
		
		void SeekTo(long time, bool accurate);
		
		void SetStartStop(long start, long stop);
		
		void Open(string mrl);
		
		void Play();
		
		void Pause();
		
		void SeekInSegment(long pos);
		
		void UpdateSegmentStartTime(long start);
		
		void UpdateSegmentStopTime(long stop);
		
		void SetPlayListElement(string fileName,long start, long stop, bool hasNext);
		
	}
}
