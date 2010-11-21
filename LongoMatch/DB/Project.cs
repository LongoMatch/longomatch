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
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Gtk;
using Gdk;
using Mono.Unix;
using LongoMatch.Common;
using LongoMatch.TimeNodes;
using LongoMatch.Video.Utils;

namespace LongoMatch.DB
{

	/// <summary>
	/// I hold the information needed by a project and provide persistency using
	/// the db4o database.
	/// I'm structured in the following way:
	/// -Project Description (<see cref="LongoMatch.Utils.PreviewMediaFile"/>
	/// -1 Categories Template
	/// -1 Local Team Template
	/// -1 Visitor Team Template
	/// -1 list of <see cref="LongoMatch.TimeNodes.MediaTimeNode"/> for each category
	/// </summary>
	///
	[Serializable]
	public class Project : IComparable
	{


		private List<Play> playsList;


		#region Constructors
		public Project(){
			playsList = new List<Play>();
			Categories = new Categories();
			LocalTeamTemplate = new TeamTemplate();
			VisitorTeamTemplate = new TeamTemplate();
			Tags = new TagsTemplate();
		}
		#endregion

		#region Properties
		
		public ProjectDescription Description {
			get;
			set;
		}

		/// <value>
		/// Categories template
		/// </value>
		public Categories Categories {
			get;
			set;
		}

		/// <value>
		/// Local team template
		/// </value>
		public TeamTemplate LocalTeamTemplate {
			get;
			set;
		}

		/// <value>
		/// Visitor team template
		/// </value>
		public TeamTemplate VisitorTeamTemplate {
			get;
			set;
		}
		
		public TagsTemplate Tags {
			get;
			set;
		}
		
		#endregion

		#region Public Methods
		/// <summary>
		/// Frees all the project's resources helping the GC
		/// </summary>
		public void Clear() {
			playsList.Clear();
			Categories.Clear();
			VisitorTeamTemplate.Clear();
			LocalTeamTemplate.Clear();
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
		public Play AddPlay(Category category, Time start, Time stop,Pixbuf miniature) {
			string count= String.Format("{0:000}",playsList.Count+1);
			string name = String.Format("{0} {1}",category.Name, count);
			// HACK: Used for capture where fps is not specified, asuming PAL@25fps
			ushort fps = Description.File != null ? Description.File.Fps : (ushort)25;

			var play = new Play {
				Name = name,
				Start = start,
				Stop = stop,
				Category = category,
				Team = Team.NONE,
				Notes = "",
				Miniature = miniature,
				Fps = fps,
			};
			playsList.Add(play);
			return play;
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
		public void RemovePlay(Play play) {
			playsList.Remove(play);
		}

		/// <summary>
		/// Delete a category
		/// </summary>
		/// <param name="sectionIndex">
		/// A <see cref="System.Int32"/>: category index
		/// </param>
		public void RemoveCategory(Category category) {
			if (Categories.Count == 1)
				throw new Exception("You can't remove the last Section");
			Categories.RemoveCategory(category);
			
			/* query for all the plays with this Category */
			var plays = 
				from play in playsList
					where play.Category.UUID == category.UUID
					select play;
			/* Delete them */
			foreach (var play in playsList)
				playsList.Remove(play);
		}
		
		public List<Play> PlaysInCategory (Category category){
			Console.WriteLine ("Plays are " + playsList.Count);
			return (from play in playsList
			        where play.Category.UUID == category.UUID
			        select play).ToList();
		}
		
		public List<Play> AllPlays (){
			return (from play in playsList
			        select play).ToList();
		}
		
		/// <summary>
		/// Returns a <see cref="Gtk.TreeStore"/> in which project categories are
		/// root nodes and their respectives plays child nodes
		/// </summary>
		/// <returns>
		/// A <see cref="TreeStore"/>
		/// </returns>
		public TreeStore GetModel() {
			Dictionary<Category, TreeIter> itersDic = new Dictionary<Category, TreeIter>();
			Gtk.TreeStore dataFileListStore = new Gtk.TreeStore(typeof(Play));
			
			IEnumerable<IGrouping<Category, Play>> queryPlaysByCategory = 
				from play in playsList
					group play by play.Category;
			
			foreach (Category cat in Categories.CategoriesList){
				Gtk.TreeIter iter = dataFileListStore.AppendValues(cat);
				itersDic.Add(cat, iter);
			} 
			
			foreach (var playsGroup in queryPlaysByCategory) {
				Category cat = playsGroup.Key;
				if (!itersDic.ContainsKey(cat))
					continue;
				foreach (Play play in playsGroup) {
					dataFileListStore.AppendValues(itersDic[cat],play);
				}
			}
			return dataFileListStore;
		}

		/// <summary>
		/// Returns a <see cref="Gtk.TreeStore"/> for the local team in which players
		///  are root nodes and their respectives tagged plays child nodes
		/// </summary>
		/// <returns>
		/// A <see cref="TreeStore"/>
		/// </returns>
		public TreeStore GetLocalTeamModel() {
			return GetTeamModel(LocalTeamTemplate);
		}

		/// <summary>
		/// Returns a <see cref="Gtk.TreeStore"/> for the visitor team in which players
		///  are root nodes and their respectives tagged plays child nodes
		/// </summary>
		/// <returns>
		/// A <see cref="TreeStore"/>
		/// </returns>
		public TreeStore GetVisitorTeamModel() {
			return GetTeamModel(VisitorTeamTemplate);
		}

		public bool Equals(Project project) {
			if (project == null)
				return false;
			else
				return Description.File.FilePath.Equals(project.Description.File.FilePath);
		}

		public int CompareTo(object obj) {
			if (obj is Project) {
				Project project = (Project) obj;
				return Description.File.FilePath.CompareTo(project.Description.File.FilePath);
			}
			else
				throw new ArgumentException("object is not a Project and cannot be compared");
		}
		
		public static void Export(Project project, string file) {
			file = Path.ChangeExtension(file,"lpr");
			IFormatter formatter = new BinaryFormatter();
			using(Stream stream = new FileStream(file, FileMode.Create, 
			                                     FileAccess.Write, FileShare.None))
				formatter.Serialize(stream, project);
		}
		
		public static Project Import(string file) {
			using(Stream stream = new FileStream(file, FileMode.Open, 
			                                     FileAccess.Read, FileShare.None))
			{
				try {
					IFormatter formatter = new BinaryFormatter();
					return (Project)formatter.Deserialize(stream);
				}
				catch {
					throw new Exception(Catalog.GetString("The file you are trying to load " +
						"is not a valid project"));
				}
			}			
		}		
		#endregion
		
		#region Private Methods
		
		private TreeStore GetTeamModel(TeamTemplate team){
			TreeStore dataFileListStore = new TreeStore(typeof(object));
			
			/* For all the players in the team */
			foreach (var player in team.PlayersList){
				/* Add a root in the tree with the player */
				var iter = dataFileListStore.AppendValues(player);
				/* Query the plays where this player is in the list of players*/
				var queryByPlayers = 
					from play in playsList
						where play.HasPlayer(player) == true 
						select play;
				/* Then add as children of the Player in the tree */
				foreach (var play in queryByPlayers)
					dataFileListStore.AppendValues(iter, play);
			}
			return dataFileListStore;
		}
		#endregion
	}
}
