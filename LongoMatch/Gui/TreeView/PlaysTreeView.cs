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
	public class PlaysTreeView : Gtk.TreeView
	{

		public event TimeNodeChangedHandler TimeNodeChanged;
		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event TimeNodeDeletedHandler TimeNodeDeleted;
		public event PlayListNodeAddedHandler PlayListNodeAdded;
		public event SnapshotSeriesHandler SnapshotSeriesEvent;
		public event PlayersTaggedHandler PlayersTagged;
		public event TagPlayHandler TagPlay;

		// Plays menu
		private Menu menu;
		private MenuItem local;
		private	MenuItem visitor;
		private MenuItem noTeam;
		private MenuItem tag;
		private MenuItem addPLN;
		private MenuItem deleteKeyFrame;
		private MenuItem snapshot;
		private MenuItem name;
		private MenuItem players;
		
		//Categories menu
		private Menu categoriesMenu;
		private RadioAction sortByName, sortByStart, sortByStop, sortByDuration;
		
		private Gtk.CellRendererText nameCell;
		private Gtk.TreeViewColumn nameColumn;
		private Color[] colors;
		private bool editing;


		public PlaysTreeView() {
			Selection.Mode = SelectionMode.Multiple;
			Selection.SelectFunction = SelectFunction;
			this.RowActivated += new RowActivatedHandler(OnTreeviewRowActivated);

			SetMenu();
			SetCategoriesMenu();

			colors = new Color[20];

			nameColumn = new Gtk.TreeViewColumn();
			nameColumn.Title = "Name";
			nameColumn.SortOrder = SortType.Ascending;
			nameCell = new Gtk.CellRendererText();
			nameCell.Edited += OnNameCellEdited;
			Gtk.CellRendererPixbuf miniatureCell = new Gtk.CellRendererPixbuf();
			nameColumn.PackStart(miniatureCell, true);
			nameColumn.PackEnd(nameCell, true);

			nameColumn.SetCellDataFunc(miniatureCell, new Gtk.TreeCellDataFunc(RenderMiniature));
			nameColumn.SetCellDataFunc(nameCell, new Gtk.TreeCellDataFunc(RenderName));

			this.AppendColumn(nameColumn);
		}
		
		public TreeStore Model{
			set{
				if (value != null){
					value.SetSortFunc(0, SortFunction);
					value.SetSortColumnId(0,SortType.Ascending);
				}
				base.Model = value;					
			}
			get{
				return (TreeStore)base.Model;
			}
		}

		public Color[]  Colors {
			set {
				this.colors = value;
			}
		}

		public bool PlayListLoaded {
			set {
				addPLN.Sensitive=value;
			}
		}

		private void SetMenu() {
			Menu teamMenu, playersMenu;
			MenuItem localPlayers, visitorPlayers;
			MenuItem team, quit;
			
			teamMenu = new Menu();
			local = new MenuItem(Catalog.GetString("Local Team"));
			visitor = new MenuItem(Catalog.GetString("Visitor Team"));
			noTeam = new MenuItem(Catalog.GetString("No Team"));
			teamMenu .Append(local);
			teamMenu .Append(visitor);
			teamMenu .Append(noTeam);

			playersMenu = new Menu();
			localPlayers = new MenuItem(Catalog.GetString("Local team"));
			visitorPlayers = new MenuItem(Catalog.GetString("Visitor team"));
			playersMenu.Append(localPlayers);
			playersMenu.Append(visitorPlayers);

			menu = new Menu();
			
			name = new MenuItem(Catalog.GetString("Edit"));
			team = new MenuItem(Catalog.GetString("Team Selection"));
			team.Submenu = teamMenu;
			tag = new MenuItem(Catalog.GetString("Add tag"));
			players = new MenuItem(Catalog.GetString("Tag player"));
			players.Submenu = playersMenu;
			quit = new MenuItem(Catalog.GetString("Delete"));
			deleteKeyFrame = new MenuItem(Catalog.GetString("Delete key frame"));
			addPLN = new MenuItem(Catalog.GetString("Add to playlist"));
			addPLN.Sensitive=false;
			snapshot = new MenuItem(Catalog.GetString("Export to PGN images"));

			menu.Append(name);
			menu.Append(tag);
			menu.Append(players);
			menu.Append(team);
			menu.Append(addPLN);
			menu.Append(quit);
			menu.Append(deleteKeyFrame);
			menu.Append(snapshot);

			name.Activated += OnEdit;
			tag.Activated += OnTag;
			local.Activated += OnTeamSelection;
			visitor.Activated += OnTeamSelection;
			noTeam.Activated += OnTeamSelection;
			localPlayers.Activated += OnLocalPlayers;
			visitorPlayers.Activated += OnVisitorPlayers;
			addPLN.Activated += OnAdded;
			quit.Activated += OnDeleted;
			deleteKeyFrame.Activated += OnDeleteKeyFrame;
			snapshot.Activated += OnSnapshot;
			menu.ShowAll();
		}

		private void SetCategoriesMenu(){
			Action edit;			
			UIManager manager;
			ActionGroup g;
			
			manager= new UIManager();
			g = new ActionGroup("CategoriesMenuGroup");
			
			edit = new Action("EditAction", Mono.Unix.Catalog.GetString("Edit name"), null, "gtk-edit");
			sortByName = new Gtk.RadioAction("SortByNameAction", Mono.Unix.Catalog.GetString("Sort by name"), null, null, 1);
			sortByStart = new Gtk.RadioAction("SortByStartAction", Mono.Unix.Catalog.GetString("Sort by start"), null, null, 2);
			sortByStop = new Gtk.RadioAction("SortByStopAction", Mono.Unix.Catalog.GetString("Sort by stop"), null, null, 3);
			sortByDuration = new Gtk.RadioAction("SortByDurationAction", Mono.Unix.Catalog.GetString("Sort by duration"), null, null, 3);
			
			edit.Activated += OnEdit;
			sortByName.Activated += OnSortActivated;
			sortByStart.Activated += OnSortActivated;
			sortByStop.Activated += OnSortActivated;
			sortByDuration.Activated += OnSortActivated;
			
			sortByName.Group = new GLib.SList(System.IntPtr.Zero);
			sortByStart.Group = sortByName.Group;
			sortByStop.Group = sortByName.Group;
			sortByDuration.Group = sortByName.Group;     
			
			
			g.Add(edit, null);
			g.Add(sortByName, null);
			g.Add(sortByStart, null);
			g.Add(sortByStop, null);
			g.Add(sortByDuration, null);
			
			manager.InsertActionGroup(g,0);
			
			manager.AddUiFromString("<ui>"+
			                        "  <popup action='CategoryMenu'>"+
			                        "    <menuitem action='EditAction'/>"+
			                        "    <menuitem action='SortByNameAction'/>"+
			                        "    <menuitem action='SortByStartAction'/>"+
			                        "    <menuitem action='SortByStopAction'/>"+
			                        "    <menuitem action='SortByDurationAction'/>"+
			                        "  </popup>"+
			                        "</ui>");
			
			categoriesMenu = manager.GetWidget("/CategoryMenu") as Menu;			
		}
		
		private void SetupSortMenu(SectionsTimeNode.SortMethod sortMethod){
			switch (sortMethod) {
				case SectionsTimeNode.SortMethod.BY_NAME:
					sortByName.Active = true;		
					break;					
				case SectionsTimeNode.SortMethod.BY_START_TIME:
					sortByStart.Active = true;
					break;
				case SectionsTimeNode.SortMethod.BY_STOP_TIME:
					sortByStop.Active = true;	
					break;
				default:
					sortByDuration.Active = true;
					break;
			}
		}
		
		private int GetSectionFromIter(TreeIter iter) {
			TreePath path = Model.GetPath(iter);
			return int.Parse(path.ToString().Split(':')[0]);
		}

		private TimeNode GetValueFromPath(TreePath path){
			Gtk.TreeIter iter;
			Model.GetIter(out iter, path);
			return (TimeNode)Model.GetValue(iter,0);					
		}	
		
		private void MultiSelectMenu (bool enabled){
			name.Sensitive = !enabled;
			snapshot.Sensitive = !enabled;
			players.Sensitive = !enabled;
			tag.Sensitive = !enabled;
		}
		
		private int SortFunction(TreeModel model, TreeIter a, TreeIter b){
			TreeStore store;
			TimeNode tna, tnb;
			TreeIter parent;
			int depth;
			SectionsTimeNode category;
			
			if (model == null)
				return 0;	
			
			store = model as TreeStore;
			
			// Retrieve the iter parent and its depth
			// When a new play is inserted, one of the iters is not a valid
			// in the model. Get the values from the valid one
			if (store.IterIsValid(a)){
				store.IterParent(out parent, a);
				depth = store.IterDepth(a);
			}
			else{
				store.IterParent(out parent, b);
				depth = store.IterDepth(b);
			}		
			
			// Dont't store categories
			if (depth == 0)
				return int.Parse(model.GetPath(a).ToString()) 
					- int.Parse(model.GetPath(b).ToString());
			
			category = model.GetValue(parent,0) as SectionsTimeNode;
			tna = model.GetValue (a, 0)as TimeNode;
			tnb = model.GetValue (b, 0) as TimeNode;
			
			switch(category.SortingMethod){
				case(SectionsTimeNode.SortMethod.BY_NAME):
					return String.Compare(tna.Name, tnb.Name);
				case(SectionsTimeNode.SortMethod.BY_START_TIME):
					return (tna.Start - tnb.Start).MSeconds;
				case(SectionsTimeNode.SortMethod.BY_STOP_TIME):
					return (tna.Stop - tnb.Stop).MSeconds;
				case(SectionsTimeNode.SortMethod.BY_DURATION):
					return (tna.Duration - tnb.Duration).MSeconds;
				default:
					return 0;
			}			
		}
		
		private bool SelectFunction(TreeSelection selection, TreeModel model, TreePath path, bool selected){
			// Don't allow multiselect for categories
			if (!selected && selection.GetSelectedRows().Length > 0){
				if (selection.GetSelectedRows().Length == 1 &&
				    GetValueFromPath(selection.GetSelectedRows()[0]) is SectionsTimeNode)
					return false;	
				return !(GetValueFromPath(path) is SectionsTimeNode);										
			}
			// Always unselect
			else
				return true;
		}
		
		private void RenderMiniature(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			TimeNode tNode = (TimeNode) model.GetValue(iter, 0);
			if (tNode is MediaTimeNode) {
				(cell as Gtk.CellRendererPixbuf).Pixbuf = ((MediaTimeNode)tNode).Miniature;
				(cell as Gtk.CellRendererPixbuf).CellBackgroundGdk = colors[GetSectionFromIter(iter)];
			}
			else {
				(cell as Gtk.CellRendererPixbuf).Pixbuf = null;
				(cell as Gtk.CellRendererPixbuf).CellBackground = "white";
			}
		}

		private void RenderName(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			TimeNode tNode = (TimeNode) model.GetValue(iter, 0);

			//Handle special case in which we replace the text in the cell by the name of the TimeNode
			//We need to check if we are editing and only change it for the path that's currently beeing edited

			if (editing && Selection.IterIsSelected(iter))
				(cell as Gtk.CellRendererText).Markup = tNode.Name;
			else if (tNode is MediaTimeNode) {
				(cell as Gtk.CellRendererText).BackgroundGdk = colors[GetSectionFromIter(iter)];
				(cell as Gtk.CellRendererText).CellBackgroundGdk = colors[GetSectionFromIter(iter)];
				(cell as Gtk.CellRendererText).Markup = (tNode as MediaTimeNode).ToString();
			}
			else {
				(cell as Gtk.CellRendererText).Background = "white";
				(cell as Gtk.CellRendererText).CellBackground = "white";
				(cell as Gtk.CellRendererText).Markup =tNode.Name;
			}
		}	

		protected virtual void OnTreeviewRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			Gtk.TreeIter iter;
			this.Model.GetIter(out iter, args.Path);
			TimeNode tNode = (TimeNode)this.Model.GetValue(iter, 0);

			if (tNode is MediaTimeNode && TimeNodeSelected != null)
				this.TimeNodeSelected((MediaTimeNode)tNode);
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
					TimeNode selectedTimeNode = GetValueFromPath(paths[0]);
					if (selectedTimeNode is MediaTimeNode) {
						deleteKeyFrame.Sensitive = (selectedTimeNode as MediaTimeNode).KeyFrameDrawing != null;
						MultiSelectMenu(false);
						menu.Popup();
					}
					else{
						SetupSortMenu((selectedTimeNode as SectionsTimeNode).SortingMethod);
						categoriesMenu.Popup();
					}
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
		
		private void OnSortActivated (object o, EventArgs args){
			SectionsTimeNode category;
			RadioAction sender;
			
			sender = o as RadioAction;
			category = GetValueFromPath(Selection.GetSelectedRows()[0]) as SectionsTimeNode;
			
			if (sender == sortByName)
				category.SortingMethod = SectionsTimeNode.SortMethod.BY_NAME;
			else if (sender == sortByStart)
				category.SortingMethod = SectionsTimeNode.SortMethod.BY_START_TIME;
			else if (sender == sortByStop)
				category.SortingMethod = SectionsTimeNode.SortMethod.BY_STOP_TIME;
			else 
				category.SortingMethod = SectionsTimeNode.SortMethod.BY_DURATION;
			// Redorder plays
			Model.SetSortFunc(0, SortFunction);
		}
		
		private void OnNameCellEdited(object o, Gtk.EditedArgs args)
		{
			Gtk.TreeIter iter;
			TimeNode tNode;
			
			Model.GetIter(out iter, new Gtk.TreePath(args.Path));
			tNode = (TimeNode)this.Model.GetValue(iter,0);
			tNode.Name = args.NewText;
			editing = false;
			nameCell.Editable=false;
			if (TimeNodeChanged != null)
				TimeNodeChanged(tNode,args.NewText);
			
			// Redorder plays
			Model.SetSortFunc(0, SortFunction);
		}

		protected void OnDeleted(object obj, EventArgs args) {
			if (TimeNodeDeleted == null)
				return;
			List<MediaTimeNode> list = new List<MediaTimeNode>();
			TreePath[] paths = Selection.GetSelectedRows();
			for (int i=0; i<paths.Length; i++){	
				list.Add((MediaTimeNode)GetValueFromPath(paths[i]));
			}
			// When a TimeNode is deleted from the tree the path changes.
			// We need first to retrieve all the TimeNodes to delete using the 
			// current path of each one and then send the TimeNodeDeleted event
			for (int i=0; i<paths.Length; i++){	
				TimeNodeDeleted(list[i], int.Parse(paths[i].ToString().Split(':')[0]));
			}			
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
					MediaTimeNode tNode = (MediaTimeNode)GetValueFromPath(paths[i]);
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

		protected void OnTeamSelection(object obj, EventArgs args) {
			MenuItem sender = (MenuItem)obj;
			Team team = Team.NONE;
			if (sender == local)
				team = Team.LOCAL;
			else if (sender == visitor)
				team = Team.VISITOR;
			else if (sender == noTeam)
				team = Team.NONE;
			
			TreePath[] paths = Selection.GetSelectedRows();
			for (int i=0; i<paths.Length; i++){	
					MediaTimeNode tNode = (MediaTimeNode)GetValueFromPath(paths[i]);
					tNode.Team = team;
			}
		}

		protected void OnAdded(object obj, EventArgs args) {
			if (PlayListNodeAdded != null){
				TreePath[] paths = Selection.GetSelectedRows();
				for (int i=0; i<paths.Length; i++){	
					MediaTimeNode tNode = (MediaTimeNode)GetValueFromPath(paths[i]);
					PlayListNodeAdded(tNode);
				}
			}
		}
		
		protected void OnTag (object obj, EventArgs args){
			if (TagPlay != null)
				TagPlay((MediaTimeNode)GetValueFromPath(Selection.GetSelectedRows()[0]));
		}

		protected void OnSnapshot(object obj, EventArgs args) {
			if (SnapshotSeriesEvent != null)
				SnapshotSeriesEvent((MediaTimeNode)GetValueFromPath(Selection.GetSelectedRows()[0]));
		}

		protected virtual void OnLocalPlayers(object o, EventArgs args) {
			if (PlayersTagged != null)
				PlayersTagged((MediaTimeNode)GetValueFromPath(Selection.GetSelectedRows()[0]), Team.LOCAL);
		}

		protected virtual void OnVisitorPlayers(object o, EventArgs args) {
			if (PlayersTagged != null)
				PlayersTagged((MediaTimeNode)GetValueFromPath(Selection.GetSelectedRows()[0]), Team.VISITOR);
		}
		
	}
}
