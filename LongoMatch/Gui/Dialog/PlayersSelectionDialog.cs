//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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

using System.Collections.Generic;
using System.Linq;
using Gtk;

using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Dialog
{


	public partial class PlayersSelectionDialog : Gtk.Dialog
	{
		TeamTemplate template;
		List<PlayerTag> selectedPlayers;
		Dictionary<CheckButton, PlayerTag> checkButtonsDict;
		bool useRadioButtons;
		RadioButton firstRB;
		
		public PlayersSelectionDialog(bool useRadioButtons)
		{
			this.Build();
			checkButtonsDict = new Dictionary<CheckButton, PlayerTag>();
			this.useRadioButtons = useRadioButtons;
		}
		
		public TeamTemplate Template {
			set{
				SetPlayersInfo(value);
			}
		}
		
		public List<PlayerTag> SelectedPlayers {
			set {
				this.selectedPlayers = value;
				foreach(var pair in checkButtonsDict)
					pair.Key.Active = value.Contains(pair.Value);
			}
			get {
				return selectedPlayers;
			}
		}

		private void SetPlayersInfo(TeamTemplate template) {
			List<PlayerTag> playersList;
			int i=0;

			if(this.template != null)
				return;

			this.template = template;
			playersList = template.PlayingPlayersList.Select(p => new PlayerTag {Value=p}).ToList();

			table1.NColumns =(uint)(playersList.Count/10);
			table1.NRows =(uint) 10;

			foreach(PlayerTag player in playersList) {
				CheckButton button;
				
				if (useRadioButtons) {
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

				uint row_top =(uint)(i%table1.NRows);
				uint row_bottom = (uint) row_top+1 ;
				uint col_left = (uint) i/table1.NRows;
				uint col_right = (uint) col_left+1 ;

				table1.Attach(button,col_left,col_right,row_top,row_bottom);
				checkButtonsDict.Add(button, player);
				i++;
			}
		}
		
		protected virtual void OnButtonToggled (object sender, System.EventArgs args) {
			CheckButton button = sender as CheckButton;
			PlayerTag player = checkButtonsDict[button];
			
			if (button.Active && !selectedPlayers.Contains(player))
				selectedPlayers.Add(player);
			else if (!button.Active)
				selectedPlayers.Remove(player);
		}
	}
}
