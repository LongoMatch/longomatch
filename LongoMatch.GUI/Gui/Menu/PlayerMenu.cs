//
//  Copyright (C) 2016 Fluendo S.A.
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
using VAS.Core;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.UI.Menus;

namespace LongoMatch.Gui.Menus
{
	public class PlayerMenu : Menu
	{
		MenuItem addToPlaylistMenu, exportToVideoFile;

		public PlayerMenu ()
		{
			addToPlaylistMenu = new MenuItem ("");
			Add (addToPlaylistMenu);
			exportToVideoFile = new MenuItem ("");
			Add (exportToVideoFile);
		}

		public void ShowMenu (Project project, IEnumerable<TimelineEventVM> eventVMs)
		{
			if (eventVMs.Count () > 0) {
				var playlistVMs = project.Playlists.Select (pl => new PlaylistVM { Model = pl });
				MenuHelpers.FillAddToPlaylistMenu (addToPlaylistMenu, playlistVMs, eventVMs);
				MenuHelpers.FillExportToVideoFileMenu (exportToVideoFile, project, eventVMs,
													   Catalog.GetString ("Export to video file"));
				Popup ();
			}
		}
	}
}
