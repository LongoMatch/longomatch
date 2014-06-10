// CategoryProperties.cs
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

using System;
using System.Collections.Generic;
using Gdk;
using Gtk;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Popup;
using LongoMatch.Gui.Helpers;
using Point = LongoMatch.Common.Point;

namespace LongoMatch.Gui.Component
{

	public delegate void HotKeyChangeHandler(HotKey prevHotKey, Category newSection);

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial  class CategoryProperties : Gtk.Bin
	{

		public event HotKeyChangeHandler HotKeyChanged;

		private Category cat;
		private ListStore model;

		public CategoryProperties()
		{
			this.Build();
			subcategoriestreeview1.SubCategoriesDeleted += OnSubcategoriesDeleted;
			subcategoriestreeview1.SubCategorySelected += OnSubcategorySelected;
			leadtimebutton.ValueChanged += OnLeadTimeChanged;;
			lagtimebutton.ValueChanged += OnLagTimeChanged;
			fieldcoordinatestagger.Background = Config.FieldBackground.Value;
			halffieldcoordinatestagger.Background = Config.HalfFieldBackground.Value;
			goalcoordinatestagger.Background = Config.GoalBackground.Value;
		}
		
		public bool CanChangeHotkey {
			set {
				if (value == true)
					changebuton.Sensitive = true;
			}
		}
		
		public Category Category {
			set {
				cat = value;
				UpdateGui();
			}
			get {
				return cat;
			}
		}
		
		public Project Project {
			set;
			get;
		}
		
		public Categories Template {
			set {
				if (value.FieldBackground != null) {
					fieldcoordinatestagger.Background = value.FieldBackground.Value;
				}
				if (value.HalfFieldBackground != null) {
					halffieldcoordinatestagger.Background = value.HalfFieldBackground.Value;
				}
				if (value.GoalBackground != null) {
					goalcoordinatestagger.Background = value.GoalBackground.Value;
				}
			}
		}

		private void  UpdateGui() {
			ListStore list;
			List<Coordinates> coords;
			Coordinates c;
			
			if(cat == null)
				return;
				
			nameentry.Text = cat.Name;
				
			leadtimebutton.Value = cat.Start.Seconds;
			lagtimebutton.Value = cat.Stop.Seconds;
			colorbutton1.Color = Helpers.Misc.ToGdkColor(cat.Color);
			sortmethodcombobox.Active = (int)cat.SortMethod;
			
			tagfieldcheckbutton.Active = cat.TagFieldPosition;
			fieldcoordinatestagger.Visible = cat.TagFieldPosition;
			coords = new List<Coordinates>();
			c = new Coordinates();
			c.Points.Add (new Point (300, 300));
			coords.Add (c);
			if (cat.FieldPositionIsDistance) {
				c.Points.Add (new Point (400, 500));
			}
			fieldcoordinatestagger.Coordinates = coords;
			trajectorycheckbutton.Active = cat.FieldPositionIsDistance;
			
			taghalffieldcheckbutton.Active = cat.TagHalfFieldPosition;
			halffieldcoordinatestagger.Visible = cat.TagHalfFieldPosition;
			coords = new List<Coordinates>();
			c = new Coordinates();
			c.Points.Add (new Point (300, 300));
			coords.Add (c);
			if (cat.FieldPositionIsDistance) {
				c.Points.Add (new Point (400, 500));
			}
			halffieldcoordinatestagger.Coordinates = coords;
			trajectoryhalfcheckbutton.Active = cat.HalfFieldPositionIsDistance;
			
			taggoalcheckbutton.Active = cat.TagGoalPosition;
			coords = new List<Coordinates>();
			c = new Coordinates();
			c.Points.Add (new Point (100, 100));
			coords.Add (c);
			goalcoordinatestagger.Coordinates = coords;
			goalcoordinatestagger.Visible = cat.TagGoalPosition;
			
			if(cat.HotKey.Defined)
				hotKeyLabel.Text = cat.HotKey.ToString();
			else hotKeyLabel.Text = Catalog.GetString("none");
			
			list = subcategoriestreeview1.Model as ListStore;
			list.Clear();
			foreach (SubCategory subcat in cat.SubCategories)
				list.AppendValues(subcat);
		}
		
		private void RenderSubcat(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			(cell as Gtk.CellRendererText).Markup =(string)model.GetValue(iter, 0);
		}
		
		private SubCategory EditSubCategoryTags (SubCategory template, bool checkName){
			return null;
//			SubCategoryTagsEditor se =  new SubCategoryTagsEditor(template, subcategoriesProvider.TemplatesNames);
//			
//			se.CheckName = checkName;
//			int ret = se.Run();
//			
//			var t = se.Template; 
//			se.Destroy();
//			
//			if (ret != (int)ResponseType.Ok)
//				return null;
//			return t;
		}

		protected virtual void OnChangebutonClicked(object sender, System.EventArgs e)
		{
			HotKeySelectorDialog dialog = new HotKeySelectorDialog();
			dialog.TransientFor=(Gtk.Window)this.Toplevel;
			HotKey prevHotKey =  cat.HotKey;
			if(dialog.Run() == (int)ResponseType.Ok) {
				cat.HotKey=dialog.HotKey;
				UpdateGui();
			}
			dialog.Destroy();
			if(HotKeyChanged != null)
				HotKeyChanged(prevHotKey,cat);
		}

		protected virtual void OnColorbutton1ColorSet(object sender, System.EventArgs e)
		{
			if(cat != null)
				cat.Color= Helpers.Misc.ToLgmColor(colorbutton1.Color);
		}

		protected virtual void OnLeadTimeChanged(object sender, System.EventArgs e)
		{
			cat.Start = new Time{Seconds=(int)leadtimebutton.Value};
		}

		protected virtual void OnLagTimeChanged(object sender, System.EventArgs e)
		{
			cat.Stop = new Time{Seconds=(int)lagtimebutton.Value};
		}

		protected virtual void OnNameentryChanged(object sender, System.EventArgs e)
		{
			cat.Name = nameentry.Text;
		}

		protected virtual void OnSortmethodcomboboxChanged(object sender, System.EventArgs e)
		{
			cat.SortMethodString = sortmethodcombobox.ActiveText;
		}
		
		protected virtual void OnSubcategorySelected(SubCategory subcat) {
			EditSubCategoryTags((SubCategory)subcat, false);
		}
		
		protected virtual void OnSubcategoriesDeleted (List<SubCategory> subcats)
		{
			if (Project != null) {
				var msg = Catalog.GetString("If you delete this subcategory you will loose" +
				                            "all the tags associated with it. Do you want to proceed?");
				if (!MessagesHelpers.QuestionMessage (this, msg)) {
					return;
				}
				Project.DeleteSubcategoryTags(Category, subcats);
			}
			Category.SubCategories.RemoveAll(s => subcats.Contains(s));
		}
		
		protected virtual void OnAddbuttonClicked (object sender, System.EventArgs e)
		{
			TreeIter iter;
			
			subcatcombobox.GetActiveIter(out iter);
			ListStore list = subcategoriestreeview1.Model as ListStore;
			var subcat = Cloner.Clone((SubCategory)model.GetValue(iter, 1));
			subcat.Name = subcatnameentry.Text;
			Category.SubCategories.Add(subcat);
			list.AppendValues(subcat);
		}
		
		protected virtual void OnSubcatcomboboxChanged (object sender, System.EventArgs e)
		{
		}
		
		protected void OnTaggoalcheckbuttonClicked (object sender, EventArgs e)
		{
			goalcoordinatestagger.Visible = taggoalcheckbutton.Active;
			cat.TagGoalPosition = taggoalcheckbutton.Active;
		}
		
		protected void OnTaghalffieldcheckbuttonClicked (object sender, EventArgs e)
		{
			halffieldcoordinatestagger.Visible = taghalffieldcheckbutton.Active;
			cat.TagHalfFieldPosition = taghalffieldcheckbutton.Active;
		}
		
		protected void OnTagfieldcheckbuttonClicked (object sender, EventArgs e)
		{
			fieldcoordinatestagger.Visible = tagfieldcheckbutton.Active;
			cat.TagFieldPosition = tagfieldcheckbutton.Active;
		}
		
		protected void OnTrajectoryhalffieldcheckbuttonClicked (object sender, EventArgs e)
		{
			List<Coordinates> coords;
			Coordinates c;
			
			cat.HalfFieldPositionIsDistance = trajectoryhalfcheckbutton.Active;
			
			coords = new List<Coordinates>();
			c = new Coordinates();
			c.Points.Add (new Point (300, 300));
			coords.Add (c);
			if (cat.HalfFieldPositionIsDistance) {
				c.Points.Add (new Point (400, 500));
			}
			halffieldcoordinatestagger.Coordinates = coords;
		}
		
		protected void OnTrajectorycheckbuttonClicked (object sender, EventArgs e)
		{
			List<Coordinates> coords;
			Coordinates c;
			
			cat.FieldPositionIsDistance = trajectorycheckbutton.Active;
			
			coords = new List<Coordinates>();
			c = new Coordinates();
			c.Points.Add (new Point (300, 300));
			coords.Add (c);
			if (cat.FieldPositionIsDistance) {
				c.Points.Add (new Point (400, 500));
			}
			fieldcoordinatestagger.Coordinates = coords;
		}
	}
}
