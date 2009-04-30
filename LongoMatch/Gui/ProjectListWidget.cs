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

		private Gtk.ListStore dataFileListStore;
		public event         ProjectSelectedHandler ProjectSelectedEvent;
		
		public ProjectListWidget()
		{
			this.Build();
			dataFileListStore = new Gtk.ListStore (typeof (Project));
			treeview.Model=dataFileListStore;
			
			Gtk.TreeViewColumn filenameColumn = new Gtk.TreeViewColumn ();
			filenameColumn.Title = Catalog.GetString("Filename");
			Gtk.CellRendererText filenameCell = new Gtk.CellRendererText ();
			filenameColumn.PackStart (filenameCell, true);
			
			
			
			filenameColumn.SetCellDataFunc (filenameCell, new Gtk.TreeCellDataFunc (RenderName));
			
			treeview.AppendColumn (filenameColumn);
			treeview.EnableGridLines = TreeViewGridLines.Horizontal;
			treeview.HeadersVisible = false;
		
		}
		
				
		private void RenderName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Project _project = (Project) model.GetValue (iter, 0);
			string _filePath = _project.File.FilePath;	
			string text;
			text = Catalog.GetString("File: ") + System.IO.Path.GetFileName(_filePath.ToString());
			text = text +"\n"+Catalog.GetString("Local Team: ") + _project.LocalName;
			text = text +"\n"+Catalog.GetString("Visitor Team: ") + _project.VisitorName;
			text = text +"\n"+Catalog.GetString("Result: ") + _project.LocalGoals+"-"+_project.VisitorGoals;
			text = text +"\n"+Catalog.GetString("Date: ") + _project.MatchDate.ToString(Catalog.GetString("MM/dd/yyyy"));
			
			(cell as Gtk.CellRendererText).Text = text;
			
			
		}
		
		
		
		
		public void Fill(ArrayList db){	
			dataFileListStore.Clear();
			db.Sort();
			
				
			foreach (Project _project in db){
				
				dataFileListStore.AppendValues(_project);
			}
			//dataFileListStore.Reorder();
		}
		
		public Project GetSelection(){
			TreePath path;
			TreeViewColumn col;
			treeview.GetCursor(out path,out col);
			return this.GetProject(path);
			
		}
		
		private Project GetProject(TreePath path){
			if (path != null){
				Gtk.TreeIter iter;
				dataFileListStore.GetIter (out iter, path);
 				Project project = (Project) dataFileListStore.GetValue (iter, 0);
				return project;
			}
			else return null;
			
		}


		protected virtual void OnTreeviewCursorChanged (object sender, System.EventArgs e)
		{
			TreeIter iter;
			this.treeview.Selection.GetSelected(out iter);
			Project selectedProject = (Project) dataFileListStore.GetValue (iter, 0);
			if (ProjectSelectedEvent!=null)
				ProjectSelectedEvent(selectedProject);
		}


	}
}
