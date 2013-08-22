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
		Gtk.Image fieldImage, halffieldImage, goalImage;
		Button fReset, hfReset, gReset;
		VBox box;
		Label periodsLabel;
		Frame periodsFrame;
		
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
				if (template.GoalBackground != null) {
					SetGoalImage (template.GoalBackground.Value);
				} else {
					SetGoalImage (null);
				}
				if (template.FieldBackground != null) {
					SetFieldImage (template.FieldBackground.Value);
				} else {
					SetFieldImage (null);
				}
				if (template.HalfFieldBackground != null) {
					SetHalfFieldImage (template.HalfFieldBackground.Value);
				} else {
					SetHalfFieldImage (null);
				}
				if (template.GamePeriods == null) {
					periodsFrame.Visible = false;
				} else {
					periodsFrame.Visible = true;
					periodsLabel.Text = String.Join (" - ", template.GamePeriods);
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
			dialog.Template = Template;
			dialog.HotKeysList = hkList;
			dialog.TransientFor = (Gtk.Window) Toplevel;
			dialog.Run();
			dialog.Destroy();
			Edited = true;
		}
		
		private void AddBackgroundsSelectionWidget () {
			Gtk.Frame fframe, gframe, hfframe;
			EventBox febox, gebox, hfebox;
			HBox fieldBox, halfFieldBox, goalBox;
			HBox periodsBox;
			Button periodsButton;
			
			fframe = new Gtk.Frame("<b>" + Catalog.GetString("Field background") + "</b>");
			(fframe.LabelWidget as Label).UseMarkup = true;
			fframe.ShadowType = ShadowType.None;
			hfframe = new Gtk.Frame("<b>" + Catalog.GetString("Half field background") + "</b>");
			(hfframe.LabelWidget as Label).UseMarkup = true;
			hfframe.ShadowType = ShadowType.None;
			gframe = new Gtk.Frame("<b>" + Catalog.GetString("Goal background") + "</b>");
			(gframe.LabelWidget as Label).UseMarkup = true;
			gframe.ShadowType = ShadowType.None;
			periodsFrame = new Frame("<b>" + Catalog.GetString ("Game periods") + "</b>");
			(periodsFrame.LabelWidget as Label).UseMarkup = true;
			periodsFrame.ShadowType = ShadowType.None;
			
			febox = new EventBox();
			fieldBox = new HBox ();
			fReset = new Button (Catalog.GetString ("Reset"));
			fReset.Clicked += (sender, e) => {
				Template.FieldBackground = null;
				SetFieldImage (null);
				};
			febox.ButtonPressEvent += OnFieldImageClicked;			
			fieldImage = new Gtk.Image ();
			SetFieldImage (null);
			
			hfebox = new EventBox();
			halfFieldBox = new HBox ();
			hfReset = new Button (Catalog.GetString ("Reset"));
			hfReset.Clicked += (sender, e) => {
				Template.HalfFieldBackground = null;
				SetHalfFieldImage (null);
				};
			hfebox.ButtonPressEvent += OnHalfFieldImageClicked;
			halffieldImage = new Gtk.Image();
			SetHalfFieldImage (null);
			
			gebox = new EventBox();
			goalBox = new HBox ();
			gReset = new Button (Catalog.GetString ("Reset"));
			gReset.Clicked += (sender, e) => {
				Template.GoalBackground = null;
				SetGoalImage (null);
				};
			gebox.ButtonPressEvent += OnGoalImageClicked;			
			goalImage = new Gtk.Image();
			SetGoalImage (null);
			
			periodsBox = new HBox ();
			periodsButton = new Button ("gtk-edit");
			periodsLabel = new Label ();
			periodsBox.PackStart (periodsLabel);
			periodsBox.PackStart (periodsButton);
			periodsButton.Clicked += HandlePeriodsClicked;
			
			febox.Add(fieldImage);
			hfebox.Add(halffieldImage);
			gebox.Add(goalImage);
			
			fieldBox.PackStart(febox, true, true, 0);
			fieldBox.PackStart(fReset, false, false, 0);
			halfFieldBox.PackStart (hfebox, true, true, 0);
			halfFieldBox.PackStart (hfReset, false, false, 0);
			goalBox.PackStart (gebox, true, true, 0);
			goalBox.PackStart (gReset, false, false, 0);
			
			fframe.Add(fieldBox);
			hfframe.Add(halfFieldBox);
			gframe.Add(goalBox);
			periodsFrame.Add (periodsBox);
			
			box = new VBox();
			box.PackStart (fframe, false, false, 0);
			box.PackStart (hfframe, false, false, 0);
			box.PackStart (gframe, false, false, 0);
			box.PackStart (periodsFrame, false, false, 0);
			box.ShowAll();
			box.Sensitive = false;
			AddUpperWidget(box);
		}

		void HandlePeriodsClicked (object sender, EventArgs e)
		{
			string res, desc;
			List<string> periods;
			
			desc = Catalog.GetString ("Game periods") + " (eg: 1 2 ex1 ex2) ";
			res = MessagesHelpers.QueryMessage (this, desc, "", String.Join (" ", Template.GamePeriods));
			if (res == null)
				return;
			
			periods = new List<string> (res.Split(' '));
			if (periods.Count == 0) {
				string msg = Catalog.GetString ("Invalid content. Periods must be separated by spaces " +
				                                "(\"1 2 ex1 ex2\")");
				MessagesHelpers.ErrorMessage (this, msg);
			} else {
				Template.GamePeriods = periods;
				periodsLabel.Text = String.Join (" - " , periods);
			}
		}
		
		void SetFieldImage (Pixbuf pix) {
			Image img;
			
			if (pix == null) {
				img = new Image (
					Gdk.Pixbuf.LoadFromResource (Constants.FIELD_BACKGROUND));
				fReset.Visible = false;
			} else {
				img = new Image(pix);
				fReset.Visible = true;
			}
			img.Scale ();
			fieldImage.Pixbuf = img.Value;
		}
		
		void SetHalfFieldImage (Pixbuf pix) {
			Image img;
			
			if (pix == null) {
				img = new Image (
					Gdk.Pixbuf.LoadFromResource (Constants.HALF_FIELD_BACKGROUND));
				hfReset.Visible = false;
			} else {
				img = new Image(pix);
				hfReset.Visible = true;
			}
			img.Scale ();
			halffieldImage.Pixbuf = img.Value;
		}
		
		void SetGoalImage (Pixbuf pix) {
			Image img;
			
			if (pix == null) {
				img = new Image (
					Gdk.Pixbuf.LoadFromResource (Constants.GOAL_BACKGROUND));
				gReset.Visible = false;
			} else {
				img = new Image(pix);
				gReset.Visible = true;
			}
			img.Scale ();
			goalImage.Pixbuf = img.Value;
		}
		
		protected virtual void OnGoalImageClicked (object sender, EventArgs args)
		{
			Pixbuf background;
			
			background = Helpers.Misc.OpenImage((Gtk.Window)this.Toplevel);
			if (background != null) {
				Template.GoalBackground = new Image (background);
				SetGoalImage (background.Copy());
				Edited = true;
			}
		}
		
		protected virtual void OnHalfFieldImageClicked (object sender, EventArgs args)
		{
			Pixbuf background;
			
			background = Helpers.Misc.OpenImage((Gtk.Window)this.Toplevel);
			if (background != null) {
				Template.HalfFieldBackground = new Image (background);
				SetHalfFieldImage (background.Copy());
				Edited = true;
			}
		}
		
		protected virtual void OnFieldImageClicked (object sender, EventArgs args)
		{
			Pixbuf background;
			
			background = Helpers.Misc.OpenImage((Gtk.Window)this.Toplevel);
			if (background != null) {
				Template.FieldBackground = new Image (background);
				SetFieldImage (background.Copy());
				Edited = true;
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
