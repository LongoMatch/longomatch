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
using Gtk;
using Gdk;
using Mono.Unix;

using LongoMatch.Gui.Component;

namespace LongoMatch.Gui.Dialog
{
	public partial class PropertiesEditor : Gtk.Dialog
	{
		Widget selectedPanel;
		ListStore prefsStore;
		
		public PropertiesEditor ()
		{
			this.Build ();
			prefsStore = new ListStore(typeof(Gdk.Pixbuf), typeof(string), typeof(Widget));
			treeview.AppendColumn ("Icon", new Gtk.CellRendererPixbuf (), "pixbuf", 0);  
			treeview.AppendColumn ("Desc", new Gtk.CellRendererText (), "text", 1);
			treeview.CursorChanged += HandleCursorChanged;
			treeview.Model = prefsStore;
			treeview.HeadersVisible = false;
			treeview.EnableGridLines = TreeViewGridLines.None;
			treeview.EnableTreeLines = false;
			AddPanels ();
			treeview.SetCursor (new TreePath("0"), null, false);
		}

		void AddPanels () {
			AddPane (Catalog.GetString ("General"),
			         Stetic.IconLoader.LoadIcon(this, "gtk-preferences", IconSize.Dialog),
			         new GeneralPreferencesPanel());
			AddPane (Catalog.GetString ("Video"),
			         Stetic.IconLoader.LoadIcon(this, "gtk-media-record", IconSize.Dialog),
			         new VideoPreferencesPanel());
			AddPane (Catalog.GetString ("Live analysis"),
			         Gdk.Pixbuf.LoadFromResource ("camera-video.png"),
			         new LiveAnalysisPreferences());
		}
		
		void AddPane (string desc, Pixbuf icon, Widget pane) {
			prefsStore.AppendValues(icon, desc, pane);
		}
		
		void HandleCursorChanged (object sender, EventArgs e)
		{
			Widget newPanel;
			TreeIter iter;
			
			if (selectedPanel != null)
				propsvbox.Remove(selectedPanel);
			
			treeview.Selection.GetSelected(out iter);
			newPanel = prefsStore.GetValue(iter, 2) as Widget;
			newPanel.Visible = true;
			propsvbox.PackStart(newPanel, true, true, 0);
			selectedPanel = newPanel;
		}
	}
}

