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

using LongoMatch.Interfaces;
using LongoMatch.Services;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Gui.Dialog;

namespace LongoMatch.Gui.Component
{

	public delegate void HotKeyChangeHandler(HotKey prevHotKey, Category newSection);

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial  class CategoryProperties : Gtk.Bin
	{

		public event HotKeyChangeHandler HotKeyChanged;

		private Category cat;
		private ITemplateProvider<SubCategoryTemplate, string> subcategoriesTemplates;
		private Dictionary<string, TagSubCategory> subCategories;

		public CategoryProperties()
		{
			this.Build();
			subcategoriesTemplates = MainClass.ts.SubCategoriesTemplateProvider;
			subCategories = new Dictionary<string, TagSubCategory>();
			LoadSubcategories();
		}

		private void LoadSubcategories() {
			subCategories.Clear();
			foreach (TagSubCategory subcat in subcategoriesTemplates.Templates) {
				subCategories.Add(subcat.Name, subcat);
				subcatcombobox.AppendText(subcat.Name);
			}
			
			/* We check here if the user already saved at least one category
			 * to hide the big helper button.*/
			if (subCategories.Count != 0) {
				newfirstbutton.Visible = false;
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

		private void  UpdateGui() {
			if(cat != null) {
				nameentry.Text = cat.Name;
				
				lagtimebutton.Value = cat.Start.Seconds;
				leadtimebutton.Value = cat.Stop.Seconds;
				colorbutton1.Color = cat.Color;
				sortmethodcombobox.Active = (int)cat.SortMethod;

				if(cat.HotKey.Defined) {
					hotKeyLabel.Text = cat.HotKey.ToString();
				}
				else hotKeyLabel.Text = Catalog.GetString("none");
			}
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
				cat.Color=colorbutton1.Color;
		}

		protected virtual void OnTimeadjustwidget1LeadTimeChanged(object sender, System.EventArgs e)
		{
			cat.Start = new Time{Seconds=(int)leadtimebutton.Value};
		}

		protected virtual void OnTimeadjustwidget1LagTimeChanged(object sender, System.EventArgs e)
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
		
		protected virtual void OnNewfirstbuttonClicked (object sender, System.EventArgs e)
		{
		}
		
		protected virtual void OnAddbuttonClicked (object sender, System.EventArgs e)
		{
		}
		
		protected virtual void OnNewbuttonClicked (object sender, System.EventArgs e)
		{
		}
	}
}
