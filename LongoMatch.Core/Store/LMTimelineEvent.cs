//
//  Copyright (C) 2016 FLUENDO S.A.
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
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store.Templates;
using Newtonsoft.Json;
using VAS.Core.Store;

namespace LongoMatch.Core.Store
{
	[Serializable]
	public class LMTimelineEvent : TimelineEvent
	{

		[PropertyChanged.DoNotNotify]
		[Obsolete ("Use Teams instead of Team to tag a team in a TimelineEvent")]
		public TeamType Team {
			get;
			set;
		}

		[JsonIgnore]
		public List<LMTeam> TaggedTeams {
			get {
				if (Project == null) {
					return Teams.Cast<LMTeam> ().ToList ();
				}
				LMProject project = Project as LMProject;
				List<LMTeam> teams = new List<LMTeam> ();
				if (Teams.Contains (project.LocalTeamTemplate) ||
					Players.Intersect (project.LocalTeamTemplate.List).Any ()) {
					teams.Add (project.LocalTeamTemplate);
				}
				if (Teams.Contains (project.VisitorTeamTemplate) ||
					Players.Intersect (project.VisitorTeamTemplate.List).Any ()) {
					teams.Add (project.VisitorTeamTemplate);
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
	public class PenaltyCardEvent : LMTimelineEvent
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
	public class ScoreEvent : LMTimelineEvent
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
	public class StatEvent : LMTimelineEvent
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
	public class LineupEvent : StatEvent
	{
		public List<LMPlayer> HomeStartingPlayers {
			get;
			set;
		}

		public List<LMPlayer> HomeBenchPlayers {
			get;
			set;
		}

		public List<LMPlayer> AwayStartingPlayers {
			get;
			set;
		}

		public List<LMPlayer> AwayBenchPlayers {
			get;
			set;
		}
	}

	[Serializable]
	public class SubstitutionEvent : StatEvent
	{
		public LMPlayer In {
			get;
			set;
		}

		public LMPlayer Out {
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

