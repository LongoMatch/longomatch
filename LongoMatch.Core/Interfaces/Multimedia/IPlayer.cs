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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LongoMatch.Core.Handlers;
using VAS.Core.Store;
using Image = VAS.Core.Common.Image;

namespace LongoMatch.Core.Interfaces.Multimedia
{
	public interface IPlayback: IDisposable
	{
		/// <summary>
		/// Gets the length of the opened media file.
		/// </summary>
		Time StreamLength { get; }

		/// <summary>
		/// Gets the current playback time of the player.
		/// </summary>
		Time CurrentTime { get; }

		/// <summary>
		/// Gets or sets the volume of the player.
		/// </summary>
		double Volume { get; set; }

		/// <summary>
		/// <c>true</c> if the playing is in a playing state or <c>false</c> if it's paused.
		/// </summary>
		bool Playing { get; }

		/// <summary>
		/// Gets or sets the playback rate.
		/// </summary>
		/// <value>The rate.</value>
		double Rate { get; set; }

		/// <summary>
		/// Starts playing.
		/// </summary>
		void Play (bool synchronous = false);

		/// <summary>
		/// Pauses the player.
		/// </summary>
		void Pause (bool synchronous = false);

		/// <summary>
		/// Stops the player.
		/// </summary>
		void Stop (bool synchronous = false);

		/// <summary>
		/// Seek the specified time.
		/// </summary>
		/// <param name="time">Seek position.</param>
		/// <param name="accurate">If set to <c>true</c>, does and Accurate seek,
		///  otherwise a Keyframe seek.</param>
		/// <param name="synchronous">If set to <c>true</c>, does a synchronous
		///  seek waiting for the seek to finish.</param>
		bool Seek (Time time, bool accurate = false, bool synchronous = false);

		/// <summary>
		/// Seeks to next frame.
		/// </summary>
		bool SeekToNextFrame ();

		/// <summary>
		/// Seeks to previous frame.
		/// </summary>
		bool SeekToPreviousFrame ();

		/// <summary>
		/// Force a redraw of the last frame.
		/// </summary>
		void Expose ();
	}

	public interface IPlayer: IPlayback
	{
		// Events
		event ErrorHandler Error;
		event EosHandler Eos;
		event StateChangeHandler StateChange;
		event ReadyToSeekHandler ReadyToSeek;

		/// <summary>
		/// Sets the window handle in when the video sink can draw.
		/// </summary>
		object WindowHandle { set; }

		/// <summary>
		/// Closes the opened media file.
		/// </summary>
		void Close ();

		/// <summary>
		/// Open the specified url.
		/// </summary>
		/// <param name="mrl">An URL with the file to open.</param>
		bool Open (string mrl);

		/// <summary>
		/// Open the specified media file.
		/// </summary>
		/// <param name="mf">A media file to open.</param>
		bool Open (MediaFile mf);

		/// <summary>
		/// Gets the current frame, scalling it to the desired width and height.
		/// If width and height are -1, the frame is returning with its original size.
		/// </summary>
		/// <returns>The current frame.</returns>
		/// <param name="width">Output width.</param>
		/// <param name="height">Output height.</param>
		Image GetCurrentFrame (int width = -1, int height = -1);
	}

	public interface IMultiPlayer: IPlayer
	{

		event ScopeStateChangedHandler ScopeChangedEvent;

		/// <summary>
		/// Open a set of media files.
		/// </summary>
		/// <param name="mfs">The set of files to open.</param>
		bool Open (MediaFileSet mfs);

		/// <summary>
		/// A list of window handles set by the view on which the player can draw.
		/// </summary>
		List<object> WindowHandles { set; }

		/// <summary>
		/// A list with the current configurations of visible cameras.
		/// Each configuration contains the camera index of the <see cref="MediaFile"/>
		/// to use from the <see cref="MediaFileSet"/> opened.
		/// The position in the list matches with the WindowHandle on which it should be drawn.
		/// {0, 1} will play MediaFileSet[0] in WindowHandles[0] and MediaFileSet[1] in WindowsHandles[1]
		/// {2, 3} will play MediaFileSet[2] in WindowHandles[0] and MediaFileSet[3] in WindowsHandles[1]
		/// </summary>
		ObservableCollection <CameraConfig> CamerasConfig { set; }

		void ApplyCamerasConfig ();

		/// <summary>
		/// Applies the configured RegionOfInterest for a given camera.
		/// </summary>
		/// <param name="camConfig">The camera to update.</param>
		void ApplyROI (CameraConfig camConfig);
	}
}
