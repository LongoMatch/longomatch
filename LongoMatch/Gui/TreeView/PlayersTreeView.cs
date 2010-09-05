//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//

using System;
using Gdk;
using Gtk;
using Mono.Unix;
using LongoMatch.Handlers;
using LongoMatch.TimeNodes;
using LongoMatch.Common;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayersTreeView : Gtk.TreeView
	{

		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event PlayListNodeAddedHandler PlayListNodeAdded;
		public event SnapshotSeriesHandler SnapshotSeriesEvent;

		private TreeIter selectedIter;
		private Menu menu;
		private MenuItem addPLN;
		private MenuItem snapshot;
		private Gtk.CellRendererText nameCell;
		private TreePath path;
		private Gtk.TreeViewColumn nameColumn;
		//Using TimeNode as in the tree there are Media and Sections timenodes
		private TimeNode selectedTimeNode;
		private bool editing;
		private bool projectIsLive;

		private Team team;


		public PlayersTreeView() {
			team = Team.LOCAL;
			this.RowActivated += new RowActivatedHandler(OnTreeviewRowActivated);

			SetMenu();
			ProjectIsLive = false;
			PlayListLoaded = false;

			nameColumn = new Gtk.TreeViewColumn();
			nameColumn.Title = "Name";
			nameCell = new Gtk.CellRendererText();
			nameCell.Edited += OnNameCellEdited;
			Gtk.CellRendererPixbuf miniatureCell = new Gtk.CellRendererPixbuf();
			nameColumn.PackStart(miniatureCell, true);
			nameColumn.PackEnd(nameCell, true);

			nameColumn.SetCellDataFunc(miniatureCell, new Gtk.TreeCellDataFunc(RenderMiniature));
			nameColumn.SetCellDataFunc(nameCell, new Gtk.TreeCellDataFunc(RenderName));

			AppendColumn(nameColumn);
		}

		public Team Team {
			set {
				team = value;
			}
			get {
				return team ;
			}
		}
		
		public bool ProjectIsLive{
			set{
				projectIsLive = value;
				addPLN.Visible = !projectIsLive;
				snapshot.Visible = !projectIsLive;
			}
		}

		public bool PlayListLoaded {
			set {
				addPLN.Sensitive = value;
			}
		}

		private void SetMenu() {
			MenuItem name;
			MenuItem delete; 

			menu = new Menu();

			name = new MenuItem(Catalog.GetString("Edit"));
			delete = new MenuItem(Catalog.GetString("Delete"));
			snapshot = new MenuItem(Catalog.GetString("Export to PGN images"));
			addPLN = new MenuItem(Catalog.GetString("Add to playlist"));
			addPLN.Sensitive=false;

			menu.Append(name);
			menu.Append(delete);
			menu.Append(addPLN);
			menu.Append(snapshot);

			delete.Activated += new EventHandler(OnDeleted);
			name.Activated += new EventHandler(OnEdit);
			addPLN.Activated += new EventHandler(OnAdded);
			snapshot.Activated += new EventHandler(OnSnapshot);
			menu.ShowAll();
		}

		protected override bool OnButtonPressEvent(EventButton evnt)
		{
			object selectedItem;

			//Call base class, to allow normal handling,
			//such as allowing the row to be selected by the right-click:
			bool returnValue = base.OnButtonPressEvent(evnt);

			//Then do our custom stuff:
			if ((evnt.Type == EventType.ButtonPress) && (evnt.Button == 3))
			{
				this.GetPathAtPos((int)evnt.X,(int)evnt.Y,out path);
				if (path!=null) {
					this.Model.GetIter(out selectedIter,path);
					selectedItem = this.Model.GetValue(selectedIter, 0);
					if (selectedItem is MediaTimeNode) {
						selectedTimeNode = selectedItem as MediaTimeNode;
						menu.Popup();
					}
					else {
						nameCell.Editable = true;
						this.SetCursor(path,  nameColumn, true);
					}
				}
			}
			return returnValue;
		}

		protected void OnDeleted(object obj, EventArgs args) {
			(Model as TreeStore).Remove(ref selectedIter);

			if (Team == Team.LOCAL)
				((MediaTimeNode) selectedTimeNode).RemoveLocalPlayer(int.Parse(path.ToString().Split(':')[0]));
			if (Team == Team.VISITOR)
				((MediaTimeNode) selectedTimeNode).RemoveVisitorPlayer(int.Parse(path.ToString().Split(':')[0]));
		}

		protected void OnSnapshot(object obj, EventArgs args) {
			if (SnapshotSeriesEvent != null)
				SnapshotSeriesEvent((MediaTimeNode)selectedTimeNode);
		}

		private void RenderMiniature(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			object item = model.GetValue(iter, 0);

			if (item is MediaTimeNode)
				(cell as Gtk.CellRendererPixbuf).Pixbuf = (item as MediaTimeNode).Miniature;

			if (item is Player)
				(cell as Gtk.CellRendererPixbuf).Pixbuf= (item as Player).Photo;
		}


		private void RenderName(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			object item = model.GetValue(iter, 0);

			if (item is MediaTimeNode) {
				MediaTimeNode tNode = item as MediaTimeNode;
				if (editing && selectedIter.Equals(iter))
					(cell as Gtk.CellRendererText).Markup = tNode.Name;
				else
					(cell as Gtk.CellRendererText).Markup = (tNode as MediaTimeNode).ToString();
			}

			else if (item is Player)
				(cell as Gtk.CellRendererText).Text = (item as Player).Name;
		}

		protected virtual void OnEdit(object obj, EventArgs args) {
			editing = true;
			nameCell.Editable = true;
			this.SetCursor(path,  nameColumn, true);
		}

		protected void OnAdded(object obj, EventArgs args) {
			if (PlayListNodeAdded != null)
				PlayListNodeAdded((MediaTimeNode)selectedTimeNode);
		}

		private void OnNameCellEdited(object o, Gtk.EditedArgs args)
		{
			Gtk.TreeIter iter;
			this.Model.GetIter(out iter, new Gtk.TreePath(args.Path));
			if (Model.GetValue(iter,0) is TimeNode) {
				TimeNode tNode = (TimeNode)this.Model.GetValue(iter,0);
				tNode.Name = args.NewText;
				nameCell.Editable=false;
				if (TimeNodeChanged != null)
					TimeNodeChanged(tNode,args.NewText);
			}
			else {
				Player player = (Player)this.Model.GetValue(iter,0);
				player.Name = args.NewText;
				nameCell.Editable=false;
			}
			editing = false;
		}


		protected virtual void OnTreeviewRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			Gtk.TreeIter iter;

			object item;
			this.Model.GetIter(out iter, args.Path);
			item = this.Model.GetValue(iter, 0);

			if (item is MediaTimeNode && TimeNodeSelected != null
			    && !projectIsLive)
				this.TimeNodeSelected(item as MediaTimeNode);
		}
		
		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			return false;
		}
	}
}
