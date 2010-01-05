// TreeWidgetPopup.cs
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
using Mono.Unix;
using LongoMatch.Handlers;
using LongoMatch.TimeNodes;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public class TagsTreeView : Gtk.TreeView
	{

		public event TimeNodeChangedHandler TimeNodeChanged;
		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event PlayListNodeAddedHandler PlayListNodeAdded;
		public event SnapshotSeriesHandler SnapshotSeriesEvent;

		private Menu menu;
		private MenuItem addPLN;
		private MenuItem name;
		private MenuItem snapshot;
		private MenuItem deleteKeyFrame;
		
		private Gtk.CellRendererText nameCell;
		private Gtk.TreeViewColumn nameColumn;
		private bool editing;
		

		public TagsTreeView() {			
			Selection.Mode = SelectionMode.Multiple;			
			RowActivated += new RowActivatedHandler(OnTreeviewRowActivated);
	
			SetMenu();

			nameColumn = new Gtk.TreeViewColumn();
			nameColumn.Title = "Tag";
			nameCell = new Gtk.CellRendererText();
			nameCell.Edited += OnNameCellEdited;
			Gtk.CellRendererPixbuf miniatureCell = new Gtk.CellRendererPixbuf();
			nameColumn.PackStart(miniatureCell, true);
			nameColumn.PackEnd(nameCell, true);

			nameColumn.SetCellDataFunc(miniatureCell, new Gtk.TreeCellDataFunc(RenderMiniature));
			nameColumn.SetCellDataFunc(nameCell, new Gtk.TreeCellDataFunc(RenderName));

			AppendColumn(nameColumn);
		}

		public bool PlayListLoaded {
			set {
				addPLN.Sensitive=value;
			}
		}		

		private void SetMenu() {
			menu = new Menu();

			name = new MenuItem(Catalog.GetString("Edit"));
			deleteKeyFrame = new MenuItem(Catalog.GetString("Delete key frame"));
			snapshot = new MenuItem(Catalog.GetString("Export to PGN images"));
			addPLN = new MenuItem(Catalog.GetString("Add to playlist"));
			addPLN.Sensitive=false;

			menu.Append(name);
			menu.Append(deleteKeyFrame);
			menu.Append(addPLN);
			menu.Append(snapshot);

			name.Activated += new EventHandler(OnEdit);
			deleteKeyFrame.Activated += OnDeleteKeyFrame;
			addPLN.Activated += new EventHandler(OnAdded);
			snapshot.Activated += new EventHandler(OnSnapshot);
			menu.ShowAll();
		}

		private MediaTimeNode GetValueFromPath(TreePath path){
			Gtk.TreeIter iter;
			Model.GetIter(out iter, path);
			return Model.GetValue(iter,0) as MediaTimeNode;					
		}	
		
		private void MultiSelectMenu (bool enabled){
			name.Sensitive = !enabled;
			snapshot.Sensitive = !enabled;
		}
		
		private void RenderMiniature(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			MediaTimeNode tNode = model.GetValue(iter, 0) as MediaTimeNode;
			(cell as Gtk.CellRendererPixbuf).Pixbuf = tNode.Miniature;
		}

		private void RenderName(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			MediaTimeNode tNode = (MediaTimeNode) model.GetValue(iter, 0);

			//Handle special case in which we replace the text in the cell by the name of the TimeNode
			//We need to check if we are editing and only change it for the path that's currently beeing edited

			if (editing && Selection.IterIsSelected(iter))
				(cell as Gtk.CellRendererText).Markup = tNode.Name;
			else 
				(cell as Gtk.CellRendererText).Markup = tNode.ToString();
		}

		private void OnNameCellEdited(object o, Gtk.EditedArgs args)
		{
			Gtk.TreeIter iter;
			Model.GetIter(out iter, new Gtk.TreePath(args.Path));
			MediaTimeNode tNode = Model.GetValue(iter,0) as MediaTimeNode;
			tNode.Name = args.NewText;
			editing = false;
			nameCell.Editable=false;
			if (TimeNodeChanged != null)
				TimeNodeChanged(tNode,args.NewText);
		}
		
		protected virtual void OnTreeviewRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			Gtk.TreeIter iter;
			Model.GetIter(out iter, args.Path);
			MediaTimeNode tNode = Model.GetValue(iter, 0) as MediaTimeNode;

			if (TimeNodeSelected != null)
				TimeNodeSelected(tNode);
		}

		protected override bool OnButtonPressEvent(EventButton evnt)
		{			
			TreePath[] paths = Selection.GetSelectedRows();
			
			if ((evnt.Type == EventType.ButtonPress) && (evnt.Button == 3))
			{
				// We don't want to unselect the play when several
				// plays are selected and we clik the right button
				// For multiedition
				if (paths.Length <= 1){
					base.OnButtonPressEvent(evnt);
					paths = Selection.GetSelectedRows();
				}
				
				if (paths.Length == 1) {
					MediaTimeNode selectedTimeNode = GetValueFromPath(paths[0]);
					deleteKeyFrame.Sensitive = selectedTimeNode.KeyFrameDrawing != null;
					MultiSelectMenu(false);
					menu.Popup();
				}
				else if (paths.Length > 1){
					MultiSelectMenu(true);
					menu.Popup();								
				}
			}
			else 
				base.OnButtonPressEvent(evnt);
			return true;
		}		
		
		protected void OnDeleteKeyFrame(object obj, EventArgs args) {
			MessageDialog md = new MessageDialog((Gtk.Window)Toplevel,
			                                     DialogFlags.Modal,
			                                     MessageType.Question,
			                                     ButtonsType.YesNo,
			                                     false,
			                                     Catalog.GetString("Do you want to delete the key frame for this play?")
			                                    );
			if (md.Run() == (int)ResponseType.Yes){
				TreePath[] paths = Selection.GetSelectedRows();
				for (int i=0; i<paths.Length; i++){	
					MediaTimeNode tNode = GetValueFromPath(paths[i]);
					tNode.KeyFrameDrawing = null;
				}
				// Refresh the thumbnails
				QueueDraw();
			}
			md.Destroy();
		}

		protected virtual void OnEdit(object obj, EventArgs args) {
			TreePath[] paths = Selection.GetSelectedRows();
			editing = true;
			nameCell.Editable = true;
			nameCell.Markup = GetValueFromPath(paths[0]).Name;
			SetCursor(paths[0],  nameColumn, true);
		}
		
		protected void OnAdded(object obj, EventArgs args) {
			if (PlayListNodeAdded != null){
				TreePath[] paths = Selection.GetSelectedRows();
				for (int i=0; i<paths.Length; i++){	
					MediaTimeNode tNode = GetValueFromPath(paths[i]);
					PlayListNodeAdded(tNode);
				}
			}
		}
		
		protected void OnSnapshot(object obj, EventArgs args) {
			if (SnapshotSeriesEvent != null)
				SnapshotSeriesEvent(GetValueFromPath(Selection.GetSelectedRows()[0]));
		}			
	}
}

