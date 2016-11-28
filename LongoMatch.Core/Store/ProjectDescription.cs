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
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace LongoMatch.Core.Store
{
	/// <summary>
	/// Describes a project in LongoMatch.
	/// </summary>
	[Serializable]
	public class ProjectDescription : BindableBase
	{
		DateTime matchDate, lastModified;

		public ProjectDescription ()
		{
			MatchDate = LastModified = DateTime.Now;

			Category = "";
			Competition = "";
			Description = "";
			Group = "";
			Phase = "";
			Season = "";
			LocalGoals = 0;
			LocalName = "";
			VisitorGoals = 0;
			VisitorName = "";
		}

		/// <summary>
		/// Title of the project
		/// </summary>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public String Title {
			get {
				return String.Format ("{0} - {1} ({2}-{3}) {4} {5}",
					LocalName, VisitorName, LocalGoals, VisitorGoals,
					Competition, Season);
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public String DateTitle {
			get {
				string ret = String.Format ("{0}-{1} {2}", LocalName, VisitorName,
								 MatchDate.ToShortDateString ());
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
		/// Name of the dashboard in use for this project.
		/// </summary>
		public string DashboardName {
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

		public string Description {
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
				matchDate = value.ToUniversalTime ();
			}
		}

		public DateTime LastModified {
			get {
				return lastModified;
			}
			set {
				lastModified = value.ToUniversalTime ();
			}
		}

		public bool Search (string text)
		{
			StringComparison sc = StringComparison.InvariantCultureIgnoreCase;

			if (text == "")
				return true;

			if (Title != null && Title.IndexOf (text, sc) > -1)
				return true;
			else if (Season != null && Season.IndexOf (text, sc) > -1)
				return true;
			else if (Competition != null && Competition.IndexOf (text, sc) > -1)
				return true;
			else if (LocalName != null && LocalName.IndexOf (text, sc) > -1)
				return true;
			else if (VisitorName != null && VisitorName.IndexOf (text, sc) > -1)
				return true;
			else if (Description != null && Description.IndexOf (text, sc) > -1)
				return true;
			else
				return false;
		}

		static public int Sort (ProjectDescription p1, ProjectDescription p2,
								ProjectSortType sortType)
		{
			int ret = 0;

			if (p1 == null && p2 == null) {
				ret = 0;
			} else if (p1 == null) {
				ret = -1;
			} else if (p2 == null) {
				ret = 1;
			} else {
				switch (sortType) {
				case ProjectSortType.SortByName: {
						ret = String.Compare (p1.Title, p2.Title);
						if (ret == 0) {
							ret = -DateTime.Compare (p1.MatchDate, p2.MatchDate);
						}
						break;
					}
				case ProjectSortType.SortByDate: {
						ret = -DateTime.Compare (p1.MatchDate, p2.MatchDate);
						if (ret == 0) {
							ret = String.Compare (p1.Title, p2.Title);
						}
						break;
					}
				case ProjectSortType.SortByModificationDate: {
						ret = -DateTime.Compare (p1.LastModified, p2.LastModified);
						break;
					}
				case ProjectSortType.SortBySeason: {
						ret = String.Compare (p1.Season, p2.Season);
						if (ret == 0) {
							ret = -DateTime.Compare (p1.MatchDate, p2.MatchDate);
						}
						break;
					}
				case ProjectSortType.SortByCompetition: {
						ret = String.Compare (p1.Competition, p2.Competition);
						if (ret == 0) {
							ret = -DateTime.Compare (p1.MatchDate, p2.MatchDate);
						}
						break;
					}
				}
			}
			return ret;
		}
	}
}
