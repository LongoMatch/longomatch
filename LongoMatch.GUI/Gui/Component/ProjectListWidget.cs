// ProjectListWidget.cs
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
using Mono.Unix;
using Gtk;

using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Video.Utils;
using Gdk;



namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProjectListWidget : Gtk.Bin
	{
		public event ProjectsSelectedHandler ProjectsSelected;
		public event ProjectSelectedHandler ProjectSelected;
		
		const int COL_DISPLAY_NAME = 0;
		const int COL_PIXBUF = 1;
		const int COL_PROJECT_DESCRIPTION = 2;
		TreeModelFilter filter;
		List<ProjectDescription> projects;
		ListStore store;
		bool swallowSignals;

		public ProjectListWidget()
		{
			this.Build();
			
			//GtkGlue.EntrySetIcon (filterEntry, GtkGlue.EntryIconPosition.Secondary, "gtk-clear");
			store = CreateStore ();
			iconview.TextColumn = COL_DISPLAY_NAME;
			iconview.PixbufColumn = COL_PIXBUF;
			iconview.SelectionChanged += OnSelectionChanged;
			iconview.ItemActivated += HandleItemActivated;
			iconview.ItemWidth = 200;
		}

		public SelectionMode SelectionMode {
			set {
				iconview.SelectionMode = value;
			}
		}
		
		public void Fill (List<ProjectDescription> projects)
		{
			Pixbuf image;
			swallowSignals = true;
			this.projects = projects;
			store.Clear ();
			foreach (ProjectDescription pdesc in projects)
			{
				if (pdesc.File.Preview != null) {
					image = pdesc.File.Preview.Value;
				} else  {
					image = Stetic.IconLoader.LoadIcon (this, Gtk.Stock.Harddisk, IconSize.Dialog);
				}
				store.AppendValues (Describe (pdesc), image, pdesc);
			}
			swallowSignals = false;
		}

		public void RemoveProjects(List<ProjectDescription> projects) {
			foreach (ProjectDescription project in projects) {
				this.projects.Remove(project);
			}
			Fill (this.projects);
		}

		public void ClearSearch() {
			filterEntry.Text="";
		}
		
		string Describe (ProjectDescription project) {
			string ret;
			
			ret = project.Title;
			ret += String.Format ("\n {0} - {1} ({2}-{3})", project.LocalName,
			                      project.VisitorName, project.LocalGoals,
			                      project.VisitorGoals);
			ret += "\n" + project.Format;
			return ret;
		}
		
		ListStore CreateStore ()
		{
			store = new ListStore (typeof (string), typeof (Gdk.Pixbuf), typeof (ProjectDescription));
			store.DefaultSortFunc = SortFunc;
			store.SetSortColumnId (COL_DISPLAY_NAME, SortType.Ascending);
			filter = new Gtk.TreeModelFilter (store, null);
			filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTree);
			iconview.Model = filter;
			return store;
		}

		int SortFunc (TreeModel model, TreeIter a, TreeIter b)
		{
			ProjectDescription pa = (ProjectDescription) model.GetValue (a, COL_PROJECT_DESCRIPTION);
			ProjectDescription pb = (ProjectDescription) model.GetValue (b, COL_PROJECT_DESCRIPTION);
			
			return (int) (pa.LastModified.Ticks - pb.LastModified.Ticks);
		}
		
		protected virtual void OnFilterentryChanged(object sender, System.EventArgs e)
		{
			filter.Refilter();
		}

		private bool FilterTree(Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			ProjectDescription project =(ProjectDescription) model.GetValue(iter, COL_PROJECT_DESCRIPTION);

			if(project == null)
				return true;

			if(filterEntry.Text == "")
				return true;

			if(project.Title.IndexOf(filterEntry.Text) > -1)
				return true;
			else if(project.Season.IndexOf(filterEntry.Text) > -1)
				return true;
			else if(project.Competition.IndexOf(filterEntry.Text) > -1)
				return true;
			else if(project.LocalName.IndexOf(filterEntry.Text) > -1)
				return true;
			else if(project.VisitorName.IndexOf(filterEntry.Text) > -1)
				return true;
			else
				return false;
		}

		protected virtual void OnSelectionChanged(object o, EventArgs args) {
			TreeIter iter;
			List<ProjectDescription> list;
			TreePath[] pathArray;
			
			if (swallowSignals)
				return;

			if(ProjectsSelected != null) {
				list = new List<ProjectDescription>();
				pathArray = iconview.SelectedItems;
				
				for(int i=0; i< pathArray.Length; i++) {
					iconview.Model.GetIterFromString (out iter, pathArray[i].ToString());
					list.Add ((ProjectDescription) iconview.Model.GetValue (iter, COL_PROJECT_DESCRIPTION));
				}
				ProjectsSelected (list);
			}
		}

		void HandleItemActivated (object o, ItemActivatedArgs args)
		{
			TreeIter iter;
			ProjectDescription pdesc;
			
			if (swallowSignals)
				return;
				
			if (ProjectSelected != null) {
				iconview.Model.GetIter (out iter, args.Path);
				pdesc = iconview.Model.GetValue (iter, COL_PROJECT_DESCRIPTION) as ProjectDescription;
				if (pdesc != null) {
					ProjectSelected (pdesc);
				}
			}
		}
	}
}
