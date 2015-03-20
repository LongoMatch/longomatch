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
using LongoMatch.Core.Store;
using LongoMatch.Core.Handlers;
using Image = LongoMatch.Core.Common.Image;

// FIXME In order to support multiple streams the current approach
// is the simplest one. There is no stream selection, enabling, etc
// just a modification on the functions that manage the player stream
// to also receive a list for multiple streams:
// Functions/Properties added/modified:
// List<IntPtr> WindowHandles { set; }
// bool Open (List<string> mrls);
// bool Open (MediaFileSet mfs);
// bool Open (MediaFile mf);
// Their simple cases are still there, we need to get rid of them later
using System.Collections.Generic;

namespace LongoMatch.Core.Interfaces.Multimedia
{
	public interface IPlayer: IDisposable
	{
		// Events
		event         ErrorHandler Error;
		event         EosHandler Eos;
		event         StateChangeHandler StateChange;
		event         ReadyToSeekHandler ReadyToSeek;

		Time StreamLength {get;}
		Time CurrentTime {get;}
		double Volume {get;set;}
		bool Playing {get;}
		double Rate {set;}

		List<IntPtr> WindowHandles { set; }
		IntPtr WindowHandle { set; }

		bool Open (List<string> mrls);
		bool Open (string mrl);
		bool Open (MediaFileSet mfs);
		bool Open (MediaFile mf);

		void Play();
		void Pause();
		void Stop();
		void Close();
		bool Seek (Time time, bool accurate = false, bool synchronous = false);
		bool SeekToNextFrame();
		bool SeekToPreviousFrame();
		Image GetCurrentFrame (int width=-1, int height=-1);
		void Expose ();
		// Functions to add that belonged to IPlayerBin
		// void Open (MediaFileSet fileSet);
		// void Seek (Time time, bool accurate);
		// void FramerateUp();
		// void FramerateDown();
		// void TogglePlay ();
	}
}
