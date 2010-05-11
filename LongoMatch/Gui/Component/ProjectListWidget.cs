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
using Db4objects.Db4o;
using LongoMatch.DB;
using LongoMatch.TimeNodes;
using LongoMatch.Video.Utils;



namespace LongoMatch.Gui.Component
{

	public delegate void ProjectSelectedHandler(ProjectDescription project);

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProjectListWidget : Gtk.Bin
	{

		private Gtk.ListStore projectsListStore;
		private TreeModelFilter filter;
		public event ProjectSelectedHandler ProjectSelectedEvent;


		public ProjectListWidget()
		{
			this.Build();
			projectsListStore = new Gtk.ListStore(typeof(Project));
			filter = new Gtk.TreeModelFilter(projectsListStore, null);
			filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc(FilterTree);
			treeview.Model = filter;

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


		private void RenderPixbuf(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			ProjectDescription project = (ProjectDescription) model.GetValue(iter, 0);
			(cell as Gtk.CellRendererPixbuf).Pixbuf= project.Preview;
		}
		
		private void RenderProperties(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			ProjectDescription project = (ProjectDescription) model.GetValue(iter, 0);
			string text;

			text = "\n"+"\n"+"\n"+"<b>"+Catalog.GetString("File length")+":</b>  " + project.Length.ToSecondsString();
			text = text +"\n"+"<b>"+Catalog.GetString("Video codec")+":</b>  " + project.VideoCodec;
			text = text +"\n"+"<b>"+Catalog.GetString("Audio codec")+":</b>  " + project.AudioCodec;
			text = text +"\n"+"<b>"+Catalog.GetString("Format")+":</b>  " + project.Format;

			(cell as Gtk.CellRendererText).Markup = text;
		}
		
		private void RenderName(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			ProjectDescription project = (ProjectDescription) model.GetValue(iter, 0);
			string text;

			text = "<b>"+Catalog.GetString("Title")+":</b>  " + project.Title;
			text = text +"\n"+"<b>"+Catalog.GetString("Local team")+":</b>  " + project.LocalName;
			text = text +"\n"+"<b>"+Catalog.GetString("Visitor team")+":</b>  " + project.VisitorName;
			text = text +"\n"+"<b>"+Catalog.GetString("Season")+":</b>  " + project.Season;
			text = text +"\n"+"<b>"+Catalog.GetString("Competition")+":</b>  " + project.Competition;
			text = text +"\n"+"<b>"+Catalog.GetString("Result")+":</b>  " + project.LocalGoals+"-"+ project.VisitorGoals;
			text = text +"\n"+"<b>"+Catalog.GetString("Date")+":</b>  " + project.MatchDate.ToShortDateString();

			(cell as Gtk.CellRendererText).Markup = text;
		}

		public void Fill(List<ProjectDescription> projectsList) {
			projectsListStore.Clear();
			projectsList.Sort();
			foreach (ProjectDescription project in projectsList) {
				projectsListStore.AppendValues(project);
			}
		}

		public ProjectDescription GetSelection() {
			TreePath path;
			TreeViewColumn col;
			treeview.GetCursor(out path,out col);
			return this.GetProject(path);
		}

		public void ClearSearch() {
			filterEntry.Text="";
		}

		private ProjectDescription GetProject(TreePath path) {
			if (path != null) {
				Gtk.TreeIter iter;
				filter.GetIter(out iter, path);
				ProjectDescription project = (ProjectDescription) filter.GetValue(iter, 0);
				return project;
			}
			else return null;
		}

		protected virtual void OnTreeviewCursorChanged(object sender, System.EventArgs e)
		{
			TreeIter iter;
			this.treeview.Selection.GetSelected(out iter);
			ProjectDescription selectedProject = (ProjectDescription) filter.GetValue(iter, 0);
			if (ProjectSelectedEvent!=null)
				ProjectSelectedEvent(selectedProject);
		}

		protected virtual void OnFilterentryChanged(object sender, System.EventArgs e)
		{
			filter.Refilter();
		}

		private bool FilterTree(Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			ProjectDescription project =(ProjectDescription) model.GetValue(iter, 0);

			if (project == null)
				return true;

			if (filterEntry.Text == "")
				return true;

			if (project.Title.IndexOf(filterEntry.Text) > -1)
				return true;
			else if (project.Season.IndexOf(filterEntry.Text) > -1)
				return true;
			else if (project.Competition.IndexOf(filterEntry.Text) > -1)
				return true;
			else if (project.LocalName.IndexOf(filterEntry.Text) > -1)
				return true;
			else if (project.VisitorName.IndexOf(filterEntry.Text) > -1)
				return true;
			else
				return false;
		}
	}
}
