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
using Gtk;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public class PlayersFilterTreeView: FilterTreeViewBase
	{
		SportsTeam local, visitor;
		PlayerLongoMatch localTeam, visitorTeam;
		TreeIter localIter, visitorIter;
		EventsFilter filter;

		public PlayersFilterTreeView () : base ()
		{
			visitorTeam = new PlayerLongoMatch ();
			localTeam = new PlayerLongoMatch ();
			HeadersVisible = false;
		}

		public void SetFilter (EventsFilter filter, ProjectLongoMatch project)
		{
			this.local = project.LocalTeamTemplate;
			this.visitor = project.VisitorTeamTemplate;
			localTeam.Name = local.TeamName;
			visitorTeam.Name = visitor.TeamName;
			this.filter = filter;
			FillTree ();
		}

		public override void ToggleAll (bool active)
		{
			TreeIter current;
			store.GetIterFirst (out current);
			ToggleAll (current, active, false);
		}

		protected override void UpdateSelection (TreeIter iter, bool active)
		{
			TreeStore store = Model as TreeStore;
			PlayerLongoMatch player = (PlayerLongoMatch)store.GetValue (iter, COL_VALUE);
			
			/* Check all children */
			if (player == localTeam || player == visitorTeam) {
				TreeIter child;
				store.IterChildren (out child, iter);
				
				filter.IgnoreUpdates = true;
				while (store.IterIsValid (child)) {
					PlayerLongoMatch childPlayer = (PlayerLongoMatch)store.GetValue (child, COL_VALUE);
					filter.FilterPlayer (childPlayer, active);
					store.SetValue (child, COL_ACTIVE, active);
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
					store.SetValue (team, COL_ACTIVE, false);
				}
			}
			
			store.SetValue (iter, COL_ACTIVE, active);
			filter.Update ();
		}

		void FillTree ()
		{
			localIter = store.AppendValues (localTeam.Name, false, localTeam);
			visitorIter = store.AppendValues (visitorTeam.Name, false, visitorTeam);

			filter.IgnoreUpdates = true;
			foreach (PlayerLongoMatch player in local.PlayingPlayersList) {
				store.AppendValues (localIter, player.ToString (), false, player);
			}
			
			foreach (PlayerLongoMatch player in visitor.PlayingPlayersList) {
				store.AppendValues (visitorIter, player.ToString (), false, player);
			}
			filter.IgnoreUpdates = false;
			filter.Update ();
		}
	}
}
