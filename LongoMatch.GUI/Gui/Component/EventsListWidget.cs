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
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Events;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class EventsListWidget : Gtk.Bin, IView<LMProjectVM>
	{
		LMTimelineEventsTreeView playsList;
		LMProjectVM viewModel;
		Helpers.IconNotebookHelper notebookHelper;

		public EventsListWidget ()
		{
			this.Build ();
			localPlayersList.Team = TeamType.LOCAL;
			visitorPlayersList.Team = TeamType.VISITOR;
			playsnotebook.Page = 0;
			playsList1.HeightRequest = StyleConf.PlayerCapturerControlsHeight;
			App.Current.EventsBroker.Subscribe<TeamTagsChangedEvent> (UpdateTeamsModels);

			playsList = new LMTimelineEventsTreeView ();
			playsList.Show ();
			eventsScrolledWindow.Add (playsList);
		}

		protected override void OnDestroyed ()
		{
			App.Current.EventsBroker.Unsubscribe<TeamTagsChangedEvent> (UpdateTeamsModels);
			playsList.Project = null;
			localPlayersList.Clear ();
			visitorPlayersList.Clear ();
			playsList1.Destroy ();
			base.OnDestroyed ();
		}

		public LMProjectVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				playsList.ViewModel = viewModel.Timeline;
				playsList.Project = viewModel;
				visitorPlayersList.Project = viewModel.Model;
				localPlayersList.Project = viewModel.Model;
				LoadIcons ();
				UpdateTeamsModels (new TeamTagsChangedEvent ());
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = viewModel as LMProjectVM;
		}

		public void AddPlay (LMTimelineEvent play)
		{
			localPlayersList.AddEvent (play);
			visitorPlayersList.AddEvent (play);
		}

		public void RemovePlays (List<LMTimelineEvent> plays)
		{
			localPlayersList.RemoveEvents (plays);
			visitorPlayersList.RemoveEvents (plays);
		}

		void UpdateTeamsModels (TeamTagsChangedEvent e)
		{
			if (viewModel == null)
				return;

			var timeline = viewModel.Model.Timeline.OfType<LMTimelineEvent> ();
			localPlayersList.SetTeam (ViewModel.Model.LocalTeamTemplate, timeline);
			visitorPlayersList.SetTeam (ViewModel.Model.VisitorTeamTemplate, timeline);
		}

		void LoadIcons ()
		{
			LMProject project = ViewModel.Model;
			notebookHelper = new Helpers.IconNotebookHelper (playsnotebook);
			notebookHelper.SetTabIcon (playsList, "longomatch-category", "longomatch-category",
				Catalog.GetString ("Both Teams"));
			if (project.LocalTeamTemplate.Shield != null) {
				var localIcon = project.LocalTeamTemplate.Shield.Scale (StyleConf.NotebookTabIconSize,
									StyleConf.NotebookTabIconSize).Value;
				notebookHelper.SetTabIcon (localPlayersList, localIcon, localIcon, project.LocalTeamTemplate.Name);
			} else {
				notebookHelper.SetTabIcon (localPlayersList, "longomatch-default-shield", "longomatch-default-shield",
					project.LocalTeamTemplate.Name);
			}

			if (project.VisitorTeamTemplate.Shield != null) {
				var visitorIcon = project.VisitorTeamTemplate.Shield.Scale (StyleConf.NotebookTabIconSize,
									  StyleConf.NotebookTabIconSize).Value;
				notebookHelper.SetTabIcon (visitorPlayersList, visitorIcon, visitorIcon, project.VisitorTeamTemplate.Name);
			} else {
				notebookHelper.SetTabIcon (visitorPlayersList, "longomatch-default-shield", "longomatch-default-shield",
					project.VisitorTeamTemplate.Name);
			}

			notebookHelper.UpdateTabs ();
		}
	}
}
