//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Common;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store.Playlists
{
	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	public class PlaylistDrawing: IPlaylistElement
	{
		public PlaylistDrawing (FrameDrawing drawing)
		{
			Drawing = drawing;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsChanged {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool Selected {
			get;
			set;
		}

		public int Width {
			get;
			set;
		}

		public int Height {
			get;
			set;
		}

		public FrameDrawing Drawing {
			get;
			set;
		}

		public Time Duration {
			get;
			set;
		}

		public string Description {
			get {
				return Duration == null ? "" : Duration.ToMSecondsString ();
			}
		}

		public Image Miniature {
			get;
			set;
		}
	}
}

