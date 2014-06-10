// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
// 
using System;
using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Store;

namespace LongoMatch.Interfaces.GUI
{
	public interface IPlayerBin
	{
		event SegmentClosedHandler SegmentClosedEvent;
		event TickHandler Tick;
		event ErrorHandler Error;
		event StateChangeHandler PlayStateChanged;
		event NextButtonClickedHandler Next;
		event PrevButtonClickedHandler Prev;
		event DrawFrameHandler DrawFrame;
		event SeekEventHandler SeekEvent;
		event DetachPlayerHandler Detach;
		event PlaybackRateChangedHandler PlaybackRateChanged;
		
		Time CurrentTime {get;}
		Time StreamLength {get;}
		Image CurrentMiniatureFrame {get;}
		Image CurrentFrame {get;}
		bool Opened {get;}
		bool Detached {get;set;}
		bool SeekingEnabled {set;}
		bool Sensitive {set; get;}

		void Open (string mrl);
		void Close();
		void Play ();
		void Pause ();
		void TogglePlay ();
		void ResetGui();
		void Seek (Time time, bool accurate);
		void StepForward();
		void StepBackward();
		void SeekToNextFrame();
		void SeekToPreviousFrame();
		void FramerateUp();
		void FramerateDown();
		void LoadPlay (string fileName, Play play, Time seekTime, bool playing);
		void LoadPlayListPlay (PlayListPlay play, bool hasNext);
		void CloseSegment();
	}
}

