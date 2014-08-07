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
using LongoMatch.Store.Playlists;

namespace LongoMatch.Interfaces.GUI
{
	public interface IPlayerBin
	{
		event StateChangeHandler PlayStateChanged;
		event SeekEventHandler SeekEvent;
		
		Time CurrentTime {get;}
		Time StreamLength {get;}
		Image CurrentMiniatureFrame {get;}
		Image CurrentFrame {get;}
		bool Opened {get;}
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
		void LoadPlay (MediaFile file, Play play, Time seekTime, bool playing);
		void LoadPlayListPlay (Playlist playlist, IPlaylistElement play);
		void CloseSegment();
	}
}

