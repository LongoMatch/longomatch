// MediaTimeNode.cs
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
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Serialization;
using LongoMatch.Core.Store.Templates;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{
	/// <summary>
	/// Represents a tagged event in the game at a specific position in the timeline.
	/// </summary>

	[Serializable]
	public class TimelineEvent : PixbufTimeNode, IStorable
	{
		ObservableCollection<FrameDrawing> drawings;
		ObservableCollection<Player> players;
		ObservableCollection<Tag> tags;
		ObservableCollection<CameraConfig> camerasConfig;
		ObservableCollection<Team> teams;

		#region Constructors

		public TimelineEvent ()
		{
			IsLoaded = true;
			Drawings = new ObservableCollection<FrameDrawing> ();
			Players = new ObservableCollection<Player> ();
			Tags = new ObservableCollection<Tag> ();
			Teams = new ObservableCollection<Team> ();
			Rate = 1.0f;
			ID = Guid.NewGuid ();
			CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (0) };
		}

		#endregion

		#region Properties

		#region IStorable

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsLoaded {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		bool IsLoading {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public IStorage Storage {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool DeleteChildren {
			get {
				return false;
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<IStorable> SavedChildren {
			get;
			set;
		}

		public Guid ID {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public string DocumentID {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Guid ParentID {
			get;
			set;
		}


		#endregion

		// All properties that are not preload must be overriden so that Fody.Loader can process
		// this properties and inject the CheckIsLoaded method
		public override Time Start {
			get {
				return base.Start;
			}
			set {
				base.Start = value;
			}
		}

		public override Time Stop {
			get {
				return base.Stop;
			}
			set {
				base.Stop = value;
			}
		}

		public override Time EventTime {
			get {
				return base.EventTime;
			}
			set {
				base.EventTime = value;
			}
		}

		public override Image Miniature {
			get {
				return base.Miniature;
			}
			set {
				base.Miniature = value;
			}
		}

		public override string Name {
			get {
				return base.Name;
			}
			set {
				base.Name = value;
			}
		}

		[PropertyChanged.DoNotNotify]
		[JsonIgnore]
		public Project Project {
			get;
			set;
		}

		/// <summary>
		/// The <see cref="EventType"/> in wich this event is tagged
		/// </summary>
		[LongoMatchPropertyPreload]
		[LongoMatchPropertyIndex (1)]
		public EventType EventType {
			get;
			set;
		}

		/// <summary>
		/// Event notes
		/// </summary>
		public string Notes {
			get;
			set;
		}

		/// <summary>
		/// Whether this event is currently selected.
		/// </summary>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool Selected {
			get;
			set;
		}

		/// <summary>
		/// List of drawings for this event
		/// </summary>
		public ObservableCollection<FrameDrawing> Drawings {
			get {
				return drawings;
			}
			set {
				if (drawings != null) {
					drawings.CollectionChanged -= ListChanged;
				}
				drawings = value;
				if (drawings != null) {
					drawings.CollectionChanged += ListChanged;
				}
			}
		}

		/// <summary>
		/// Whether this event has at least one <see cref="FrameDrawing"/>
		/// </summary>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool HasDrawings {
			get {
				return Drawings.Count > 0;
			}
		}

		/// <summary>
		/// List of players tagged in this event.
		/// </summary>
		[LongoMatchPropertyIndex (0)]
		public ObservableCollection<Player> Players {
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

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		[Obsolete ("Use Teams instead of Team to tag a team in a TimelineEvent")]
		public TeamType Team {
			get;
			set;
		}

		/// <summary>
		/// A list of teams tagged in this event.
		/// </summary>
		[LongoMatchPropertyIndex (3)]
		public ObservableCollection<Team> Teams {
			get {
				return teams;
			}
			set {
				if (teams != null) {
					teams.CollectionChanged -= ListChanged;
				}
				teams = value;
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
				List<Team> teams = new List<Team> ();
				if (Teams.Contains (Project.LocalTeamTemplate) ||
				    Players.Intersect (Project.LocalTeamTemplate.List).Any ()) {
					teams.Add (Project.LocalTeamTemplate);
				}
				if (Teams.Contains (Project.VisitorTeamTemplate) ||
				    Players.Intersect (Project.VisitorTeamTemplate.List).Any ()) {
					teams.Add (Project.VisitorTeamTemplate);
				}
				return teams;
			}
		}


		/// <summary>
		/// List of tags describing this event.
		/// </summary>
		/// <value>The tags.</value>
		public ObservableCollection<Tag> Tags {
			get {
				return tags;
			}
			set {
				if (tags != null) {
					tags.CollectionChanged -= ListChanged;
				}
				tags = value;
				if (tags != null) {
					tags.CollectionChanged += ListChanged;
				}
			}
		}

		/// <summary>
		/// Position of this event in the field.
		/// </summary>
		/// <value>The field position.</value>
		public Coordinates FieldPosition {
			get;
			set;
		}

		/// <summary>
		/// Position of this event in the half field.
		/// </summary>
		public Coordinates HalfFieldPosition {
			get;
			set;
		}

		/// <summary>
		/// Position of this event in the goal.
		/// </summary>
		public Coordinates GoalPosition {
			get;
			set;
		}

		/// <summary>
		/// An opaque object used by the view to describe the cameras layout.
		/// </summary>
		public object CamerasLayout {
			get;
			set;
		}

		/// <summary>
		/// A list of visible <see cref="CameraConfig"/> for this event.
		/// </summary>
		public ObservableCollection<CameraConfig> CamerasConfig {
			get {
				return camerasConfig;
			}
			set {
				if (camerasConfig != null) {
					camerasConfig.CollectionChanged -= ListChanged;
				}
				camerasConfig = value;
				if (camerasConfig != null) {
					camerasConfig.CollectionChanged += ListChanged;
				}
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public virtual string Description {
			get {
				return 
					(Name + "\n" +
				TagsDescription () + "\n" +
				TimesDesription ());
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public virtual Color Color {
			get {
				return EventType.Color;
			}
		}

		#endregion

		#region Public methods

		protected void CheckIsLoaded ()
		{
			if (!IsLoaded && !IsLoading) {
				IsLoading = true;
				if (Storage == null) {
					throw new StorageException ("Storage not set in preloaded object");
				}
				Storage.Fill (this);
				IsLoaded = true;
				IsLoading = false;
			}
		}

		public string TagsDescription ()
		{
			return String.Join ("-", Tags.Select (t => t.Value));
		}

		public string TimesDesription ()
		{
			if (Start != null && Stop != null) {
				if (Rate != 1) {
					return Start.ToMSecondsString () + " - " + Stop.ToMSecondsString () + " (" + RateString + ")";
				} else {
					return Start.ToMSecondsString () + " - " + Stop.ToMSecondsString ();
				}
			} else if (EventTime != null) {
				return EventTime.ToMSecondsString ();
			} else {
				return "";
			}
		}

		public void UpdateMiniature ()
		{
			if (Drawings.Count == 0) {
				Miniature = null;
			} else {
				Miniature = Drawings [0].Miniature;
			}
		}

		public void AddDefaultPositions ()
		{
			if (EventType.TagFieldPosition) {
				if (FieldPosition == null) {
					FieldPosition = new Coordinates ();
					FieldPosition.Points.Add (new Point (0.5, 0.5));
					if (EventType.FieldPositionIsDistance) {
						FieldPosition.Points.Add (new Point (0.5, 0.1));
					}
				}
			}
			if (EventType.TagHalfFieldPosition) {
				if (HalfFieldPosition == null) {
					HalfFieldPosition = new Coordinates ();
					HalfFieldPosition.Points.Add (new Point (0.5, 0.5));
					if (EventType.HalfFieldPositionIsDistance) {
						HalfFieldPosition.Points.Add (new Point (0.5, 0.1));
					}
				}
			}
			
			if (EventType.TagGoalPosition) {
				if (GoalPosition == null) {
					GoalPosition = new Coordinates ();
					GoalPosition.Points.Add (new Point (0.5, 0.5));
				}
			}
		}

		public Coordinates CoordinatesInFieldPosition (FieldPositionType pos)
		{
			switch (pos) {
			case FieldPositionType.Field:
				return FieldPosition;
			case FieldPositionType.HalfField:
				return HalfFieldPosition;
			case FieldPositionType.Goal:
				return GoalPosition;
			}
			return null;
		}

		public void UpdateCoordinates (FieldPositionType pos, ObservableCollection<Point> points)
		{
			Coordinates co = new Coordinates ();
			co.Points = points;
			
			switch (pos) {
			case FieldPositionType.Field:
				FieldPosition = co;
				break;
			case FieldPositionType.HalfField:
				HalfFieldPosition = co;
				break;
			case FieldPositionType.Goal:
				GoalPosition = co;
				break;
			}
		}

		public override string ToString ()
		{
			return Name;
		}

		public override bool Equals (object obj)
		{
			TimelineEvent evt = obj as TimelineEvent;
			if (evt == null)
				return false;
			return ID.Equals (evt.ID);
		}

		public override int GetHashCode ()
		{
			return ID.GetHashCode ();
		}

		#endregion

		void ListChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}
	}

	/// <summary>
	/// An event in the game for a penalty.
	/// </summary>
	[Serializable]
	public class PenaltyCardEvent: TimelineEvent
	{
		public PenaltyCard PenaltyCard {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override Color Color {
			get {
				return PenaltyCard != null ? PenaltyCard.Color : EventType.Color;
			}
		}
	}

	[Serializable]
	public class ScoreEvent: TimelineEvent
	{
		public Score Score {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override Color Color {
			get {
				return Score != null ? Score.Color : EventType.Color;
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override string Description {
			get {
				return String.Format ("{0} - {1}\n{2}\n{3}\n", Score.Points, Name,
					TagsDescription (), Start.ToMSecondsString (),
					Stop.ToMSecondsString ());
			}
		}
	}

	[Serializable]
	public class StatEvent: TimelineEvent
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
	public class SubstitutionEvent: StatEvent
	{
		public Player In {
			get;
			set;
		}

		public Player Out {
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

	[Serializable]
	public class LineupEvent: StatEvent
	{
		public List<Player> HomeStartingPlayers {
			get;
			set;
		}

		public List<Player> HomeBenchPlayers {
			get;
			set;
		}

		public List<Player> AwayStartingPlayers {
			get;
			set;
		}

		public List<Player> AwayBenchPlayers {
			get;
			set;
		}
	}
}
