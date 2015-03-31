﻿//
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
		event LoadDrawingsHandler LoadDrawingsEvent;
		event PlaybackRateChangedHandler PlaybackRateChangedEvent;
		event VolumeChangedHandler VolumeChangedEvent;
		event ElementLoadedHandler ElementLoadedEvent;
		event PARChangedHandler PARChangedEvent;

		/// <summary>
		/// The file set currently openned by the player.
		/// </summary>
		MediaFileSet FileSet { get; }

		/// <summary>
		/// <c>true</c> if a <see cref="MediaFileSet"/> is currently opened.
		/// </summary>
		bool Opened { get; }

		/// <summary>
		/// Gets the current miniature frame.
		/// </summary>
		Image CurrentMiniatureFrame { get; }

		/// <summary>
		/// Gets the current frame.
		/// </summary>
		Image CurrentFrame { get; }

		/// <summary>
		/// The time to step in <see cref="StepForward"/> and <see cref="StepBackward"/>.
		/// </summary>
		Time Step { get; set; }

		/// <summary>
		/// When set to <c>true</c> clock ticks will be ignored.
		/// This can be used by the view to prevent updates after a seek
		/// when seeking through the seek bar.
		/// </summary>
		bool IgnoreTicks { get; set; }

		/// <summary>
		/// The cameras' layout set by the view
		/// </summary>
		object CamerasLayout { get; set; }

		/// <summary>
		/// The list of visible cameras.
		/// </summary>
		List<int> CamerasVisible { get; set; }

		/// <summary>
		/// Open the specified fileSet.
		/// </summary>
		void Open (MediaFileSet fileSet);

		/// <summary>
		/// Increases the framerate.
		/// </summary>
		void FramerateUp ();

		/// <summary>
		/// Decreases the framerate.
		/// </summary>
		void FramerateDown ();

		/// <summary>
		/// Step the amount in <see cref="Step"/> forward.
		/// </summary>
		void StepForward ();

		/// <summary>
		/// Step the amount in <see cref="Step"/> backward.
		/// </summary>
		void StepBackward ();

		/// <summary>
		/// Changes the playback state pause/playing.
		/// </summary>
		void TogglePlay ();

		/// <summary>
		/// Loads a timeline event.
		/// </summary>
		/// <param name="file">The file set for this event.</param>
		/// <param name="evt">The timeline event.</param>
		/// <param name="seekTime">Seek time.</param>
		/// <param name="playing">If set to <c>true</c> playing.</param>
		void LoadEvent (MediaFileSet file, TimelineEvent evt, Time seekTime, bool playing);

		/// <summary>
		/// Loads a playlist event.
		/// </summary>
		/// <param name="playlist">The playlist for this event.</param>
		/// <param name="element">The event to load.</param>
		void LoadPlaylistEvent (Playlist playlist, IPlaylistElement element);

		/// <summary>
		/// Unloads the current event.
		/// </summary>
		void UnloadCurrentEvent ();

		/// <summary>
		/// Seek the specified absolute position.
		/// </summary>
		/// <param name="time">The position to seek to.</param>
		/// <param name="accurate">If set to <c>true</c> performs an accurate, otherwise a keyframe seek.</param>
		/// <param name="synchronous">If set to <c>true</c> performs a synchronous seek.</param>
		/// <param name="throttled">If set to <c>true</c> performs a throttled seek.</param>
		bool Seek (Time time, bool accurate = false, bool synchronous = false, bool throttled = false);

		/// <summary>
		/// Performs a seek to proportional the duration of the event loaded.
		/// </summary>
		/// <param name="pos">Position.</param>
		void Seek (double pos);


		/// <summary>
		/// Jump the next element in the playlist if a <see cref="IPlaylistElement"/> is loaded.
		/// </summary>
		void Next ();

		/// <summary>
		/// Jump to the previous element if a <see cref="IPlaylistElement"/> is loaded,
		/// to the beginning of the event if a <see cref="TimelineEvent"/> is loaded or
		/// to the beginning of the stream of no element is loaded.
		/// </summary>
		void Previous ();

		void Ready ();
	}
}