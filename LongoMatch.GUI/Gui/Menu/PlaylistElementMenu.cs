//
//  Copyright (C) 2015 Fluendo S.A.
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
using Gdk;
using Gtk;
using LongoMatch.Core;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Gui.Dialog;

namespace LongoMatch.Gui.Menus
{
	public class PlaylistElementMenu
	{
		Menu menu;
		MenuItem editMenu, deleteMenu, prependMenu, appendMenu;
		readonly Playlist playlist;
		readonly List<IPlaylistElement> elements;
		readonly Widget widget;

		public PlaylistElementMenu (Widget widget, Playlist playlist, List<IPlaylistElement> elements)
		{
			this.widget = widget;
			this.playlist = playlist;
			this.elements = elements;
			if (elements.Count > 0) {
				CreateMenu ();
			} else {
				menu = null;
			}
		}

		public void Popup ()
		{
			menu?.Popup ();
		}

		void AddVideo (IPlaylistElement element, bool prepend)
		{
			MediaFile file = LongoMatch.Gui.Helpers.Misc.OpenFile (widget);
			if (file != null) {
				PlaylistVideo video = new PlaylistVideo (file);
				int index = playlist.Elements.IndexOf (element);
				if (!prepend) {
					index++;
				}
				playlist.Elements.Insert (index, video);
			}
		}

		void AddImage (IPlaylistElement element, bool prepend)
		{
			Pixbuf pix = LongoMatch.Gui.Helpers.Misc.OpenImage (widget);
			if (pix != null) {
				var image = new LongoMatch.Core.Common.Image (pix);
				PlaylistImage plimage = new PlaylistImage (image, new Time (5000));
				int index = playlist.Elements.IndexOf (element);
				if (!prepend) {
					index++;
				}
				playlist.Elements.Insert (index, plimage);
			}
		}

		Menu CreateExternalsMenu (IPlaylistElement element, bool prepend)
		{
			Menu addMenu = new Menu ();
			MenuItem video = new MenuItem (Catalog.GetString ("External video"));
			video.Activated += (sender, e) => AddVideo (element, prepend);
			addMenu.Append (video);
			MenuItem stillImage = new MenuItem (Catalog.GetString ("External image"));
			stillImage.Activated += (sender, e) => AddImage (element, prepend);
			addMenu.Append (stillImage);
			return addMenu;
		}

		void CreateMenu ()
		{
			IPlaylistElement first, last;

			menu = new Menu ();

			first = playlist.Elements.First (elements.Contains);
			last = playlist.Elements.Last (elements.Contains);

			if (elements.Count == 1 && !(first is PlaylistVideo)) {
				editMenu = new MenuItem (Catalog.GetString ("Edit properties"));
				editMenu.Activated += (sender, e) => {
					var dialog = new EditPlaylistElementProperties ((Gtk.Window)widget.Toplevel, first);
					dialog.Run ();
					dialog.Destroy ();
				};
				menu.Append (editMenu);
			}

			prependMenu = new MenuItem (Catalog.GetString ("Insert before"));
			prependMenu.Submenu = CreateExternalsMenu (first, true);
			menu.Append (prependMenu);

			appendMenu = new MenuItem (Catalog.GetString ("Insert after"));
			appendMenu.Submenu = CreateExternalsMenu (last, false);
			menu.Append (appendMenu);

			deleteMenu = new MenuItem (Catalog.GetString ("Delete"));
			deleteMenu.Activated += (sender, e) => elements.ForEach (el => playlist.Remove (el));
			menu.Append (deleteMenu);
			menu.ShowAll ();
		}
	}
}

