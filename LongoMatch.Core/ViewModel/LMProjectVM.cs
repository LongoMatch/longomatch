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
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.ViewModel;

namespace LongoMatch.Core.ViewModel
{
	/// <summary>
	/// ViewModel for sports projects.
	/// </summary>
	public class LMProjectVM : ProjectVM<LMProject>
	{
		public LMProjectVM ()
		{
			HomeTeam = new LMTeamVM ();
			AwayTeam = new LMTeamVM ();
			Timeline = new LMTimelineVM (HomeTeam, AwayTeam);
		}

		/// <summary>
		/// Gets the description of the project
		/// </summary>
		/// <value>The description.</value>
		public string Description {
			get {
				return Model.Description.Description;
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
				return Model.Description.MatchDate;
			}
		}

		/// <summary>
		/// Gets or sets the match season.
		/// </summary>
		/// <value>The season.</value>
		public string Season {
			get {
				return Model.Description.Season;
			}
			set {
				Model.Description.Season = value;
			}
		}

		/// <summary>
		/// Gets or sets the match competition.
		/// </summary>
		/// <value>The competition.</value>
		public string Competition {
			get {
				return Model.Description.Competition;
			}
			set {
				Model.Description.Competition = value;
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

		protected override void SyncLoadedModel ()
		{
			base.SyncLoadedModel ();
			HomeTeam.Model = Model.LocalTeamTemplate;
			AwayTeam.Model = Model.VisitorTeamTemplate;
			(Timeline as LMTimelineVM).UpdatePredicates ();
		}
	}
}

