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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using LongoMatch.Core.Common;
using LongoMatch.Core.Serialization;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Core.Store.Templates;
using Newtonsoft.Json;

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
	public class Project : StorableBase, IComparable
	{
		ObservableCollection<TimelineEvent> timeline;
		ObservableCollection<Period> periods;
		ObservableCollection<Timer> timers;
		ObservableCollection<Playlist> playlists;
		ObservableCollection<EventType> eventTypes;
		SubstitutionEventType subsType;

		#region Constructors

		public Project ()
		{
			ID = System.Guid.NewGuid ();
			Timeline = new ObservableCollection<TimelineEvent> ();
			Dashboard = new Dashboard ();
			LocalTeamTemplate = new Team ();
			VisitorTeamTemplate = new Team ();
			Timers = new ObservableCollection<Timer> ();
			Periods = new ObservableCollection<Period> ();
			Playlists = new ObservableCollection<Playlist> ();
			EventTypes = new ObservableCollection<EventType> ();
		}

		[OnDeserialized ()]
		internal void OnDeserializedMethod (StreamingContext context)
		{
			foreach (TimelineEvent evt in Timeline) {
				evt.Project = this;
				// FIXME: remove this after the migration tool is ready
				evt.FileSet = Description.FileSet;
			}
		}

		#endregion

		#region Properties

		public ObservableCollection<TimelineEvent> Timeline {
			get {
				return timeline;
			}
			set {
				if (timeline != null) {
					timeline.CollectionChanged -= ListChanged;
				}
				timeline = value;
				if (timeline != null) {
					timeline.CollectionChanged += ListChanged;
				}
			}
		}

		[LongoMatchPropertyPreload]
		public ProjectDescription Description {
			get;
			set;
		}

		[JsonProperty (Order = -7)]
		public ObservableCollection<EventType> EventTypes {
			get {
				return eventTypes;
			}
			set {
				if (eventTypes != null) {
					eventTypes.CollectionChanged -= ListChanged;
				}
				eventTypes = value;
				if (eventTypes != null) {
					eventTypes.CollectionChanged += ListChanged;
				}
			}
		}

		/// <value>
		/// Categories template
		/// </value>
		[JsonProperty (Order = -10)]
		public Dashboard Dashboard {
			get;
			set;
		}

		/// <value>
		/// Local team template
		/// </value>
		[JsonProperty (Order = -9)]
		public Team LocalTeamTemplate {
			get;
			set;
		}

		/// <value>
		/// Visitor team template
		/// </value>
		[JsonProperty (Order = -8)]
		public Team VisitorTeamTemplate {
			get;
			set;
		}

		public ObservableCollection<Period> Periods {
			get {
				return periods;
			}
			set {
				if (periods != null) {
					periods.CollectionChanged -= ListChanged;
				}
				periods = value;
				if (periods != null) {
					periods.CollectionChanged += ListChanged;
				}
			}
		}

		public ObservableCollection<Timer> Timers {
			get {
				return timers;
			}
			set {
				if (timers != null) {
					timers.CollectionChanged -= ListChanged;
				}
				timers = value;
				if (timers != null) {
					timers.CollectionChanged += ListChanged;
				}
			}
		}

		public ObservableCollection<Playlist> Playlists {
			get {
				return playlists;
			}
			set {
				if (playlists != null) {
					playlists.CollectionChanged -= ListChanged;
				}
				playlists = value;
				if (playlists != null) {
					playlists.CollectionChanged += ListChanged;
				}
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<Score> Scores {
			get {
				var scores = Dashboard.List.OfType<ScoreButton> ().Select (b => b.Score);
				return ScoreEvents.Select (e => e.Score).Union (scores).OrderByDescending (s => s.Points).ToList ();
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<PenaltyCard> PenaltyCards {
			get {
				var pc = Dashboard.List.OfType<PenaltyCardButton> ().Select (b => b.PenaltyCard);
				return PenaltyCardsEvents.Select (e => e.PenaltyCard).Union (pc).ToList ();
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<ScoreEvent> ScoreEvents {
			get {
				return Timeline.OfType<ScoreEvent> ().Select (t => t).ToList ();
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<PenaltyCardEvent> PenaltyCardsEvents {
			get {
				return Timeline.OfType<PenaltyCardEvent> ().Select (t => t).ToList ();
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public IEnumerable<IGrouping<EventType, TimelineEvent>> EventsGroupedByEventType {
			get {
				return Timeline.GroupBy (play => play.EventType);
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
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

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public LineupEvent Lineup {
			get {
				LineupEvent lineup = Timeline.OfType <LineupEvent> ().FirstOrDefault ();
				if (lineup == null) {
					lineup = CreateLineupEvent ();
				}
				return lineup;
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public ProjectType ProjectType {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsFakeCapture {
			get {
				if (Description != null) {
					MediaFileSet fileSet = Description.FileSet;
					if (fileSet != null) {
						MediaFile file = fileSet.FirstOrDefault ();
						if (file != null)
							return file.IsFakeCapture;
					}
				}
				return false;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Frees all the project's resources helping the GC
		/// </summary>
		public void Clear ()
		{
			Timeline.Clear ();
			Dashboard.List.Clear ();
			VisitorTeamTemplate.List.Clear ();
			LocalTeamTemplate.List.Clear ();
			Periods.Clear ();
			Timers.Clear ();
		}

		public void UpdateScore ()
		{
			Description.LocalGoals = GetScore (LocalTeamTemplate);
			Description.VisitorGoals = GetScore (VisitorTeamTemplate);
		}

		public TimelineEvent AddEvent (EventType type, Time start, Time stop, Time eventTime, Image miniature,
		                               Score score, PenaltyCard card, bool addToTimeline = true)
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
			evt.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (0) };
			evt.FileSet = Description.FileSet;
			evt.Project = this;

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
			play.FileSet = Description.FileSet;
			play.Project = this;
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
		public void RemoveEvents (List<TimelineEvent> plays)
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
			IEnumerable<EventType> dashboardtypes = new List<EventType> ();
			IEnumerable<EventType> timelinetypes;

			if (Dashboard != null) {
				/* Timers */
				IEnumerable<Timer> timers = Dashboard.List.OfType<TimerButton> ().Select (b => b.Timer);
				Timers.AddRange (timers.Except (Timers));
			
				/* Update event types list that changes when the user adds or remove a
				 * a new button to the dashboard or after importing a project with events
				 * tagged with a different dashboard */
				dashboardtypes = Dashboard.List.OfType<EventButton> ().Select (b => b.EventType);
			}

			/* Remove event types that have no events and are not in the dashboard anymore */
			foreach (EventType evt in EventTypes.Except (dashboardtypes).ToList ()) {
				if (evt == SubstitutionsEventType) {
					continue;
				}
				if (Timeline.Count (e => e.EventType == evt) == 0) {
					EventTypes.Remove (evt);
				}
			}
			EventTypes.AddRange (dashboardtypes.Except (EventTypes));
			timelinetypes = Timeline.Select (t => t.EventType).Distinct ().Except (EventTypes);
			EventTypes.AddRange (timelinetypes.Except (EventTypes));
			if (!EventTypes.Contains (SubstitutionsEventType)) {
				EventTypes.Add (SubstitutionsEventType);
			}

			/* Remove null EventTypes just in case */
			EventTypes = new ObservableCollection<EventType> (EventTypes.Where (e => e != null));
		}

		public SubstitutionEvent SubsitutePlayer (Team team, Player playerIn, Player playerOut,
		                                          SubstitutionReason reason, Time subsTime)
		{
			LineupEvent lineup;
			SubstitutionEvent se;
			
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
			se.Teams.Add (team);
			Timeline.Add (se);
			return se;
		}

		public void CurrentLineup (Time currentTime,
		                           out List<Player> homeFieldPlayers,
		                           out List<Player> homeBenchPlayers,
		                           out List<Player> awayFieldPlayers,
		                           out List<Player> awayBenchPlayers)
		{
			Team homeTeam, awayTeam;
			List<Player> homeTeamPlayers, awayTeamPlayers;

			homeTeamPlayers = Lineup.HomeStartingPlayers.Concat (Lineup.HomeBenchPlayers).ToList ();
			awayTeamPlayers = Lineup.AwayStartingPlayers.Concat (Lineup.AwayBenchPlayers).ToList ();

			foreach (SubstitutionEvent ev in Timeline.OfType<SubstitutionEvent> ().
			         Where (e => e.EventTime <= currentTime)) {
				if (ev.In != null && ev.Out != null) {
					if (ev.Teams.Contains (LocalTeamTemplate)) {
						homeTeamPlayers.Swap (ev.In, ev.Out);
					} else {
						awayTeamPlayers.Swap (ev.In, ev.Out);
					}
				}
			}

			homeTeam = new Team {
				Formation = LocalTeamTemplate.Formation,
				List = new ObservableCollection<Player> (homeTeamPlayers)
			};
			awayTeam = new Team {
				Formation = VisitorTeamTemplate.Formation,
				List = new ObservableCollection<Player> (awayTeamPlayers)
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
				startTime = Periods [0].PeriodNode.Start;
			}

			lineup = new LineupEvent {
				Name = Catalog.GetString ("Lineup"),
				EventType = SubstitutionsEventType,
				HomeStartingPlayers = LocalTeamTemplate.StartingPlayersList,
				HomeBenchPlayers = LocalTeamTemplate.BenchPlayersList,
				AwayStartingPlayers = VisitorTeamTemplate.StartingPlayersList,
				AwayBenchPlayers = VisitorTeamTemplate.BenchPlayersList, 
				EventTime = startTime
			};
			AddEvent (lineup);

			return lineup;
		}

		public List<TimelineEvent> EventsByType (EventType evType)
		{
			return Timeline.Where (p => p.EventType.ID == evType.ID).ToList ();
		}

		public IEnumerable<TimelineEvent> EventsByTeam (Team team)
		{
			return Timeline.Where (e => e.Teams.Contains (team) || e.Players.Intersect (team.List).Any ());
		}

		public int GetScore (Team team)
		{
			return EventsByTeam (team).OfType<ScoreEvent> ().Sum (s => s.Score.Points);
		}

		public Image GetBackground (FieldPositionType pos)
		{
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

		public void ConsolidateDescription ()
		{
			Description.LastModified = DateTime.UtcNow;
			Description.LocalName = LocalTeamTemplate.Name;
			Description.LocalShield = LocalTeamTemplate.Shield;
			Description.LocalGoals = GetScore (LocalTeamTemplate);
			Description.VisitorName = VisitorTeamTemplate.Name;
			Description.VisitorShield = VisitorTeamTemplate.Shield;
			Description.VisitorGoals = GetScore (VisitorTeamTemplate);
			Description.DashboardName = Dashboard.Name;
		}


		/// <summary>
		/// Resynchronize events with the periods synced with the video file.
		/// Imported projects or fake analysis projects create events assuming periods
		/// don't have gaps between them.
		/// After adding a file to the project and synchronizing the periods with the
		/// video file, all events must be offseted with the new start time of the period.
		/// 
		/// Before sync:
		///   Period 1: start=00:00:00 Period 2: start=00:30:00
		///   evt1 00:10:00            evt2 00:32:00
		/// After sync:
		///   Period 1: start=00:05:00 Period 2: start= 00:39:00
		///   evt1 00:15:00            evt2 00:41:00
		/// </summary>
		/// <param name="periods">The new periods syncrhonized with the video file.</param>
		public void ResyncEvents (IList<Period> periods)
		{
			ObservableCollection<TimelineEvent> newTimeline = new ObservableCollection<TimelineEvent> ();

			if (periods.Count != Periods.Count) {
				throw new IndexOutOfRangeException (
					"Periods count is different from the project's ones");
			}

			for (int i = 0; i < periods.Count; i++) {
				Period oldPeriod = Periods [i];
				TimeNode oldTN = oldPeriod.PeriodNode;
				TimeNode newTN = periods [i].PeriodNode;
				Time diff = newTN.Start - oldTN.Start;

				/* Find the events in this period */
				var periodEvents = Timeline.Where (e =>
					e.EventTime >= oldTN.Start &&
				                   e.EventTime <= oldTN.Stop).ToList ();

				/* Apply new offset and move the new timeline so that the next
				 * iteration for the following period does not use them anymore */
				periodEvents.ForEach (e => {
					e.Move (diff);
					newTimeline.Add (e);
					Timeline.Remove (e);
				});
				foreach (TimeNode tn in oldPeriod.Nodes) {
					tn.Move (diff);
				}
			}
			Timeline = newTimeline;
		}

		public int CompareTo (object obj)
		{
			if (obj is Project) {
				Project project = (Project)obj;
				return ID.CompareTo (project.ID);
			} else
				throw new ArgumentException ("object is not a Project and cannot be compared");
		}

		public static void Export (Project project, string file)
		{
			file = Path.ChangeExtension (file, Constants.PROJECT_EXT);
			Serializer.Instance.Save (project, file);
		}

		public static Project Import ()
		{
			string file = Config.GUIToolkit.OpenFile (Catalog.GetString ("Import project"), null, Config.HomeDir, Constants.PROJECT_NAME,
				              new string[] { "*" + Constants.PROJECT_EXT });
			if (file == null)
				return null;
			return Project.Import (file);
		}

		public static Project Import (string file)
		{
			Project project = null;
			try {
				project = Serializer.Instance.Load<Project> (file);
			} catch (Exception e) {
				Log.Exception (e);
				throw new Exception (Catalog.GetString ("The file you are trying to load " +
				"is not a valid project"));
			}
			ConvertTeams (project);
			return project;
		}

		#pragma warning disable 0618
		internal static void ConvertTeams (Project project)
		{
			// Convert old Team tags to Teams
			foreach (TimelineEvent evt in project.Timeline.Where (e => e.Team != TeamType.NONE)) {
				if (evt.Team == TeamType.LOCAL || evt.Team == TeamType.BOTH) {
					evt.Teams.Add (project.LocalTeamTemplate);
				}
				if (evt.Team == TeamType.VISITOR || evt.Team == TeamType.BOTH) {
					evt.Teams.Add (project.VisitorTeamTemplate);
				}
			}
		}

		#pragma warning restore 0618

		void ListChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		#endregion
	}
}
