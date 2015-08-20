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
using LongoMatch.Core.Common;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{
	/// <summary>
	/// Defines a configuration for a camera.
	/// </summary>
	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	public class CameraConfig
	{
		public CameraConfig (int index)
		{
			Index = index;
			RegionOfInterest = new Area (0, 0, 0, 0);
		}

		[JsonIgnore]
		public bool IsChanged {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the index of this camera with regards to the MediaFileSet.
		/// </summary>
		/// <value>The index of the camera.</value>
		public int Index {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the region of interest for this camera.
		/// </summary>
		/// <value>The region of interest.</value>
		public Area RegionOfInterest {
			get;
			set;
		}

		public override bool Equals (object obj)
		{
			CameraConfig config = obj as CameraConfig;
			if (config == null)
				return false;
			if (config.Index != Index ||
			    config.RegionOfInterest != RegionOfInterest) {
				return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public static bool operator == (CameraConfig c1, CameraConfig c2)
		{
			if (Object.ReferenceEquals (c1, c2)) {
				return true;
			}

			if ((object)c1 == null || (object)c2 == null) {
				return false;
			}

			return c1.Equals (c2);
		}

		public static bool operator != (CameraConfig c1, CameraConfig c2)
		{
			return !(c1 == c2);
		}

		public override string ToString ()
		{
			return string.Format ("{0}, {1}", Index, RegionOfInterest);
		}
	}
}

