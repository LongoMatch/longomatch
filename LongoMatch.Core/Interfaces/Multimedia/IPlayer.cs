// IPlayer.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using LongoMatch.Store;
using LongoMatch.Handlers;
using Image = LongoMatch.Common.Image;


namespace LongoMatch.Interfaces.Multimedia
{
	public interface IPlayer
	{
		// Events
		event         ErrorHandler Error;
		event         System.EventHandler Eos;
		event         StateChangeHandler StateChange;
		event         TickHandler Tick;
		event         ReadyToSeekHandler ReadyToSeek;

		Time StreamLength {get;}
		Time CurrentTime {get;}
		double Volume {get;set;}
		bool Playing {get;}
		double Rate {set;}
		IntPtr WindowHandle {set;}

		bool Open (string mrl);
		void Play();
		void Pause();
		void Stop();
		void Close();
		bool Seek (Time time, bool accurate);
		bool SeekToNextFrame();
		bool SeekToPreviousFrame();
		Image GetCurrentFrame (int width=-1, int height=-1);
		void Expose ();
		void Dispose();
	}
}
