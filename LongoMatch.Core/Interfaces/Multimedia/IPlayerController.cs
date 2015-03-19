//
//  Copyright (C) 2015 jl
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

// IController -> Controller
// Controller.Player.CurrentTime
// Controller.Player.Open()
// Controller.LoadPlay
// Controller.LoadPlayListPlay
// SetWindowHandle(MediaFile)
// SetActiveMediaFiles(List<MediaFile> actives, List<IntPtr> windows)
// EnableMediaFile(0, gpointer id)
// EnableMediaFile(1, gpointer id)
//
/* MultiPlayerController : IPlayerController
		* {
		    List<IPlayer> players;

			SetActiveMediaFiles(List<MediaFile> actives, List<IntPtr> windows)
			{
				IPlayer == MultimediaBackend.GetPlayer();
				// sincronizar dos players
				Iplayer.Link(IPlayer);
			}
		}
/*
 * class PlayerController : IPlayerController
 * {
 * 
 * }
 * 
 * class OPEPlayerController : PlayerControler
 * {
 *   Open()
 *   Play()
 *   Pause()
 *   EnableMediaFile();
 * }
 *  PalyerController : IPlayerController {
 *   PlayerControler () {
 *     this.player = Multimedia.getPlayer();
 *   }
 * }
 * class MultiPlayerView()
 * {
 *    MultiPlayerView(IPlayerController controller)
 * {
 * 		// eventos
+=

	* }
	* }
	* 
	* 
	* /
	//
	/*
	* 
	* 
	* /
	// EnableMediaFile(int i, gpointer id)
	// DisableMediaFile(int i)

	void Open (MediaFileSet fileSet);
void Close();
void Play ();
void Pause ();
void TogglePlay ();
void Seek (Time time, bool accurate);
void StepForward();
void StepBackward();
void SeekToNextFrame();
void SeekToPreviousFrame();
void FramerateUp();
void FramerateDown();
//
*/
using LongoMatch.Core.Store.Playlists;

namespace LongoMatch.Core.Interfaces.Multimedia
{
	public interface IPlayerController
	{
		/// <summary>
		/// Gets the player.
		/// </summary>
		/// <returns>The player.</returns>
		IPlayer GetPlayer();
		// FIXME I have no idea what this functions do, why a CloseSegment() with no OpenSegment?
		// why LoadPlay? Load and then Play?
		void LoadPlay (MediaFileSet file, TimelineEvent play, Time seekTime, bool playing);
		void LoadPlayListPlay (Playlist playlist, IPlaylistElement play);
		void CloseSegment();
	}
}