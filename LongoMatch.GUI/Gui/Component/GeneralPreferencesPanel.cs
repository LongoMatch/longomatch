//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using Mono.Unix;
using Gtk;
using LongoMatch.Core.Common;
using System.Globalization;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class GeneralPreferencesPanel : Gtk.Bin
	{
	
		ListStore langsStore;
		ListStore templates;
		CheckButton autosavecb;

		public GeneralPreferencesPanel ()
		{
			this.Build ();
			FillLangs ();
			FillTemplates ();
			autosavecb = new CheckButton ();
			table1.Attach (autosavecb, 1, 2, 1, 2,
				AttachOptions.Shrink,
				AttachOptions.Shrink, 0, 0);
			autosavecb.CanFocus = false;
			autosavecb.Show ();
			autosavecb.Active = Config.AutoSave;
			autosavecb.Toggled += (sender, e) => {
				Config.AutoSave = autosavecb.Active;
			};
		}

		void FillLangs ()
		{
			int index = 0, active = 0;
			
			langsStore = new ListStore (typeof(string), typeof(CultureInfo));
			langsStore.AppendValues (Catalog.GetString ("Default"), null);
			index++;
			
			foreach (CultureInfo lang in Gettext.Languages) {
				langsStore.AppendValues (lang.DisplayName, lang);
				if (lang.Name == Config.Lang)
					active = index;
				index++;
			}
			langcombobox.Model = langsStore;
			langcombobox.Active = active;
			langcombobox.Changed += HandleChanged;
		}

		void FillTemplates ()
		{
			int i = 0, active = -1;
			templates = new ListStore (typeof(string));

			foreach (var template in Config.CategoriesTemplatesProvider.Templates) {
				if (template.Name == Config.DefaultTemplate) {
					active = i;
				}
				templates.AppendValues (template.Name);
				i++;
			}
			templatescombobox.Model = templates;
			if (active != -1) {
				templatescombobox.Active = active;
			}
			templatescombobox.Changed += (sender, e) => {
				Config.DefaultTemplate = templatescombobox.ActiveText;
			};
		}

		void HandleChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			CultureInfo info;
			
			langcombobox.GetActiveIter (out iter);
			info = (CultureInfo)langsStore.GetValue (iter, 1);
			if (info == null) {
				Config.Lang = null;
			} else {
				Config.Lang = info.Name;
			}
		}
	}
}

