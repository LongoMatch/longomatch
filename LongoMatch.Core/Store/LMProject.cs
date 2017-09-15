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
using System.Collections.ObjectModel;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Migration;
using LongoMatch.Core.Store.Templates;
using Newtonsoft.Json;
using VAS.Core.Common;
using VAS.Core.Serialization;
using VAS.Core.Store;
using Constants = LongoMatch.Core.Common.Constants;

namespace LongoMatch.Core.Store
{
	/// <summary>
	/// I hold the information needed by a project and provide persistency.
	/// I'm structured in the following way:
	/// -Project Description (<see cref="LongoMatch.Utils.PreviewMediaFile"/>
	/// -1 Categories Template
	/// -1 Local Team Template
	/// -1 Visitor Team Template
	/// -1 list of <see cref="LongoMatch.Store.MediaTimeNode"/> for each category
	/// </summary>
	///
	[Serializable]
	public class LMProject : Project
	{
		SubstitutionEventType subsType;

		#region Constructors

		public LMProject ()
		{
			Dashboard = new LMDashboard ();
			LocalTeamTemplate = new LMTeam ();
			VisitorTeamTemplate = new LMTeam ();
		}

		protected override void DisposeManagedResources ()
		{
			LocalTeamTemplate?.Dispose ();
			VisitorTeamTemplate?.Dispose ();
		}
		#endregion

		#region Properties

		/// <value>
		/// Local team template
		/// </value>
		[JsonProperty (Order = -9)]
		public LMTeam LocalTeamTemplate {
			get;
			set;
		}

		/// <value>
		/// Visitor team template
		/// </value>
		[JsonProperty (Order = -8)]
		public LMTeam VisitorTeamTemplate {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<TimelineEvent> ScoreEvents {
			get {
				return Timeline.Where (e => e.EventType is ScoreEventType).ToList ();
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<TimelineEvent> PenaltyCardsEvents {
			get {
				return Timeline.Where (e => e.EventType is PenaltyCardEventType).ToList ();
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
				LineupEvent lineup = Timeline.OfType<LineupEvent> ().FirstOrDefault ();
				if (lineup == null) {
					lineup = CreateLineupEvent ();
				}
				return lineup;
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override bool IsFakeCapture {
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

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override String ShortDescription {
			get {
				return Description.DateTitle;
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override DateTime LastModified {
			get {
				return Description.LastModified;
			}
			set {
				Description.LastModified = value;
			}
		}

		#endregion

		#region Public Methods

		public void UpdateScore ()
		{
			ProjectDescription LMDescription = Description;
			LMDescription.LocalGoals = GetScore (LocalTeamTemplate);
			LMDescription.VisitorGoals = GetScore (VisitorTeamTemplate);
		}

		public void CurrentLineup (Time currentTime,
								   out List<LMPlayer> homeFieldPlayers,
								   out List<LMPlayer> homeBenchPlayers,
								   out List<LMPlayer> awayFieldPlayers,
								   out List<LMPlayer> awayBenchPlayers)
		{
			LMTeam homeTeam, awayTeam;
			List<LMPlayer> homeTeamPlayers, awayTeamPlayers;

			homeTeamPlayers = Lineup.HomeStartingPlayers.Concat (Lineup.HomeBenchPlayers).ToList ();
			awayTeamPlayers = Lineup.AwayStartingPlayers.Concat (Lineup.AwayBenchPlayers).ToList ();

			foreach (var ev in Timeline.OfType<SubstitutionEvent> ().
				Where (e => e.EventTime <= currentTime)) {
				if (ev.In != null && ev.Out != null) {
					if (ev.Teams.Contains (LocalTeamTemplate)) {
						homeTeamPlayers.Swap (ev.In, ev.Out);
					} else {
						awayTeamPlayers.Swap (ev.In, ev.Out);
					}
				}
			}

			homeTeam = new LMTeam {
				Formation = LocalTeamTemplate.Formation,
			};
			homeTeam.List.Reset (homeTeamPlayers);
			awayTeam = new LMTeam {
				Formation = VisitorTeamTemplate.Formation,
			};
			awayTeam.List.Reset (awayTeamPlayers);

			homeFieldPlayers = homeTeam.StartingPlayersList;
			homeBenchPlayers = homeTeam.BenchPlayersList;
			awayFieldPlayers = awayTeam.StartingPlayersList;
			awayBenchPlayers = awayTeam.BenchPlayersList;
		}

		public SubstitutionEvent SubsitutePlayer (LMTeam team, LMPlayer playerIn, LMPlayer playerOut,
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

		[PropertyPreload]
		public ProjectDescription Description {
			get;
			set;
		} = new ProjectDescription ();

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

		public IEnumerable<LMTimelineEvent> EventsByTeam (LMTeam team)
		{
			var timelineEventsLongomatch = new ObservableCollection<LMTimelineEvent> ();
			foreach (var timeLineEvent in Timeline) {
				timelineEventsLongomatch.Add (timeLineEvent as LMTimelineEvent);
			}
			return timelineEventsLongomatch.Where (e => e.Teams.Contains (team) || e.Players.Intersect (team.List).Any ());
		}

		public int GetScore (LMTeam team)
		{
			return EventsByTeam (team).Where (e => e.EventType is ScoreEventType).
				Sum (e => (e.EventType as ScoreEventType).Score.Points);
		}

		public override TimelineEvent CreateEvent (EventType type, Time start, Time stop, Time eventTime, Image miniature, int index = 0)
		{
			LMTimelineEvent evt;
			string count;
			string name;

			count = String.Format ("{0:000}", index);
			name = String.Format ("{0} {1}", type.Name, count);
			evt = new LMTimelineEvent ();

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

			return evt;
		}

		public override void AddEvent (TimelineEvent play)
		{
			play.FileSet = Description.FileSet;
			play.Project = this;
			Timeline.Add (play);

			if (play.EventType is ScoreEventType) {
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
		public void RemoveEvents (List<LMTimelineEvent> plays)
		{
			bool updateScore = false;

			foreach (var play in plays) {
				Timeline.Remove (play);
				if (play.EventType is ScoreEventType) {
					updateScore = true;
				}
			}
			if (updateScore) {
				UpdateScore ();
			}
		}

		public override void UpdateEventTypesAndTimers ()
		{
			IEnumerable<EventType> dashboardtypes = new List<EventType> ();
			IEnumerable<EventType> timelinetypes;

			if (Dashboard != null) {
				/* Timers */
				IEnumerable<LMTimer> timers = Dashboard.List.OfType<TimerButton> ().Select (b => b.Timer).OfType<LMTimer> ();
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
			EventTypes = new RangeObservableCollection<EventType> (EventTypes.Where (e => e != null));
		}

		public void ConsolidateDescription ()
		{
			Description.LastModified = DateTime.UtcNow;
			Description.DashboardName = Dashboard.Name;
			Description.LocalName = LocalTeamTemplate.Name;
			Description.LocalShield = LocalTeamTemplate.Shield;
			Description.LocalGoals = GetScore (LocalTeamTemplate);
			Description.VisitorName = VisitorTeamTemplate.Name;
			Description.VisitorShield = VisitorTeamTemplate.Shield;
			Description.VisitorGoals = GetScore (VisitorTeamTemplate);
		}

		public static Project Import ()
		{
			string file = App.Current.Dialogs.OpenFile (Catalog.GetString ("Import project"), null, App.Current.HomeDir,
							  Constants.PROJECT_NAME, new string [] { "*" + Constants.PROJECT_EXT });
			if (file == null)
				return null;
			return Import (file);
		}

		public static Project Import (string file)
		{
			Project project = null;
			project = Project.Import (file);
			ProjectMigration.Migrate (project as LMProject);
			return project;
		}

		// FIXME: THIS MUST BE REMOVED WHEN MOVING FROM PROJECTDESCRIPTION TO PROJECT
		/// <summary>
		/// Media file asigned to this project
		/// </summary>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override MediaFileSet FileSet {
			get {
				if (Description != null) {
					return Description.FileSet;
				}
				return null;
			}
			set {
				if (Description != null) {
					Description.FileSet = value;
				}
			}
		}

		#endregion
	}
}

