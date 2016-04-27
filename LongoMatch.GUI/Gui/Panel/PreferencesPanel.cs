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
using Gdk;
using Gtk;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Gui.Component;
using VAS.Core;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PreferencesPanel : Gtk.Bin, IPanel
	{
		public event BackEventHandle BackEvent;

		Widget selectedPanel;
		ListStore prefsStore;

		public PreferencesPanel ()
		{
			this.Build ();
			prefsStore = new ListStore (typeof(Pixbuf), typeof(string), typeof(Widget));
			treeview.AppendColumn ("Icon", new CellRendererPixbuf (), "pixbuf", 0);  
			treeview.AppendColumn ("Desc", new CellRendererText (), "text", 1);
			treeview.CursorChanged += HandleCursorChanged;
			treeview.Model = prefsStore;
			treeview.HeadersVisible = false;
			treeview.EnableGridLines = TreeViewGridLines.None;
			treeview.EnableTreeLines = false;
			AddPanels ();
			treeview.SetCursor (new TreePath ("0"), null, false);
			panelheader1.ApplyVisible = false;
			panelheader1.Title = Catalog.GetString ("PREFERENCES");
			panelheader1.BackClicked += (sender, e) => {
				if (BackEvent != null) {
					BackEvent ();
				}
				;
			};
		}

		public void OnLoaded ()
		{

		}

		public void OnUnloaded ()
		{

		}

		void AddPanels ()
		{
			AddPane (Catalog.GetString ("General"),
				Helpers.Misc.LoadIcon ("longomatch-preferences", IconSize.Dialog, 0),
				new GeneralPreferencesPanel ());
			AddPane (Catalog.GetString ("Keyboard shortcuts"),
				Helpers.Misc.LoadIcon ("longomatch-shortcut", IconSize.Dialog, 0),
				new HotkeysConfiguration ());
			AddPane (Catalog.GetString ("Video"),
				Helpers.Misc.LoadIcon ("longomatch-record", IconSize.Dialog, 0),
				new VideoPreferencesPanel ());
			AddPane (Catalog.GetString ("Live analysis"),
				Helpers.Misc.LoadIcon ("longomatch-video-device", IconSize.Dialog, 0),
				new LiveAnalysisPreferences ());
			AddPane (Catalog.GetString ("Plugins"),
				Helpers.Misc.LoadIcon ("longomatch-plugin", IconSize.Dialog, 0),
				new PluginsPreferences ());
		}

		void AddPane (string desc, Pixbuf icon, Widget pane)
		{
			prefsStore.AppendValues (icon, desc, pane);
		}

		void HandleCursorChanged (object sender, EventArgs e)
		{
			Widget newPanel;
			TreeIter iter;
			
			if (selectedPanel != null)
				propsvbox.Remove (selectedPanel);
			
			treeview.Selection.GetSelected (out iter);
			newPanel = prefsStore.GetValue (iter, 2) as Widget;
			newPanel.Visible = true;
			propsvbox.PackStart (newPanel, true, true, 0);
			selectedPanel = newPanel;
		}
	}
}

