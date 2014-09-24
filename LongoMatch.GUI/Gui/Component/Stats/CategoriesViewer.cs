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

using LongoMatch.Core.Stats;
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;

namespace LongoMatch.Gui.Component.Stats
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CategoriesViewer : Gtk.Bin
	{
		ListStore store;
		ProjectStats pstats;
		
		public CategoriesViewer ()
		{
			this.Build ();
			store = new ListStore(typeof(EventType), typeof(string));
			treeview.AppendColumn ("Desc", new Gtk.CellRendererText (), "text", 1);
			treeview.CursorChanged += HandleCursorChanged;
			treeview.Model = store;
			treeview.HeadersVisible = false;
			treeview.EnableGridLines = TreeViewGridLines.None;
			treeview.EnableTreeLines = false;
		}
		
		public void LoadStats (ProjectStats pstats, Project project) {
			categoryviewer1.HomeName = pstats.LocalTeam;
			categoryviewer1.AwayName = pstats.VisitorTeam;
			categoryviewer1.LoadBackgrounds (project);
			this.pstats = pstats;
			ReloadStats();
		}
		
		public void ReloadStats () {
			TreeIter iter;
			TreePath selected = null;
			
			treeview.Selection.GetSelected (out iter);
			if (store.IterIsValid (iter))
				selected = store.GetPath (iter);
			
			store.Clear();
			foreach (CategoryStats cstats in pstats.CategoriesStats) {
				store.AppendValues (cstats, cstats.Name);
			}
			
			/* Keep the selected category for when we reload the stats changing players */
			if (selected != null) {
				store.GetIter (out iter, selected);
			} else {
				store.GetIterFirst(out iter);
			}
			treeview.Selection.SelectIter(iter);
			categoryviewer1.LoadStats (store.GetValue (iter, 0) as CategoryStats);
		}
		
		void HandleCursorChanged (object sender, EventArgs e)
		{
			CategoryStats stats;
			TreeIter iter;
			
			treeview.Selection.GetSelected(out iter);
			stats = store.GetValue(iter, 0) as CategoryStats;
			categoryviewer1.LoadStats (stats);
		}
	}
}

