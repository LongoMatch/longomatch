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
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Component
{

    [System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public class CategoriesFilterTreeView: FilterTreeViewBase
	{
		Categories categories;
		
		public CategoriesFilterTreeView (): base()
		{
			firstColumnName = Catalog.GetString("Category");
			HeadersVisible = false;
		}
		
		public override void SetFilter (PlaysFilter filter, Project project) {
			this.categories = project.Categories;
			base.SetFilter(filter, project);
		}
		
		protected override void FillTree () {
			store = new TreeStore (typeof (object), typeof (bool));
			
			foreach (Category cat in  categories) {
				TreeIter catIter;
				
				catIter = store.AppendValues(cat, filter.VisibleCategories.Contains(cat));
				foreach (var subcat in cat.SubCategories) {
					TreeIter subcatIter;
					if (subcat is TagSubCategory) {
						subcatIter = store.AppendValues(catIter, subcat, true);
						foreach (string desc in subcat.ElementsDesc()) {
							store.AppendValues(subcatIter, new StringObject{Value=desc, SubCategory=subcat, Category=cat}, true);
						}
					}
				}
			}
			Model = store;
		}
		
		protected override void UpdateSelection(TreeIter iter, bool active) {
			TreeIter child;
			
			object o = store.GetValue(iter, 0);
			
			if (o is StringObject) {
				StringObject so = o as StringObject;
				
				filter.FilterSubCategory(so.Category, so.SubCategory, so.Value, active);
			} else {
				/* don't do anything here and let the children do the filtering */
			}
			store.SetValue(iter, 1, active);
			
			/* Check/Uncheck all children */
			store.IterChildren(out child, iter);
			while (store.IterIsValid(child)) {
				UpdateSelection(child, active);
				store.IterNext(ref child);
			}
			filter.Update();
		}
 
		protected override void RenderColumn (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			object obj = store.GetValue(iter, 0);
			string text = "";
			
			if (obj is Category) {
				Category cat = obj as Category;
				text = cat.Name;
			}
			else if (obj is ISubCategory) {
				ISubCategory subCat = obj as ISubCategory;
				text = subCat.Name;
			}
			else if (obj is StringObject){
				text = (obj as StringObject).Value;
			}
			
			(cell as CellRendererText).Text = text;
		}
		
		protected override void Select(bool select_all) {
			TreeIter iter;
			
			store.GetIterFirst(out iter);
			while (store.IterIsValid(iter)){
				UpdateSelection(iter, select_all);
				store.IterNext(ref iter);
			}
		}
	}
	
	class StringObject
	{
		public string Value {get;set;}
		public ISubCategory SubCategory {get;set;}
		public Category Category {get;set;}
	}
}

