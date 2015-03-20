//
//  Copyright (C) 2015 FLUENDO S.A.
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
using LongoMatch.Core.Store;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Common;
using System.Collections.Generic;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Core.Interfaces.Multimedia;


namespace LongoMatch.Core.Interfaces
{

	public interface IPlayerController: IPlayback
	{
		event TimeChangedHandler TimeChangedEvent;
		event StateChangeHandler PlaybackStateChangedEvent;
		event LoadImageHander LoadImageEvent;
		event PlaybackRateChangedHandler PlaybackRateChangedEvent;
		event VolumeChangedHandler VolumeChangedEvent;
		event ElementLoadedHandler ElementLoadedEvent;
		event ElementUnloadedHandler ElementUnloadedEvent;
		event PARChangedHandler PARChangedEvent;

		Image CurrentMiniatureFrame { get; }

		Image CurrentFrame { get; }

		Time Step { get; set; }

		bool IgnoreTicks { get; set; }

		object CamerasLayout { get; set; }

		List<int> CamerasVisible { get; set; }

		void Open (MediaFileSet fileSet);

		void FramerateUp ();

		void FramerateDown ();

		void StepForward ();

		void StepBackward ();

		void TogglePlay ();

		void LoadEvent (MediaFileSet file, TimelineEvent play, Time seekTime, bool playing);

		void LoadPlayListEvent (Playlist playlist, IPlaylistElement play);

		void UnloadCurrentEvent ();

	}
}