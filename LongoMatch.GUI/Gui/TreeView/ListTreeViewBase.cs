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

namespace LongoMatch.Gui.Component
{


	public abstract class ListTreeViewBase:TreeView
	{
		// Plays menu
		protected Menu menu, catMenu;
		protected MenuItem tag, delete, addPLN, deleteKeyFrame, snapshot;
		protected MenuItem name, render, moveCat, duplicate;

		protected Gtk.CellRendererText nameCell;
		protected Gtk.TreeViewColumn nameColumn;
		protected bool editing;
		protected bool enableCategoryMove = false;
		
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

			SetMenu();
			PlayListLoaded = false;

			nameColumn = new Gtk.TreeViewColumn();
			nameColumn.Title = "Name";
			nameCell = new Gtk.CellRendererText();
			nameCell.Edited += OnNameCellEdited;
			Gtk.CellRendererPixbuf miniatureCell = new Gtk.CellRendererPixbuf();
			nameColumn.PackStart(nameCell, true);
			nameColumn.PackEnd(miniatureCell, true);

			nameColumn.SetCellDataFunc(miniatureCell, new Gtk.TreeCellDataFunc(RenderMiniature));
			nameColumn.SetCellDataFunc(nameCell, new Gtk.TreeCellDataFunc(RenderName));

			AppendColumn(nameColumn);

		}

		public bool Colors {
			get;
			set;
		}

		public bool PlayListLoaded {
			set {
				addPLN.Sensitive = value;
			}
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
			set {
				if (!enableCategoryMove)
					return;
				catsDict = new Dictionary<MenuItem, Category>();
				catMenu = new Menu();
				foreach (Category cat in value.Categories.List) {
					var item = new MenuItem (cat.Name);
					catMenu.Append (item);
					catsDict.Add(item, cat);
					item.Activated += OnCatChanged; 
				}
				catMenu.ShowAll();
				moveCat.Submenu = catMenu;
			}
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

		protected void SetMenu() {
			menu = new Menu();

			name = new MenuItem(Catalog.GetString("Edit name"));
			tag = new MenuItem(Catalog.GetString("Edit tags"));
			duplicate = new MenuItem(Catalog.GetString("Duplicate"));
			delete = new MenuItem(Catalog.GetString("Delete"));
			deleteKeyFrame = new MenuItem(Catalog.GetString("Delete key frame"));
			addPLN = new MenuItem(Catalog.GetString("Add to playlist"));
			addPLN.Sensitive=false;
			render = new MenuItem(Catalog.GetString("Export to video file"));
			snapshot = new MenuItem(Catalog.GetString("Export to PGN images"));
			moveCat = new MenuItem(Catalog.GetString("Move to category"));
			
			menu.Append(name);
			menu.Append(tag);
			menu.Append(addPLN);
			menu.Append(delete);
			menu.Append(duplicate);
			menu.Append(deleteKeyFrame);
			menu.Append(render);
			menu.Append(snapshot);
			menu.Append(moveCat);

			name.Activated += OnEdit;
			tag.Activated += OnTag;
			duplicate.Activated += OnDuplicate;
			addPLN.Activated += OnAdded;
			delete.Activated += OnDeleted;
			deleteKeyFrame.Activated += OnDeleteKeyFrame;
			render.Activated += OnRender;
			snapshot.Activated += OnSnapshot;
			menu.ShowAll();
		}
		
		protected Play SelectedPlay {
			get {
				return GetValueFromPath(Selection.GetSelectedRows()[0]) as Play;
			}
		}

		protected void MultiSelectMenu(bool enabled) {
			name.Sensitive = !enabled;
			snapshot.Sensitive = !enabled;
			tag.Sensitive = !enabled;
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

		protected void OnDeleted(object obj, EventArgs args) {
			List <Play> playsList = new List<Play>();
			List <TreeIter> iters = new List<TreeIter>();
			TreePath[] paths = Selection.GetSelectedRows();

			/* Get the iter for all of the paths first, because the path changes
			 * each time a row is deleted */
			foreach(var path in paths) {
				TreeIter iter;
				modelFilter.GetIter(out iter, path);
				playsList.Add((Play)modelFilter.GetValue(iter, 0));
				iters.Add(iter);
			}
			/* Delete all the iters now */
			for(int i=0; i< iters.Count; i++) {
				TreeIter iter = iters[i];
				Model.Remove(ref iter);
			}
			
			Config.EventsBroker.EmitPlaysDeleted (playsList);
		}

		protected void OnDeleteKeyFrame(object obj, EventArgs args) {
			var msg = Catalog.GetString("Do you want to delete the key frame for this play?");
			if (MessagesHelpers.QuestionMessage (Toplevel, msg)) {
				TreePath[] paths = Selection.GetSelectedRows();
				for(int i=0; i<paths.Length; i++) {
					Play tNode = (Play)GetValueFromPath(paths[i]);
					tNode.Drawings.Clear();
				}
				// Refresh the thumbnails
				QueueDraw();
			}
		}

		void OnDuplicate (object sender, EventArgs e)
		{
			Config.EventsBroker.EmitDuplicatePlay (SelectedPlay);
		}

		protected virtual void OnEdit(object obj, EventArgs args) {
			TreePath[] paths = Selection.GetSelectedRows();

			editing = true;
			nameCell.Editable = true;
			SetCursor(paths[0],  nameColumn, true);
		}

		protected void OnAdded(object obj, EventArgs args) {
			List<Play> list = new List<Play>();
			TreePath[] paths = Selection.GetSelectedRows();
			for(int i=0; i<paths.Length; i++) {
				Play tNode = (Play)GetValueFromPath(paths[i]);
				list.Add (tNode);
			}
			Config.EventsBroker.EmitPlayListNodeAdded (list);
		}

		protected void OnTag(object obj, EventArgs args) {
			Config.EventsBroker.EmitTagPlay (SelectedPlay);
			Refilter();
		}

		protected void OnSnapshot(object obj, EventArgs args) {
			Config.EventsBroker.EmitSnapshotSeries (SelectedPlay);
		}
		
		protected void OnRender(object obj, EventArgs args) {
			if (NewRenderingJob != null)
				NewRenderingJob(this, null);
		}
		
		protected void OnCatChanged(object obj, EventArgs args) {
			Config.EventsBroker.EmitPlayCategoryChanged (SelectedPlay,
			                                             catsDict[obj as MenuItem]);
		}

		protected void OnFilterUpdated() {
			modelFilter.Refilter();
		}
		
		protected abstract bool SelectFunction(TreeSelection selection, TreeModel model, TreePath path, bool selected);
		protected abstract int SortFunction(TreeModel model, TreeIter a, TreeIter b);
		
	}
}
