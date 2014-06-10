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

using LongoMatch.Store;
using Gtk;
using Mono.Unix;
using LongoMatch.Store.Templates;
using LongoMatch.Stats;
using LongoMatch.Common;

namespace LongoMatch.Gui.Component.Stats
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayersViewer : Gtk.Bin
	{
		TreeStore store;
		ProjectStats pstats;
		PlaysFilter filter;
		Player current;
		
		public PlayersViewer ()
		{
			this.Build ();
			store = new TreeStore(typeof(string), typeof(object));
			treeview1.AppendColumn ("Desc", new Gtk.CellRendererText (), "text", 0);
			treeview1.CursorChanged += HandleCursorChanged;
			treeview1.Model = store;
			treeview1.HeadersVisible = false;
			treeview1.EnableGridLines = TreeViewGridLines.None;
			treeview1.EnableTreeLines = false;
		}
		
		public void LoadProject (Project project, ProjectStats stats) {
			TreeIter first;
			
			store.Clear();
			pstats = stats;
			filter = new PlaysFilter (project);
			filter.PlayersFilterEnabled = true;
			pstats.Filter = filter;
			categoriesviewer.LoadStats (pstats);
			AddTeam (project.LocalTeamTemplate, project.Categories);
			AddTeam (project.VisitorTeamTemplate, project.Categories);
			filter.Update();
			store.GetIter (out first, new TreePath ("0:0"));
			treeview1.Selection.SelectIter (first);
		}
		
		void AddTeam (TeamTemplate tpl, Categories cats) {
			TreeIter teamIter;
			
			teamIter = store.AppendValues (tpl.TeamName, null);
			foreach (Player p in tpl.List) {
				store.AppendValues (teamIter, p.Name, p);
				filter.FilterPlayer (p);
			}
		}
		
		void HandleCursorChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			
			if (current != null)
				filter.FilterPlayer (current);
			
			treeview1.Selection.GetSelected(out iter);
			current = store.GetValue(iter, 1) as Player;
			if (current != null) {
				filter.UnFilterPlayer (current);
				filter.Update();
				pstats.UpdateStats ();
				categoriesviewer.ReloadStats ();
			}
		}
	}
}

