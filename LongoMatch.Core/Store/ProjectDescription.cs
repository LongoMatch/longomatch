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
using LongoMatch.Store;

namespace LongoMatch.Store
{

	/// <summary>
	/// Describes a project in LongoMatch.
	/// </summary>
	[Serializable]
	public class ProjectDescription :  IComparable
	{
		/// <summary>
		/// Unique ID of the parent project
		/// </summary>
		public Guid UUID {
			get;
			set;
		}
		
		/// <summary>
		/// Title of the project
		/// </summary>
		public String Title {
			get {
				return System.IO.Path.GetFileNameWithoutExtension(File.FilePath);
			}
		}

		/// <summary>
		/// Media file asigned to this project
		/// </summary>
		public MediaFile File {
			get;
			set;
		}

		/// <summary>
		/// Season of the game
		/// </summary>
		public String Season {
			get;
			set;
		}

		/// <summary>
		/// Comptetition of the game
		/// </summary>
		public String Competition {
			get;
			set;
		}

		/// <summary>
		/// Name of the local team
		/// </summary>
		public String LocalName {
			get;
			set;
		}


		/// <summary>
		/// Name of the visitor team
		/// </summary>
		public String VisitorName {
			get;
			set;
		}

		/// <summary>
		/// Goals of the local team
		/// </summary>
		public int LocalGoals {
			get;
			set;
		}

		/// <summary>
		/// Goals of the visitor team
		/// </summary>
		public int VisitorGoals {
			get;
			set;
		}

		/// <summary>
		/// Date of the game
		/// </summary>
		public DateTime MatchDate {
			get;
			set;
		}

		/// <summary>
		/// String representing the video format like "widhtxheight@fps"
		/// </summary>
		public String Format {
			get {
				return String.Format("{0}x{1}@{2}fps",
				                     File.VideoWidth, File.VideoHeight, File.Fps);
			}
		}
		
		public DateTime LastModified {
			get;
			set;
		}

		public int CompareTo(object obj) {
			if(obj is ProjectDescription) {
				ProjectDescription project = (ProjectDescription) obj;

				return this.File.FilePath.CompareTo(project.File.FilePath);
			}
			else
				throw new ArgumentException("object is not a ProjectDescription and cannot be compared");
		}
	}
}
