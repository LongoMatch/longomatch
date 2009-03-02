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
using LongoMatch.Video.Handlers;


namespace LongoMatch.Video.Player
	      
{
	public interface IPlayer
	{
	

		// Events
		
		
		event         SegmentDoneHandler SegmentDoneEvent;
		event         ErrorHandler Error;
		event         System.EventHandler Eos;
		event         LongoMatch.Video.Handlers.StateChangedHandler StateChanged;
		event         TickHandler Tick;
		event         System.EventHandler GotDuration;
		event         System.EventHandler SegmentDone;

		string Mrl{
			get;			
		}
		
		
		long StreamLength{
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
		
		
		Widget Window{
			get;
		}
		
		bool Playing {
			get;
		}
		
		Pixbuf CurrentFrame{
			get;
		}
		
		string Logo {
			set;
		}
		
		long AccurateCurrentTime{
			get;
		}
		
		bool SeekTo(long time, bool accurate);
		
		bool Play();
		
		bool Open(string mrl);
		
		bool SetRate(float rate);
		
		bool SetRateInSegment(float rate, long stopTime);
		
		
		
		void TogglePlay();
		
		void Pause();
		
		void Stop();
				
		void Close();
		
		
		void Dispose();
		
		bool SegmentSeek(long start, long stop);
		
		bool SeekInSegment(long pos);
		
		bool NewFileSeek(long start, long stop);
			
		void UpdateSegmentStartTime(long start);
		
		void UpdateSegmentStopTime(long stop);
		
		void SeekToNextFrame(bool in_segment);
		
		void SeekToPreviousFrame(bool in_segment);
		
		void CancelProgramedStop();
			
		
		
		
	}
}
