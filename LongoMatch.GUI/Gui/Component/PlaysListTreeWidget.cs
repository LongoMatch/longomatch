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
using LongoMatch.Store;
using LongoMatch.Common;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlaysListTreeWidget : Gtk.Bin
	{

		public event PlaySelectedHandler TimeNodeSelected;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event PlaysDeletedHandler TimeNodeDeleted;
		public event PlayListNodeAddedHandler PlayListNodeAdded;
		public event SnapshotSeriesHandler SnapshotSeriesEvent;
		public event TagPlayHandler TagPlay;
		public event RenderPlaylistHandler RenderPlaylistEvent;

		private Project project;

		public PlaysListTreeWidget()
		{
			this.Build();
			treeview.TimeNodeChanged += OnTimeNodeChanged;
			treeview.TimeNodeSelected += OnTimeNodeSelected;
			treeview.TimeNodeDeleted += OnTimeNodeDeleted;
			treeview.PlayListNodeAdded += OnPlayListNodeAdded;
			treeview.SnapshotSeriesEvent += OnSnapshotSeriesEvent;
			treeview.EditProperties += OnEditProperties;
			treeview.TagPlay += OnTagPlay;
			treeview.NewRenderingJob += OnNewRenderingJob;
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

		public bool ProjectIsLive {
			set {
				treeview.ProjectIsLive = value;
			}
		}

		public Project Project {
			set {
				project = value;
				if(project != null) {
					treeview.Model = GetModel(project);
					treeview.Colors = true;
				}
				else {
					treeview.Model = null;
				}
			}
		}

		public bool PlayListLoaded {
			set {
				treeview.PlayListLoaded=value;
			}
		}
		
		private TreeStore GetModel(Project project){
			Dictionary<Category, TreeIter> itersDic = new Dictionary<Category, TreeIter>();
			Gtk.TreeStore dataFileListStore = new Gtk.TreeStore(typeof(Play));

			foreach(Category cat in project.Categories) {
				Gtk.TreeIter iter = dataFileListStore.AppendValues(cat);
				itersDic.Add(cat, iter);
			}
			
			var queryPlaysByCategory = project.PlaysGroupedByCategory;
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

		private string CategoryPath(Category cat) {
			return project.Categories.IndexOf(cat).ToString();
		}
		
		protected virtual void OnEditProperties(TimeNode tNode, object val) {
			EditCategoryDialog dialog = new EditCategoryDialog();
			dialog.Category = tNode as Category; 
			dialog.Project = project;
			dialog.Run();
			dialog.Destroy();
			if(TimeNodeChanged != null)
				TimeNodeChanged(tNode, tNode.Name);
		}

		protected virtual void OnTimeNodeChanged(TimeNode tNode,object val) {
			if(TimeNodeChanged != null)
				TimeNodeChanged(tNode,val);
		}

		protected virtual void OnTimeNodeSelected(Play tNode) {
			if(TimeNodeSelected != null)
				TimeNodeSelected(tNode);
		}

		protected virtual void OnTimeNodeDeleted(List<Play> plays) {
			if(TimeNodeDeleted != null)
				TimeNodeDeleted(plays);
		}

		protected virtual void OnPlayListNodeAdded(Play tNode)
		{
			if(PlayListNodeAdded != null)
				PlayListNodeAdded(tNode);
		}

		protected virtual void OnSnapshotSeriesEvent(LongoMatch.Store.Play tNode)
		{
			if(SnapshotSeriesEvent != null)
				SnapshotSeriesEvent(tNode);
		}

		protected virtual void OnTagPlay(LongoMatch.Store.Play tNode)
		{
			if(TagPlay != null)
				TagPlay(tNode);
		}
		
		protected virtual void OnNewRenderingJob (object sender, EventArgs args)
		{
			PlayList playlist = new PlayList();
			TreePath[] paths = treeview.Selection.GetSelectedRows();

			foreach(var path in paths) {
				TreeIter iter;
				
				treeview.Model.GetIter(out iter, path);
				playlist.Add(new PlayListPlay((Play)treeview.Model.GetValue(iter, 0),
				                              project.Description.File, 1, true));
			}
			
			if (RenderPlaylistEvent != null)
				RenderPlaylistEvent(playlist);
		}
	}
}
