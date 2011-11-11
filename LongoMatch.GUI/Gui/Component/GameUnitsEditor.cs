// 
//  Copyright (C) 2011 andoni
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

using LongoMatch.Store;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GameUnitsEditor : Gtk.Bin
	{
		
		GameUnitsList gameUnits;
		Dictionary<GameUnit, Widget> dict;
		
		public GameUnitsEditor ()
		{
			this.Build ();
			dict = new Dictionary<GameUnit, Widget>();
			entry1.Activated += OnAddGameUnit;
			entry1.Changed += (sender, e) => {addbutton.Sensitive = entry1.Text != "";};
			addbutton.Clicked += OnAddGameUnit;
		}
		
		public void SetRootGameUnit (GameUnitsList gameUnits) {
			/* Make everything visible */
			vbox2.Visible = true;
			
			/* Clear everything first */
			if (this.gameUnits != null) {
				foreach (GameUnit gameUnit in this.gameUnits)
					RemoveGameUnit(gameUnit, false);
			}
			this.gameUnits = gameUnits;
			
			/* Add the game units one by one */
			foreach (GameUnit gameUnit in gameUnits) 
				AddGameUnit(gameUnit, false);
		}
		
		private void AddGameUnit (string name) {
			if (name == "")
				return;
			AddGameUnit(new GameUnit(name), true);
		}
		
		private void AddGameUnit (GameUnit gameUnit, bool append) {
			HBox hbox;
			Label label;
			Button button;
			
			Log.Debug("Adding new game unit" + gameUnit);
			label1.Hide();
			outerbox.Visible = true;
			
			if (append)
				gameUnits.Add(gameUnit);
			
			/* Create widget that display the game unit name and a button to remove it */
			hbox = new HBox();
			label = new Label(gameUnit.Name);
			label.Justify = Justification.Left;
			button = new Button("gtk-delete");
			button.Clicked += (sender, e) => {RemoveGameUnitAndChildren(gameUnit);};
			dict.Add(gameUnit, hbox);
			
			/* Pack everything */
			hbox.PackStart(label, false, false, (uint)((gameUnits.GameUnitDepth(gameUnit) * 10) + 10));
			hbox.PackEnd(button, false, false, 0);
			label.Show();
			button.Show();
			hbox.Show();
			phasesbox.PackStart(hbox, true, false, 0);
		}
		
		private void RemoveGameUnit (GameUnit gameUnit, bool delete) {
			phasesbox.Remove(dict[gameUnit]);
			dict[gameUnit].Destroy();
			dict.Remove(gameUnit);
			if (delete)
				gameUnits.Remove(gameUnit);
		}
		
		private void RemoveGameUnitAndChildren (GameUnit gameUnit) {
			int depth = gameUnits.GameUnitDepth(gameUnit);
			
			foreach (var g in gameUnits.GetRange(depth, gameUnits.Count - depth))
				RemoveGameUnit(g, true);
		}

		protected void OnAddGameUnit (object sender, System.EventArgs e)
		{
			AddGameUnit(entry1.Text);
			entry1.Text = "";
		}
	}
}
