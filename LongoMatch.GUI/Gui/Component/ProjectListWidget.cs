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
using System.Linq;
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
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ProjectListWidget : Gtk.Bin
	{
		public event ProjectsSelectedHandler ProjectsSelected;
		public event ProjectSelectedHandler ProjectSelected;

		const int COL_DISPLAY_NAME = 0;
		const int COL_PIXBUF1 = 1;
		const int COL_PIXBUF2 = 2;
		const int COL_PIXBUF3 = 3;
		const int COL_PROJECT = 4;
		TreeModelFilter filter;
		TreeModelSort sort;
		List<Project> projects;
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

		public void Fill (List<Project> projects)
		{
			Pixbuf image, homeShield, awayShield;

			swallowSignals = true;
			this.projects = projects;
			store.Clear ();
			foreach (Project p in projects) {
				ProjectDescription pdesc = p.Description;
				MediaFile file = pdesc.FileSet.FirstOrDefault ();
				if (file != null && file.IsFakeCapture) {
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
				
				store.AppendValues (FormatDesc (pdesc), image, homeShield, awayShield, p);
			}
			swallowSignals = false;
			iconview.SelectPath (new TreePath ("0"));
			treeview.SetCursor (new TreePath ("0"), null, false);
		}

		/// <summary>
		/// Removes the provided projects from the list. Matching is done using the project description instance, not the project ID.
		/// </summary>
		/// <param name="projects">List of project description to remove.</param>
		public void RemoveProjects (List<Project> projects)
		{
			foreach (Project project in projects) {
				this.projects.Remove (project);
			}
			// Regenerate our list, this will trigger selected event for the first item.
			Fill (this.projects);
		}

		/// <summary>
		/// Updates the project description with a matching ID to the new description.
		/// </summary>
		/// <param name="description">Project Description.</param>
		public void UpdateProject (Project project)
		{
			TreeIter first;

			/* Projects are only update in the treeview mode */
			store.GetIterFirst (out first);
			while (store.IterIsValid (first)) {
				Project p = store.GetValue (first, COL_PROJECT) as Project;
				if (project.ID == p.ID) {
					// Change value in model
					store.SetValue (first, COL_DISPLAY_NAME, FormatDesc (project.Description));
					store.SetValue (first, COL_PROJECT, project);
					// Also update our internal list. Although it's a new instance of Project the ID is the same
					// and IndexOf should return the index of the old project to replace.
					projects [projects.IndexOf (project)] = project;
					break;
				}
				store.IterNext (ref first);
			}
		}


		public void ClearSearch ()
		{
			filterEntry.Text = "";
		}

		static string FormatDesc (ProjectDescription pdesc)
		{
			string desc = String.Format ("{0}-{1} ({2}-{3})\n{4}: {5}\n{6}: {7}\n{8}: {9}",
				              pdesc.LocalName, pdesc.VisitorName, pdesc.LocalGoals,
				              pdesc.VisitorGoals, Catalog.GetString ("Date"),
				              pdesc.MatchDate.ToShortDateString (), Catalog.GetString ("Competition"),
				              pdesc.Competition, Catalog.GetString ("Season"), pdesc.Season);
			return desc;
		}

		ListStore CreateStore ()
		{
			store = new ListStore (typeof(string), typeof(Gdk.Pixbuf), typeof(Gdk.Pixbuf),
				typeof(Gdk.Pixbuf), typeof(Project));
			
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
			Project p1, p2;
			
			p1 = (Project)model.GetValue (a, COL_PROJECT);
			p2 = (Project)model.GetValue (b, COL_PROJECT);

			if (p1 == null && p2 == null) {
				return 0;
			} else if (p1 == null) {
				return -1;
			} else if (p2 == null) {
				return 1;
			}

			return ProjectDescription.Sort (p1.Description, p2.Description,
				(ProjectSortType)sortcombobox.Active);
		}

		protected virtual void OnFilterentryChanged (object sender, System.EventArgs e)
		{
			filter.Refilter ();
		}

		bool FilterTree (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Project project = (Project)model.GetValue (iter, COL_PROJECT);

			if (project == null)
				return true;
			
			return project.Description.Search (filterEntry.Text);
		}

		void HandleSelectionChanged (TreeModel model, TreePath[] selectedItems)
		{
			TreeIter iter;
			List<Project> list;

			if (swallowSignals)
				return;

			if (ProjectsSelected != null) {
				list = new List<Project> ();
				for (int i = 0; i < selectedItems.Length; i++) {
					model.GetIterFromString (out iter, selectedItems [i].ToString ());
					list.Add ((Project)model.GetValue (iter, COL_PROJECT));
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
			HandleSelectionChanged (treeview.Model, treeview.Selection.GetSelectedRows ());
		}

		void HandleItemActivated (object o, ItemActivatedArgs args)
		{
			TreeIter iter;
			Project project;
			
			if (swallowSignals)
				return;
				
			if (ProjectSelected != null) {
				iconview.Model.GetIter (out iter, args.Path);
				project = iconview.Model.GetValue (iter, COL_PROJECT) as Project;
				if (project != null) {
					ProjectSelected (project);
				}
			}
		}

	}
}
