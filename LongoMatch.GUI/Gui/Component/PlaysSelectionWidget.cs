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
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using VAS.Core;
using VAS.Core.Common;
using VAS.UI.Helpers;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlaysSelectionWidget : Gtk.Bin
	{
		const int PAGE_CATEGORIES = 0;
		const int PAGE_PLAYERS = 1;
		const int PAGE_PLAYLISTS = 1;
		const int PAGE_FILTERS = 2;

		PlayersFilterTreeView playersfilter;
		CategoriesFilterTreeView categoriesfilter;
		Helpers.IconNotebookHelper notebookHelper, notebookHelperPlaylist, notebookHelperFilter;

		public PlaysSelectionWidget ()
		{
			this.Build ();

			LoadIcons ();
			AddFilters ();
			Helpers.Misc.SetFocus (this, false, typeof (TreeView));
			eventbox.ModifyBg (StateType.Normal, Helpers.Misc.ToGdkColor (App.Current.Style.PaletteBackground));
			hseparator1.ModifyBg (StateType.Normal, Helpers.Misc.ToGdkColor (App.Current.Style.PaletteBackgroundLight));
			notebook.Page = 0;
			filtersnotebook.Page = PAGE_CATEGORIES;
			clearButton.Clicked += HandleClearClicked;
			hbox3.NoShowAll = true;
		}

		public bool ExpandTabs {
			set {
				if (value) {
					notebook.GetNthPage (PAGE_FILTERS).Reparent (notebookFilter);
					notebook.GetNthPage (PAGE_PLAYLISTS).Reparent (notebookPlaylist);
					notebookHelperPlaylist.UpdateTabs ();
					notebookHelperFilter.UpdateTabs ();
					notebook.TabPos = PositionType.Top;
				} else {
					notebookPlaylist.GetNthPage (0).Reparent (notebook);
					notebookFilter.GetNthPage (0).Reparent (notebook);
					notebookHelper.UpdateTabs ();
					notebook.TabPos = PositionType.Left;
				}
				notebookPlaylist.Visible = value;
				notebookFilter.Visible = value;
			}
		}

		protected override void OnDestroyed ()
		{
			eventslistwidget.Destroy ();
			playlistwidget.Destroy ();
			base.OnDestroyed ();
		}

		#region Plubic Methods

		public void SetProject (LMProject project, EventsFilter filter)
		{
			eventslistwidget.SetProject (project, filter);
			playersfilter.SetFilter (filter, project);
			categoriesfilter.SetFilter (filter, project);
			playlistwidget.Project = project;
		}

		public void AddPlay (LMTimelineEvent play)
		{
			eventslistwidget.AddPlay (play);
		}

		public void RemovePlays (List<LMTimelineEvent> plays)
		{
			eventslistwidget.RemovePlays (plays);
		}

		#endregion

		void LoadIcons ()
		{
			notebookHelperFilter = new IconNotebookHelper (notebookFilter);
			notebookHelperFilter.SetTabIcon (filtersvbox, "longomatch-tab-filter", "longomatch-tab-active-filter",
				Catalog.GetString ("Filters"));
			notebookHelperPlaylist = new IconNotebookHelper (notebookPlaylist);
			notebookHelperPlaylist.SetTabIcon (playlistwidget, "longomatch-tab-playlist", "longomatch-tab-active-playlist",
				Catalog.GetString ("Playlists"));
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
			playersfilter.Name = "backgroundtreeviewplayers";
			categoriesfilter = new CategoriesFilterTreeView ();
			categoriesfilter.Name = "backgroundtreeviewcategories";

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

		void HandleClearClicked (object sender, EventArgs e)
		{
			if (filtersnotebook.Page == PAGE_CATEGORIES) {
				categoriesfilter.ToggleAll (false);
			} else {
				playersfilter.ToggleAll (false);
			}
		}
	}
}

