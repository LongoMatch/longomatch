// ProjectListWidget.cs
//
//  Copyright (C) 2007 Andoni Morales Alastruey
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using System.Collections;
using System.IO;
using Mono.Unix;
using Gtk;
using Db4objects.Db4o;
using LongoMatch.DB;



namespace LongoMatch.Gui.Component
{
	
	public delegate void ProjectSelectedHandler (Project project);
	
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
			projectsListStore = new Gtk.ListStore (typeof (Project));
			filter = new Gtk.TreeModelFilter (projectsListStore, null);	
			filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTree);			
			treeview.Model = filter;
			
			Gtk.TreeViewColumn fileDescriptionColumn = new Gtk.TreeViewColumn ();
			fileDescriptionColumn.Title = Catalog.GetString("Filename");
			Gtk.CellRendererText filenameCell = new Gtk.CellRendererText ();
			Gtk.CellRendererPixbuf miniatureCell = new Gtk.CellRendererPixbuf ();
			fileDescriptionColumn.PackStart (miniatureCell,false);
			fileDescriptionColumn.PackStart (filenameCell, true);			
			
			fileDescriptionColumn.SetCellDataFunc (filenameCell, new Gtk.TreeCellDataFunc (RenderName));
			fileDescriptionColumn.SetCellDataFunc (miniatureCell, new Gtk.TreeCellDataFunc(RenderPixbuf));
			
			treeview.AppendColumn (fileDescriptionColumn);
			treeview.EnableGridLines = TreeViewGridLines.Horizontal;
			treeview.HeadersVisible = false;	
		}		
				
		
		private void RenderPixbuf (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Project project = (Project) model.GetValue (iter, 0);			
 			(cell as Gtk.CellRendererPixbuf).Pixbuf= project.File.Preview;			
		}
		private void RenderName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Project _project = (Project) model.GetValue (iter, 0);
			string _filePath = _project.File.FilePath;	
			string text;
			
			
			text = Catalog.GetString("<b>File:</b>  ") + System.IO.Path.GetFileName(_filePath.ToString());
			text = text +"\n"+Catalog.GetString("<b>Local Team:</b>  ") + _project.LocalName;
			text = text +"\n"+Catalog.GetString("<b>Visitor Team:</b>  ") + _project.VisitorName;
			text = text +"\n"+Catalog.GetString("<b>Season:</b>  ") + _project.Season;
			text = text +"\n"+Catalog.GetString("<b>Competition:</b>  ") + _project.Competition;
			text = text +"\n"+Catalog.GetString("<b>Result:</b>  ") + _project.LocalGoals+"-"+_project.VisitorGoals;
			text = text +"\n"+Catalog.GetString("<b>Date:</b>  ") + _project.MatchDate.ToString(Catalog.GetString("MM/dd/yyyy"));
			
			(cell as Gtk.CellRendererText).Markup = text;	
		}
				
		public void Fill(ArrayList db){	
			projectsListStore.Clear();
			db.Sort();
				
			foreach (Project _project in db){				
				projectsListStore.AppendValues(_project);
			}
		}
		
		public Project GetSelection(){
			TreePath path;
			TreeViewColumn col;
			treeview.GetCursor(out path,out col);
			return this.GetProject(path);
			
		}
		
		public void ClearSearch(){
			filterEntry.Text="";
		}
		
		private Project GetProject(TreePath path){
			if (path != null){
				Gtk.TreeIter iter;
				filter.GetIter (out iter, path);
 				Project project = (Project) filter.GetValue (iter, 0);
				return project;
			}
			else return null;			
		}


		protected virtual void OnTreeviewCursorChanged (object sender, System.EventArgs e)
		{
			TreeIter iter;
			this.treeview.Selection.GetSelected(out iter);
			Project selectedProject = (Project) filter.GetValue (iter, 0);
			if (ProjectSelectedEvent!=null)
				ProjectSelectedEvent(selectedProject);
		}

		protected virtual void OnFilterentryChanged (object sender, System.EventArgs e)
		{
			filter.Refilter ();

		}
		
		private bool FilterTree (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Project project =(Project) model.GetValue (iter, 0); 
			
			if (project == null)
				return true;
 
			if (filterEntry.Text == "")
				return true;
 
			if (project.Title.IndexOf (filterEntry.Text) > -1)
				return true;
			else if (project.Season.IndexOf (filterEntry.Text) > -1)
				return true;
			else if (project.Competition.IndexOf (filterEntry.Text) > -1)
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
