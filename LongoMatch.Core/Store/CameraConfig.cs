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

namespace LongoMatch.Core.Store
{
	/// <summary>
	/// Defines a configuration for a camera.
	/// </summary>
	[Serializable]
	public class CameraConfig
	{
		public CameraConfig (int index)
		{
			Index = index;
			RegionOfInterest = new Area (0, 0, 0, 0);
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
	}
}

