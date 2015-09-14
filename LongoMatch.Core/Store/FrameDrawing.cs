//
//  Copyright (C) 2009 Andoni Morales Alastruey
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
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store.Drawables;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{

	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	public class FrameDrawing: IChanged
	{
		ObservableCollection<Drawable> drawables;
		const int DEFAULT_PAUSE_TIME = 5000;

		/// <summary>
		/// Represent a drawing in the database using a {@Gdk.Pixbuf} stored
		/// in a bytes array in PNG format for serialization. {@Drawings}
		/// are used by {@MediaTimeNodes} to store the key frame drawing
		/// which stop time is stored in a int value
		/// </summary>
		public FrameDrawing ()
		{
			Pause = new Time (DEFAULT_PAUSE_TIME);
			Drawables = new ObservableCollection<Drawable> ();
			CameraConfig = new CameraConfig (0);
			RegionOfInterest = new Area ();
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsChanged {
			get;
			set;
		}

		public Image Miniature {
			get;
			set;
		}

		public Image Freehand {
			get;
			set;
		}

		/// <summary>
		/// List of <see cref="Drawable"/> objects in the canvas
		/// </summary>
		public ObservableCollection<Drawable> Drawables {
			get {
				return drawables;
			}
			set {
				if (drawables != null) {
					drawables.CollectionChanged -= ListChanged;
				}
				drawables = value;
				if (drawables != null) {
					drawables.CollectionChanged += ListChanged;
				}
			}
		}

		/// <summary>
		/// Render time of the drawing
		/// </summary>
		public Time Render {
			get;
			set;
		}

		/// <summary>
		/// Time to pause the playback and display the drawing
		/// </summary>
		public Time Pause {
			set;
			get;
		}

		/// <summary>
		/// The camera configuration for this event.
		/// </summary>
		public CameraConfig CameraConfig {
			get;
			set;
		}

		/// <summary>
		/// The region of interest for this drawing, can be used to override
		/// the one in <see cref="CameraConfig"/>
		/// </summary>
		public Area RegionOfInterest {
			get;
			set;
		}

		void ListChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}
	}
}
