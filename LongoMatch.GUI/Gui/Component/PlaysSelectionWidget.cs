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

using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Interfaces;
using LongoMatch.Store;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlaysSelectionWidget : Gtk.Bin
	{
	
		Project project;
		PlaysFilter filter;
		PlayersFilterTreeView playersfilter;
		CategoriesFilterTreeView categoriesfilter;
		
		public PlaysSelectionWidget ()
		{
			this.Build ();
			localPlayersList.Team = Team.LOCAL;
			visitorPlayersList.Team = Team.VISITOR;
			AddFilters();
			Config.EventsBroker.TeamTagsChanged += UpdateTeamsModels;
		}
		
		#region Plubic Methods
		
		public void SetProject(Project project, bool isLive, PlaysFilter filter) {
			this.project = project;
			this.filter = filter;
			playsList.ProjectIsLive = isLive;
			localPlayersList.ProjectIsLive = isLive;
			visitorPlayersList.ProjectIsLive = isLive;
			playsList.Filter = filter;
			localPlayersList.Filter = filter;
			visitorPlayersList.Filter = filter;
			playersfilter.SetFilter(filter, project);
			categoriesfilter.SetFilter(filter, project);
			playsList.Project=project;
			visitorPlayersList.Project = project;
			localPlayersList.Project = project;
			visitorPlaysList.LabelProp = project.VisitorTeamTemplate.TeamName;
			localPlaysList.LabelProp = project.LocalTeamTemplate.TeamName;
			UpdateTeamsModels();
		}
		
		public void Clear() {
			playsList.Project = null;
			localPlayersList.Clear();
			visitorPlayersList.Clear();
			Config.EventsBroker.TeamTagsChanged -= UpdateTeamsModels;
		}
		
		public bool PlayListLoaded {
			set {
				playsList.PlayListLoaded = value;
				localPlayersList.PlayListLoaded = value;
				visitorPlayersList.PlayListLoaded = value;
			}
		}
		
		public void AddPlay(Play play) {
			playsList.AddPlay(play);
			UpdateTeamsModels();
		}
		
		public void RemovePlays (List<Play> plays) {
			playsList.RemovePlays(plays);
			UpdateTeamsModels();
		}
		#endregion
		
		void DisableFocus (Container w) {
			w.CanFocus = false;
			foreach (Widget child in w.AllChildren) {
				Console.WriteLine (child);
				if (child is Container) {
					DisableFocus (child as Container);
				} else {
					if (!(child is TreeView))
					child.CanFocus = false;
				}
			}
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
			localPlayersList.SetTeam(project.LocalTeamTemplate, project.AllPlays());
			visitorPlayersList.SetTeam(project.VisitorTeamTemplate, project.AllPlays());
		}
		
		protected void OnCategoriesFiltersbuttonClicked (object sender, System.EventArgs e)
		{
			if (catfiltersbutton.Active) {
				catfiltersbutton.Label = Catalog.GetString("Disable categories filters");
			} else {
				catfiltersbutton.Label = Catalog.GetString("Enable categories filters");
			}
			filter.CategoriesFilterEnabled = catfiltersbutton.Active;
		}
		
		protected void OnPlayersFiltersbuttonClicked (object sender, System.EventArgs e)
		{
			if (playersfiltersbutton.Active) {
				playersfiltersbutton.Label = Catalog.GetString("Disable players filters");
			} else {
				playersfiltersbutton.Label = Catalog.GetString("Enable players filters");
			}
			filter.PlayersFilterEnabled = playersfiltersbutton.Active;
		}
	}
}

