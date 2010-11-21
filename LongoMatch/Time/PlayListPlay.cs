// PlayListTimeNode.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using Gdk;
using LongoMatch.Video.Utils;

namespace LongoMatch.TimeNodes
{
	/// <summary>
	/// Represents a video segment used by <see cref="LongoMatch.Playlist.Playlist"/>.
	/// It stores the information of the video file so that it can be used outside a project.
	/// </summary>
	[Serializable]
	public class PlayListPlay : TimeNode
	{
		private PreviewMediaFile mediaFile;

		#region Constructors
		public PlayListPlay()
		{
		}
		#endregion

		#region  Properties
		/// <summary>
		/// Video file with snapshot preview
		/// </value>
		public PreviewMediaFile MediaFile {
			set;
			get;
		}

		/// <summary>
		/// Play rate
		/// </summary>
		public float Rate {
			get;
			set;
		}

		//// <value>
		/// Defines it the file exists and thus, it can be used in the playlist
		/// </value>
		public bool Valid {
			get;
			set;
		}
		#endregion
	}
}
