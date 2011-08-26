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
using Gdk;
using Gtk;
using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using Mono.Unix;

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
	public class Project : IComparable
	{

		private readonly Guid _UUID;
		private ProjectDescription description;
		private List<Play> timeline;

		#region Constructors
		public Project() {
			_UUID = System.Guid.NewGuid();
			timeline = new List<Play>();
			Categories = new Categories();
			LocalTeamTemplate = new TeamTemplate();
			VisitorTeamTemplate = new TeamTemplate();
		}
		#endregion

		#region Properties

		/// <summary>
		/// Unique ID for the project
		/// </summary>
		public Guid UUID {
			get {
				return _UUID;
			}
		}
		
		public ProjectDescription Description {
			get{
				return description;
			}
			set {
				value.UUID = UUID;
				description = value;
			}
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

		#endregion

		#region Public Methods
		/// <summary>
		/// Frees all the project's resources helping the GC
		/// </summary>
		public void Clear() {
			timeline.Clear();
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
		public Play AddPlay(Category category, Time start, Time stop, Pixbuf miniature) {
			string count= String.Format("{0:000}",timeline.Count+1);
			string name = String.Format("{0} {1}",category.Name, count);

			var play = new Play {
				Name = name,
				Start = start,
				Stop = stop,
				Category = category,
				Notes = "",
				Miniature = miniature,
				Fps = Description.File.Fps,
			};
			timeline.Add(play);
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
		public void RemovePlays(List<Play> plays) {
			foreach(Play play in plays)
				timeline.Remove(play);
		}

		/// <summary>
		/// Delete a category
		/// </summary>
		/// <param name="sectionIndex">
		/// A <see cref="System.Int32"/>: category index
		/// </param>
		public void RemoveCategory(Category category) {
			if(Categories.Count == 1)
				throw new Exception("You can't remove the last Category");
			Categories.Remove(category);

			timeline.RemoveAll(p => p.Category.UUID == category.UUID);
		}
		
		public void DeleteSubcategoryTags(Category cat, List<ISubCategory> subcategories) {
			foreach (var play in timeline.Where(p => p.Category == cat)) {
				foreach (var subcat in subcategories) {
					Log.Error(play.Name);
					if (subcat is TagSubCategory)
						play.Tags.RemoveBySubcategory(subcat);
					else if (subcat is TeamSubCategory)
						play.Teams.RemoveBySubcategory(subcat);
					else if (subcat is PlayerSubCategory)
						play.Players.RemoveBySubcategory(subcat);
				}
			}
		}

		public List<Play> PlaysInCategory(Category category) {
			return timeline.Where(p => p.Category.UUID == category.UUID).ToList();
		}

		public List<Play> AllPlays() {
			return timeline;
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

			foreach(Category cat in Categories) {
				Gtk.TreeIter iter = dataFileListStore.AppendValues(cat);
				itersDic.Add(cat, iter);
			}
			
			var queryPlaysByCategory =
				timeline.GroupBy(play => play.Category);
			foreach(var playsGroup in queryPlaysByCategory) {
				Category cat = playsGroup.Key;
				if(!itersDic.ContainsKey(cat))
					continue;
				foreach(Play play in playsGroup) {
					dataFileListStore.AppendValues(itersDic[cat],play);
				}
			}
			return dataFileListStore;
		}

		public bool Equals(Project project) {
			if(project == null)
				return false;
			else
				return UUID == project.UUID;
		}

		public int CompareTo(object obj) {
			if(obj is Project) {
				Project project = (Project) obj;
				return UUID.CompareTo(project.UUID);
			}
			else
				throw new ArgumentException("object is not a Project and cannot be compared");
		}

		public static void Export(Project project, string file) {
			file = Path.ChangeExtension(file,"lpr");
			SerializableObject.Save(project, file);
		}

		public static Project Import(string file) {
			try {
				return SerializableObject.Load<Project>(file);
			}
			catch  (Exception e){
				Log.Exception (e);
				throw new Exception(Catalog.GetString("The file you are trying to load " +
				                                      "is not a valid project"));
			}
		}
		#endregion

		public void GetPlayersModel(out TreeStore localTeam, out TreeStore visitorTeam) {
			Dictionary<Player, TreeIter> localDict = new Dictionary<Player, TreeIter>();
			Dictionary<Player, TreeIter> visitorDict = new Dictionary<Player, TreeIter>();
			
			localTeam = new TreeStore(typeof(object));
			visitorTeam = new TreeStore(typeof(object));

			foreach(var player in LocalTeamTemplate) {
				/* Add a root in the tree with the option name */
				var iter = localTeam.AppendValues(player);
				localDict.Add(player, iter);
			}
			
			foreach(var player in VisitorTeamTemplate) {
				/* Add a root in the tree with the option name */
				var iter = visitorTeam.AppendValues(player);
				visitorDict.Add(player, iter);
			}
			
			foreach (var play in timeline) {
				foreach (var player in play.Players.AllUniqueElements) {
					if (localDict.ContainsKey(player.Value))
						localTeam.AppendValues(localDict[player.Value], new object[1] {play});
					else
						visitorTeam.AppendValues(visitorDict[player.Value], new object[1] {play});
				}
			}
		}

		#region Private Methods
		#endregion
	}
}
