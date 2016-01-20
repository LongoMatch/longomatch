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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;


namespace LongoMatch.Core.Interfaces
{

	public interface IPlayerController: IPlayback
	{
		event TimeChangedHandler TimeChangedEvent;
		event LoadDrawingsHandler LoadDrawingsEvent;
		event PlaybackRateChangedHandler PlaybackRateChangedEvent;
		event VolumeChangedHandler VolumeChangedEvent;
		event ElementLoadedHandler ElementLoadedEvent;
		event MediaFileSetLoadedHandler MediaFileSetLoadedEvent;
		event PrepareViewHandler PrepareViewEvent;

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
		/// The list of configurations for visible cameras.
		/// </summary>
		ObservableCollection<CameraConfig> CamerasConfig { get; set; }

		/// <summary>
		/// List of view ports set by the view.
		/// </summary>
		List<IViewPort> ViewPorts { set; }

		/// <summary>
		/// Flag indicating whether the object is active
		/// </summary>
		bool Active { get; set; }

		/// <summary>
		/// Gets or sets the loaded playlist.
		/// </summary>
		/// <value>The loaded playlist.</value>
		Playlist LoadedPlaylist{ get; set; }

		/// <summary>
		/// Flag indicating whether this <see cref="LongoMatch.Core.Interfaces.IPlayerController"/>
		/// is in presentation mode.
		/// Presentation mode emits ticks relative to the full duration of the loaded playlist.
		/// </summary>
		/// <value><c>true</c> if presentation mode; otherwise, <c>false</c>.</value>
		bool PresentationMode { get; set; }

		/// <summary>
		/// Switch the specified play, playlist and element.
		/// </summary>
		/// <param name="play">Play.</param>
		/// <param name="playlist">Playlist.</param>
		/// <param name="element">Element.</param>
		void Switch (TimelineEvent play, Playlist playlist, IPlaylistElement element);

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
		/// The file set for this event comes from <see cref="evt.Fileset"/>
		/// </summary>
		/// <param name="evt">The timeline event.</param>
		/// <param name="seekTime">Seek time.</param>
		/// <param name="playing">If set to <c>true</c> playing.</param>
		void LoadEvent (TimelineEvent evt, Time seekTime, bool playing);

		/// <summary>
		/// Loads a playlist event.
		/// </summary>
		/// <param name="playlist">The playlist for this event.</param>
		/// <param name="element">The event to load.</param>
		/// <param name = "playing">Flag to start playing the event being loaded</param>
		void LoadPlaylistEvent (Playlist playlist, IPlaylistElement element, bool playing);

		/// <summary>
		/// Unloads the current event.
		/// </summary>
		void UnloadCurrentEvent ();

		/// <summary>
		/// Seek the specified position. This position should be relative to whatever is loaded.
		/// There are 3 options:
		/// - Playing a video, no events loaded -> A seek needs to be relative
		/// 	to the video file, no adjustments needed.
		/// - Playing a loaded event (a presentation event or a single event)
		///  -> A seek needs to be relative to the current event.
		/// 	Event start is Time(0)
		/// - Playing a playlist (with presentationMode on) -> A seek needs to
		/// 	be relative to the full playlist duration.
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
		/// Jump to the previous element / to the begining if a <see cref="IPlaylistElement"/> is loaded,
		/// to the beginning of the event if a <see cref="TimelineEvent"/> is loaded or
		/// to the beginning of the stream of no element is loaded.
		/// </summary>
		/// <param name="force">Force it to jump to the previous element, regardless of played time.</param>
		void Previous (bool force = false);

		/// <summary>
		/// The view should call it when it's ready to start playback,
		/// once it has set a valid window handle to start rendering.
		/// </summary>
		void Ready ();

		/// <summary>
		/// Applies the configured RegionOfInterest for a given camera.
		/// </summary>
		/// <param name="camConfig">The camera to update.</param>
		void ApplyROI (CameraConfig camConfig);

		/// <summary>
		/// Emits a DrawFrame event with the currently loaded frame.
		/// </summary>
		void DrawFrame ();
	}
}