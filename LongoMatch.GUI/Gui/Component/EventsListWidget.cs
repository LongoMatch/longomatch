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
using LongoMatch.Core.Common;
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
		TeamTimelineEventsTreeView homeTeamTreeView, awayTeamTreeView;
		LMProjectVM viewModel;
		Helpers.IconNotebookHelper notebookHelper;

		public EventsListWidget ()
		{
			this.Build ();
			playsnotebook.Page = 0;
			playsList1.HeightRequest = StyleConf.PlayerCapturerControlsHeight;

			playsList = new LMTimelineEventsTreeView ();
			playsList.Show ();
			eventsScrolledWindow.Add (playsList);
			homeTeamTreeView = new TeamTimelineEventsTreeView (TeamType.LOCAL);
			homeTeamTreeView.Show ();
			homescrolledwindow.Add (homeTeamTreeView);
			awayTeamTreeView = new TeamTimelineEventsTreeView (TeamType.VISITOR);
			awayTeamTreeView.Show ();
			awayscrolledwindow.Add (awayTeamTreeView);
		}

		protected override void OnDestroyed ()
		{
			playsList.ViewModel = null;
			homeTeamTreeView.ViewModel = null;
			awayTeamTreeView.ViewModel = null;
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
				homeTeamTreeView.ViewModel = viewModel.Timeline;
				homeTeamTreeView.Project = viewModel;
				awayTeamTreeView.ViewModel = viewModel.Timeline;
				awayTeamTreeView.Project = viewModel;
				LoadIcons ();
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = viewModel as LMProjectVM;
		}

		void LoadIcons ()
		{
			LMProject project = ViewModel.Model;
			notebookHelper = new Helpers.IconNotebookHelper (playsnotebook);
			notebookHelper.SetTabIcon (eventsScrolledWindow, "longomatch-category", "longomatch-category",
				Catalog.GetString ("Both Teams"));
			if (project.LocalTeamTemplate.Shield != null) {
				var localIcon = project.LocalTeamTemplate.Shield.Scale (StyleConf.NotebookTabIconSize,
									StyleConf.NotebookTabIconSize).Value;
				notebookHelper.SetTabIcon (homescrolledwindow, localIcon, localIcon, project.LocalTeamTemplate.Name);
			} else {
				notebookHelper.SetTabIcon (homescrolledwindow, "longomatch-default-shield", "longomatch-default-shield",
					project.LocalTeamTemplate.Name);
			}

			if (project.VisitorTeamTemplate.Shield != null) {
				var visitorIcon = project.VisitorTeamTemplate.Shield.Scale (StyleConf.NotebookTabIconSize,
									  StyleConf.NotebookTabIconSize).Value;
				notebookHelper.SetTabIcon (awayscrolledwindow, visitorIcon, visitorIcon, project.VisitorTeamTemplate.Name);
			} else {
				notebookHelper.SetTabIcon (awayscrolledwindow, "longomatch-default-shield", "longomatch-default-shield",
					project.VisitorTeamTemplate.Name);
			}

			notebookHelper.UpdateTabs ();
		}
	}
}
