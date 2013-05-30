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
using System.Collections.Generic;
using Gtk;
using Gdk;
using Mono.Unix;

using LongoMatch.Gui.Base;
using LongoMatch.Gui.Dialog;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Gui.Helpers;
using LongoMatch.Common;
using Image = LongoMatch.Common.Image;

namespace LongoMatch.Gui.Component
{
	public class CategoriesTemplateEditorWidget: TemplatesEditorWidget<Categories, Category> 
	{
		CategoriesTreeView categoriestreeview;
		List<HotKey> hkList;
		GameUnitsEditor gameUnitsEditor;
		Gtk.Image fieldImage, goalImage;
		VBox box;
		
		ITemplatesService ts;

		public CategoriesTemplateEditorWidget (ITemplatesService ts): base(ts.CategoriesTemplateProvider)
		{
			hkList = new List<HotKey>();
			categoriestreeview = new CategoriesTreeView();
			categoriestreeview.CategoryClicked += this.OnCategoryClicked;
			categoriestreeview.CategoriesSelected += this.OnCategoriesSelected;
			CurrentPage = 0;
			FirstPageName = Catalog.GetString("Categories");
			AddBackgroundsSelectionWidget ();
			AddTreeView(categoriestreeview);
			gameUnitsEditor = new GameUnitsEditor();
			if (Config.UseGameUnits) {
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
				if (template.GoalBackgroundImage != null) {
					goalImage.Pixbuf = template.GoalBackgroundImage.Value;
				} else {
					Image img = new Image (
						Gdk.Pixbuf.LoadFromResource (Constants.GOAL_BACKGROUND));
					img.Scale();
					goalImage.Pixbuf = img.Value; 
				}
				if (template.FieldBackgroundImage != null) {
					fieldImage.Pixbuf = template.FieldBackgroundImage.Value;
				} else {
					Image img = new Image (
						Gdk.Pixbuf.LoadFromResource (Constants.FIELD_BACKGROUND));
					img.Scale();
					fieldImage.Pixbuf = img.Value; 
				}
				box.Sensitive = true;
			}
		}
		
		protected override void RemoveSelected (){
			if(Project != null) {
				var msg = Catalog.GetString("You are about to delete a category and all the plays added to this category. Do you want to proceed?");
				if (MessagesHelpers.QuestionMessage (this, msg)) {
					try {
						foreach(var cat in selected)
							Project.RemoveCategory (cat);
					} catch {
						MessagesHelpers.WarningMessage (this,
						                                Catalog.GetString("A template needs at least one category"));
					}
				}
			} else {
				foreach(Category cat in selected) {
					if(template.Count == 1) {
						MessagesHelpers.WarningMessage (this,
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
		
		private void AddBackgroundsSelectionWidget () {
			Gtk.Frame fframe, gframe;
			EventBox febox, gebox;
			Image img;
			
			fframe = new Gtk.Frame("<b>" + Catalog.GetString("Field background") + "</b>");
			(fframe.LabelWidget as Label).UseMarkup = true;
			fframe.ShadowType = ShadowType.None;
			gframe = new Gtk.Frame("<b>" + Catalog.GetString("Goal background") + "</b>");
			(gframe.LabelWidget as Label).UseMarkup = true;
			gframe.ShadowType = ShadowType.None;
			
			febox = new EventBox();
			febox.ButtonPressEvent += OnFieldImageClicked;			
			fieldImage = new Gtk.Image();
			img = new Image (
				Gdk.Pixbuf.LoadFromResource (Constants.FIELD_BACKGROUND));
			img.Scale();
			fieldImage.Pixbuf = img.Value; 
			
			gebox = new EventBox();
			gebox.ButtonPressEvent += OnGoalImageClicked;			
			goalImage = new Gtk.Image();
			img = new Image (
				Gdk.Pixbuf.LoadFromResource (Constants.GOAL_BACKGROUND));
			img.Scale();
			goalImage.Pixbuf = img.Value;
			
			fframe.Add(febox);
			gframe.Add(gebox);
			febox.Add(fieldImage);
			gebox.Add(goalImage);
			
			box = new VBox();
			box.PackStart (fframe, false, false, 0);
			box.PackStart (gframe, false, false, 0);
			box.ShowAll();
			box.Sensitive = false;
			AddUpperWidget(box);
		}
		
		protected virtual void OnGoalImageClicked (object sender, EventArgs args)
		{
			Pixbuf background;
			
			background = Helpers.Misc.OpenImage((Gtk.Window)this.Toplevel);
			if (background != null) {
				Image img = new Image(background);
				img.Scale();
				Template.GoalBackgroundImage = img; 
				goalImage.Pixbuf = img.Value;
			}
		}
		
		protected virtual void OnFieldImageClicked (object sender, EventArgs args)
		{
			Pixbuf background;
			
			background = Helpers.Misc.OpenImage((Gtk.Window)this.Toplevel);
			if (background != null) {
				Image img = new Image(background);
				img.Scale();
				Template.FieldBackgroundImage = img; 
				fieldImage.Pixbuf = img.Value;
			}
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
