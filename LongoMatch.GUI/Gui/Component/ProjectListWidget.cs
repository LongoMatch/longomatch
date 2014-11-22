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
using Gdk;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using Misc = LongoMatch.Gui.Helpers.Misc;
using Mono.Unix;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProjectListWidget : Gtk.Bin
	{
		public event ProjectsSelectedHandler ProjectsSelected;
		public event ProjectSelectedHandler ProjectSelected;

		const int COL_DISPLAY_NAME = 0;
		const int COL_PIXBUF1 = 1;
		const int COL_PIXBUF2 = 2;
		const int COL_PIXBUF3 = 3;
		const int COL_PROJECT_DESCRIPTION = 4;
		TreeModelFilter filter;
		TreeModelSort sort;
		List<ProjectDescription> projects;
		ListStore store;
		bool swallowSignals;

		public ProjectListWidget ()
		{
			this.Build ();
			
			store = CreateStore ();
			iconview.TextColumn = COL_DISPLAY_NAME;
			iconview.PixbufColumn = COL_PIXBUF1;
			iconview.SelectionChanged += HandleIconViewSelectionChanged;
			iconview.ItemActivated += HandleItemActivated;
			iconview.ItemWidth = 200;

			treeview.HeadersVisible = false;
			treeview.AppendColumn ("Home", new CellRendererPixbuf (), "pixbuf", COL_PIXBUF2); 
			treeview.AppendColumn ("Away", new CellRendererPixbuf (), "pixbuf", COL_PIXBUF3); 
			treeview.AppendColumn ("Desc", new CellRendererText (), "text", COL_DISPLAY_NAME); 
			treeview.Selection.Mode = SelectionMode.Multiple;
			treeview.EnableGridLines = TreeViewGridLines.None;
			treeview.Selection.Changed += HandleTreeviewSelectionChanged;
	
			sortcombobox.Active = (int)Config.ProjectSortMethod;
			sortcombobox.Changed += (sender, e) => {
				/* Hack to make it actually resort */
				sort.SetSortFunc (COL_DISPLAY_NAME, SortFunc);
				Config.ProjectSortMethod = (ProjectSortMethod)sortcombobox.Active;
			};
			focusimage.Pixbuf = Misc.LoadIcon ("longomatch-search", 27);
			ShowList = false;
		}

		public SelectionMode SelectionMode {
			set {
				iconview.SelectionMode = value;
				treeview.Selection.Mode = value;
			}
		}

		public bool ShowList {
			set {
				icoscrolledwindow.Visible = !value;
				treeviewscrolledwindow.Visible = value;
			}
		}

		public void Fill (List<ProjectDescription> projects)
		{
			Pixbuf image, homeShield, awayShield;

			swallowSignals = true;
			this.projects = projects;
			store.Clear ();
			foreach (ProjectDescription pdesc in projects) {
				MediaFile file = pdesc.FileSet.GetAngle (MediaFileAngle.Angle1);
				if (file != null && file.FilePath == Constants.FAKE_PROJECT) {
					image = Misc.LoadIcon ("longomatch-video-device-fake", 50);
				} else if (pdesc.FileSet.Preview != null) {
					image = pdesc.FileSet.Preview.Value;
				} else {
					image = Misc.LoadIcon ("longomatch-video-file", 50);
				}
				if (pdesc.LocalShield != null) {
					homeShield = pdesc.LocalShield.Scale (50, 50).Value;
				} else {
					homeShield = Misc.LoadIcon ("longomatch-default-shield", 50);
				}
				if (pdesc.VisitorShield != null) {
					awayShield = pdesc.VisitorShield.Scale (50, 50).Value;
				} else {
					awayShield = Misc.LoadIcon ("longomatch-default-shield", 50);
				}
				store.AppendValues (pdesc.Description, image, homeShield, awayShield, pdesc);
			}
			swallowSignals = false;
			iconview.SetCursor (new TreePath ("0"), null, false);
			treeview.SetCursor (new TreePath ("0"), null, false);
		}

		public void RemoveProjects (List<ProjectDescription> projects)
		{
			foreach (ProjectDescription project in projects) {
				this.projects.Remove (project);
			}
			Fill (this.projects);
			if (ProjectsSelected != null) {
				ProjectsSelected (new List<ProjectDescription> ());
			}
		}

		public void ClearSearch ()
		{
			filterEntry.Text = "";
		}

		ListStore CreateStore ()
		{
			store = new ListStore (typeof(string), typeof(Gdk.Pixbuf), typeof(Gdk.Pixbuf),
			                       typeof(Gdk.Pixbuf), typeof(ProjectDescription));
			
			filter = new Gtk.TreeModelFilter (store, null);
			filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTree);
			sort = new TreeModelSort (filter);
			sort.SetSortFunc (COL_DISPLAY_NAME, SortFunc);
			sort.SetSortColumnId (COL_DISPLAY_NAME, SortType.Ascending);
			iconview.Model = sort;
			treeview.Model = sort;
			return store;
		}

		int SortFunc (TreeModel model, TreeIter a, TreeIter b)
		{
			ProjectDescription p1, p2;
			int ret;
			
			p1 = (ProjectDescription)model.GetValue (a, COL_PROJECT_DESCRIPTION);
			p2 = (ProjectDescription)model.GetValue (b, COL_PROJECT_DESCRIPTION);

			if (p1 == null && p2 == null) {
				return 0;
			} else if (p1 == null) {
				return -1;
			} else if (p2 == null) {
				return 1;
			}
			
			if (sortcombobox.Active == 0) {
				ret = String.Compare (p1.Title, p2.Title);
				if (ret == 0) {
					ret = -DateTime.Compare (p1.MatchDate, p2.MatchDate);
				}
				return ret;
			} else if (sortcombobox.Active == 1) {
				ret = -DateTime.Compare (p1.MatchDate, p2.MatchDate);
				if (ret == 0) {
					ret = String.Compare (p1.Title, p2.Title);
				}
				return ret;
			} else if (sortcombobox.Active == 2) {
				return -DateTime.Compare (p1.LastModified, p2.LastModified);
			} else if (sortcombobox.Active == 3) {
				ret = String.Compare (p1.Season, p2.Season);
				if (ret == 0) {
					ret = String.Compare (p1.Title, p2.Title);
				}
				return ret;
			} else if (sortcombobox.Active == 4) {
				ret = String.Compare (p1.Competition, p2.Competition);
				if (ret == 0) {
					ret = String.Compare (p1.Title, p2.Title);
				}
				return ret;
			} else {
				return  String.Compare (p1.Title, p2.Title);
			}
		}

		protected virtual void OnFilterentryChanged (object sender, System.EventArgs e)
		{
			filter.Refilter ();
		}

		bool FilterTree (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			StringComparison sc = StringComparison.InvariantCultureIgnoreCase;
			ProjectDescription project = (ProjectDescription)model.GetValue (iter, COL_PROJECT_DESCRIPTION);

			if (project == null)
				return true;

			if (filterEntry.Text == "")
				return true;

			if (project.Title.IndexOf (filterEntry.Text, sc) > -1)
				return true;
			else if (project.Season.IndexOf (filterEntry.Text, sc) > -1)
				return true;
			else if (project.Competition.IndexOf (filterEntry.Text, sc) > -1)
				return true;
			else if (project.LocalName.IndexOf (filterEntry.Text, sc) > -1)
				return true;
			else if (project.VisitorName.IndexOf (filterEntry.Text, sc) > -1)
				return true;
			else
				return false;
		}

		void HandleSelectionChanged (TreeModel model, TreePath[] selectedItems)
		{
			TreeIter iter;
			List<ProjectDescription> list;

			if (swallowSignals)
				return;

			if (ProjectsSelected != null) {
				list = new List<ProjectDescription> ();
				for (int i = 0; i < selectedItems.Length; i++) {
					model.GetIterFromString (out iter, selectedItems [i].ToString ());
					list.Add ((ProjectDescription)model.GetValue (iter, COL_PROJECT_DESCRIPTION));
				}
				ProjectsSelected (list);
			}
		}

		protected virtual void HandleIconViewSelectionChanged (object o, EventArgs args)
		{
			HandleSelectionChanged (iconview.Model, iconview.SelectedItems);
		}
		
		void HandleTreeviewSelectionChanged (object sender, EventArgs e)
		{
			HandleSelectionChanged (treeview.Model, treeview.Selection.GetSelectedRows());
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
