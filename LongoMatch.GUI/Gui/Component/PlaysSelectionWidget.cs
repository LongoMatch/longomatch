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
using Mono.Unix;
using Gtk;

using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using Helpers = LongoMatch.Gui.Helpers;
using Gdk;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlaysSelectionWidget : Gtk.Bin
	{
	
		Project project;
		EventsFilter filter;
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

			localPlayersList.Team = Team.LOCAL;
			visitorPlayersList.Team = Team.VISITOR;
			AddFilters();
			Config.EventsBroker.TeamTagsChanged += UpdateTeamsModels;
			playsnotebook.Page = 0;
			notebook.Page = currentPage = 0;
			
			notebook.SwitchPage += HandleSwitchPage;
			SetTabProps (playsnotebook, false);
			SetTabProps (playlistwidget, false);
			SetTabProps (filtersvbox, false);
		}

		protected override void OnDestroyed ()
		{
			Config.EventsBroker.TeamTagsChanged -= UpdateTeamsModels;
			playsList.Project = null;
			localPlayersList.Clear();
			visitorPlayersList.Clear();
			playsList1.Destroy ();
			playlistwidget.Destroy ();
			base.OnDestroyed ();
		}
		
		#region Plubic Methods
		
		public void SetProject(Project project, EventsFilter filter) {
			this.project = project;
			this.filter = filter;
			playsList.Filter = filter;
			localPlayersList.Filter = filter;
			visitorPlayersList.Filter = filter;
			playersfilter.SetFilter(filter, project);
			categoriesfilter.SetFilter(filter, project);
			playsList.Project=project;
			visitorPlayersList.Project = project;
			localPlayersList.Project = project;
			playlistwidget.Project = project;
			visitorPlaysList.LabelProp = project.VisitorTeamTemplate.TeamName;
			localPlaysList.LabelProp = project.LocalTeamTemplate.TeamName;
			UpdateTeamsModels();
		}
		
		public void AddPlay(TimelineEvent play) {
			playsList.AddPlay(play);
			UpdateTeamsModels();
		}
		
		public void RemovePlays (List<TimelineEvent> plays) {
			playsList.RemovePlays(plays);
			UpdateTeamsModels();
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

			if (widget == playsnotebook) {
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

		void AddFilters() {
			ScrolledWindow s1 = new ScrolledWindow();
			ScrolledWindow s2 = new ScrolledWindow();
			
			playersfilter = new PlayersFilterTreeView();
			categoriesfilter = new CategoriesFilterTreeView();
			
			s1.Add(categoriesfilter);
			s2.Add(playersfilter);
			filtersnotebook.AppendPage(s1, new Gtk.Label(Catalog.GetString("Categories filter")));
			filtersnotebook.AppendPage(s2, new Gtk.Label(Catalog.GetString("Players filter")));
			filtersnotebook.ShowAll();
		}
		
		private void UpdateTeamsModels() {
			if (project == null)
				return;
			localPlayersList.SetTeam(project.LocalTeamTemplate, project.Timeline);
			visitorPlayersList.SetTeam(project.VisitorTeamTemplate, project.Timeline);
		}
	}
}

