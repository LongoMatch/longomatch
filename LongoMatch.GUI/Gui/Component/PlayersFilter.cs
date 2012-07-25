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
using Mono.Unix;
using Gtk;

using LongoMatch.Common;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayersFilter : Gtk.Bin
	{
		PlaysFilter filter;
		TeamTemplate local, visitor;
		Player localTeam, visitorTeam;
		TreeIter localIter, visitorIter;
		
		public PlayersFilter ()
		{
			this.Build ();
			localTeam = new Player();
			visitorTeam = new Player();
			CreateTree();
		}
		
		public void SetFilter (PlaysFilter filter, TeamTemplate local, TeamTemplate visitor) {
			this.filter  = filter;
			this.local = local;
			this.visitor = visitor;
			localTeam.Name = local.TeamName;
			visitorTeam.Name = visitor.TeamName;
			FillTree();
		}
		
		private void CreateTree () {
			TreeViewColumn nameColumn = new TreeViewColumn ();
			CellRendererText nameCell = new CellRendererText ();
			nameColumn.Title = Catalog.GetString("Player");
			nameColumn.PackStart (nameCell, true);
			nameColumn.SetCellDataFunc (nameCell, new TreeCellDataFunc (RenderPlayer));
 
			TreeViewColumn filterColumn = new TreeViewColumn ();
			CellRendererToggle filterCell = new CellRendererToggle ();
			filterColumn.Title = Catalog.GetString("Filter");
			filterCell.Toggled += HandleFilterCellToggled;
			filterColumn.PackStart (filterCell, true);
			filterColumn.AddAttribute(filterCell, "active", 1);

			tree.AppendColumn (nameColumn);
			tree.AppendColumn (filterColumn);
		}

		private void FillTree () {
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
			tree.Model = store;
		}
 
		void HandleFilterCellToggled (object o, ToggledArgs args)
		{
			Gtk.TreeIter iter;
			TreeStore store = tree.Model as TreeStore;
			
			if (store.GetIterFromString(out iter, args.Path))
			{
				Player player = (Player) store.GetValue(iter, 0);
				bool active = !((bool) store.GetValue(iter, 1));
				
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
		}
		
		private void RenderPlayer (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			Player player = (Player) model.GetValue (iter, 0);
			string name = player.ToString();
			if (player == localTeam || player == visitorTeam) {
				name = player.Name;
			}
			(cell as CellRendererText).Text = name;
		}
	}
}

