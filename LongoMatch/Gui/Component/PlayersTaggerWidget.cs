// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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

using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Gui.Dialog;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayersTaggerWidget : Gtk.Bin
	{
		private List<PlayerTag> players;
		private TeamTemplate template;
		
		public PlayersTaggerWidget (String subcategoryName, TeamTemplate template, List<PlayerTag> players) {
			this.Build ();
			this.players = players;
			this.template = template;
			CategoryLabel.Markup = "<b>" + subcategoryName + "</b>";
			LoadTagsLabel();
		}
		
		private void LoadTagsLabel () {
			var playersNames = players.Select(p => p.Value.Name).ToArray();
			playerslabel.Text = String.Join(" ; ", playersNames);
		}
		
		protected virtual void OnEditClicked (object sender, System.EventArgs e)
		{
			PlayersSelectionDialog dialog = new PlayersSelectionDialog();
			dialog.TransientFor = this.Toplevel as Gtk.Window;
			dialog.Template = template;
			dialog.SelectedPlayers = players;
			dialog.Run();
			dialog.Destroy();
			LoadTagsLabel();
		}
	}
}

