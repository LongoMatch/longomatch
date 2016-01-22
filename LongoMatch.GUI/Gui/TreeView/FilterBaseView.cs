// 
//  Copyright (C) 2012 Andoni Morales Alastruey
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
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.Core;
using Misc = LongoMatch.Gui.Helpers.Misc;

namespace LongoMatch.Gui.Component
{

	public abstract class FilterTreeViewBase: TreeView
	{
		protected Menu playersMenu;
		protected Project project;
		protected string firstColumnName = "";
		protected TreeStore store;
		protected EventsFilter filter;

		public FilterTreeViewBase ()
		{
			PrepareTree ();
			CreateMenu ();
		}

		public virtual void SetFilter (EventsFilter filter, Project project)
		{
			this.project = project;
			this.filter = filter;
			FillTree ();
		}

		public new TreeModel Model {
			set {
				base.Model = value;
				store = value as TreeStore;
			}
			get {
				return base.Model;
			}
		}

		private void PrepareTree ()
		{
			TreeViewColumn nameColumn = new TreeViewColumn ();
			CellRendererText nameCell = new CellRendererText ();
			nameColumn.Title = Catalog.GetString (firstColumnName);
			nameColumn.PackStart (nameCell, true);
			nameColumn.SetCellDataFunc (nameCell, new TreeCellDataFunc (RenderColumn));
 
			TreeViewColumn filterColumn = new TreeViewColumn ();
			CellRendererToggle filterCell = new CellRendererToggle ();
			filterColumn.Title = Catalog.GetString ("Visible");
			filterCell.Toggled += HandleFilterCellToggled;
			filterColumn.PackStart (filterCell, true);
			filterColumn.AddAttribute (filterCell, "active", 1);

			AppendColumn (nameColumn);
			AppendColumn (filterColumn);
		}

		void CreateMenu ()
		{
			Gtk.Action select_all;
			Gtk.Action select_none;
			UIManager manager;
			ActionGroup g;

			manager = new UIManager ();
			g = new ActionGroup ("MenuGroup");

			select_all = new Gtk.Action ("AllAction", Mono.Unix.Catalog.GetString ("Select all"), null, "gtk-edit");
			select_none = new Gtk.Action ("NoneAction", Mono.Unix.Catalog.GetString ("Unselect all"), null, "gtk-edit");

			g.Add (select_all, null);
			g.Add (select_none, null);

			manager.InsertActionGroup (g, 0);

			manager.AddUiFromString ("<ui>" +
			"  <popup action='Menu'>" +
			"    <menuitem action='AllAction'/>" +
			"    <menuitem action='NoneAction'/>" +
			"  </popup>" +
			"</ui>");

			playersMenu = manager.GetWidget ("/Menu") as Menu;

			select_all.Activated += (sender, e) => Select (true);
			select_none.Activated += (sender, e) => Select (false);
		}

		protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
		{
			if (Misc.RightButtonClicked (evnt))
				playersMenu.Popup ();
			return base.OnButtonPressEvent (evnt);
		}

		protected void HandleFilterCellToggled (object o, ToggledArgs args)
		{
			Gtk.TreeIter iter;
			
			if (store.GetIterFromString (out iter, args.Path)) {
				bool active = !((bool)store.GetValue (iter, 1));
				UpdateSelection (iter, active);
			}
		}


		protected abstract void FillTree ();

		protected abstract void UpdateSelection (TreeIter iter, bool active);

		protected abstract void RenderColumn (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter);

		protected abstract void Select (bool select_all);

		protected override bool OnKeyPressEvent (EventKey evnt)
		{
			return false;
		}


	}
}

