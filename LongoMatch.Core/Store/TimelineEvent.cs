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
using System.Linq;
using Mono.Unix;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{
	/// <summary>
	/// Represents a Play in the game.
	/// </summary>

	[Serializable]
	public class  TimelineEvent : PixbufTimeNode, IIDObject
	{

		#region Constructors
		public TimelineEvent ()
		{
			Drawings = new List<FrameDrawing> ();
			Players = new List<Player> ();
			Tags = new List<Tag> ();
			Rate = 1.0f;
			ID = Guid.NewGuid ();
			ActiveViews = new HashSet<MediaFileAngle>();
			ActiveViews.Add (MediaFileAngle.Angle1);
		}
		#endregion
		#region Properties
		public Guid ID {
			get;
			set;
		}

		/// <summary>
		/// Category in which this play is tagged
		/// </summary>
		public EventType EventType {
			get;
			set;
		}

		/// <summary>
		/// A string with the play's notes
		/// </summary>
		public string Notes {
			get;
			set;
		}

		/// <summary>
		/// Get/Set wheter this play is actually loaded. Used in  <see cref="LongoMatch.Gui.Component.TimeScale">
		/// </summary>
		[JsonIgnore]
		public bool Selected {
			get;
			set;
		}

		/// <summary>
		/// List of drawings for this play
		/// </summary>
		public List<FrameDrawing> Drawings {
			get;
			set;
		}

		/// <summary>
		/// Get wether the play has at least a frame drawing
		/// </summary>
		[JsonIgnore]
		public bool HasDrawings {
			get {
				return Drawings.Count > 0;
			}
		}

		public List<Player> Players {
			get;
			set;
		}

		public TeamType Team {
			get;
			set;
		}

		public List<Tag> Tags {
			get;
			set;
		}

		public Coordinates FieldPosition {
			get;
			set;
		}

		public Coordinates HalfFieldPosition {
			get;
			set;
		}

		public Coordinates GoalPosition {
			get;
			set;
		}

		public HashSet<MediaFileAngle> ActiveViews {
			get;
			set;
		}

		[JsonIgnore]
		public virtual string Description {
			get {
				return 
					(Name + "\n" +
					TagsDescription () + "\n" +
					TimesDesription ());
			}
		}

		[JsonIgnore]
		public virtual Color Color {
			get {
				return EventType.Color;
			}
		}
		#endregion
		#region Public methods
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
			} else if (EventType != null) {
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
				Miniature = Drawings[0].Miniature;
			}
		}

		public void AddDefaultPositions ()
		{
			if (EventType.TagFieldPosition) {
				if (FieldPosition == null) {
					FieldPosition = new Coordinates ();
					FieldPosition.Points.Add (new Point (0.5, 0.5));
				}
				if (EventType.FieldPositionIsDistance) {
					FieldPosition.Points.Add (new Point (0.5, 0.1));
				}
			}
			if (EventType.TagHalfFieldPosition) {
				if (HalfFieldPosition == null) {
					HalfFieldPosition = new Coordinates ();
					HalfFieldPosition.Points.Add (new Point (0.5, 0.5));
				}
				if (EventType.HalfFieldPositionIsDistance) {
					HalfFieldPosition.Points.Add (new Point (0.5, 0.1));
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

		public void UpdateCoordinates (FieldPositionType pos, List<Point> points)
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
			return Description;
		}
		#endregion
	}

	[Serializable]
	public class PenaltyCardEvent: TimelineEvent
	{
		public PenaltyCard PenaltyCard {
			get;
			set;
		}

		[JsonIgnore]
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
		public override Color Color {
			get {
				return Score != null ? Score.Color : EventType.Color;
			}
		}

		[JsonIgnore]
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
				return Reason.ToString();
			}
			set {
			}
		}

		public SubstitutionReason Reason {
			get;
			set;
		}

		[JsonIgnore]
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
