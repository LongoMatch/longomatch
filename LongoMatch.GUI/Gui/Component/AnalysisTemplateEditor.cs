//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using LongoMatch.Store.Templates;
using LongoMatch.Common;
using LongoMatch.Gui.Helpers;
using LongoMatch.Store;
using Mono.Unix;
using System.Collections.Generic;
using Gdk;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AnalysisTemplateEditor : Gtk.Bin
	{
		bool edited;
		Categories template;
		Category selectedCategory;

		public AnalysisTemplateEditor ()
		{
			this.Build ();
			buttonswidget.Mode = LongoMatch.Common.TagMode.Predifined;
			buttonswidget.NewTagEvent += HandleCategorySelected;
			categoryproperties.Visible = false;
			savebutton.Clicked += HandleSaveClicked;
			deletebutton.Sensitive = false;
			newbutton.Sensitive = false;
			newbutton.Clicked += HandleNewClicked;
			deletebutton.Clicked += HandleDeleteClicked;
			fieldeventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			hfieldeventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			goaleventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
		}

		public Categories Template {
			set {
				template = value;
				categoryproperties.Template = value;
				newbutton.Sensitive = true;
				buttonswidget.UpdateCategories (value);
				fieldimage.Pixbuf = value.FieldBackground.Scale (50, 50).Value;
				hfieldimage.Pixbuf = value.HalfFieldBackground.Scale (50, 50).Value;
				goalimage.Pixbuf = value.GoalBackground.Scale (50, 50).Value;
				Edited = false;
			}
		}
		
		public bool Edited {
			set {
				edited = value;
			}
			get {
				return edited;
			}
		}
		
		void HandleDeleteClicked (object sender, EventArgs e)
		{
			string msg = Catalog.GetString ("Do you want to delete: ") +
				selectedCategory.Name + "?";
			if (Config.GUIToolkit.QuestionMessage (msg, null, this)) {
				template.List.Remove (selectedCategory);
				buttonswidget.UpdateCategories (template);
				Edited = true;
			}
		}

		void HandleNewClicked (object sender, EventArgs e)
		{
			template.AddDefaultItem (template.List.Count);
			buttonswidget.UpdateCategories (template);
			Edited = true;
		}
		
		void HandleCategorySelected (Category category, List<Player> players)
		{
			categoryproperties.Visible = true;
			deletebutton.Sensitive = true;
			categoryproperties.Category = category;
			selectedCategory = category;
		}
		
		void HandleSaveClicked (object sender, EventArgs e)
		{
			if (template != null) {
				Config.CategoriesTemplatesProvider.Update (template);
			}
		}
		
		void HandleFieldButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			LongoMatch.Common.Image background;
			Pixbuf pix = Helpers.Misc.OpenImage (this);
			
			if (pix == null) {
				return;
			}
			
			background = new LongoMatch.Common.Image (pix);
			background.ScaleInplace (Constants.MAX_BACKGROUND_WIDTH,
			                         Constants.MAX_BACKGROUND_HEIGHT);
			if (o == fieldeventbox) {
				template.FieldBackground = background;
				fieldimage.Pixbuf = background.Scale (50, 50).Value;
			} else if (o == hfieldeventbox) {
				template.HalfFieldBackground = background;
				hfieldimage.Pixbuf = background.Scale (50, 50).Value;
			} else if (o == goaleventbox) {
				template.GoalBackground = background;
				goalimage.Pixbuf = background.Scale (50, 50).Value;
			}
		}
	}
}

