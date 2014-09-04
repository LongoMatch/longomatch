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

using LongoMatch.Store;
using LongoMatch.Core.Handlers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GameUnitsTagger : Gtk.Bin
	{
		public event GameUnitHandler GameUnitEvent;
		
		List<GameUnitWidget> widgets;
		
		public GameUnitsTagger ()
		{
			this.Build ();
			widgets = new List<GameUnitWidget>();
		}
		
		public GameUnitsList GameUnits {
			set {
				if (widgets.Count != 0) {
					foreach (var widget in widgets) {
						gameunitsbox1.Remove(widget);
						widget.Destroy();
					}
					widgets.Clear();
				}
				SetGameUnitsWidgets(value);
			}
		}

		public Time CurrentTime {
			set{
				foreach (GameUnitWidget guw in widgets) 
					guw.CurrentTime = value;
			}
		}
		
		private void SetGameUnitsWidgets(GameUnitsList gameUnits) {
			foreach (GameUnit gameUnit in gameUnits) {
				GameUnitWidget guw = new GameUnitWidget(gameUnit);
				guw.GameUnit = gameUnit;
				widgets.Add(guw);
				guw.GameUnitEvent += (g, t) => {GameUnitEvent(g, t);};
				guw.Show();
				gameunitsbox1.PackStart(guw, false , true, 0);
			}
		}
	}
}

