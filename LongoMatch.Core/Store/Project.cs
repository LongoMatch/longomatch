// Project.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Mono.Unix;

using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.Core.Store
{

	/// <summary>
	/// I hold the information needed by a project and provide persistency using
	/// the db4o database.
	/// I'm structured in the following way:
	/// -Project Description (<see cref="LongoMatch.Utils.PreviewMediaFile"/>
	/// -1 Categories Template
	/// -1 Local Team Template
	/// -1 Visitor Team Template
	/// -1 list of <see cref="LongoMatch.Store.MediaTimeNode"/> for each category
	/// </summary>
	///
	[Serializable]
	public class Project : IComparable, IIDObject
	{
		ProjectDescription description;
		SubstitutionEventType subsType;

		#region Constructors
		public Project() {
			ID = System.Guid.NewGuid();
			Timeline = new List<TimelineEvent>();
			Dashboard = new Dashboard();
			LocalTeamTemplate = new TeamTemplate();
			VisitorTeamTemplate = new TeamTemplate();
			Timers = new List<Timer> ();
			Periods = new List<Period> ();
			Playlists = new List<Playlist> ();
			EventTypes = new List<EventType> ();
		}
		#endregion

		#region Properties

		/// <summary>
		/// Unique ID for the project
		/// </summary>
		public Guid ID {
			get;
			set;
		}
		
		public List<TimelineEvent> Timeline {
			get;
			set;
		}
		
		public ProjectDescription Description {
			get{
				return description;
			}
			set {
				if (value != null) {
					value.ID = ID;
				}
				description = value;
			}
		}

		[JsonProperty(Order = -7)]
		public List<EventType> EventTypes {
			get;
			set;
		}

		/// <value>
		/// Categories template
		/// </value>
		[JsonProperty(Order = -10)]
		public Dashboard Dashboard {
			get;
			set;
		}

		/// <value>
		/// Local team template
		/// </value>
		[JsonProperty(Order = -9)]
		public TeamTemplate LocalTeamTemplate {
			get;
			set;
		}

		/// <value>
		/// Visitor team template
		/// </value>
		[JsonProperty(Order = -8)]
		public TeamTemplate VisitorTeamTemplate {
			get;
			set;
		}
		
		public List<Period> Periods {
			get;
			set;
		}
		
		public List<Timer> Timers {
			get;
			set;
		}
		
		public List<Playlist> Playlists {
			get;
			set;
		}
		
		[JsonIgnore]
		public List<TimelineEvent> ScorePlays {
			get {
				return Timeline.OfType<ScoreEvent>().Select (t => (TimelineEvent) t).ToList();
			}
		}
		
		[JsonIgnore]
		public List<TimelineEvent> PenaltyCardsPlays {
			get {
				return Timeline.OfType<PenaltyCardEvent>().Select (t => (TimelineEvent) t).ToList();
			}
		}

		[JsonIgnore]
		public IEnumerable<IGrouping<EventType, TimelineEvent>> PlaysGroupedByEventType {
			get {
				return Timeline.GroupBy(play => play.EventType);
			}
		}
		
		[JsonIgnore]
		public SubstitutionEventType SubstitutionsEventType {
			get {
				if (subsType == null) {
					subsType = EventTypes.OfType<SubstitutionEventType> ().FirstOrDefault ();
					if (subsType == null) {
						subsType = new SubstitutionEventType ();
						subsType.SortMethod = SortMethodType.SortByStartTime;
					}
				}
				return subsType;
			}
		}

		#endregion

		#region Public Methods
		/// <summary>
		/// Frees all the project's resources helping the GC
		/// </summary>
		public void Clear() {
			Timeline.Clear();
			Dashboard.List.Clear();
			VisitorTeamTemplate.List.Clear();
			LocalTeamTemplate.List.Clear();
			Periods.Clear();
			Timers.Clear();
		}

		public void UpdateScore () {
			Description.LocalGoals = GetScore (Team.LOCAL);
			Description.VisitorGoals = GetScore (Team.VISITOR);
		}

		public TimelineEvent AddEvent (EventType type, Time start, Time stop, Time eventTime, Image miniature,
		                               Score score, PenaltyCard card, bool addToTimeline=true)
		{
			TimelineEvent evt;
			string count;
			string name;

			count = String.Format ("{0:000}", EventsByType (type).Count + 1);
			if (type is PenaltyCardEventType) {
				name = String.Format ("{0} {1}", card.Name, count);
				evt = new PenaltyCardEvent { PenaltyCard = card };
			} else if (type is ScoreEventType) {
				name = String.Format ("{0} {1}", score.Name, count);
				evt = new ScoreEvent { Score = score };
			} else {
				name = String.Format ("{0} {1}", type.Name, count);
				evt = new TimelineEvent ();
			}
			
			evt.Name = name;
			evt.Start = start;
			evt.Stop = stop;
			evt.EventTime = eventTime;
			evt.EventType = type;
			evt.Notes = "";
			evt.Miniature = miniature;

			if (addToTimeline) {
				Timeline.Add (evt);
				if (evt is ScoreEvent) {
					UpdateScore ();
				}
			}
			return evt;
		}
		
		public void AddEvent (TimelineEvent play)
		{
			Timeline.Add (play);
			if (play is ScoreEvent) {
				UpdateScore ();
			}
		}
		
		/// <summary>
		/// Delete a play from the project
		/// </summary>
		/// <param name="tNode">
		/// A <see cref="MediaTimeNode"/>: play to be deleted
		/// </param>
		/// <param name="section">
		/// A <see cref="System.Int32"/>: category the play belongs to
		/// </param>
		public void RemovePlays (List<TimelineEvent> plays)
		{
			bool updateScore = false;

			foreach (TimelineEvent play in plays) {
				Timeline.Remove (play);
				if (play is ScoreEvent) {
					updateScore = true;
				}
			}
			if (updateScore) {
				UpdateScore ();
			}
		}

		public void CleanupTimers ()
		{
			foreach (Timer t in Timers) {
				t.Nodes.RemoveAll (tn => tn.Start == null || tn.Stop == null);
			}
		}

		public void UpdateEventTypesAndTimers ()
		{
			/* Timers */
			IEnumerable<Timer> timers = Dashboard.List.OfType<TimerButton> ().Select (b => b.Timer);
			Timers.AddRange (timers.Except (Timers));
			
			/* Event Types */
			IEnumerable<EventType> types = Dashboard.List.OfType<EventButton> ().Select (b => b.EventType);
			EventTypes.AddRange (types.Except (EventTypes));
			types = Timeline.Select (t => t.EventType).Distinct ().Except (EventTypes);
			EventTypes.AddRange (types.Except (EventTypes));
			if (!EventTypes.Contains (SubstitutionsEventType)) {
				EventTypes.Add (SubstitutionsEventType);
			}
		}

		public SubstitutionEvent SubsitutePlayer (TeamTemplate template, Player playerIn, Player playerOut,
		                                          SubstitutionReason reason, Time subsTime)
		{
			Team team;
			LineupEvent lineup;
			SubstitutionEvent se;
			
			if (template == LocalTeamTemplate) {
				team = Team.LOCAL;
			} else {
				team = Team.VISITOR;
			}
			lineup = Timeline.OfType<LineupEvent> ().FirstOrDefault ();
			if (lineup == null) {
				throw new SubstitutionException (Catalog.GetString ("No lineup events found"));
			}
			if (subsTime < lineup.EventTime) {
				throw new SubstitutionException (Catalog.GetString ("A substitution can't happen before the lineup event"));
			}
			se = new SubstitutionEvent ();
			se.EventType = SubstitutionsEventType;
			se.In = playerIn;
			se.Out = playerOut;
			se.Reason = reason;
			se.EventTime = subsTime;
			se.Team = team;
			Timeline.Add (se);
			return se;
		}

		public void CurrentLineup (Time currentTime,
		                         out List<Player> homeFieldPlayers,
		                         out List<Player> homeBenchPlayers,
		                         out List<Player> awayFieldPlayers,
		                         out List<Player> awayBenchPlayers)
		{
			TeamTemplate homeTeam, awayTeam;
			LineupEvent lineup;
			List<Player> homeTeamPlayers, awayTeamPlayers;

			lineup = Timeline.OfType <LineupEvent> ().FirstOrDefault ();
			if (lineup == null) {
				lineup = CreateLineupEvent ();
			}

			homeTeamPlayers = lineup.HomeStartingPlayers.Concat (lineup.HomeBenchPlayers).ToList ();
			awayTeamPlayers = lineup.AwayStartingPlayers.Concat (lineup.AwayBenchPlayers).ToList ();

			foreach (SubstitutionEvent ev in Timeline.OfType<SubstitutionEvent> ().
			         Where (e => e.EventTime <= currentTime)) {
				if (ev.In != null && ev.Out != null) {
					if (ev.Team == Team.LOCAL) {
						homeTeamPlayers.Swap (ev.In, ev.Out);
					} else {
						awayTeamPlayers.Swap (ev.In, ev.Out);
					}
				}
			}

			homeTeam = new TeamTemplate {
				Formation = LocalTeamTemplate.Formation,
				List = homeTeamPlayers
			};
			awayTeam = new TeamTemplate {
				Formation = VisitorTeamTemplate.Formation,
				List = awayTeamPlayers
			};
			
			homeFieldPlayers = homeTeam.StartingPlayersList;
			homeBenchPlayers = homeTeam.BenchPlayersList;
			awayFieldPlayers = awayTeam.StartingPlayersList;
			awayBenchPlayers = awayTeam.BenchPlayersList;
		}

		public bool LineupChanged (Time start, Time stop)
		{
			return Timeline.OfType<SubstitutionEvent> ().
				Count (s => s.EventTime > start && s.EventTime <= stop) > 0;
		}
		
		public LineupEvent CreateLineupEvent ()
		{
			Time startTime;
			LineupEvent lineup;

			if (Periods.Count == 0) {
				startTime = new Time (0);
			} else {
				startTime = Periods[0].PeriodNode.Start;
			}

			lineup = new LineupEvent {
				Name = Catalog.GetString ("Lineup ") + LocalTeamTemplate.TeamName,
				EventType = SubstitutionsEventType,
				HomeStartingPlayers = LocalTeamTemplate.StartingPlayersList,
				HomeBenchPlayers = LocalTeamTemplate.BenchPlayersList,
				AwayStartingPlayers = VisitorTeamTemplate.StartingPlayersList,
				AwayBenchPlayers = VisitorTeamTemplate.BenchPlayersList, 
				EventTime = startTime};
			Timeline.Add (lineup);

			return lineup;
		}

		public List<TimelineEvent> EventsByType (EventType evType) {
			return Timeline.Where(p => p.EventType.ID == evType.ID).ToList();
		}

		public int GetScore (Team team) {
			return Timeline.OfType<ScoreEvent>().Where (s => PlayTaggedTeam (s) == team).Sum(s => s.Score.Points); 
		}
		
		public Team PlayTaggedTeam (TimelineEvent play) {
			bool home=false, away=false;
			
			if (play.Team == Team.LOCAL || play.Team == Team.BOTH ||
			    play.Players.Count (p => LocalTeamTemplate.List.Contains (p)) > 0) {
				home = true;
			}
			if (play.Team == Team.VISITOR || play.Team == Team.BOTH ||
			    play.Players.Count (p => VisitorTeamTemplate.List.Contains (p)) > 0) {
				away = true;
			}
			
			if (away && home) {
				return Team.BOTH;
			} else if (home) {
				return Team.LOCAL;
			} else if (away) {
				return Team.VISITOR;
			} else {
				return Team.NONE;
			}
		}
		
		public Image GetBackground (FieldPositionType pos) {
			switch (pos) {
			case FieldPositionType.Field:
				return Dashboard.FieldBackground;
			case FieldPositionType.HalfField:
				return Dashboard.HalfFieldBackground;
			case FieldPositionType.Goal:
				return Dashboard.GoalBackground;
			}
			return null;
		}

		public bool Equals(Project project) {
			if(project == null)
				return false;
			else
				return ID == project.ID;
		}

		public int CompareTo(object obj) {
			if(obj is Project) {
				Project project = (Project) obj;
				return ID.CompareTo(project.ID);
			}
			else
				throw new ArgumentException("object is not a Project and cannot be compared");
		}

		public static void Export(Project project, string file) {
			file = Path.ChangeExtension(file, Constants.PROJECT_EXT);
			Serializer.Save(project, file);
		}

		public static Project Import(string file) {
			try {
				return Serializer.Load<Project>(file);
			}
			catch  (Exception e){
				Log.Exception (e);
				throw new Exception(Catalog.GetString("The file you are trying to load " +
				                                      "is not a valid project"));
			}
		}
		#endregion
	}
}
