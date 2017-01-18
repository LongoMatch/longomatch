//
//  Copyright (C) 2017 Fluendo S.A.
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
using LongoMatch.Core;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Menus;
using VAS.Core.Interfaces;
using VAS.Core.Store.Playlists;
using VAS.UI.Common;

namespace LongoMatch.Gui.Component
{
	public class LMPlaylistTreeView : PlaylistTreeView
	{
		public LMPlaylistTreeView ()
		{
			ShowExpanders = false;
		}

		protected override void CreateViews ()
		{
			CellRenderer descCell = new PlaysCellRenderer ();
			AppendColumn (null, descCell, RenderPlaylist);
		}

		protected override void RenderPlaylist (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			var item = model.GetValue (iter, 0);
			PlaysCellRenderer c = cell as PlaysCellRenderer;
			c.Item = item;
			c.Count = model.IterNChildren (iter);
		}

		protected override void ShowMenu ()
		{
			base.ShowMenu ();
		}

		void ShowPlaylistElementMenu (Playlist playlist, IPlaylistElement element)
		{
			PlaylistElementMenu menu = new PlaylistElementMenu (this, playlist, new List<IPlaylistElement> { element });
			menu.Popup ();
		}

		void ShowPlaylistMenu (Playlist playlist)
		{
			Menu menu;
			MenuItem edit, delete, render;

			menu = new Menu ();

			edit = new MenuItem (Catalog.GetString ("Edit name"));
			edit.Activated += (sender, e) => {
				string name = App.Current.Dialogs.QueryMessage (Catalog.GetString ("Name:"), null,
								  playlist.Name).Result;
				if (!String.IsNullOrEmpty (name)) {
					playlist.Name = name;
				}
			};
			menu.Append (edit);

			render = new MenuItem (Catalog.GetString ("Render"));
			//render.Activated += (sender, e) => ViewModel.Render (playlist);
			menu.Append (render);

			delete = new MenuItem (Catalog.GetString ("Delete"));
			//delete.Activated += (sender, e) => ViewModel.Delete (playlist);
			menu.Append (delete);

			menu.ShowAll ();
			menu.Popup ();
		}

	}
}
