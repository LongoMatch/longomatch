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

using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Playlists;
using LongoMatch.Store.Templates;

namespace LongoMatch.Store
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

		#region Constructors
		public Project() {
			ID = System.Guid.NewGuid();
			Timeline = new List<Play>();
			Categories = new Categories();
			LocalTeamTemplate = new TeamTemplate();
			VisitorTeamTemplate = new TeamTemplate();
			Timers = new List<Timer> ();
			Periods = new List<Period> ();
			ScoreTimeline = new List<ScoreEvent>();
			PenaltyCardsTimeline = new List<PenaltyCardEvent> ();
			Playlists = new List<Playlist> ();
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
		
		public List<Play> Timeline {
			get;
			set;
		}
		
		public List<ScoreEvent> ScoreTimeline {
			get;
			set;
		}
		
		public List<PenaltyCardEvent> PenaltyCardsTimeline {
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

		/// <value>
		/// Categories template
		/// </value>
		[JsonProperty(Order = -10)]
		public Categories Categories {
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
		public IEnumerable<IGrouping<Category, Play>> PlaysGroupedByCategory {
			get {
				return Timeline.GroupBy(play => play.Category);
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Frees all the project's resources helping the GC
		/// </summary>
		public void Clear() {
			Timeline.Clear();
			Categories.List.Clear();
			VisitorTeamTemplate.List.Clear();
			LocalTeamTemplate.List.Clear();
			Periods.Clear();
			Timers.Clear();
		}


		/// <summary>
		/// Adds a new play to a given category
		/// </summary>
		/// <param name="dataSection">
		/// A <see cref="System.Int32"/>: category index
		/// </param>
		/// <param name="start">
		/// A <see cref="Time"/>: start time of the play
		/// </param>
		/// <param name="stop">
		/// A <see cref="Time"/>: stop time of the play
		/// </param>
		/// <param name="thumbnail">
		/// A <see cref="Pixbuf"/>: snapshot of the play
		/// </param>
		/// <returns>
		/// A <see cref="MediaTimeNode"/>: created play
		/// </returns>
		public Play AddPlay(Category category, Time start, Time stop, Image miniature) {
			string count= String.Format("{0:000}", PlaysInCategory (category).Count + 1);
			string name = String.Format("{0} {1}",category.Name, count);

			var play = new Play {
				Name = name,
				Start = start,
				Stop = stop,
				Category = category,
				Notes = "",
				Miniature = miniature,
			};
			Timeline.Add(play);
			return play;
		}
		
		public void AddPlay (Play play) {
			Timeline.Add(play);
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
		public void RemovePlays(List<Play> plays) {
			foreach(Play play in plays)
				Timeline.Remove(play);
		}

		/// <summary>
		/// Delete a category
		/// </summary>
		/// <param name="sectionIndex">
		/// A <see cref="System.Int32"/>: category index
		/// </param>
		public void RemoveCategory(Category category) {
			if(Categories.CategoriesList.Count == 1)
				throw new Exception("You can't remove the last Category");
			Categories.List.Remove(category);
			Timeline.RemoveAll(p => p.Category.ID == category.ID);
		}
		
		public void RemovePlayer(TeamTemplate template, Player player) {
			if(template.List.Count == 1)
				throw new Exception("You can't remove the last Player");
			template.List.Remove(player);
			foreach (var play in Timeline) {
				play.Players.RemoveAll (p => p == player);
			}
		}
		
		public List<Play> PlaysInCategory(Category category) {
			return Timeline.Where(p => p.Category.ID == category.ID).ToList();
		}

		public int GetScore (Team team) {
			return ScoreTimeline.Where (s => PlayTaggedTeam (s) == team).Sum(s => s.Score.Points); 
		}
		
		public Team PlayTaggedTeam (Play play) {
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
				return Categories.FieldBackground;
			case FieldPositionType.HalfField:
				return Categories.HalfFieldBackground;
			case FieldPositionType.Goal:
				return Categories.GoalBackground;
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
