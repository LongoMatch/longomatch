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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Events;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Services.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Drawing.Cairo;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CodingWidget : Gtk.Bin, IView<LMProjectAnalysisVM>
	{
		LMTeamTaggerView teamtagger;
		List<LMPlayer> selectedPlayers;
		List<Window> activeWindows;
		Helpers.IconNotebookHelper notebookHelper;
		bool sizeAllocated;
		LMProjectAnalysisVM viewModel;

		public CodingWidget ()
		{
			this.Build ();

			LoadIcons ();

			notebook.ShowBorder = false;
			notebook.Group = this.Handle;
			notebook.SwitchPage += HandleSwitchPage;
			Notebook.WindowCreationHook = CreateNewWindow;
			activeWindows = new List<Window> ();

			notebook.Page = 0;

			teamtagger = new LMTeamTaggerView (new WidgetWrapper (teamsdrawingarea));
			teamtagger.SelectionMode = MultiSelectionMode.Multiple;
			teamtagger.PlayersSelectionChangedEvent += HandlePlayersSelectionChangedEvent;
			teamtagger.PlayersSubstitutionEvent += HandlePlayersSubstitutionEvent;
			teamtagger.Compact = true;
			teamtagger.ShowTeamsButtons = true;

			teamsdrawingarea.HeightRequest = 200;
			teamsdrawingarea.WidthRequest = 300;
			timeline.HeightRequest = 200;
			playspositionviewer1.HeightRequest = 200;

			App.Current.EventsBroker.Subscribe<CapturerTickEvent> (HandleCapturerTick);
			App.Current.EventsBroker.Subscribe<EventsDeletedEvent> (HandleEventsDeletedEvent);
			App.Current.EventsBroker.Subscribe<TimeNodeStoppedEvent> (HandleTimeNodeStoppedEvent);
			App.Current.EventsBroker.Subscribe<EventEditedEvent> (HandleEventEdited);

			Helpers.Misc.SetFocus (this, false);

			buttonswidget.NewTagEvent += HandleNewTagEvent;
			buttonswidget.CodingDashboardMode = true;

			dashboardhpaned.SizeAllocated += HandleSizeAllocated;
			TagPositions = true;
		}

		public bool TagPositions {
			get;
			set;
		}

		protected override void OnDestroyed ()
		{
			foreach (Window w in activeWindows) {
				w.Destroy ();
			}

			App.Current.EventsBroker.Unsubscribe<CapturerTickEvent> (HandleCapturerTick);
			App.Current.EventsBroker.Unsubscribe<TimeNodeStoppedEvent> (HandleTimeNodeStoppedEvent);
			App.Current.EventsBroker.Unsubscribe<EventEditedEvent> (HandleEventEdited);
			App.Current.EventsBroker.Unsubscribe<EventsDeletedEvent> (HandleEventsDeletedEvent);

			buttonswidget.Destroy ();
			timeline.Destroy ();
			playspositionviewer1.Destroy ();
			teamtagger.Dispose ();
			base.OnDestroyed ();
		}

		public void ZoomIn ()
		{
			timeline.ZoomIn ();
		}

		public void ZoomOut ()
		{
			timeline.ZoomOut ();
		}

		public void FitTimeline ()
		{
			timeline.Fit ();
		}

		public void ShowDashboard ()
		{
			SelectPage (dashboardhpaned);
		}

		public void ShowTimeline ()
		{
			if (ViewModel.Project.ProjectType == ProjectType.FileProject) {
				SelectPage (timeline);
			}
		}

		public void ShowZonalTags ()
		{
			SelectPage (playspositionviewer1);
		}

		public void TagPlayer (LMPlayer player)
		{
			teamtagger.Select (player);
		}

		public void TagTeam (TeamType team)
		{
			teamtagger.Select (team);
		}

		public LMProjectAnalysisVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandlePropertyChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					viewModel.PropertyChanged += HandlePropertyChanged;
				}
				LoadProject ();
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMProjectAnalysisVM)viewModel;
		}

		public void LoadIcons ()
		{
			notebookHelper = new Helpers.IconNotebookHelper (notebook);
			notebookHelper.SetTabIcon (timeline, "longomatch-tab-timeline", "longomatch-tab-active-timeline",
				Catalog.GetString ("Timeline view"));
			notebookHelper.SetTabIcon (dashboardhpaned, "longomatch-tab-dashboard", "longomatch-tab-active-dashboard",
				Catalog.GetString ("Coding View"));
			notebookHelper.SetTabIcon (playspositionviewer1, "longomatch-tab-position", "longomatch-tab-active-position",
				Catalog.GetString ("Zonal tags"));
			notebookHelper.SetTabIcon (eventslistwidget, "longomatch-tab-dashboard", "longomatch-tab-active-dashboard",
				Catalog.GetString ("Events list"));

			notebookHelper.UpdateTabs ();
		}

		void LoadProject ()
		{
			buttonswidget.Visible = true;
			ViewModel.Project.Dashboard.Mode = DashboardMode.Code;
			buttonswidget.ViewModel = ViewModel.Project.Dashboard;
			// FIXME: team tagger is not ported yet to MVVM
			teamtagger.Project = ViewModel.Project.Model;
			teamtagger.LoadTeams (ViewModel.Project.Model.LocalTeamTemplate,
								  ViewModel.Project.Model.VisitorTeamTemplate,
								  ViewModel.Project.Model.Dashboard.FieldBackground);
			teamtagger.CurrentTime = new Time (0);

			eventslistwidget.Visible = ViewModel.Project.ProjectType == ProjectType.FakeCaptureProject;
			timeline.Visible = ViewModel.Project.ProjectType == ProjectType.FileProject;

			if (ViewModel.Project.ProjectType == ProjectType.FileProject) {
				timeline.ViewModel = ViewModel;
			} else if (ViewModel.Project.ProjectType == ProjectType.FakeCaptureProject) {
				eventslistwidget.ViewModel = ViewModel.Project;
			}
			playspositionviewer1.ViewModel = ViewModel.Project;
		}

		void SelectPage (Widget widget)
		{
			for (int i = 0; i < notebook.NPages; i++) {
				if (notebook.GetNthPage (i) == widget) {
					notebook.Page = i;
					break;
				}
			}
		}

		Notebook CreateNewWindow (Notebook source, Widget page, int x, int y)
		{
			Helpers.ExternalWindow window;
			EventBox box;
			Notebook notebook;

			window = new Helpers.ExternalWindow ();
			if (page == timeline) {
				window.Title = Catalog.GetString ("Timeline");
			} else if (page == dashboardhpaned) {
				window.Title = Catalog.GetString ("Analysis dashboard");
			} else if (page == playspositionviewer1) {
				window.Title = Catalog.GetString ("Zonal tags viewer");
			}

			notebook = new Notebook ();
			notebook.ShowTabs = false;
			notebook.CanFocus = false;
			//notebook.Group = source.Group;

			window.Add (notebook);
			window.SetDefaultSize (page.Allocation.Width, page.Allocation.Height);
			window.Move (x, y);
			window.ShowAll ();
			activeWindows.Add (window);
			window.DeleteEvent += (o, args) => {
				Widget pa = notebook.CurrentPageWidget;
				activeWindows.Remove (window);
				notebook.Remove (pa);
				Visible = true;
				source.AppendPage (pa, null);
				notebookHelper.UpdateTabs ();
				notebook.Destroy ();
			};

			/* If we are remove the last visible page, hide the widget to
			 * free the empty space for the rest of widgets */
			int visiblePages = 0;
			for (int i = 0; i < source.NPages; i++) {
				if (source.GetNthPage (i).Visible) {
					visiblePages++;
				}
			}
			if (visiblePages == 1) {
				Visible = false;
			}
			return notebook;
		}

		void HandleSwitchPage (object o, SwitchPageArgs args)
		{
			notebook.SetTabDetachable (notebook.GetNthPage ((int)args.PageNum), true);
		}

		void HandleCapturerTick (CapturerTickEvent e)
		{
			if (ViewModel.Project.ProjectType != ProjectType.FileProject) {
				buttonswidget.CurrentTime = e.Time;
				teamtagger.CurrentTime = e.Time;
			}
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (ViewModel.Capturer.CurrentCaptureTime)
				&& ViewModel.Project.ProjectType == ProjectType.FileProject) {
				timeline.CurrentTime = ViewModel.VideoPlayer.CurrentTime;
				buttonswidget.CurrentTime = ViewModel.VideoPlayer.CurrentTime;
				teamtagger.CurrentTime = ViewModel.VideoPlayer.CurrentTime;
			}
		}

		void HandlePlayersSelectionChangedEvent (List<LMPlayer> players)
		{
			selectedPlayers = players.ToList ();
		}

		void HandleNewTagEvent (EventType eventType, List<Player> players, ObservableCollection<Team> teams, List<Tag> tags,
								Time start, Time stop, Time eventTime, DashboardButton btn)
		{
			int index = ViewModel.Project.Model.EventsByType (eventType).Count + 1;
			LMTimelineEvent play = ViewModel.Project.Model.CreateEvent (eventType, start, stop, eventTime, null, index) as LMTimelineEvent;
			play.Teams.Replace (teamtagger.SelectedTeams);
			play.Players.Replace (selectedPlayers);
			play.Tags.Replace (tags);
			teamtagger.ResetSelection ();
			selectedPlayers = null;
			App.Current.EventsBroker.Publish<NewDashboardEvent> (
				new NewDashboardEvent {
					TimelineEvent = play,
					DashboardButton = btn,
					Edit = true,
					DashboardButtons = null
				}
			);
		}

		void HandlePlayersSubstitutionEvent (LMTeam team, LMPlayer p1, LMPlayer p2,
											 SubstitutionReason reason, Time time)
		{
			App.Current.EventsBroker.Publish<PlayerSubstitutionEvent> (
				new PlayerSubstitutionEvent {
					Team = team,
					Player1 = p1,
					Player2 = p2,
					SubstitutionReason = reason,
					Time = time
				}
			);
		}

		void HandleSizeAllocated (object o, SizeAllocatedArgs args)
		{
			if (!sizeAllocated) {
				dashboardhpaned.Position = dashboardhpaned.MaxPosition / 2;
				sizeAllocated = true;
			}
		}

		void HandleTimeNodeStoppedEvent (TimeNodeStoppedEvent e)
		{
			if (e.TimerButton is TimerButton) {
				ViewModel.Project.Timers.Model.Add (e.TimerButton.Timer);
			}
		}

		void HandleEventEdited (EventEditedEvent e)
		{
			if (e.TimelineEvent is SubstitutionEvent || e.TimelineEvent is LineupEvent) {
				teamtagger.Reload ();
			}
		}

		void HandleEventsDeletedEvent (EventsDeletedEvent e)
		{
			if (e.TimelineEvents.Count (s => s is SubstitutionEvent) != 0) {
				teamtagger.Reload ();
			}
		}
	}
}
