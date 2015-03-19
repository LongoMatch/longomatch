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
using LongoMatch.Core.Interfaces.Multimedia;

namespace LongoMatch.Core.Store
{
	public class PlayerController : IPlayerController
	{
		IPlayer player;

		public PlayerController ()
		{
			player = Config.MultimediaToolkit.GetPlayer ();
		}

		~PlayerController ()
		{
			player.Dispose ();
		}

		#region IPlayerController implementation
		IPlayer IPlayerController.GetPlayer ()
		{
			return player;
		}
		void IPlayerController.LoadPlay (MediaFileSet file, TimelineEvent play, Time seekTime, bool playing)
		{
			throw new NotImplementedException ();
		}
		void IPlayerController.LoadPlayListPlay (LongoMatch.Core.Store.Playlists.Playlist playlist, LongoMatch.Core.Interfaces.IPlaylistElement play)
		{
			throw new NotImplementedException ();
		}
		void IPlayerController.CloseSegment ()
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}

