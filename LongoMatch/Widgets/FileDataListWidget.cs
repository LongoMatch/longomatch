// FileDataListWidget.cs
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



namespace LongoMatch.Widgets.Component
{
	
	public delegate void FileDataSelectedHandler (FileData fData);
	
	public partial class FileDataListWidget : Gtk.Bin
	{

		private Gtk.ListStore dataFileListStore;
		public event         FileDataSelectedHandler FileDataSelectedEvent;
		
		public FileDataListWidget()
		{
			this.Build();
			dataFileListStore = new Gtk.ListStore (typeof (FileData));
			treeview.Model=dataFileListStore;
			
			Gtk.TreeViewColumn filenameColumn = new Gtk.TreeViewColumn ();
			filenameColumn.Title = Catalog.GetString("Filename");
			Gtk.CellRendererText filenameCell = new Gtk.CellRendererText ();
			filenameColumn.PackStart (filenameCell, true);
			
			Gtk.TreeViewColumn dateColumn = new Gtk.TreeViewColumn ();
			dateColumn.Title = Catalog.GetString("Date");
			Gtk.CellRendererText dateCell = new Gtk.CellRendererText ();
			dateColumn.PackStart (dateCell, true);
 
			Gtk.TreeViewColumn localNameColumn = new Gtk.TreeViewColumn ();
			localNameColumn.Title = Catalog.GetString("Local Team");
			Gtk.CellRendererText localNameCell = new Gtk.CellRendererText ();
			localNameColumn.PackStart (localNameCell, true);
			
			Gtk.TreeViewColumn visitorNameColumn = new Gtk.TreeViewColumn ();
			visitorNameColumn.Title = Catalog.GetString("Visitor Team");
			Gtk.CellRendererText visitorNameCell = new Gtk.CellRendererText ();
			visitorNameColumn.PackStart (visitorNameCell, true);
			
			Gtk.TreeViewColumn resultColumn = new Gtk.TreeViewColumn ();
			resultColumn.Title = Catalog.GetString("Result");
			Gtk.CellRendererText resultCell = new Gtk.CellRendererText ();
			resultColumn.PackStart (resultCell, true);
			
			filenameColumn.SetCellDataFunc (filenameCell, new Gtk.TreeCellDataFunc (RenderName));
			dateColumn.SetCellDataFunc (dateCell, new Gtk.TreeCellDataFunc (RenderDate));
			localNameColumn.SetCellDataFunc (localNameCell, new Gtk.TreeCellDataFunc (RenderLocalName));
			visitorNameColumn.SetCellDataFunc (visitorNameCell, new Gtk.TreeCellDataFunc (RenderVisitorName));
			resultColumn.SetCellDataFunc (resultCell, new Gtk.TreeCellDataFunc (RenderResult));
			
			treeview.AppendColumn (filenameColumn);
			treeview.AppendColumn (dateColumn);
			treeview.AppendColumn (localNameColumn);
			treeview.AppendColumn (visitorNameColumn);
			treeview.AppendColumn (resultColumn);
		}
		
				
		private void RenderName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			FileData _fData = (FileData) model.GetValue (iter, 0);
			string _filePath = _fData.File.FilePath;	
			(cell as Gtk.CellRendererText).Text = System.IO.Path.GetFileName(_filePath.ToString());
			
			
		}
		
		private void RenderDate (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			FileData _fData = (FileData) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = _fData.MatchDate.ToString(Catalog.GetString("MM/dd/yyyy"));;
			
			
		}
 
		
		private void RenderLocalName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			FileData _fData = (FileData) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = _fData.LocalName;
				
			
		}
		
		private void RenderVisitorName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			FileData _fData = (FileData) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = _fData.VisitorName;

		}
		
		private void RenderResult (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			FileData _fData = (FileData) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = _fData.LocalGoals+"-"+_fData.VisitorGoals;;
		}
		
		
		public void Fill(ArrayList db){	
			dataFileListStore.Clear();
			db.Sort();
			
				
			foreach (FileData _fData in db){
				
				dataFileListStore.AppendValues(_fData);
			}
			//dataFileListStore.Reorder();
		}
		
		public FileData GetSelection(){
			TreePath path;
			TreeViewColumn col;
			treeview.GetCursor(out path,out col);
			return this.GetFileData(path);
			
		}
		
		private FileData GetFileData(TreePath path){
			if (path != null){
				Gtk.TreeIter iter;
				dataFileListStore.GetIter (out iter, path);
 				FileData fData = (FileData) dataFileListStore.GetValue (iter, 0);
				return fData;
			}
			else return null;
			
		}


		protected virtual void OnTreeviewCursorChanged (object sender, System.EventArgs e)
		{
			TreeIter iter;
			this.treeview.Selection.GetSelected(out iter);
			FileData selectedFileData = (FileData) dataFileListStore.GetValue (iter, 0);
			if (FileDataSelectedEvent!=null)
				FileDataSelectedEvent(selectedFileData);
		}


	}
}
