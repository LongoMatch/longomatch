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
using VAS.UI.Menus;

namespace LongoMatch.Gui.Menus
{
	public class PlayerMenu : Menu
	{
		Menu playerMenu;
		MenuItem addToPlaylistMenu, exportToVideoFile;

		public PlayerMenu ()
		{
			playerMenu = new Menu ();
			addToPlaylistMenu = new MenuItem ("");
			playerMenu.Add (addToPlaylistMenu);
			exportToVideoFile = new MenuItem ("");
			exportToVideoFile.Add (exportToVideoFile);
		}

		public void ShowMenu (Project project, IEnumerable<TimelineEvent> events)
		{
			if (events.Count () > 0) {
				MenuHelpers.FillAddToPlaylistMenu (addToPlaylistMenu, project.Playlists, events);
				MenuHelpers.FillExportToVideoFileMenu (addToPlaylistMenu, project, events,
													   Catalog.GetString ("Export to video file"));
				Popup ();
			}
		}
	}
}
