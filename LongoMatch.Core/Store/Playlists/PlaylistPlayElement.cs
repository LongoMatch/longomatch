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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store.Playlists
{
	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	public class PlaylistPlayElement: IPlaylistElement
	{
		ObservableCollection<CameraConfig> camerasConfig;

		public PlaylistPlayElement (TimelineEvent play)
		{
			Play = play;
			Title = play.Name;
			Rate = play.Rate;
			CamerasLayout = play.CamerasLayout;
			CamerasConfig = new ObservableCollection<CameraConfig> (play.CamerasConfig);
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsChanged {
			get;
			set;
		}

		/// <summary>
		/// The event associated to this playlist element
		/// </summary>
		public TimelineEvent Play {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool Selected {
			get;
			set;
		}

		/// <summary>
		/// The duration of the playlist element
		/// </summary>
		public Time Duration {
			get {
				return Play.Duration;
			}
		}

		/// <summary>
		/// The title of the playlist element
		/// </summary>
		public string Title {
			get;
			set;
		}

		/// <summary>
		/// Override the default <see cref="TimelineEvent.Rate"/>
		/// defined by the <see cref="TimelineEvent"/>
		/// </summary>
		public float Rate {
			get;
			set;
		}

		/// <summary>
		/// A string representing this playback rate
		/// </summary>
		public string RateString {
			get {
				return String.Format ("{0}X", Rate);
			}
		}

		/// <summary>
		/// Override the default <see cref="TimelineEvent.CamerasLayout"/>
		/// defined by the <see cref="TimelineEvent"/>
		/// </summary>
		public object CamerasLayout {
			get;
			set;
		}

		/// <summary>
		/// Override the default <see cref="TimelineEvent.CamerasConfig"/>
		/// defined by the <see cref="TimelineEvent"/>
		/// </summary>
		public ObservableCollection<CameraConfig> CamerasConfig {
			get {
				return camerasConfig;
			}
			set {
				if (camerasConfig != null) {
					camerasConfig.CollectionChanged -= ListChanged;
				}
				camerasConfig = value;
				if (camerasConfig != null) {
					camerasConfig.CollectionChanged += ListChanged;
				}
			}
		}


		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public string Description {
			get {
				return (Play.Name + "\n" + TagsDescription () + "\n" + TimesDesription ());
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Image Miniature {
			get {
				return Play.Miniature;
			}
		}

		public override string ToString ()
		{
			if (CamerasConfig.Count > 0) {
				return string.Format ("Duration={0}, Title={1}, Rate={2}, ROI={3}, Drawings={4}",
					Duration, Title, RateString, CamerasConfig [0].RegionOfInterest,
					Play.Drawings.Count);
			} else {
				return string.Format ("Duration={0}, Title={1}, Rate={2}, ROI=None, Drawings={3}",
					Duration, Title, RateString,
					Play.Drawings.Count);
			}

		}

		void ListChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		string TagsDescription ()
		{
			return String.Join ("-", Play.Tags.Select (t => t.Value));
		}

		string TimesDesription ()
		{
			var desc = Play.Start.ToMSecondsString () + " - " + Play.Stop.ToMSecondsString ();
			if (Rate != 1) {
				desc += " (" + RateString + ")";
			}
			return desc;
		}
	}
}

