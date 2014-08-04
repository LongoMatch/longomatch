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
using LongoMatch.Common;
using LongoMatch.Interfaces;
using Newtonsoft.Json;

namespace LongoMatch.Store
{

	/// <summary>
	/// Represents a Play in the game.
	/// </summary>

	[Serializable]
	public class  Play : PixbufTimeNode, ITimelineNode, IIDObject
	{

		#region Constructors
		public Play() {
			Drawings = new List<FrameDrawing>();
			Players = new List<Player> ();
			Tags = new List<Tag>();
			PlaybackRate = 1.0;
			ID = Guid.NewGuid ();
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
		public Category Category {
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

		/* FIXME: Keep this until we support multiple drawings */
		[JsonIgnore]
		public FrameDrawing KeyFrameDrawing {
			get {
				if(Drawings.Count > 0)
					return Drawings.First();
				else
					return null;
			} set {
				if (Drawings.Count == 0)
					Drawings.Add (value);
				else
					Drawings[0] = value;
			}
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
		
		public Team Team {
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
		
		public double PlaybackRate {
			get;
			set;
		}
		
		#endregion

		#region Public methods
		
		public string TagsDescription () {
			return String.Join ("-", Tags.Select (t => t.Value));
		}

		public void AddDefaultPositions () {
			if (Category.TagFieldPosition) {
				if (FieldPosition == null) {
					FieldPosition = new Coordinates ();
					FieldPosition.Points.Add (new Point (0.5, 0.5));
				}
				if (Category.FieldPositionIsDistance) {
					FieldPosition.Points.Add (new Point (0.5, 0.1));
				}
			}
			if (Category.TagHalfFieldPosition) {
				if (HalfFieldPosition == null) {
					HalfFieldPosition = new Coordinates ();
					HalfFieldPosition.Points.Add (new Point (0.5, 0.5));
				}
				if (Category.HalfFieldPositionIsDistance) {
					HalfFieldPosition.Points.Add (new Point (0.5, 0.1));
				}
			}
			
			if (Category.TagGoalPosition) {
				if (GoalPosition == null) {
					GoalPosition = new Coordinates ();
					GoalPosition.Points.Add (new Point (0.5, 0.5));
				}
			}
		}
		
		public Coordinates CoordinatesInFieldPosition (FieldPositionType pos) {
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
		
		public void UpdateCoordinates (FieldPositionType pos, List<Point> points) {
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
		
		public override string ToString()
		{
			return 
				Name + "\n" +
				TagsDescription () + "\n" +
				Start.ToMSecondsString() + " - " + Stop.ToMSecondsString();
		}
		#endregion
	}
}
