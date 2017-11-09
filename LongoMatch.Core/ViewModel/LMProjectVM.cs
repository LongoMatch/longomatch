//
//  Copyright (C) 2016 Fluendo S.A.
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
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace LongoMatch.Core.ViewModel
{
	/// <summary>
	/// ViewModel for sports projects.
	/// </summary>
	public class LMProjectVM : ProjectVM<LMProject>
	{
		string description;
		DateTime matchDate;
		string season;
		string competition;

		public LMProjectVM ()
		{
			HomeTeam = new LMTeamVM ();
			AwayTeam = new LMTeamVM ();
			Timeline = new LMTimelineVM (HomeTeam, AwayTeam);
			Dashboard = new LMDashboardVM ();
			ShowMenu = new Command (() => IsMenuVisible = !IsMenuVisible);
			Visible = true;
		}

		/// <summary>
		/// Gets the description of the project
		/// </summary>
		/// <value>The description.</value>
		public string Description {
			get {
				if (Stateful && description != null) {
					return description;
				} else {
					return Model.Description.Description;
				}
			}
			set {
				if (Stateful) {
					description = value;
				} else {
					Model.Description.Description = value;
				}
			}
		}

		/// <summary>
		/// Gets the name of the home team.
		/// </summary>
		/// <value>The home team name.</value>
		public string HomeTeamText {
			get {
				return Model.Description.LocalName;
			}
		}

		/// <summary>
		/// Gets the name of the away team.
		/// </summary>
		/// <value>The away team name.</value>
		public string AwayTeamText {
			get {
				return Model.Description.VisitorName;
			}
		}

		/// <summary>
		/// Gets the home team shield.
		/// </summary>
		/// <value>The home team shield.</value>
		public Image HomeTeamShield {
			get {
				return Model.Description.LocalShield;
			}
		}

		/// <summary>
		/// Gets the away team shield.
		/// </summary>
		/// <value>The away team shield.</value>
		public Image AwayTeamShield {
			get {
				return Model.Description.VisitorShield;
			}
		}

		/// <summary>
		/// Gets the local team score.
		/// </summary>
		/// <value>The local score.</value>
		public string LocalScore {
			get {
				return Model.Description.LocalGoals.ToString ();
			}
		}

		/// <summary>
		/// Gets the away team score.
		/// </summary>
		/// <value>The away score.</value>
		public string AwayScore {
			get {
				return Model.Description.VisitorGoals.ToString ();
			}
		}

		/// <summary>
		/// Gets the name of the dashboard.
		/// </summary>
		/// <value>The dashboard text.</value>
		public string DashboardText {
			get {
				return Model.Description.DashboardName;
			}
		}

		/// <summary>
		/// Gets the match date.
		/// </summary>
		/// <value>The match date.</value>
		public DateTime MatchDate {
			get {
				if (Stateful && matchDate != default (DateTime)) {
					return matchDate;
				} else {
					return Model.Description.MatchDate;
				}
			}
			set {
				if (Stateful) {
					matchDate = value;
				} else {
					Model.Description.MatchDate = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the match season.
		/// </summary>
		/// <value>The season.</value>
		public string Season {
			get {
				if (Stateful && season != null) {
					return season;
				} else {
					return Model.Description.Season;
				}
			}
			set {
				if (Stateful) {
					season = value;
				} else {
					Model.Description.Season = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the match competition.
		/// </summary>
		/// <value>The competition.</value>
		public string Competition {
			get {
				if (Stateful && competition != null) {
					return competition;
				} else {
					return Model.Description.Competition;
				}
			}
			set {
				if (Stateful) {
					competition = value;
				} else {
					Model.Description.Competition = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:LongoMatch.Core.ViewModel.LMProjectVM"/> is menu visible.
		/// </summary>
		/// <value><c>true</c> if is card menu visible; otherwise, <c>false</c>.</value>
		public bool IsMenuVisible { get; set; }

		/// <summary>
		/// Gets the display options command.
		/// </summary>
		/// <value>The display options.</value>
		public Command ShowMenu { get; private set; }

		/// <summary>
		/// Gets the preview of the first file in set or null if the set is empty.
		/// </summary>
		/// <value>The preview.</value>
		public Image Preview {
			get {
				return Model.Description.FileSet?.Preview;
			}
		}

		/// <summary>
		/// Gets the max duration from all files in MediaFileSet
		/// </summary>
		/// <value>The duration</value>
		public Time Duration {
			get {
				return Model.Description.FileSet?.Duration;
			}
		}

		/// <summary>
		/// Gets the title.
		/// </summary>
		/// <value>The title.</value>
		public string Title {
			get {
				return Competition;
			}
		}

		/// <summary>
		/// Gets the bottom team.
		/// </summary>
		/// <value>The bottom team.</value>
		public LMTeamVM HomeTeam {
			get;
			protected set;
		}

		/// <summary>
		/// Gets the top team.
		/// </summary>
		/// <value>The top team.</value>
		public LMTeamVM AwayTeam {
			get;
			protected set;
		}

		public override IEnumerable<TeamVM> Teams {
			get {
				yield return HomeTeam;
				yield return AwayTeam;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether close  has been already handled or if the step want to be avoided
		/// </summary>
		/// <value><c>true</c> if close has been handled; otherwise, <c>false</c>.</value>
		public bool CloseHandled { get; set; }

		public override void CommitState ()
		{
			Model.Description.Description = description;
			description = null;
			Model.Description.Season = season;
			season = null;
			Model.Description.Competition = competition;
			competition = null;
			Model.Description.MatchDate = matchDate;
			matchDate = default (DateTime);
		}

		protected override void SyncLoadedModel ()
		{
			HomeTeam.Model = Model.LocalTeamTemplate;
			AwayTeam.Model = Model.VisitorTeamTemplate;
			//Set StartingPlayers
			base.SyncLoadedModel ();
		}
	}
}

