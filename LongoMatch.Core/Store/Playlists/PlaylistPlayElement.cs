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
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;

namespace LongoMatch.Core.Store.Playlists
{
	[Serializable]
	public class PlaylistPlayElement: IPlaylistElement
	{
		public PlaylistPlayElement (TimelineEvent play, MediaFileSet fileset=null)
		{
			Play = play;
			Title = play.Name;
			Rate = play.Rate;
			Angles = new HashSet<MediaFileAngle> (play.ActiveViews);
			FileSet = fileset;
		}

		public TimelineEvent Play {
			get;
			set;
		}

		[JsonIgnore]
		public bool Selected {
			get;
			set;
		}

		public Time Duration {
			get {
				return Play.Duration;
			}
		}

		public string Title {
			get;
			set;
		}

		public float Rate {
			get;
			set;
		}

		public string RateString {
			get {
				return String.Format ("{0}X", Rate);
			}
		}

		public MediaFileSet FileSet {
			get;
			set;
		}

		public HashSet<MediaFileAngle> Angles {
			get;
			set;
		}

		[JsonIgnore]
		public string Description {
			get {
				if (Rate != 1) {
					return Title + " " + Play.Start.ToSecondsString () + " " + Play.Stop.ToSecondsString () + " (" + RateString + ")";
				} else {
					return Title + " " + Play.Start.ToSecondsString () + " " + Play.Stop.ToSecondsString ();
				}
			}
		}

		[JsonIgnore]
		public Image Miniature {
			get {
				return Play.Miniature;
			}
		}
	}
}

