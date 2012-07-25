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
using Gtk;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CategoriesFilter : Gtk.Bin
	{
		PlaysFilter filter;
		Categories categories;
		TreeView tree;
		
		public CategoriesFilter ()
		{
			this.Build ();
			tree = new TreeView();
			this.Add(tree);
			tree.Show();
			CreateTree();
		}
		
		public void SetFilter (PlaysFilter filter, Categories categories) {
			this.filter  = filter;
			this.categories = categories;
			FillTree();
		}
		
		private void CreateTree () {
			TreeViewColumn nameColumn = new TreeViewColumn ();
			CellRendererText nameCell = new CellRendererText ();
			nameColumn.Title = Catalog.GetString("Category");
			nameColumn.PackStart (nameCell, true);
			nameColumn.SetCellDataFunc (nameCell, new TreeCellDataFunc (RenderCategory));
 
			TreeViewColumn filterColumn = new TreeViewColumn ();
			CellRendererToggle filterCell = new CellRendererToggle ();
			filterColumn.Title = Catalog.GetString("Filter");
			filterCell.Toggled += HandleFilterCellToggled;
			filterColumn.PackStart (filterCell, true);
			filterColumn.AddAttribute(filterCell, "active", 1);

			tree.AppendColumn (nameColumn);
			tree.AppendColumn (filterColumn);
		}

		private void FillTree () {
			ListStore store = new ListStore (typeof (Category), typeof (bool));
			
			foreach (Category cat in  categories) {
				store.AppendValues(cat, filter.VisibleCategories.Contains(cat));
			}
			tree.Model = store;
		}
 
		void HandleFilterCellToggled (object o, ToggledArgs args)
		{
			Gtk.TreeIter iter;
			ListStore store = tree.Model as ListStore;
			
			if (store.GetIterFromString(out iter, args.Path))
			{
				Category cat = (Category) store.GetValue(iter, 0);
				bool active = !((bool) store.GetValue(iter, 1));
				
				if (active) {
					filter.UnFilterCategory(cat);
				} else {
					filter.FilterCategory(cat);
				}
			
				store.SetValue(iter, 1, active);
				filter.Update();
			}
		}
		
		private void RenderCategory (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			Category category = (Category) model.GetValue (iter, 0);
			(cell as CellRendererText).Text = category.Name;
		}
	}
}


