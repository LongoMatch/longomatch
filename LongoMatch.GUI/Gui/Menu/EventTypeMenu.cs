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
using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.UI.Menus;

namespace LongoMatch.Gui.Menus
{
	public class EventTypeMenu : Menu
	{
		public event EditEventTypeHandler EditProperties;
		public event EventHandler SortEvent;

		Menu sortMenu;
		MenuItem addToPlaylistMenuItem, exportToVideoFileItem, editItem, sortItem;
		RadioMenuItem sortByName, sortByStart, sortByStop, sortByDuration;
		IEnumerable<TimelineEventVM> eventVMs;
		EventType eventType;

		public EventTypeMenu ()
		{
			FillMenu ();
		}

		public void ShowMenu (LMProject project, EventType eventType, IEnumerable<LMTimelineEventVM> eventVMs)
		{
			this.eventType = eventType;
			this.eventVMs = eventVMs;
			SetupSortMenu ();
			var playlistVMs = project.Playlists.Select (pl => new PlaylistVM { Model = pl });

			MenuHelpers.FillAddToPlaylistMenu (addToPlaylistMenuItem, playlistVMs, eventVMs);
			MenuHelpers.FillExportToVideoFileMenu (exportToVideoFileItem, project, eventVMs, Catalog.GetString ("Export to video file"));
			Popup ();
		}

		void FillMenu ()
		{
			editItem = new MenuItem (Catalog.GetString ("Edit properties"));

			sortItem = new MenuItem (Catalog.GetString ("Sort Method"));
			sortMenu = new Menu ();

			sortByName = new RadioMenuItem (Catalog.GetString ("Sort by name"));
			sortByStart = new RadioMenuItem (Catalog.GetString ("Sort by start time"));
			sortByStop = new RadioMenuItem (Catalog.GetString ("Sort by stop time"));
			sortByDuration = new RadioMenuItem (Catalog.GetString ("Sort by duration"));

			sortByName.Group = new GLib.SList (IntPtr.Zero);
			sortByName.Active = false;
			sortByStart.Group = sortByName.Group;
			sortByStart.Active = false;
			sortByStop.Group = sortByName.Group;
			sortByStop.Active = false;
			sortByDuration.Group = sortByName.Group;
			sortByDuration.Active = false;

			addToPlaylistMenuItem = new MenuItem ();
			exportToVideoFileItem = new MenuItem ();

			Add (editItem);
			Add (sortItem);
			sortItem.Submenu = sortMenu;
			sortMenu.Add (sortByName);
			sortMenu.Add (sortByStart);
			sortMenu.Add (sortByStop);
			sortMenu.Add (sortByDuration);
			Add (addToPlaylistMenuItem);
			Add (exportToVideoFileItem);

			sortByName.Activated += OnSortActivated;
			sortByStart.Activated += OnSortActivated;
			sortByStop.Activated += OnSortActivated;
			sortByDuration.Activated += OnSortActivated;

			editItem.Activated += (s, e) => EditProperties (eventType);
			exportToVideoFileItem.Activated += HandleExportEvents;
			sortByName.Active = true;
			ShowAll ();
		}

		void SetupSortMenu ()
		{
			switch (eventType.SortMethod) {
			case SortMethodType.SortByName:
				sortByName.Active = true;
				break;
			case SortMethodType.SortByStartTime:
				sortByStart.Active = true;
				break;
			case SortMethodType.SortByStopTime:
				sortByStop.Active = true;
				break;
			default:
				sortByDuration.Active = true;
				break;
			}
		}

		void OnSortActivated (object o, EventArgs args)
		{
			RadioMenuItem sender;

			sender = o as RadioMenuItem;

			if (sender == sortByName)
				eventType.SortMethod = SortMethodType.SortByName;
			else if (sender == sortByStart)
				eventType.SortMethod = SortMethodType.SortByStartTime;
			else if (sender == sortByStop)
				eventType.SortMethod = SortMethodType.SortByStopTime;
			else
				eventType.SortMethod = SortMethodType.SortByDuration;

			// Redorder plays
			if (SortEvent != null) {
				SortEvent (this, null);
			}
		}

		void HandleExportEvents (object sender, EventArgs e)
		{
			MenuHelpers.EmitRenderPlaylist (eventVMs);
		}
	}
}
