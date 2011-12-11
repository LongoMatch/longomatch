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
using Mono.Unix;
using Gtk;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Video.Utils;



namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProjectListWidget : Gtk.Bin
	{

		private Gtk.ListStore projectsListStore;
		private List<ProjectDescription> projectsList;
		private TreeModelFilter filter;
		public event ProjectsSelectedHandler ProjectsSelected;


		public ProjectListWidget()
		{
			this.Build();
			projectsListStore = new Gtk.ListStore(typeof(Project));

			Gtk.TreeViewColumn fileDescriptionColumn = new Gtk.TreeViewColumn();
			fileDescriptionColumn.Title = Catalog.GetString("Filename");
			Gtk.CellRendererText filenameCell = new Gtk.CellRendererText();
			Gtk.CellRendererText filePropertiesCell = new Gtk.CellRendererText();
			Gtk.CellRendererPixbuf miniatureCell = new Gtk.CellRendererPixbuf();
			fileDescriptionColumn.PackStart(miniatureCell,false);
			fileDescriptionColumn.PackStart(filenameCell, true);
			fileDescriptionColumn.PackStart(filePropertiesCell, true);

			fileDescriptionColumn.SetCellDataFunc(filenameCell, new Gtk.TreeCellDataFunc(RenderName));
			fileDescriptionColumn.SetCellDataFunc(filePropertiesCell, new Gtk.TreeCellDataFunc(RenderProperties));
			fileDescriptionColumn.SetCellDataFunc(miniatureCell, new Gtk.TreeCellDataFunc(RenderPixbuf));

			treeview.AppendColumn(fileDescriptionColumn);
			treeview.EnableGridLines = TreeViewGridLines.Horizontal;
			treeview.HeadersVisible = false;
		}

		public SelectionMode SelectionMode {
			set {
				treeview.Selection.Mode = value;
			}
		}

		public void RemoveProjects(List<ProjectDescription> projects) {
			/* FIXME: to delete projects from the treeview we need to remove the filter
			 * and clear everything, otherwise we have seen several crashes trying
			 * to render cells with an invalid iter. It's not very performant, but
			 * it's safe. */
			treeview.Model = projectsListStore;
			projectsListStore.Clear();
			foreach(ProjectDescription project in projects)
				projectsList.Remove(project);
			Fill(projectsList);
		}

		public void Fill(List<ProjectDescription> projects) {
			projectsList = projects;
			projectsList.Sort();
			projectsListStore.Clear();
			foreach(ProjectDescription project in projectsList) {
				projectsListStore.AppendValues(project);
			}
			filter = new Gtk.TreeModelFilter(projectsListStore, null);
			filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc(FilterTree);
			treeview.Model = filter;
			treeview.Selection.Mode = SelectionMode.Multiple;
			treeview.Selection.Changed += OnSelectionChanged;
		}

		public void ClearSearch() {
			filterEntry.Text="";
		}

		private void RenderPixbuf(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			ProjectDescription project = (ProjectDescription) model.GetValue(iter, 0);

			(cell as Gtk.CellRendererPixbuf).Pixbuf= project.File.Preview != null ? project.File.Preview.Value : null;
		}

		private void RenderProperties(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			string text;
			ProjectDescription project = (ProjectDescription) model.GetValue(iter, 0);

			text = "\n"+"\n"+"\n"+"<b>"+Catalog.GetString("File length")+":</b>  " +
			       (new Time {MSeconds = (int)project.File.Length}).ToSecondsString();
			text = text +"\n"+"<b>"+Catalog.GetString("Video codec")+":</b>  " + project.File.VideoCodec;
			text = text +"\n"+"<b>"+Catalog.GetString("Audio codec")+":</b>  " + project.File.AudioCodec;
			text = text +"\n"+"<b>"+Catalog.GetString("Format")+":</b>  " + project.Format;

			(cell as Gtk.CellRendererText).Markup = text;
		}

		private void RenderName(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			string text;
			ProjectDescription project = (ProjectDescription) model.GetValue(iter, 0);

			text = "<b>"+Catalog.GetString("Title")+":</b>  " + project.Title;
			text = text +"\n"+"<b>"+Catalog.GetString("Local team")+":</b>  " + project.LocalName;
			text = text +"\n"+"<b>"+Catalog.GetString("Visitor team")+":</b>  " + project.VisitorName;
			text = text +"\n"+"<b>"+Catalog.GetString("Season")+":</b>  " + project.Season;
			text = text +"\n"+"<b>"+Catalog.GetString("Competition")+":</b>  " + project.Competition;
			text = text +"\n"+"<b>"+Catalog.GetString("Result")+":</b>  " + project.LocalGoals+"-"+ project.VisitorGoals;
			text = text +"\n"+"<b>"+Catalog.GetString("Date")+":</b>  " + project.MatchDate.ToShortDateString();

			(cell as Gtk.CellRendererText).Markup = text;
		}

		protected virtual void OnFilterentryChanged(object sender, System.EventArgs e)
		{
			filter.Refilter();
		}

		private bool FilterTree(Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			ProjectDescription project =(ProjectDescription) model.GetValue(iter, 0);

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

			list = new List<ProjectDescription>();
			pathArray = treeview.Selection.GetSelectedRows();

			for(int i=0; i< pathArray.Length; i++) {
				treeview.Model.GetIterFromString(out iter, pathArray[i].ToString());
				list.Add((ProjectDescription) treeview.Model.GetValue(iter, 0));
			}
			if(ProjectsSelected != null)
				ProjectsSelected(list);
		}
	}
}
