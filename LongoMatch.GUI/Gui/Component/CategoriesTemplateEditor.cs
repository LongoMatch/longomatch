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

using System.Collections.Generic;
using Gtk;
using Mono.Unix;
using LongoMatch.Gui.Base;
using LongoMatch.Gui.Dialog;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Component
{
	public class CategoriesTemplateEditorWidget: TemplatesEditorWidget<Categories, Category> 
	{
		CategoriesTreeView categoriestreeview;
		List<HotKey> hkList;
		GameUnitsEditor gameUnitsEditor;
		
		ITemplatesService ts;

		public CategoriesTemplateEditorWidget (ITemplatesService ts): base(ts.CategoriesTemplateProvider)
		{
			hkList = new List<HotKey>();
			categoriestreeview = new CategoriesTreeView();
			categoriestreeview.CategoryClicked += this.OnCategoryClicked;
			categoriestreeview.CategoriesSelected += this.OnCategoriesSelected;
			CurrentPage = 0;
			FirstPageName = Catalog.GetString("Categories");
			AddTreeView(categoriestreeview);
			gameUnitsEditor = new GameUnitsEditor();
			if (Config.useGameUnits) {
				AddPage(gameUnitsEditor, "Game phases");
			}
			this.ts = ts;
		}
		
		public override Categories Template {
			get {
				return template;
			}
			set {
				template = value;
				Edited = false;
				Gtk.TreeStore categoriesListStore = new Gtk.TreeStore(typeof(Category));
				hkList.Clear();

				foreach(var cat in template) {
					categoriesListStore.AppendValues(cat);
					try {
						hkList.Add(cat.HotKey);
					} catch {}; //Do not add duplicated hotkeys
				}
				categoriestreeview.Model = categoriesListStore;
				ButtonsSensitive = false;
				gameUnitsEditor.SetRootGameUnit(value.GameUnits);
			}
		}
		
		protected override void RemoveSelected (){
			if(Project != null) {
				MessageDialog dialog = new MessageDialog((Gtk.Window)this.Toplevel,DialogFlags.Modal,MessageType.Question,
				                                         ButtonsType.YesNo,true,
				                                         Catalog.GetString("You are about to delete a category and all the plays added to this category. Do you want to proceed?"));
				if(dialog.Run() == (int)ResponseType.Yes) {
					try {
						foreach(var cat in selected)
							Project.RemoveCategory (cat);
					} catch {
						MessagePopup.PopupMessage(this,MessageType.Warning,
						                          Catalog.GetString("A template needs at least one category"));
					}
				}
				dialog.Destroy();
			} else {
				foreach(Category cat in selected) {
					if(template.Count == 1) {
						MessagePopup.PopupMessage(this,MessageType.Warning,
						                          Catalog.GetString("A template needs at least one category"));
					} else
						template.Remove(cat);
				}
			}	
			base.RemoveSelected();
		}
		
		protected override void EditSelected() {
			EditCategoryDialog dialog = new EditCategoryDialog(ts);
			dialog.Category = selected[0];
			dialog.HotKeysList = hkList;
			dialog.TransientFor = (Gtk.Window) Toplevel;
			dialog.Run();
			dialog.Destroy();
			Edited = true;
		}
		private void OnCategoryClicked(Category cat)
		{
			selected = new List<Category> ();
			selected.Add (cat);
			EditSelected();
		}

		private void OnCategoriesSelected(List<Category> catList)
		{
			selected = catList;
			if(catList.Count == 0)
				ButtonsSensitive = false;
			else if(catList.Count == 1) {
				ButtonsSensitive = true;
			}
			else {
				MultipleSelection();
			}
		}
	}
}
