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
using System.Collections.Generic;
using System.Linq;
using Gtk;

using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayersTagger : Gtk.Bin
	{
		PlayerSubCategory subcat;
		TeamTemplate local, visitor;
		PlayersTagStore players;
		Dictionary<CheckButton, PlayerTag> checkButtonsDict;
		RadioButton firstRB;
		const uint DEFAULT_WIDTH = 6;
		
		public PlayersTagger ()
		{
			this.Build ();
		}
		
		public void Load (PlayerSubCategory subcat, TeamTemplate local,
			TeamTemplate visitor, PlayersTagStore players)
		{
			this.subcat = subcat;
			this.local = local;
			this.visitor = visitor;
			this.players = players;
			SetPlayersInfo();
			UpdateSelectedPlayers();
		}

		void UpdateSelectedPlayers () {
			foreach(var pair in checkButtonsDict) {
				pair.Key.Active = players.Contains(pair.Value);
			}
		}

		void SetPlayersInfo() {
			checkButtonsDict = new Dictionary<CheckButton, PlayerTag>();
			if (local != null)
				SetPlayersInfo(localtable, local);
			if (visitor != null)
				SetPlayersInfo(visitortable, visitor);
		}
		
		void SetPlayersInfo(Table table, TeamTemplate template) {
			List<PlayerTag> playersList;
			int i=0;

			playersList = template.PlayingPlayersList.Select(p => new PlayerTag {Value=p, SubCategory=subcat}).ToList();

			table.NRows =(uint)(playersList.Count/DEFAULT_WIDTH);
			table.NColumns =(uint) DEFAULT_WIDTH;

			foreach(PlayerTag player in playersList) {
				CheckButton button;
				
				if (!subcat.AllowMultiple) {
					if (firstRB == null)
						button = firstRB = new RadioButton("");
					else
						button = new RadioButton(firstRB);
				} else {
					button = new CheckButton();
				}
				button.Label = player.Value.Number + "-" + player.Value.Name;
				button.Name = i.ToString();
				button.Toggled += OnButtonToggled;
				button.Show();

				uint row_top =(uint)(i / table.NColumns);
				uint row_bottom = (uint) row_top + 1 ;
				uint col_left = (uint) i % table.NColumns;
				uint col_right = (uint) col_left + 1 ;

				table.Attach(button,col_left,col_right,row_top,row_bottom);
				checkButtonsDict.Add(button, player);
				i++;
			}
		}
		
		protected virtual void OnButtonToggled (object sender, System.EventArgs args) {
			CheckButton button = sender as CheckButton;
			PlayerTag player = checkButtonsDict[button];
			
			if (button.Active && !players.Contains(player))
				players.Add(player);
			else if (!button.Active)
				players.Remove(player);
		}
	}
}
