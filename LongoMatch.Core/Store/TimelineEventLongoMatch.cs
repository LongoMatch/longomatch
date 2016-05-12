//
//  Copyright (C) 2016 dfernandez
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
using System.Collections.ObjectModel;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store.Templates;
using Newtonsoft.Json;
using VAS.Core.Serialization;
using VAS.Core.Store;

namespace LongoMatch.Core.Store
{
	// TODO: Rename to TeamSportTimelineEvent

	[Serializable]
	public class TimelineEventLongoMatch : TimelineEvent
	{
		ObservableCollection<PlayerLongoMatch> players;
		ObservableCollection<Team> teams;

		public TimelineEventLongoMatch ()
		{
			Players = new ObservableCollection<PlayerLongoMatch> ();
			Teams = new ObservableCollection<Team> ();
		}

		/// <summary>
		/// List of players tagged in this event.
		/// </summary>
		[PropertyIndex (0)]
		public ObservableCollection<PlayerLongoMatch> Players {
			get {
				return players;
			}
			set {
				if (players != null) {
					players.CollectionChanged -= ListChanged;
				}
				players = value;
				if (players != null) {
					players.CollectionChanged += ListChanged;
				}
			}
		}

		[PropertyChanged.DoNotNotify]
		[Obsolete ("Use Teams instead of Team to tag a team in a TimelineEvent")]
		public TeamType Team {
			get;
			set;
		}

		/// <summary>
		/// A list of teams tagged in this event.
		/// </summary>
		[PropertyIndex (3)]
		public ObservableCollection<Team> Teams {
			get {
				return teams;
			}
			set {
				if (teams != null) {
					teams.CollectionChanged -= ListChanged;
				}
				teams = value ?? new ObservableCollection<Team> ();
				if (teams != null) {
					teams.CollectionChanged += ListChanged;
				}
			}
		}

		[JsonIgnore]
		public List<Team> TaggedTeams {
			get {
				if (Project == null) {
					return Teams.ToList ();
				}
				ProjectLongoMatch LMProject = Project as ProjectLongoMatch;
				List<Team> teams = new List<Team> ();
				if (Teams.Contains (LMProject.LocalTeamTemplate) ||
				    Players.Intersect (LMProject.LocalTeamTemplate.List).Any ()) {
					teams.Add (LMProject.LocalTeamTemplate);
				}
				if (Teams.Contains (LMProject.VisitorTeamTemplate) ||
				    Players.Intersect (LMProject.VisitorTeamTemplate.List).Any ()) {
					teams.Add (LMProject.VisitorTeamTemplate);
				}
				return teams;
			}
		}
	}

	/// <summary>
	/// An event in the game for a penalty.
	/// </summary>
	[Serializable]
	[Obsolete ("Create a TimelineEvent with a PenaltyCardEventType")]
	public class PenaltyCardEvent: TimelineEvent
	{
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public PenaltyCardEventType PenaltyCardEventType {
			get {
				return EventType as PenaltyCardEventType;
			}
		}

		public PenaltyCard PenaltyCard {
			get;
			set;
		}
	}

	[Serializable]
	[Obsolete ("Create a TimelineEvent with a ScoreEventType")]
	public class ScoreEvent: TimelineEvent
	{
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public ScoreEventType ScoreEventType {
			get {
				return EventType as ScoreEventType;
			}
		}

		public Score Score {
			get;
			set;
		}
	}

	[Serializable]
	public class StatEvent: TimelineEventLongoMatch
	{
		public override Time Start {
			get {
				return EventTime;
			}
			set {
				EventTime = value;
			}
		}

		public override Time Stop {
			get {
				return EventTime;
			}
			set {
				EventTime = value;
			}
		}
	}

	[Serializable]
	public class LineupEvent: StatEvent
	{
		public List<PlayerLongoMatch> HomeStartingPlayers {
			get;
			set;
		}

		public List<PlayerLongoMatch> HomeBenchPlayers {
			get;
			set;
		}

		public List<PlayerLongoMatch> AwayStartingPlayers {
			get;
			set;
		}

		public List<PlayerLongoMatch> AwayBenchPlayers {
			get;
			set;
		}
	}

	[Serializable]
	public class SubstitutionEvent: StatEvent
	{
		public PlayerLongoMatch In {
			get;
			set;
		}

		public PlayerLongoMatch Out {
			get;
			set;
		}

		public override string Name {
			get {
				return Reason.ToString ();
			}
			set {
			}
		}

		public SubstitutionReason Reason {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override string Description {
			get {
				string desc = "";
				if (In != null && Out != null) {
					desc = String.Format ("{0} ⟲ {1}", In, Out);
				} else if (In != null) {
					desc = "↷ " + In;
				} else if (Out != null) {
					desc = "↶ " + Out;
				}
				return desc += "\n" + EventTime.ToMSecondsString ();
			}
		}
	}
}

