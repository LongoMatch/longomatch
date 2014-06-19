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
	
		MenuItem newPlay, del, tag, addPLN, snapshot, render;
		List<Play> plays;
		Category cat;
		Time time;
		MediaFile projectFile;
	
		
		public PlaysMenu ()
		{
			CreateMenu ();
		}
		
		public void ShowMenu (List<Play> plays) {
			ShowMenu (plays, null, null, null);
		}
		
		public void ShowMenu (List<Play> plays, Category cat, Time time,
		                      MediaFile projectFile) {
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
			
			tag.Visible = plays.Count == 1;
			snapshot.Visible = plays.Count == 1;
			del.Visible = plays.Count > 0;;
			addPLN.Visible = plays.Count > 0;;
			render.Visible = plays.Count > 0;;

			if (plays.Count > 0 ) {
				string label = String.Format ("{0} ({1})",Catalog.GetString("Delete"), plays.Count);
				GtkGlue.MenuItemSetLabel (del, label);
				label = String.Format ("{0} ({1})",Catalog.GetString("Add to playlist"), plays.Count);
				GtkGlue.MenuItemSetLabel (addPLN, label);
				label = String.Format ("{0} ({1})", Catalog.GetString("Export to video file"), plays.Count);
				GtkGlue.MenuItemSetLabel (render, label);
			}
			Popup();
		}
		
		void CreateMenu () {
			newPlay = new MenuItem("");
			Add (newPlay);
			newPlay.Activated += HandleNePlayActivated;

			tag = new MenuItem(Catalog.GetString("Edit tags"));
			tag.Activated += (sender, e) => Config.EventsBroker.EmitTagPlay (plays[0]);
			Add (tag);
			
			snapshot = new MenuItem(Catalog.GetString("Export to PGN images"));
			snapshot.Activated += (sender, e) => Config.EventsBroker.EmitSnapshotSeries (plays[0]);
			Add (snapshot);

			del = new MenuItem ("");
			del.Activated += (sender, e) => Config.EventsBroker.EmitPlaysDeleted (plays);
			Add (del);
			
			addPLN = new MenuItem ("");
			addPLN.Activated += (sender, e) => Config.EventsBroker.EmitPlayListNodeAdded (plays);
			Add (addPLN);
			
			render = new MenuItem ("");
			render.Activated += (sender, e) => EmitRenderPlaylist (plays);
			Add (render);
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
