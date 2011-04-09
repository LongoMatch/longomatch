// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using Gtk;
using Gdk;

using LongoMatch.Interfaces;

namespace LongoMatch.Gui
{

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public class SubCategoriesTreeView: TreeView
	{
		private Menu menu;
		private TreeIter selectedIter;
		
		public SubCategoriesTreeView ()
		{	
			this.HeadersVisible = false;

			ListStore ls = new ListStore(typeof(ISubCategory));
			this.Model = ls;

			TreeViewColumn subcatColumn = new Gtk.TreeViewColumn();
			CellRendererText subcatCell = new Gtk.CellRendererText();
			
			subcatColumn.PackStart(subcatCell, true);
			subcatColumn.SetCellDataFunc(subcatCell, new Gtk.TreeCellDataFunc(RenderSubcat));
			this.AppendColumn(subcatColumn);
			
			SetMenu();
		}
		
		protected void OnEdit(object obj, EventArgs args) {
		}

		protected void OnRemove(object obj, EventArgs args) {
			(Model as ListStore).Remove(ref selectedIter);
		}
		
		private void RenderSubcat(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			var subcat = (ISubCategory)model.GetValue(iter, 0);
			(cell as Gtk.CellRendererText).Markup = subcat.ToMarkupString();
		}
		
		private void SetMenu() {
			Gtk.Action edit, rmv;
			UIManager manager;
			ActionGroup g;

			manager= new UIManager();
			g = new ActionGroup("SubCategoriesMenuGroup");

			edit = new Gtk.Action("EditAction", 
			                      Mono.Unix.Catalog.GetString("Edit name"),
			                      null, "gtk-edit");
			rmv = new Gtk.Action("RemoveAction", 
			                        Mono.Unix.Catalog.GetString("Remove sub-category"), 
			                        null, "gtk-remove");
			
			g.Add(edit, null);
			g.Add(rmv, null);

			manager.InsertActionGroup(g,0);

			manager.AddUiFromString("<ui>"+
			                        "  <popup action='CategoriesMenu'>"+
			                        "    <menuitem action='EditAction'/>"+
			                        "    <menuitem action='RemoveAction'/>"+
			                        "  </popup>"+
			                        "</ui>");

			menu = manager.GetWidget("/CategoriesMenu") as Menu;

			edit.Activated += OnEdit;
			rmv.Activated += OnRemove;
		}
		
		protected override bool OnButtonPressEvent(EventButton evnt)
		{
			if((evnt.Type == EventType.ButtonPress) && (evnt.Button == 3))
			{
				TreePath path;
				GetPathAtPos((int)evnt.X,(int)evnt.Y,out path);
				if(path!=null) {
					Model.GetIter(out selectedIter,path);
					menu.Popup();
				}
			}
			return base.OnButtonPressEvent(evnt);
		}
	}
}

