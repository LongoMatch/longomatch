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
using Gtk;
using LongoMatch.Core.Store;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Store;
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
		IEnumerable<TimelineEvent> events;
		EventType eventType;

		public EventTypeMenu ()
		{
			FillMenu ();
		}

		public void ShowMenu (ProjectLongoMatch project, EventType eventType,
							  IList<TimelineEventLongoMatch> events)
		{
			this.eventType = eventType;
			this.events = events;
			SetupSortMenu ();
			MenuHelpers.FillAddToPlaylistMenu (addToPlaylistMenuItem, project.Playlists, events);
			MenuHelpers.FillExportToVideoFileMenu (exportToVideoFileItem, project, events);
			Popup ();
		}

		void FillMenu ()
		{
			editItem = new MenuItem (Catalog.GetString ("Edit properties"));

			sortItem = new MenuItem ();
			sortMenu = new Menu ();

			sortByName = new RadioMenuItem (Catalog.GetString ("Sort by name"));
			sortByStart = new RadioMenuItem (Catalog.GetString ("Sort by start time"));
			sortByStop = new RadioMenuItem (Catalog.GetString ("Sort by stop time"));
			sortByDuration = new RadioMenuItem (Catalog.GetString ("Sort by duration"));

			sortByName.Group = new GLib.SList (IntPtr.Zero);
			sortByStart.Group = sortByName.Group;
			sortByStop.Group = sortByName.Group;
			sortByDuration.Group = sortByName.Group;

			addToPlaylistMenuItem = new MenuItem ();
			exportToVideoFileItem = new MenuItem ();

			Add (editItem);
			Add (sortMenu);
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
			MenuHelpers.EmitRenderPlaylist (events);
		}
	}
}
