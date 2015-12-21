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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using Newtonsoft.Json;

#if HAVE_GTK
using SImage = Gdk.Pixbuf;
#endif

namespace LongoMatch.Core.Store.Playlists
{
	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	public class PlaylistImage: IPlaylistElement
	{
		public PlaylistImage (Image image, Time duration)
		{
			Image = image;
			Miniature = image.Scale (Constants.MAX_THUMBNAIL_SIZE,
			                         Constants.MAX_THUMBNAIL_SIZE);
			Duration = duration;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsChanged {
			get;
			set;
		}

		public Image Image {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool Selected {
			get;
			set;
		}

		public Time Duration {
			get;
			set;
		}

		public Image Miniature {
			get;
			set;
		}

		public string Description {
			get {
				return Duration.ToSecondsString ();
			}
		}
	}
}

