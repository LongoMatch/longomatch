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
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Core.Interfaces;

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
		public IPlayer Player
		{
			get {
				return player;
			}
		}

		public void LoadEvent (MediaFileSet file, TimelineEvent ev, Time seekTime, bool playing)
		{
			throw new NotImplementedException ();
		}

		public void LoadPlayListEvent (Playlist playlist, IPlaylistElement ev)
		{
			throw new NotImplementedException ();
		}

		public void UnloadCurrentEvent()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

