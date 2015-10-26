// 
//  Copyright (C) 2012 Andoni Morales Alastruey
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
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using Mono.Unix;

namespace LongoMatch.Gui.Component
{

	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public class PlayersFilterTreeView: FilterTreeViewBase
	{
		Team local, visitor;
		Player localTeam, visitorTeam;
		TreeIter localIter, visitorIter;

		public PlayersFilterTreeView () : base ()
		{
			visitorTeam = new Player ();
			localTeam = new Player ();
			HeadersVisible = false;
		}

		public override void SetFilter (EventsFilter filter, Project project)
		{
			this.local = project.LocalTeamTemplate;
			this.visitor = project.VisitorTeamTemplate;
			localTeam.Name = local.TeamName;
			visitorTeam.Name = visitor.TeamName;
			base.SetFilter (filter, project);
		}

		protected override void FillTree ()
		{
			TreeStore store = new TreeStore (typeof(Player), typeof(bool));
			localIter = store.AppendValues (localTeam);
			visitorIter = store.AppendValues (visitorTeam);
			store.SetValue (localIter, 1, false);
			store.SetValue (visitorIter, 1, false);
			
			filter.IgnoreUpdates = true;
			foreach (Player player in local.PlayingPlayersList) {
				filter.FilterPlayer (player, true);
				store.AppendValues (localIter, player, true);
			}
			
			foreach (Player player in visitor.PlayingPlayersList) {
				filter.FilterPlayer (player, true);
				store.AppendValues (visitorIter, player, true);
			}
			filter.IgnoreUpdates = false;
			filter.Update ();
			Model = store;
		}

		
		protected override void UpdateSelection (TreeIter iter, bool active)
		{
			TreeStore store = Model as TreeStore;
			Player player = (Player)store.GetValue (iter, 0);
			
			/* Check all children */
			if (player == localTeam || player == visitorTeam) {
				TreeIter child;
				store.IterChildren (out child, iter);
				
				filter.IgnoreUpdates = true;
				while (store.IterIsValid (child)) {
					Player childPlayer = (Player)store.GetValue (child, 0);
					filter.FilterPlayer (childPlayer, active);
					store.SetValue (child, 1, active);
					store.IterNext (ref child);
				}
				filter.IgnoreUpdates = false;
			} else {
				filter.FilterPlayer (player, active);
				if (!active) {
					TreeIter team;
					/* Uncheck the team check button */
					if (local.List.Contains (player))
						team = localIter;
					else
						team = visitorIter;
					store.SetValue (team, 1, false);
				}
			}
			
			store.SetValue (iter, 1, active);
			filter.Update ();
		}

		protected override void RenderColumn (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			Player player = (Player)model.GetValue (iter, 0);
			string name = player.ToString ();
			if (player == localTeam || player == visitorTeam) {
				name = player.Name;
			}
			(cell as CellRendererText).Text = name;
		}

		protected override void Select (bool select_all)
		{
			UpdateSelection (localIter, select_all);
			UpdateSelection (visitorIter, select_all);
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			return false;
		}


	}
}

