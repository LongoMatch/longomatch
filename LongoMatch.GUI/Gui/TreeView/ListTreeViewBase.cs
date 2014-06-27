//
//  Copyright (C) 2010 Andoni Morales Alastruey
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
using System.Linq;
using System.Collections.Generic;
using Gdk;
using Gtk;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Gui.Helpers;
using Image = LongoMatch.Common.Image;
using Color = Gdk.Color;
using LongoMatch.Gui.Menus;

namespace LongoMatch.Gui.Component
{


	public abstract class ListTreeViewBase:TreeView
	{
		protected Gtk.CellRendererText nameCell;
		protected Gtk.TreeViewColumn nameColumn;
		protected bool editing;
		protected bool enableCategoryMove = false;
		protected PlaysMenu playsMenu;
		
		TreeModelFilter modelFilter;
		PlaysFilter filter;
		Dictionary<MenuItem, Category> catsDict;

		public event EventHandler NewRenderingJob;

		public ListTreeViewBase()
		{
			Selection.Mode = SelectionMode.Multiple;
			Selection.SelectFunction = SelectFunction;
			RowActivated += new RowActivatedHandler(OnTreeviewRowActivated);
			HeadersVisible = false;

			nameColumn = new Gtk.TreeViewColumn();
			nameColumn.Title = "Name";
			nameCell = new Gtk.CellRendererText();
			nameCell.Edited += OnNameCellEdited;
			Gtk.CellRendererPixbuf miniatureCell = new Gtk.CellRendererPixbuf();
			nameColumn.PackStart(nameCell, true);
			nameColumn.PackEnd(miniatureCell, true);

			nameColumn.SetCellDataFunc(miniatureCell, new Gtk.TreeCellDataFunc(RenderMiniature));
			nameColumn.SetCellDataFunc(nameCell, new Gtk.TreeCellDataFunc(RenderName));

			playsMenu = new PlaysMenu ();
			playsMenu.EditNameEvent += OnEdit;
			AppendColumn(nameColumn);

		}

		public bool Colors {
			get;
			set;
		}

		public PlaysFilter Filter {
			set {
				filter = value;
				filter.FilterUpdated += OnFilterUpdated;
				Refilter();
			}
			get {
				return filter;
			}
		}
		
		public void Refilter() {
			if (modelFilter != null)
				modelFilter.Refilter();
		}

		public Project Project {
			set;
			protected get;
		}
		
		new public TreeStore Model {
			set {
				if(value != null) {
					modelFilter = new TreeModelFilter (value, null);
					modelFilter.VisibleFunc = new TreeModelFilterVisibleFunc (FilterFunction);
					value.SetSortFunc(0, SortFunction);
					value.SetSortColumnId(0,SortType.Ascending);
					// Assign the filter as our tree's model
					base.Model = modelFilter;
				} else {
					base.Model = null;
				}
			}
			get {
				return (base.Model as TreeModelFilter).ChildModel as TreeStore;
			}
		}

		protected Play SelectedPlay {
			get {
				return GetValueFromPath(Selection.GetSelectedRows()[0]) as Play;
			}
		}
		
		protected List<Play> SelectedPlays {
			get {
				return Selection.GetSelectedRows().Select (
					p => GetValueFromPath(p) as Play).ToList ();
			}
		}
		
		protected void ShowMenu () {
			playsMenu.ShowListMenu (SelectedPlays, Project.Description.File,
			                        Project.Categories.List);
		}

		protected object GetValueFromPath(TreePath path) {
			Gtk.TreeIter iter;
			modelFilter.GetIter(out iter, path);
			return modelFilter.GetValue(iter,0);
		}
		
		protected void RenderMiniature(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			var item = model.GetValue(iter, 0);
			var c = cell as CellRendererPixbuf;

			if(item is Play) {
				Image img = (item as Play).Miniature;
				c.Pixbuf = img != null ? img.Value : null;
				if(Colors) {
					c.CellBackgroundGdk = Helpers.Misc.ToGdkColor((item as Play).Category.Color);
				} else {
					c.CellBackground = "white";
				}
			}
			else if(item is Player) {
				Image img = (item as Player).Photo;
				c.Pixbuf = img != null ? img.Value : null;
				c.CellBackground = "white";
			}
			else {
				c.Pixbuf = null;
				c.CellBackground = "white";
			}
		}

		protected void RenderName(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			object o = model.GetValue(iter, 0);
			var c = cell as CellRendererText;

			/* Handle special case in which we replace the text in the cell by the name of the TimeNode
			 * We need to check if we are editing and only change it for the path that's currently beeing edited */
			if(editing && Selection.IterIsSelected(iter)) {
				if(o is Player)
					c.Markup = GLib.Markup.EscapeText ((o as Player).Name);
				else
					c.Markup = GLib.Markup.EscapeText ((o as TimeNode).Name);
				return;
			}

			if(o is Play) {
				var mtn = o as Play;
				if(Colors) {
					Color col = Helpers.Misc.ToGdkColor(mtn.Category.Color);
					c.CellBackgroundGdk = col;
					c.BackgroundGdk = col;
				} else {
					c.Background = "white";
					c.CellBackground = "white";
				}
				c.Markup = GLib.Markup.EscapeText (mtn.ToString());
			} else if(o is Player) {
				c.Background = "white";
				c.CellBackground = "white";
				c.Markup = String.Format("{0} ({1})", GLib.Markup.EscapeText ((o as Player).ToString()),
				                         modelFilter.IterNChildren(iter));
			} else if(o is Category) {
				c.Background = "white";
				c.CellBackground = "white";
				c.Markup = String.Format("{0} ({1})", GLib.Markup.EscapeText ((o as TimeNode).Name),
				                         modelFilter.IterNChildren(iter));
			}
		}

		protected bool FilterFunction(TreeModel model, TreeIter iter) {
			if (Filter == null)
				return true;
			object o = model.GetValue(iter, 0);
			return Filter.IsVisible(o);
		}	
		
		protected virtual void OnNameCellEdited(object o, Gtk.EditedArgs args)
		{
			Gtk.TreeIter iter;
			object item;

			modelFilter.GetIter(out iter, new Gtk.TreePath(args.Path));
			item = modelFilter.GetValue(iter,0);

			if(item is TimeNode) {
				(item as TimeNode).Name = args.NewText;
				Config.EventsBroker.EmitTimeNodeChanged (
					(item as TimeNode), args.NewText);
			} else if(item is Player) {
				(item as Player).Name = args.NewText;
			}
			editing = false;
			nameCell.Editable=false;

		}

		protected virtual void OnTreeviewRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			Gtk.TreeIter iter;
			modelFilter.GetIter(out iter, args.Path);
			object item = modelFilter.GetValue(iter, 0);

			if(!(item is Play))
				return;

			Config.EventsBroker.EmitPlaySelected (item as Play);
		}

		protected virtual void OnEdit (object obj, EventArgs args) {
			TreePath[] paths = Selection.GetSelectedRows();

			editing = true;
			nameCell.Editable = true;
			SetCursor(paths[0],  nameColumn, true);
		}

		protected void OnFilterUpdated() {
			modelFilter.Refilter();
		}
		
		protected abstract bool SelectFunction(TreeSelection selection, TreeModel model, TreePath path, bool selected);
		protected abstract int SortFunction(TreeModel model, TreeIter a, TreeIter b);
		
	}
}
