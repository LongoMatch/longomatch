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
using System.Collections.Generic;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Helpers;
using LongoMatch.Core;
using Helpers = LongoMatch.Gui.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlaysSelectionWidget : Gtk.Bin
	{
		const int PAGE_CATEGORIES = 0;
		const int PAGE_PLAYERS = 1;

		PlayersFilterTreeView playersfilter;
		CategoriesFilterTreeView categoriesfilter;
		IconNotebookHelper notebookHelper;

		public PlaysSelectionWidget ()
		{
			this.Build ();
			
			LoadIcons ();

			AddFilters ();

			LongoMatch.Gui.Helpers.Misc.SetFocus (this, false, typeof(TreeView));

			notebook.Page = 0;
			filtersnotebook.Page = PAGE_CATEGORIES;
			allButton.Clicked += HandleButtonClicked;
			noneButton.Clicked += HandleButtonClicked;
		}

		protected override void OnDestroyed ()
		{
			eventslistwidget.Destroy ();
			playlistwidget.Destroy ();
			base.OnDestroyed ();
		}

		#region Plubic Methods

		public void SetProject (Project project, EventsFilter filter)
		{
			eventslistwidget.SetProject (project, filter);
			playersfilter.SetFilter (filter, project);
			categoriesfilter.SetFilter (filter, project);
			playlistwidget.Project = project;
		}

		public void AddPlay (TimelineEvent play)
		{
			eventslistwidget.AddPlay (play);
		}

		public void RemovePlays (List<TimelineEvent> plays)
		{
			eventslistwidget.RemovePlays (plays);
		}

		#endregion

		void LoadIcons ()
		{
			notebookHelper = new IconNotebookHelper (notebook);
			notebookHelper.SetTabIcon (eventslistwidget, "longomatch-tab-dashboard", "longomatch-tab-active-dashboard",
				Catalog.GetString ("Events List"));
			notebookHelper.SetTabIcon (filtersvbox, "longomatch-tab-filter", "longomatch-tab-active-filter",
				Catalog.GetString ("Filters"));
			notebookHelper.SetTabIcon (playlistwidget, "longomatch-tab-playlist", "longomatch-tab-active-playlist",
				Catalog.GetString ("Playlists"));

			notebookHelper.UpdateTabs ();
		}

		void AddFilters ()
		{
			Label l;
			ScrolledWindow s1 = new ScrolledWindow ();
			ScrolledWindow s2 = new ScrolledWindow ();
			
			playersfilter = new PlayersFilterTreeView ();
			categoriesfilter = new CategoriesFilterTreeView ();
			
			s1.Add (categoriesfilter);
			s2.Add (playersfilter);
			l = new Gtk.Label (Catalog.GetString ("Categories filter"));
			l.HeightRequest = StyleConf.PlayerCapturerControlsHeight;
			filtersnotebook.AppendPage (s1, l);
			l = new Gtk.Label (Catalog.GetString ("Players filter"));
			l.HeightRequest = StyleConf.PlayerCapturerControlsHeight;
			filtersnotebook.AppendPage (s2, l);
			filtersnotebook.ShowAll ();
		}

		void HandleButtonClicked (object sender, EventArgs e)
		{
			bool active = sender == allButton;
			if (filtersnotebook.Page == PAGE_CATEGORIES) {
				categoriesfilter.ToggleAll (active);
			} else {
				playersfilter.ToggleAll (active);
			}
		}
	}
}

