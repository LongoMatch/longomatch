// 
//  Copyright (C) 2009 andoni
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 

using System;
using Gdk;
using Gtk;
using Mono.Unix;
using LongoMatch.Handlers;
using LongoMatch.TimeNodes;

namespace LongoMatch.Gui.Component
{
	
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayersTreeView : Gtk.TreeView
	{
		
		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event SnapshotSeriesHandler SnapshotSeriesEvent;

		private TreeIter selectedIter;
		private Menu menu;
		private Gtk.CellRendererText nameCell;
		private TreePath path;
		private Gtk.TreeViewColumn nameColumn;
		//Using TimeNode as in the tree there are Media and Sections timenodes
		private TimeNode selectedTimeNode;
		
		private Team team;

		
		public PlayersTreeView(){
			
			team = Team.LOCAL;
			this.RowActivated += new RowActivatedHandler(OnTreeviewRowActivated);
						
			SetMenu();				
			
			nameColumn = new Gtk.TreeViewColumn ();
			nameColumn.Title = "Name";
			nameCell = new Gtk.CellRendererText ();
			nameCell.Edited += OnNameCellEdited;
			Gtk.CellRendererPixbuf miniatureCell = new Gtk.CellRendererPixbuf ();
			nameColumn.PackStart (miniatureCell, true);
			nameColumn.PackEnd (nameCell, true);
			

			Gtk.TreeViewColumn startTimeColumn = new Gtk.TreeViewColumn ();
			startTimeColumn.Title = "Start";
			Gtk.CellRendererText startTimeCell = new Gtk.CellRendererText ();
			startTimeColumn.PackStart (startTimeCell, true);
			
			Gtk.TreeViewColumn stopTimeColumn = new Gtk.TreeViewColumn ();
			stopTimeColumn.Title = "Stop";
			Gtk.CellRendererText stopTimeCell = new Gtk.CellRendererText ();
			stopTimeColumn.PackStart (stopTimeCell, true);

			nameColumn.SetCellDataFunc (miniatureCell, new Gtk.TreeCellDataFunc(RenderMiniature));
			nameColumn.SetCellDataFunc (nameCell, new Gtk.TreeCellDataFunc (RenderName));
			startTimeColumn.SetCellDataFunc (startTimeCell, new Gtk.TreeCellDataFunc (RenderStartTime));
			stopTimeColumn.SetCellDataFunc (stopTimeCell, new Gtk.TreeCellDataFunc (RenderStopTime));
			
			
			this.AppendColumn (nameColumn);
			this.AppendColumn (startTimeColumn);
			this.AppendColumn (stopTimeColumn);
		
		}
		
		public Team Team{
			set {team = value;}
			get {return team ;}
		}
	
		
		private void SetMenu(){
		
			menu = new Menu();
			
			MenuItem name = new MenuItem(Catalog.GetString("Edit"));
			MenuItem delete = new MenuItem(Catalog.GetString("Delete"));
			MenuItem snapshot = new MenuItem(Catalog.GetString("Export to PGN images"));
			menu.Append(name);
			menu.Append(delete);
			menu.Append(snapshot);
			 
			delete.Activated += new EventHandler(OnDeleted);
			name.Activated += new EventHandler(OnEdit);
			snapshot.Activated += new EventHandler(OnSnapshot);
			menu.ShowAll();		
		}

		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			object selectedItem;

			//Call base class, to allow normal handling,
			//such as allowing the row to be selected by the right-click:
			bool returnValue = base.OnButtonPressEvent(evnt);
			
			//Then do our custom stuff:
			if( (evnt.Type == EventType.ButtonPress) && (evnt.Button == 3) )
			{				
				this.GetPathAtPos((int)evnt.X,(int)evnt.Y,out path);
				if (path!=null){
					this.Model.GetIter (out selectedIter,path); 
					selectedItem = this.Model.GetValue (selectedIter, 0);
					if (selectedItem is MediaTimeNode ){
						selectedTimeNode = selectedItem as MediaTimeNode;
					    menu.Popup();
					}
					else{
						nameCell.Editable = true;
						this.SetCursor(path,  nameColumn, true);
					}
				}
			}
			return returnValue;								
		}
		
		protected void OnDeleted(object obj, EventArgs args){	
			(Model as TreeStore).Remove(ref selectedIter);
			
			if (Team == Team.LOCAL)
				((MediaTimeNode) selectedTimeNode).RemoveLocalPlayer(int.Parse(path.ToString().Split(':')[0]));
			if (Team == Team.VISITOR)
				((MediaTimeNode) selectedTimeNode).RemoveVisitorPlayer(int.Parse(path.ToString().Split(':')[0]));
		}	
		
		protected void OnSnapshot(object obj, EventArgs args){
			if (SnapshotSeriesEvent != null)
				SnapshotSeriesEvent((MediaTimeNode)selectedTimeNode);			
		}
		
		private void RenderMiniature (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			object item = model.GetValue (iter, 0);			
 			
			if (item is MediaTimeNode)
 				(cell as Gtk.CellRendererPixbuf).Pixbuf = (item as MediaTimeNode).Miniature;
			
			if (item is Player)
				(cell as Gtk.CellRendererPixbuf).Pixbuf= (item as Player).Photo;			
		}

		
		private void RenderName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			object item = model.GetValue (iter, 0);			
 			
			if (item is MediaTimeNode)
 				(cell as Gtk.CellRendererText).Text = (item as MediaTimeNode).Name;
			
			else if (item is Player)
				(cell as Gtk.CellRendererText).Text = (item as Player).Name;
		}
 
		
		private void RenderStartTime (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{			
			object item = model.GetValue (iter, 0);			
 			
			if (item is MediaTimeNode)
 				(cell as Gtk.CellRendererText).Text = (item as MediaTimeNode).Start.ToMSecondsString();
			else if (item is Player)
 				(cell as Gtk.CellRendererText).Text = "";
		}
		
		private void RenderStopTime (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			object item = model.GetValue (iter, 0);			
 			
			if (item is MediaTimeNode)
 				(cell as Gtk.CellRendererText).Text = (item as MediaTimeNode).Stop.ToMSecondsString();
			else if (item is Player)
 				(cell as Gtk.CellRendererText).Text = "";
		}
		
		protected virtual void OnEdit(object obj, EventArgs args){
			nameCell.Editable = true;
			this.SetCursor(path,  nameColumn, true);
		}
		
		private void OnNameCellEdited (object o, Gtk.EditedArgs args)
		{
			Gtk.TreeIter iter;
			this.Model.GetIter (out iter, new Gtk.TreePath (args.Path)); 
			if (Model.GetValue (iter,0) is TimeNode){
				TimeNode tNode = (TimeNode)this.Model.GetValue (iter,0);
				tNode.Name = args.NewText;
				nameCell.Editable=false;
				if (TimeNodeChanged != null)
					TimeNodeChanged(tNode,args.NewText);
			}
			else{
				Player player = (Player)this.Model.GetValue (iter,0);
				player.Name = args.NewText;
				nameCell.Editable=false;				
			}
		}
			
		
		protected virtual void OnTreeviewRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			Gtk.TreeIter iter;
			
			object item;
			this.Model.GetIter (out iter, args.Path);
			item = this.Model.GetValue (iter, 0);
			
			if (item is MediaTimeNode && TimeNodeSelected != null)
				this.TimeNodeSelected(item as MediaTimeNode);	
		}
	}
}
