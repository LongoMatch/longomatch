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
using System.Collections.Generic;
using Gtk;
using LongoMatch.Addins;
using LongoMatch.Addins.ExtensionPoints;
using Mono.Addins.Description;
using VAS.Addins.ExtensionPoints;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PluginsPreferences : Gtk.Bin
	{
		ListStore pluginsStore;

		public PluginsPreferences ()
		{
			this.Build ();
			pluginsStore = new ListStore (typeof(string), typeof(AddinDescription), typeof(IPlugin));
			treeview1.Model = pluginsStore;
			treeview1.HeadersVisible = false;
			treeview1.AppendColumn ("Text", new CellRendererText (), "text", 0); 
			treeview1.EnableGridLines = TreeViewGridLines.None;
			treeview1.CursorChanged += HandleCursorChanged;
			FillStore ();
		}

		void FillStore ()
		{
			TreeIter first;

			foreach (var plugin in AddinsManager.Plugins) {
				pluginsStore.AppendValues (plugin.Key.Name, plugin.Key, plugin.Value);
			}
			if (pluginsStore.GetIterFirst (out first)) {
				treeview1.Selection.SelectIter (first);
				LoadAddin (pluginsStore.GetValue (first, 1) as AddinDescription,
					pluginsStore.GetValue (first, 2) as List<ConfigurablePlugin>);
			}
		}

		void LoadAddin (AddinDescription addin, List<ConfigurablePlugin> plugins)
		{
			if (addin == null) {
				vbox1.Visible = false;
				return;
			}
			vbox1.Visible = true;
			namelabel.Text = addin.Name;
			desclabel.Text = addin.Description;
			authorlabel.Text = addin.Author;
			filelabel.Text = addin.AddinFile;
			
			if (plugins != null && plugins.Count > 0) {
				configframe.Visible = true;
				foreach (Widget w in configbox.Children) {
					configbox.Remove (w);
					w.Destroy ();
				}
				foreach (ConfigurablePlugin plugin in plugins) {
					foreach (AttributeAndProperty attrprop in plugin.Properties) {
						if (attrprop.Property.PropertyType == typeof(Boolean)) {
							CheckButton button = new CheckButton (attrprop.Attribute.description);
							button.Active = (bool)attrprop.Property.GetValue (plugin, null);
							button.Clicked += (sender, e) => {
								attrprop.Property.SetValue (plugin, button.Active, null);
							};
							button.Show ();
							configbox.PackStart (button, false, true, 0);
						}
					}
				}
			} else {
				configframe.Visible = false;
			}
		}

		void HandleCursorChanged (object sender, EventArgs e)
		{
			TreeIter iter;

			treeview1.Selection.GetSelected (out iter);
			LoadAddin (pluginsStore.GetValue (iter, 1) as AddinDescription,
				pluginsStore.GetValue (iter, 2) as List<ConfigurablePlugin>);
		}
		
	}
}

