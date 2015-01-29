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
using Newtonsoft.Json;
using LongoMatch.Core.Store;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Common;

namespace LongoMatch.Core.Store
{

	/// <summary>
	/// Describes a project in LongoMatch.
	/// </summary>
	[Serializable]
	public class ProjectDescription :  IComparable, IIDObject
	{
		DateTime matchDate, lastModified;

		/// <summary>
		/// Unique ID of the parent project
		/// </summary>
		public Guid ID {
			get;
			set;
		}
		
		/// <summary>
		/// Title of the project
		/// </summary>
		[JsonIgnore]
		public String Title {
			get {
				return String.Format ("{0} - {1} ({2}-{3}) {4} {5}",
				                      LocalName, VisitorName, LocalGoals, VisitorGoals,
				                      Competition, Season);
			}
		}

		[JsonIgnore]
		public String DateTitle {
			get {
				string ret = String.Format ("{0}-{1} {2}", LocalName, VisitorName,
				                            MatchDate.ToShortDateString());
				if (!String.IsNullOrEmpty (Season)) {
					ret += " " + Season;
				}
				if (!String.IsNullOrEmpty (Competition)) {
					ret += " " + Competition;
				}
				return ret;
			}
		}

		/// <summary>
		/// Media file asigned to this project
		/// </summary>
		public MediaFileSet FileSet {
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
		
		public string Category {
			get;
			set;
		}

		public string Group {
			get;
			set;
		}
		
		public string Phase {
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
		/// Gets or sets the shield for the local team.
		/// </summary>
		/// <value>The local team shield.</value>
		public Image LocalShield {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the shield for the visitor team.
		/// </summary>
		/// <value>The visitor team shield.</value>
		public Image VisitorShield {
			get;
			set;
		}

		/// <summary>
		/// Date of the game
		/// </summary>
		public DateTime MatchDate {
			get {
				return matchDate;
			}
			set {
				matchDate = value.ToUniversalTime();
			}
		}
		
		public DateTime LastModified {
			get {
				return lastModified;
			}
			set {
				lastModified = value.ToUniversalTime();
			}
		}

		public int CompareTo(object obj) {
			if(obj is ProjectDescription) {
				ProjectDescription project = (ProjectDescription) obj;
				return ID.CompareTo(project.ID);
			}
			throw new ArgumentException("object is not a ProjectDescription and cannot be compared");
		}
	}
}
