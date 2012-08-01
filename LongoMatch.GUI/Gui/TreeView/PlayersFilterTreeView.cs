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
using Gdk;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Component
{

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public class PlayersFilterTreeView: FilterTreeViewBase
	{
		TeamTemplate local, visitor;
		Player localTeam, visitorTeam;
		TreeIter localIter, visitorIter;
		
		public PlayersFilterTreeView (): base()
		{
			visitorTeam = new Player();
			localTeam = new Player();
			HeadersVisible = false;
		}
		
		public override void SetFilter (PlaysFilter filter, Project project) {
			this.local = project.LocalTeamTemplate;
			this.visitor = project.VisitorTeamTemplate;
			localTeam.Name = local.TeamName;
			visitorTeam.Name = visitor.TeamName;
			base.SetFilter(filter, project);
		}
		
		protected override void FillTree () {
			TreeStore store = new TreeStore (typeof (Player), typeof (bool));
			localIter = store.AppendValues (localTeam);
			visitorIter = store.AppendValues (visitorTeam);
			store.SetValue(localIter, 1, false);
			store.SetValue(visitorIter, 1, false);
			
			foreach (Player player in local.PlayingPlayersList) {
				store.AppendValues (localIter, player, filter.VisiblePlayers.Contains(player));
			}
			
			foreach (Player player in visitor.PlayingPlayersList) {
				store.AppendValues (visitorIter, player, filter.VisiblePlayers.Contains(player));
			}
			Model = store;
		}
 
		protected override void HandleFilterCellToggled (object o, ToggledArgs args)
		{
			Gtk.TreeIter iter;
			TreeStore store = Model as TreeStore;
			
			if (store.GetIterFromString(out iter, args.Path))
			{
				bool active = !((bool) store.GetValue(iter, 1));
				UpdateSelection(iter, active);
			}
		}
		
		void UpdateSelection(TreeIter iter, bool active) {
			TreeStore store = Model as TreeStore;
			Player player = (Player) store.GetValue(iter, 0);
			
			/* Check all children */
			if (player == localTeam || player == visitorTeam)
			{
				TreeIter child;
				store.IterChildren(out child, iter);
				
				while (store.IterIsValid(child)) {
					Player childPlayer = (Player) store.GetValue(child, 0);
					if (active)
						filter.UnFilterPlayer(childPlayer);
					else
						filter.FilterPlayer(childPlayer);
					store.SetValue(child, 1, active);
					store.IterNext(ref child);
				}
			} else {
				if (active) {
					filter.UnFilterPlayer(player);
				} else {
					TreeIter team;
					filter.FilterPlayer(player);
					/* Uncheck the team check button */
					if (local.Contains(player))
						team = localIter;
					else
						team = visitorIter;
					store.SetValue(team, 1, false);
				}
			}
			
			store.SetValue(iter, 1, active);
			filter.Update();
		}
		
		protected override void RenderColumn (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			Player player = (Player) model.GetValue (iter, 0);
			string name = player.ToString();
			if (player == localTeam || player == visitorTeam) {
				name = player.Name;
			}
			(cell as CellRendererText).Text = name;
		}
		
		
		protected override void Select(bool select_all) {
			UpdateSelection(localIter, select_all);
			UpdateSelection(visitorIter, select_all);
		}
	}
}

