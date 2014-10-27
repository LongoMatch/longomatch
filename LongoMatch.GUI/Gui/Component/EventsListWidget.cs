//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;
using System.Collections.Generic;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EventsListWidget : Gtk.Bin
	{
		Project project;

		public EventsListWidget ()
		{
			this.Build ();
			localPlayersList.Team = Team.LOCAL;
			visitorPlayersList.Team = Team.VISITOR;
			playsnotebook.Page = 0;
			playsList1.HeightRequest = StyleConf.PlayerCapturerControlsHeight;
			Config.EventsBroker.TeamTagsChanged += UpdateTeamsModels;
		}
		
		protected override void OnDestroyed ()
		{
			Config.EventsBroker.TeamTagsChanged -= UpdateTeamsModels;
			playsList.Project = null;
			localPlayersList.Clear();
			visitorPlayersList.Clear();
			playsList1.Destroy ();
			base.OnDestroyed ();
		}
		
		public void SetProject (Project project, EventsFilter filter)
		{
			this.project = project;
			playsList.Filter = filter;
			localPlayersList.Filter = filter;
			visitorPlayersList.Filter = filter;
			playsList.Project = project;
			visitorPlayersList.Project = project;
			localPlayersList.Project = project;
			visitorPlaysList.LabelProp = project.VisitorTeamTemplate.TeamName;
			localPlaysList.LabelProp = project.LocalTeamTemplate.TeamName;
			UpdateTeamsModels ();
		}
		
		public void AddPlay(TimelineEvent play) {
			playsList.AddPlay(play);
			localPlayersList.AddEvent (play);
			visitorPlayersList.AddEvent (play);
		}
		
		public void RemovePlays (List<TimelineEvent> plays) {
			playsList.RemovePlays(plays);
			localPlayersList.RemoveEvents (plays);
			visitorPlayersList.RemoveEvents (plays);
		}
		
		void UpdateTeamsModels() {
			if (project == null)
				return;
			localPlayersList.SetTeam(project.LocalTeamTemplate, project.Timeline);
			visitorPlayersList.SetTeam(project.VisitorTeamTemplate, project.Timeline);
		}

	}
}

