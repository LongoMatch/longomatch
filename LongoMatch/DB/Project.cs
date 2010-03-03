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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Gtk;
using Gdk;
using Mono.Unix;
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

		private PreviewMediaFile file;

		private string title;

		private string localName;

		private string visitorName;

		private int localGoals;

		private int visitorGoals;

		private DateTime matchDate;

		private string season;

		private string competition;

		private Sections sections;

		private List<List<MediaTimeNode>> sectionPlaysList;

		private TeamTemplate visitorTeamTemplate;

		private TeamTemplate localTeamTemplate;
		
		private TagsTemplate tagsTemplate;
		//Keep this fiel for DB retrocompatibility
		private List<MediaTimeNode>[] dataSectionArray;

		/// <summary>
		/// Creates a new project
		/// </summary>
		/// <param name="file">
		/// A <see cref="PreviewMediaFile"/>: video file information
		/// </param>
		/// <param name="localName">
		/// A <see cref="System.String"/>: local team's name
		/// </param>
		/// <param name="visitorName">
		/// A <see cref="System.String"/>: visitor team's name
		/// </param>
		/// <param name="season">
		/// A <see cref="System.String"/>: season information
		/// </param>
		/// <param name="competition">
		/// A <see cref="System.String"/>: competition information
		/// </param>
		/// <param name="localGoals">
		/// A <see cref="System.Int32"/>: local team's goals
		/// </param>
		/// <param name="visitorGoals">
		/// A <see cref="System.Int32"/>: visitor team's goals
		/// </param>
		/// <param name="matchDate">
		/// A <see cref="DateTime"/>: game date
		/// </param>
		/// <param name="sections">
		/// A <see cref="Sections"/>: categories template
		/// </param>
		/// <param name="localTeamTemplate">
		/// A <see cref="TeamTemplate"/>: local team template
		/// </param>
		/// <param name="visitorTeamTemplate">
		/// A <see cref="TeamTemplate"/>: visitor team template
		/// </param>
		#region Constructors
		public Project(PreviewMediaFile file, String localName, String visitorName,
		               String season, String competition, int localGoals,
		               int visitorGoals, DateTime matchDate, Sections sections,
		               TeamTemplate localTeamTemplate, TeamTemplate visitorTeamTemplate) {

			this.file = file;
			this.localName = localName;
			this.visitorName = visitorName;
			this.localGoals = localGoals;
			this.visitorGoals = visitorGoals;
			this.matchDate = matchDate;
			this.season = season;
			this.competition = competition;
			this.localTeamTemplate = localTeamTemplate;
			this.visitorTeamTemplate = visitorTeamTemplate;
			this.sections = sections;
			this.sectionPlaysList = new List<List<MediaTimeNode>>();

			for (int i=0;i<sections.Count;i++) {
				sectionPlaysList.Add(new List<MediaTimeNode>());
			}
			
			this.tagsTemplate = new TagsTemplate();
			this.Title = file == null ? "" : System.IO.Path.GetFileNameWithoutExtension(this.file.FilePath);
		}
		#endregion

		#region Properties
		/// <value>
		/// Video File
		/// </value>
		public PreviewMediaFile File {
			get {
				return file;
			}
			set {
				file=value;
			}
		}

		/// <value>
		/// Project title
		/// </value>
		public String Title {
			get {
				return title;
			}
			set {
				title=value;
			}
		}

		/// <value>
		/// Season description
		/// </value>
		public String Season {
			get {
				return season;
			}
			set {
				season = value;
			}
		}

		/// <value>
		/// Competition description
		/// </value>
		public String Competition {
			get {
				return competition;
			}
			set {
				competition= value;
			}
		}

		/// <value>
		/// Local team name
		/// </value>
		public String LocalName {
			get {
				return localName;
			}
			set {
				localName=value;
			}
		}

		/// <value>
		/// Visitor team name
		/// </value>
		public String VisitorName {
			get {
				return visitorName;
			}
			set {
				visitorName=value;
			}
		}

		/// <value>
		/// Local team goals
		/// </value>
		public int LocalGoals {
			get {
				return localGoals;
			}
			set {
				localGoals=value;
			}
		}

		/// <value>
		/// Visitor team goals
		/// </value>
		public int VisitorGoals {
			get {
				return visitorGoals;
			}
			set {
				visitorGoals=value;
			}
		}

		/// <value>
		/// Game date
		/// </value>
		public DateTime MatchDate {
			get {
				return matchDate;
			}
			set {
				matchDate=value;
			}
		}

		/// <value>
		/// Categories template
		/// </value>
		public Sections Sections {
			get {
				return this.sections;
			}
			set {
				this.sections = value;
			}
		}

		/// <value>
		/// Local team template
		/// </value>
		public TeamTemplate LocalTeamTemplate {
			get {
				return localTeamTemplate;
			}
			set {
				localTeamTemplate=value;
			}
		}

		/// <value>
		/// Visitor team template
		/// </value>
		public TeamTemplate VisitorTeamTemplate {
			get {
				return visitorTeamTemplate;
			}
			set {
				visitorTeamTemplate=value;
			}
		}
		
		/// <value>
		/// Template with the project tags
		/// </value>
		public TagsTemplate Tags{
			//Since 0.15.5
			get{
				if (tagsTemplate == null)
					tagsTemplate = new TagsTemplate();
				return tagsTemplate;
			}
			set{
				tagsTemplate = value;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Frees all the project's resources helping the GC
		/// </summary>
		public void Clear() {
			//Help the GC freeing objects
			foreach (List<MediaTimeNode> list in sectionPlaysList)
				list.Clear();
			sectionPlaysList.Clear();
			Sections.Clear();
			visitorTeamTemplate.Clear();
			localTeamTemplate.Clear();
			sectionPlaysList=null;
			Sections=null;
			visitorTeamTemplate=null;
			localTeamTemplate=null;
		}

		/// <summary>
		/// Adds a new analysis category to the project
		/// </summary>
		/// <param name="tn">
		/// A <see cref="SectionsTimeNode"/>
		/// </param>
		public void AddSection(SectionsTimeNode tn) {
			AddSectionAtPos(tn,sections.Count);
		}

		/// <summary>
		/// Add a new category to the project at a given position
		/// </summary>
		/// <param name="tn">
		/// A <see cref="SectionsTimeNode"/>: category default values
		/// </param>
		/// <param name="sectionIndex">
		/// A <see cref="System.Int32"/>: position index
		/// </param>
		public void AddSectionAtPos(SectionsTimeNode tn,int sectionIndex) {
			sectionPlaysList.Insert(sectionIndex,new List<MediaTimeNode>());
			sections.AddSectionAtPos(tn,sectionIndex);
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
		public MediaTimeNode AddTimeNode(int dataSection, Time start, Time stop,Pixbuf thumbnail) {
			MediaTimeNode tn ;
			List<MediaTimeNode> playsList= sectionPlaysList[dataSection];
			int count= playsList.Count+1;
			string name = sections.GetName(dataSection) + " " +count;
			// HACK: Used for capture where fps is not specified, asuming PAL@25fps
			ushort fps = file != null ? file.Fps : (ushort)25;

			tn = new MediaTimeNode(name, start, stop,"",fps,thumbnail);
			playsList.Add(tn);
			return tn;
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
		public void DeleteTimeNode(MediaTimeNode tNode,int section) {
			sectionPlaysList[section].Remove(tNode);
		}

		/// <summary>
		/// Delete a category
		/// </summary>
		/// <param name="sectionIndex">
		/// A <see cref="System.Int32"/>: category index
		/// </param>
		public void DeleteSection(int sectionIndex) {
			if (sections.Count == 1)
				throw new Exception("You can't remove the last Section");
			sections.RemoveSection(sectionIndex);
			sectionPlaysList.RemoveAt(sectionIndex);
		}

		/// <summary>
		/// Return an array of strings with the categories names
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string[] GetSectionsNames() {
			return sections.GetSectionsNames();
		}

		/// <summary>
		/// Return an array of <see cref="LongoMatch.TimeNodes.Time"/> with
		/// the categories default lead time
		/// </summary>
		/// <returns>
		/// A <see cref="Time"/>
		/// </returns>
		public Time[] GetSectionsStartTimes() {
			return sections.GetSectionsStartTimes();
		}

		/// <summary>
		/// Return an array of <see cref="LongoMatch.TimeNodes.Time"/> with
		/// the categories default lag time
		/// </summary>
		/// <returns>
		/// A <see cref="Time"/>
		/// </returns>
		public Time[] GetSectionsStopTimes() {
			return sections.GetSectionsStopTimes();
		}

		/// <summary>
		/// Returns a <see cref="Gtk.TreeStore"/> in which project categories are
		/// root nodes and their respectives plays child nodes
		/// </summary>
		/// <returns>
		/// A <see cref="TreeStore"/>
		/// </returns>
		public TreeStore GetModel() {
			Gtk.TreeStore dataFileListStore = new Gtk.TreeStore(typeof(MediaTimeNode));
			for (int i=0;i<sections.Count;i++) {
				Gtk.TreeIter iter = dataFileListStore.AppendValues(sections.GetTimeNode(i));
				foreach (MediaTimeNode tNode in sectionPlaysList[i]) {
					dataFileListStore.AppendValues(iter,tNode);
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
			List<TreeIter> itersList = new List<TreeIter>();
			Gtk.TreeStore dataFileListStore = new Gtk.TreeStore(typeof(object));
			for (int i=0;i<localTeamTemplate.PlayersCount;i++) {
				itersList.Add(dataFileListStore.AppendValues(localTeamTemplate.GetPlayer(i)));
			}
			for (int i=0;i<sections.Count;i++) {
				foreach (MediaTimeNode tNode in sectionPlaysList[i]) {
					foreach (int player in tNode.LocalPlayers)
						dataFileListStore.AppendValues(itersList[player],tNode);
				}
			}
			return dataFileListStore;
		}

		/// <summary>
		/// Returns a <see cref="Gtk.TreeStore"/> for the visitor team in which players
		///  are root nodes and their respectives tagged plays child nodes
		/// </summary>
		/// <returns>
		/// A <see cref="TreeStore"/>
		/// </returns>
		public TreeStore GetVisitorTeamModel() {
			List<TreeIter> itersList = new List<TreeIter>();
			Gtk.TreeStore dataFileListStore = new Gtk.TreeStore(typeof(object));
			for (int i=0;i<visitorTeamTemplate.PlayersCount;i++) {
				itersList.Add(dataFileListStore.AppendValues(visitorTeamTemplate.GetPlayer(i)));
			}
			for (int i=0;i<sections.Count;i++) {
				foreach (MediaTimeNode tNode in sectionPlaysList[i]) {
					foreach (int player in tNode.VisitorPlayers)
						dataFileListStore.AppendValues(itersList[player],tNode);
				}
			}
			return dataFileListStore;
		}

		/// <summary>
		/// Returns a list of plays' lists. Actually used by the timeline
		/// </summary>
		/// <returns>
		/// A <see cref="List"/>
		/// </returns>
		public List<List<MediaTimeNode>> GetDataArray() {
			return sectionPlaysList;
		}

		public bool Equals(Project project) {
			if (project == null)
				return false;
			else
				return this.File.FilePath.Equals(project.File.FilePath);
		}

		public int CompareTo(object obj) {
			if (obj is Project) {
				Project project = (Project) obj;
				return this.File.FilePath.CompareTo(project.File.FilePath);
			}
			else
				throw new ArgumentException("object is not a Project and cannot be compared");
		}
		
		public static void Export(Project project, string file) {
			file = Path.ChangeExtension(file,"lpr");
			IFormatter formatter = new BinaryFormatter();
			using(Stream stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None)){
				formatter.Serialize(stream, project);			
			}
		}
		
		public static Project Import(string file) {
			using(Stream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None))
			{
				try {
					IFormatter formatter = new BinaryFormatter();
					return (Project)formatter.Deserialize(stream);
				}
				catch {
					throw new Exception(Catalog.GetString("The file you are trying to load is not a valid project"));
				}
			}			
		}		
		#endregion
	}
}
