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
using Gtk;
using Mono.Unix;
using LongoMatch.DB;
using LongoMatch.Handlers;
using LongoMatch.TimeNodes;
using LongoMatch.Common;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlaysListTreeWidget : Gtk.Bin
	{

		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event TimeNodeDeletedHandler TimeNodeDeleted;
		public event PlayListNodeAddedHandler PlayListNodeAdded;
		public event SnapshotSeriesHandler SnapshotSeriesEvent;
		public event PlayersTaggedHandler PlayersTagged;
		public event TagPlayHandler TagPlay;

		private Project project;

		public PlaysListTreeWidget()
		{
			this.Build();
			treeview.TimeNodeChanged += OnTimeNodeChanged;
            treeview.TimeNodeSelected += OnTimeNodeSelected;
            treeview.TimeNodeDeleted += OnTimeNodeDeleted;
            treeview.PlayListNodeAdded += OnPlayListNodeAdded;
            treeview.SnapshotSeriesEvent += OnSnapshotSeriesEvent;
            treeview.PlayersTagged += OnPlayersTagged;
            treeview.TagPlay += OnTagPlay;
		}

		public void RemovePlay(Play play) {
			if (project != null) {
				TreeIter iter;
				TreeIter child;
				
				var category = play.Category;
				var model = (TreeStore)treeview.Model;
				model.GetIterFromString(out iter, CategoryPath(category));
				model.IterChildren(out child, iter);
				// Searching the TimeNode to remove it
				while (model.IterIsValid(child)) {
					Play mtn = (Play) model.GetValue(child,0);
					if (mtn == play) {
						model.Remove(ref child);
						break;
					}
					TreeIter prev = child;
					model.IterNext(ref child);
					if (prev.Equals(child))
						break;
				}
			}
		}

		public void AddPlay(Play play) {
			TreeIter categoryIter;
			
			if (project == null)
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

		public bool ProjectIsLive{
			set{
				treeview.ProjectIsLive = value;
			}
		}

		public Project Project {
			set {
				project = value;
				if (project != null) {
					treeview.Model = project.GetModel();
					treeview.Colors = true;
					treeview.VisitorTeam = project.Description.VisitorName;
					treeview.LocalTeam = project.Description.LocalName;
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
		
		private string CategoryPath(Category cat){
			return project.Categories.CategoriesList.IndexOf(cat).ToString();
		}

		protected virtual void OnTimeNodeChanged(TimeNode tNode,object val) {
			if (TimeNodeChanged != null)
				TimeNodeChanged(tNode,val);
		}

		protected virtual void OnTimeNodeSelected(Play tNode) {
			if (TimeNodeSelected != null)
				TimeNodeSelected(tNode);
		}

		protected virtual void OnTimeNodeDeleted(Play tNode){
			if (TimeNodeDeleted != null)
				TimeNodeDeleted(tNode);
		}

		protected virtual void OnPlayListNodeAdded(Play tNode)
		{
			if (PlayListNodeAdded != null)
				PlayListNodeAdded(tNode);
		}

		protected virtual void OnSnapshotSeriesEvent(LongoMatch.TimeNodes.Play tNode)
		{
			if (SnapshotSeriesEvent != null)
				SnapshotSeriesEvent(tNode);
		}

		protected virtual void OnPlayersTagged(LongoMatch.TimeNodes.Play tNode, Team team)
		{
			if (PlayersTagged != null)
				PlayersTagged(tNode,team);
		}

		protected virtual void OnTagPlay (LongoMatch.TimeNodes.Play tNode)
		{
			if (TagPlay != null)
				TagPlay(tNode);
		}
	}
}
