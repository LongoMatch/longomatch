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
using Gtk;
using LongoMatch.Core.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.UI.Helpers;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlaysSelectionWidget : Gtk.Bin, IView<LMProjectVM>
	{
		const int PAGE_CATEGORIES = 0;
		const int PAGE_PLAYERS = 1;
		const int PAGE_PLAYLISTS = 1;
		const int PAGE_FILTERS = 2;

		EventsFilterTreeView categoriesfilter;
		EventsFilterTreeView playersfilter;
		LMProjectVM viewModel;
		IconNotebookHelper notebookHelper, notebookHelperPlaylist, notebookHelperFilter;

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

		protected override void OnDestroyed ()
		{
			eventslistwidget.Destroy ();
			playlistwidget.Destroy ();
			base.OnDestroyed ();
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

		public LMProjectVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				eventslistwidget.ViewModel = value;
				playlistwidget.ViewModel = value?.Playlists;
				categoriesfilter.Predicate = (value?.Timeline as LMTimelineVM)?.CategoriesPredicate;
				playersfilter.Predicate = (value?.Timeline as LMTimelineVM)?.TeamsPredicate;
			}
		}

		#region Plubic Methods

		public void SetViewModel (object viewModel)
		{
			ViewModel = viewModel as LMProjectVM;
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

			playersfilter = new EventsFilterTreeView ();
			playersfilter.Name = "backgroundtreeviewplayers";
			categoriesfilter = new EventsFilterTreeView ();
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

