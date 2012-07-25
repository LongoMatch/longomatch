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

using Gdk;
using Gtk;
using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Gui.Dialog;
using LongoMatch.Store;
using System;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public class PlaysTreeView : ListTreeViewBase
	{
		
		public event TimeNodeChangedHandler EditProperties;

		//Categories menu
		Menu categoriesMenu;
		private RadioAction sortByName, sortByStart, sortByStop, sortByDuration;

		public PlaysTreeView() {
			SetCategoriesMenu();
		}

		new public TreeStore Model {
			set {
				if(value != null) {
					value.SetSortFunc(0, SortFunction);
					value.SetSortColumnId(0,SortType.Ascending);
				}
				base.Model = value;
			}
			get {
				return base.Model as TreeStore;
			}
		}
		
		private void SetCategoriesMenu() {
			Gtk.Action edit, editProp, sortMenu;
			UIManager manager;
			ActionGroup g;

			manager= new UIManager();
			g = new ActionGroup("CategoriesMenuGroup");

			edit = new Gtk.Action("EditAction", Mono.Unix.Catalog.GetString("Edit name"), null, "gtk-edit");
			sortMenu = new Gtk.Action("SortMenuAction", Mono.Unix.Catalog.GetString("Sort Method"), null, null);
			sortByName = new Gtk.RadioAction("SortByNameAction", Mono.Unix.Catalog.GetString("Sort by name"), null, null, 1);
			sortByStart = new Gtk.RadioAction("SortByStartAction", Mono.Unix.Catalog.GetString("Sort by start time"), null, null, 2);
			sortByStop = new Gtk.RadioAction("SortByStopAction", Mono.Unix.Catalog.GetString("Sort by stop time"), null, null, 3);
			sortByDuration = new Gtk.RadioAction("SortByDurationAction", Mono.Unix.Catalog.GetString("Sort by duration"), null, null, 3);
			editProp = new Gtk.Action("EditPropAction", Mono.Unix.Catalog.GetString("Edit properties"), null, "gtk-edit");

			sortByName.Group = new GLib.SList(System.IntPtr.Zero);
			sortByStart.Group = sortByName.Group;
			sortByStop.Group = sortByName.Group;
			sortByDuration.Group = sortByName.Group;


			g.Add(edit, null);
			g.Add(sortMenu, null);
			g.Add(sortByName, null);
			g.Add(sortByStart, null);
			g.Add(sortByStop, null);
			g.Add(sortByDuration, null);
			g.Add(editProp, null);

			manager.InsertActionGroup(g,0);

			manager.AddUiFromString("<ui>"+
			                        "  <popup action='CategoryMenu'>"+
			                        "    <menuitem action='EditAction'/>"+
			                        "    <menu action='SortMenuAction'>"+
			                        "      <menuitem action='SortByNameAction'/>"+
			                        "      <menuitem action='SortByStartAction'/>"+
			                        "      <menuitem action='SortByStopAction'/>"+
			                        "      <menuitem action='SortByDurationAction'/>"+
			                        "    </menu>"+
			                        "    <menuitem action='EditPropAction'/>"+
			                        "  </popup>"+
			                        "</ui>");

			categoriesMenu = manager.GetWidget("/CategoryMenu") as Menu;

			edit.Activated += OnEdit;
			sortByName.Activated += OnSortActivated;
			sortByStart.Activated += OnSortActivated;
			sortByStop.Activated += OnSortActivated;
			sortByDuration.Activated += OnSortActivated;
			editProp.Activated += delegate(object sender, EventArgs e) {
				EditProperties(GetValueFromPath(Selection.GetSelectedRows()[0]) as Category, null);
			};
		}

		private void SetupSortMenu(SortMethodType sortMethod) {
			switch(sortMethod) {
			case SortMethodType.SortByName:
				sortByName.Active = true;
				break;
			case SortMethodType.SortByStartTime:
				sortByStart.Active = true;
				break;
			case SortMethodType.SortByStopTime:
				sortByStop.Active = true;
				break;
			default:
				sortByDuration.Active = true;
				break;
			}
		}

		protected override int SortFunction(TreeModel model, TreeIter a, TreeIter b) {
			TreeStore store;
			TimeNode tna, tnb;
			TreeIter parent;
			int depth;
			Category category;

			if(model == null)
				return 0;

			store = model as TreeStore;

			// Retrieve the iter parent and its depth
			// When a new play is inserted, one of the iters is not a valid
			// in the model. Get the values from the valid one
			if(store.IterIsValid(a)) {
				store.IterParent(out parent, a);
				depth = store.IterDepth(a);
			}
			else {
				store.IterParent(out parent, b);
				depth = store.IterDepth(b);
			}

			// Dont't store categories
			if(depth == 0)
				return int.Parse(model.GetPath(a).ToString())
				       - int.Parse(model.GetPath(b).ToString());

			category = model.GetValue(parent,0) as Category;
			tna = model.GetValue(a, 0)as TimeNode;
			tnb = model.GetValue(b, 0) as TimeNode;

			switch(category.SortMethod) {
			case(SortMethodType.SortByName):
				return String.Compare(tna.Name, tnb.Name);
			case(SortMethodType.SortByStartTime):
				return (tna.Start - tnb.Start).MSeconds;
			case(SortMethodType.SortByStopTime):
				return (tna.Stop - tnb.Stop).MSeconds;
			case(SortMethodType.SortByDuration):
				return (tna.Duration - tnb.Duration).MSeconds;
			default:
				return 0;
			}
		}
		
		private void OnSortActivated(object o, EventArgs args) {
			Category category;
			RadioAction sender;

			sender = o as RadioAction;
			category = GetValueFromPath(Selection.GetSelectedRows()[0]) as Category;

			if(sender == sortByName)
				category.SortMethod = SortMethodType.SortByName;
			else if(sender == sortByStart)
				category.SortMethod = SortMethodType.SortByStartTime;
			else if(sender == sortByStop)
				category.SortMethod = SortMethodType.SortByStopTime;
			else
				category.SortMethod = SortMethodType.SortByDuration;
			// Redorder plays
			Model.SetSortFunc(0, SortFunction);
		}

		override protected bool SelectFunction(TreeSelection selection, TreeModel model, TreePath path, bool selected) {
			// Don't allow multiselect for categories
			if(!selected && selection.GetSelectedRows().Length > 0) {
				if(selection.GetSelectedRows().Length == 1 &&
				                GetValueFromPath(selection.GetSelectedRows()[0]) is Category)
					return false;
				return !(GetValueFromPath(path) is Category);
			}
			// Always unselect
			else
				return true;
		}

		override protected void OnNameCellEdited(object o, Gtk.EditedArgs args)
		{
			base.OnNameCellEdited(o, args);
			Model.SetSortFunc(0, SortFunction);
		}

		override protected bool OnButtonPressEvent(EventButton evnt)
		{
			TreePath[] paths = Selection.GetSelectedRows();

			if((evnt.Type == EventType.ButtonPress) && (evnt.Button == 3))
			{
				// We don't want to unselect the play when several
				// plays are selected and we clik the right button
				// For multiedition
				if(paths.Length <= 1) {
					base.OnButtonPressEvent(evnt);
					paths = Selection.GetSelectedRows();
				}

				if(paths.Length == 1) {
					TimeNode selectedTimeNode = GetValueFromPath(paths[0]) as TimeNode;
					if(selectedTimeNode is Play) {
						deleteKeyFrame.Sensitive = (selectedTimeNode as Play).HasDrawings;
						MultiSelectMenu(false);
						menu.Popup();
					}
					else {
						SetupSortMenu((selectedTimeNode as Category).SortMethod);
						categoriesMenu.Popup();
					}
				}
				else if(paths.Length > 1) {
					MultiSelectMenu(true);
					menu.Popup();
				}
			}
			else
				base.OnButtonPressEvent(evnt);
			return true;
		}

		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			return false;
		}
	}
}
