//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using LongoMatch.Handlers;
using Mono.Unix;
using LongoMatch.Common;

namespace LongoMatch.Gui.Menus
{
	public class PlaysMenu: Gtk.Menu
	{
	
		public event EventHandler EditNameEvent;

		MenuItem edit, newPlay, del, addPLN, snapshot, render, duplicate, moveCat;
		List<Play> plays;
		Category cat;
		Time time;
		MediaFile projectFile;
	
		
		public PlaysMenu ()
		{
			CreateMenu ();
		}
		
		public void ShowListMenu (List<Play> plays, MediaFile projectFile,
		                          List<Category> categories) {
			ShowMenu (plays, null, null, null, categories, true);
		}

		public void ShowMenu (List<Play> plays, MediaFile projectFile) {
			ShowMenu (plays, null, null, projectFile, null, false);
		}
		
		public void ShowTimelineMenu (List<Play> plays, Category cat, Time time,
		                              MediaFile projectFile)
		{
			ShowMenu (plays, cat, time, projectFile, null, false);
		}
		
		private void ShowMenu (List<Play> plays, Category cat, Time time,
		                       MediaFile projectFile, List<Category> categories,
		                       bool editableName) {
			this.plays = plays;
			this.cat = cat;
			this.time = time;
			this.projectFile = projectFile;

			if (cat != null) {
				string label = String.Format ("{0} in {1}", Catalog.GetString("Add new play"), cat.Name);
				GtkGlue.MenuItemSetLabel (newPlay, label); 
				newPlay.Visible = true;
			} else {
				newPlay.Visible = false;
			}
			
			if (plays == null)
				plays = new List<Play> ();
			
			edit.Visible = editableName;
			snapshot.Visible = plays.Count == 1;
			moveCat.Visible = plays.Count == 1 && categories != null;
			del.Visible = plays.Count > 0;
			addPLN.Visible = plays.Count > 0;
			render.Visible = plays.Count > 0;
			duplicate.Visible = plays.Count > 0;

			if (plays.Count > 0 ) {
				string label = String.Format ("{0} ({1})",Catalog.GetString("Delete"), plays.Count);
				GtkGlue.MenuItemSetLabel (del, label);
				label = String.Format ("{0} ({1})",Catalog.GetString("Add to playlist"), plays.Count);
				GtkGlue.MenuItemSetLabel (addPLN, label);
				label = String.Format ("{0} ({1})", Catalog.GetString("Export to video file"), plays.Count);
				GtkGlue.MenuItemSetLabel (render, label);
				label = String.Format ("{0} ({1})", Catalog.GetString("Duplicate "), plays.Count);
				GtkGlue.MenuItemSetLabel (duplicate, label);
			}
			
			if (moveCat.Visible) {
				Menu catMenu = new Menu();
				foreach (Category c in categories) {
					if (plays[0].Category == c)
						continue;
					var item = new MenuItem (c.Name);
					catMenu.Append (item);
					item.Activated += (sender, e) => {
						Config.EventsBroker.EmitPlayCategoryChanged (plays[0], c);
					}; 
				}
				catMenu.ShowAll();
				moveCat.Submenu = catMenu;
			}
			
			Popup();
		}
		
		void CreateMenu () {
			newPlay = new MenuItem("");
			Add (newPlay);
			newPlay.Activated += HandleNePlayActivated;

			edit = new MenuItem (Catalog.GetString ("Edit name"));
			edit.Activated += (sender, e) => {
				if (EditNameEvent != null) {
					EditNameEvent (this, null);
				}
			};
			Add (edit);

			moveCat = new MenuItem (Catalog.GetString ("Move to"));
			Add (moveCat);

			del = new MenuItem ("");
			del.Activated += (sender, e) => Config.EventsBroker.EmitPlaysDeleted (plays);
			Add (del);
			
			duplicate = new MenuItem ("");
			duplicate.Activated += (sender, e) => Config.EventsBroker.EmitDuplicatePlay (plays);
			Add (duplicate);

			addPLN = new MenuItem ("");
			addPLN.Activated += (sender, e) => Config.EventsBroker.EmitPlayListNodeAdded (plays);
			Add (addPLN);
			
			render = new MenuItem ("");
			render.Activated += (sender, e) => EmitRenderPlaylist (plays);
			Add (render);
			
			snapshot = new MenuItem(Catalog.GetString("Export to PGN images"));
			snapshot.Activated += (sender, e) => Config.EventsBroker.EmitSnapshotSeries (plays[0]);
			Add (snapshot);

			ShowAll ();
		}

		void HandleNePlayActivated (object sender, EventArgs e)
		{
			Config.EventsBroker.EmitNewTagAtPos (cat, time);
		}
		
		void EmitRenderPlaylist (List<Play> plays)
		{
			PlayList pl = new PlayList();
			foreach (Play p in plays) {
				pl.Add (new PlayListPlay (p, projectFile, true));
			}
			Config.EventsBroker.EmitRenderPlaylist (pl);
		}
	}
}
