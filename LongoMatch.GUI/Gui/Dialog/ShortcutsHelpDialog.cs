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
using Mono.Unix;

namespace LongoMatch.Gui.Dialog
{
	public partial class ShortcutsHelpDialog : Gtk.Dialog
	{
		string[,] player_shortcuts = {
			{"Space", Catalog.GetString ("Pause or play")},
			{"⇨", Catalog.GetString ("Step forward one frame")},
			{"⇦", Catalog.GetString ("Step backward one frame")},
			{"Shift + ⇨", Catalog.GetString ("Jump forward X seconds")},
			{"shift + ⇦", Catalog.GetString ("Jump backward X seconds")},
			{"⇧", Catalog.GetString ("Increase playback speed")},
			{"⇩", Catalog.GetString ("Decrease playback speed")},
			};
			
		public ShortcutsHelpDialog ()
		{
			this.Build ();
			
			for (int i=0; i < 7; i++){
				shortcutsvbox.PackStart(new Label(player_shortcuts[i, 0]));
				desc_vbox.PackStart(new Label(player_shortcuts[i, 1]));
			} 
			ShowAll();
		}
	}
}

