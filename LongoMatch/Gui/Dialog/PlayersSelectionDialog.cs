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

using System;
using System.Collections.Generic;
using Gtk;
using LongoMatch.DB;
using LongoMatch.TimeNodes;

namespace LongoMatch.Gui.Dialog
{


	public partial class PlayersSelectionDialog : Gtk.Dialog
	{
		TeamTemplate template;
		Dictionary<int, CheckButton> checkButtonsDict;

		public PlayersSelectionDialog()
		{
			this.Build();
			checkButtonsDict = new Dictionary<int, CheckButton>();
		}

		public void SetPlayersInfo(TeamTemplate template) {
			CheckButton button;
			Player player;
			int playersCount=0;

			if (this.template != null)
				return;

			this.template = template;

			table1.NColumns =(uint)(template.PlayersCount/10);
			table1.NRows =(uint) 10;

			for (int i=0;i<template.PlayersCount;i++) {
				player = template.GetPlayer(i);
				if (player.Discarded)
					continue;

				button = new CheckButton();
				button.Label = player.Number + "-" + player.Name;
				button.Name = i.ToString();
				button.Show();

				uint row_top =(uint)(playersCount%table1.NRows);
				uint row_bottom = (uint) row_top+1 ;
				uint col_left = (uint) playersCount/table1.NRows;
				uint col_right = (uint) col_left+1 ;

				table1.Attach(button,col_left,col_right,row_top,row_bottom);
				checkButtonsDict.Add(i, button);
				playersCount++;
			}
		}

		public List<int> PlayersChecked {
			set {
				foreach (int player in checkButtonsDict.Keys)
					checkButtonsDict[player].Active = value.Contains(player);
			}
			get {
				List<int> playersList = new List<int>();
				foreach (int player in checkButtonsDict.Keys) {
					if (checkButtonsDict[player].Active)
						playersList.Add (player);
				}
				return playersList;
			}
		}
	}
}
