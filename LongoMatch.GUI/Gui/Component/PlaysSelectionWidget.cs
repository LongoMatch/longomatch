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
using Gdk;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using Mono.Unix;
using Helpers = LongoMatch.Gui.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlaysSelectionWidget : Gtk.Bin
	{
	
		Project project;
		PlayersFilterTreeView playersfilter;
		CategoriesFilterTreeView categoriesfilter;
		Pixbuf listIco, listActiveIco;
		Pixbuf playlistIco, playlistActiveIco;
		Pixbuf filtersIco, filtersActiveIco;
		int currentPage;

		public PlaysSelectionWidget ()
		{
			this.Build ();
			
			LoadIcons ();

			AddFilters ();
			notebook.Page = currentPage = 0;
			
			notebook.SwitchPage += HandleSwitchPage;
			SetTabProps (eventslistwidget, false);
			SetTabProps (playlistwidget, false);
			SetTabProps (filtersvbox, false);
			LongoMatch.Gui.Helpers.Misc.SetFocus (this, false, typeof(TreeView));
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
			this.project = project;
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
			int s = StyleConf.NotebookTabIconSize;
			IconLookupFlags f = IconLookupFlags.ForceSvg;
 
			listIco = Helpers.Misc.LoadIcon ("longomatch-tab-dashboard", s, f);
			listActiveIco = Helpers.Misc.LoadIcon ("longomatch-tab-active-dashboard", s, f);
			filtersIco = Helpers.Misc.LoadIcon ("longomatch-tab-filter", s, f);
			filtersActiveIco = Helpers.Misc.LoadIcon ("longomatch-tab-active-filter", s, f);
			playlistIco = Helpers.Misc.LoadIcon ("longomatch-tab-playlist", s, f);
			playlistActiveIco = Helpers.Misc.LoadIcon ("longomatch-tab-active-playlist", s, f);
		}

		void SetTabProps (Widget widget, bool active)
		{
			Gdk.Pixbuf icon;
			Gtk.Image img;

			img = notebook.GetTabLabel (widget) as Gtk.Image;
			if (img == null) {
				img = new Gtk.Image ();
				img.WidthRequest = StyleConf.NotebookTabSize;
				img.HeightRequest = StyleConf.NotebookTabSize;
				notebook.SetTabLabel (widget, img);
			}

			if (widget == eventslistwidget) {
				icon = active ? listActiveIco : listIco;
			} else if (widget == filtersvbox) {
				icon = active ? filtersActiveIco : filtersIco;
			} else if (widget == playlistwidget) {
				icon = active ? playlistActiveIco : playlistIco;
			} else {
				return;
			}
			img.Pixbuf = icon;
		}

		void HandleSwitchPage (object o, SwitchPageArgs args)
		{
			SetTabProps (notebook.GetNthPage (currentPage), false);
			SetTabProps (notebook.GetNthPage ((int)args.PageNum), true);
			currentPage = (int)args.PageNum;
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
	}
}

