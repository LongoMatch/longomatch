// TreeWidget.cs
//
//  Copyright(C) 20072009 Andoni Morales Alastruey
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
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Collections.Generic;
using Gtk;
using Mono.Unix;
using LongoMatch.Gui.Dialog;
using LongoMatch.Handlers;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Common;
using LongoMatch.Store.Playlists;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlaysListTreeWidget : Gtk.Bin
	{

		Project project;

		public PlaysListTreeWidget()
		{
			this.Build();
			treeview.EditProperties += OnEditProperties;
			treeview.NewRenderingJob += OnNewRenderingJob;
		}
		
		public PlaysFilter Filter {
			set{
				treeview.Filter = value;
			}
		}

		public void RemovePlays(List<Play> plays) {
			TreeIter iter, child;
			TreeStore model;
			List<TreeIter> removeIters;

			if(project == null)
				return;

			removeIters = new List<TreeIter>();
			model = (TreeStore)treeview.Model;
			model.GetIterFirst(out iter);
			/* Scan all the tree and store the iter of each play
			 * we need to delete, but don't delete it yet so that
			 * we don't alter the tree */
			do {
				if(!model.IterHasChild(iter))
					continue;

				model.IterChildren(out child, iter);
				do {
					Play play = (Play) model.GetValue(child,0);
					if(plays.Contains(play)) {
						removeIters.Add(child);
					}
				} while(model.IterNext(ref child));
			} while(model.IterNext(ref iter));

			/* Remove the selected iters now */
			for(int i=0; i < removeIters.Count; i++) {
				iter = removeIters[i];
				model.Remove(ref iter);
			}
		}

		public void AddPlay(Play play) {
			TreeIter categoryIter;

			if(project == null)
				return;

			var cat = play.Category;
			var model = (TreeStore)treeview.Model;
			model.GetIterFromString(out categoryIter, CategoryPath(cat));
			var playIter = model.AppendValues(categoryIter,play);
			var playPath = model.GetPath(playIter);
			treeview.Selection.UnselectAll();
			treeview.ExpandToPath(playPath);
			treeview.Selection.SelectIter(playIter);
		}

		public Project Project {
			set {
				project = value;
				if (project != null) {
					treeview.Model = GetModel (project);
					treeview.Colors = true;
					treeview.Project = value;
				} else {
					treeview.Model = null;
				}
			}
			get {
				return project;
			}
		}

		private TreeStore GetModel (Project project)
		{
			Gtk.TreeIter iter;
			Dictionary<TaggerButton, TreeIter> itersDic = new Dictionary<TaggerButton, TreeIter> ();
			Gtk.TreeStore dataFileListStore = new Gtk.TreeStore (typeof(object));

			/* Add scores */
			if (project.Categories.Scores.Count > 0) {
				iter = dataFileListStore.AppendValues (
					new AnalysisCategory { Name = Catalog.GetString ("Score"),
					SortMethod = SortMethodType.SortByStartTime,
					Color = Config.Style.PaletteActive}, null);
				foreach (Score s in project.Categories.Scores) {
					itersDic.Add(s, iter);
				}
			}
			
			/* Add penalty cards*/
			if (project.Categories.PenaltyCards.Count > 0) {
				iter = dataFileListStore.AppendValues (
					new AnalysisCategory { Name = Catalog.GetString ("Penalty Cards"),
					SortMethod = SortMethodType.SortByStartTime,
					Color = Config.Style.PaletteActive}, null);
				foreach (PenaltyCard pc in project.Categories.PenaltyCards) {
					itersDic.Add(pc, iter);
				}
			}
			
			foreach(TaggerButton cat in project.Categories.CategoriesList) {
				iter = dataFileListStore.AppendValues(cat);
				itersDic.Add(cat, iter);
			}
			
			var queryPlaysByCategory = project.PlaysGroupedByCategory;
			foreach(var playsGroup in queryPlaysByCategory) {
				TaggerButton cat = playsGroup.Key;
				if(!itersDic.ContainsKey(cat))
					continue;
				foreach(Play play in playsGroup) {
					dataFileListStore.AppendValues(itersDic[cat], play);
				}
			}
			return dataFileListStore;
		}

		private string CategoryPath(TaggerButton cat) {
			return project.Categories.List.IndexOf(cat).ToString();
		}
		
		protected virtual void OnEditProperties(AnalysisCategory cat) {
			EditCategoryDialog dialog = new EditCategoryDialog(project, cat);
			dialog.Run();
			dialog.Destroy();
		}

		protected virtual void OnNewRenderingJob (object sender, EventArgs args)
		{
			Playlist playlist;
			TreePath[] paths;

			playlist = new Playlist();
			paths = treeview.Selection.GetSelectedRows();

			foreach(var path in paths) {
				TreeIter iter;
				PlaylistPlayElement element;
				
				treeview.Model.GetIter(out iter, path);
				element = new PlaylistPlayElement (treeview.Model.GetValue(iter, 0) as Play,
				                                   project.Description.File);
				playlist.Elements.Add (element);
			}
			
			Config.EventsBroker.EmitRenderPlaylist (playlist);
		}
	}
}
