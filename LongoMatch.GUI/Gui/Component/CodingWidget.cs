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
using System.Linq;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Events;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Drawing.Widgets;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Drawing.Cairo;
using Helpers = VAS.UI.Helpers;
using LMCommon = LongoMatch.Core.Common;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CodingWidget : Gtk.Bin
	{
		TeamTagger teamtagger;
		ProjectType projectType;
		ProjectLongoMatch project;
		List<PlayerLongoMatch> selectedPlayers;
		List<Window> activeWindows;
		Helpers.IconNotebookHelper notebookHelper;
		bool sizeAllocated;
		IPlayerController player;

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

			teamtagger = new TeamTagger (new WidgetWrapper (teamsdrawingarea));
			teamtagger.SelectionMode = MultiSelectionMode.Multiple;
			teamtagger.PlayersSelectionChangedEvent += HandlePlayersSelectionChangedEvent;
			teamtagger.PlayersSubstitutionEvent += HandlePlayersSubstitutionEvent;
			teamtagger.Compact = true;
			teamtagger.ShowTeamsButtons = true;

			teamsdrawingarea.HeightRequest = 200;
			teamsdrawingarea.WidthRequest = 300;
			timeline.HeightRequest = 200;
			playspositionviewer1.HeightRequest = 200;
			
			App.Current.EventsBroker.Subscribe<PlayerTickEvent> (HandleTick);
			App.Current.EventsBroker.Subscribe<CapturerTickEvent> (HandleCapturerTick);
			App.Current.EventsBroker.Subscribe<EventLoadedEvent> (HandlePlayLoaded);
			App.Current.EventsBroker.Subscribe<EventsDeletedEvent> (HandleEventsDeletedEvent);
			App.Current.EventsBroker.Subscribe<TimeNodeStoppedEvent> (HandleTimeNodeStoppedEvent);
			App.Current.EventsBroker.Subscribe<EventEditedEvent> (HandleEventEdited);

			Helpers.Misc.SetFocus (this, false);
			
			buttonswidget.Mode = DashboardMode.Code;
			buttonswidget.FitMode = FitMode.Fit;
			buttonswidget.ButtonsVisible = true;
			buttonswidget.NewTagEvent += HandleNewTagEvent;
			
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
			App.Current.EventsBroker.Unsubscribe<PlayerTickEvent> (HandleTick);
			App.Current.EventsBroker.Unsubscribe<CapturerTickEvent> (HandleCapturerTick);
			App.Current.EventsBroker.Unsubscribe<EventLoadedEvent> (HandlePlayLoaded);
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
			if (projectType == ProjectType.FileProject) {
				SelectPage (timeline);
			} 
		}

		public void ShowZonalTags ()
		{
			SelectPage (playspositionviewer1);
		}

		public void ClickButton (DashboardButton button, Tag tag = null)
		{
			buttonswidget.ClickButton (button, tag);
		}

		public void TagPlayer (PlayerLongoMatch player)
		{
			teamtagger.Select (player);
		}

		public void TagTeam (TeamType team)
		{
			teamtagger.Select (team);
		}

		public IPlayerController Player {
			get {
				return player;
			}
			set {
				player = value;
				timeline.Player = player;
			}
		}

		public void SetProject (ProjectLongoMatch project, ProjectType projectType, EventsFilter filter)
		{
			this.projectType = projectType;
			this.project = project;
			buttonswidget.Visible = true;
			if (project != null) {
				buttonswidget.Project = project;
			}
			buttonswidget.Mode = DashboardMode.Code;
			teamtagger.Project = project;
			teamtagger.LoadTeams (project.LocalTeamTemplate, project.VisitorTeamTemplate,
				project.Dashboard.FieldBackground);
			teamtagger.CurrentTime = new Time (0);
			if (projectType == ProjectType.FileProject) {
				timeline.SetProject (project, filter);
			} else if (projectType == ProjectType.FakeCaptureProject) {
				eventslistwidget.SetProject (project, filter);
			}
			eventslistwidget.Visible = projectType == ProjectType.FakeCaptureProject;
			timeline.Visible = projectType == ProjectType.FileProject;
			playspositionviewer1.LoadProject (project, filter);
		}

		public void AddPlay (TimelineEventLongoMatch play)
		{
			if (projectType == ProjectType.FileProject) {
				timeline.AddPlay (play);
			} else if (projectType == ProjectType.FakeCaptureProject) {
				eventslistwidget.AddPlay (play);
			}
			playspositionviewer1.AddPlay (play);
		}

		public void DeletePlays (List<TimelineEventLongoMatch> plays)
		{
			if (projectType == ProjectType.FileProject) {
				timeline.RemovePlays (plays.Cast<TimelineEvent> ().ToList ());
			} else if (projectType == ProjectType.FakeCaptureProject) {
				eventslistwidget.RemovePlays (plays);
			}
			playspositionviewer1.RemovePlays (plays);
		}

		public void UpdateCategories ()
		{
			buttonswidget.Refresh ();
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

		void HandlePlayLoaded (EventLoadedEvent e)
		{
			timeline.LoadPlay (e.TimelineEvent as TimelineEventLongoMatch);
		}

		void HandleCapturerTick (CapturerTickEvent e)
		{
			if (projectType != ProjectType.FileProject) {
				timeline.CurrentTime = e.Time;
				buttonswidget.CurrentTime = e.Time;
				teamtagger.CurrentTime = e.Time;
			}
		}

		void HandleTick (PlayerTickEvent e)
		{
			if (projectType == ProjectType.FileProject) {
				timeline.CurrentTime = e.Time;
				buttonswidget.CurrentTime = e.Time;
				teamtagger.CurrentTime = e.Time;
			}
		}

		void HandlePlayersSelectionChangedEvent (List<PlayerLongoMatch> players)
		{
			selectedPlayers = players.ToList ();
		}

		void HandleNewTagEvent (EventType eventType, List<Player> players, ObservableCollection<Team> teams, List<Tag> tags,
		                        Time start, Time stop, Time eventTime, DashboardButton btn)
		{
			TimelineEventLongoMatch play = project.AddEvent (eventType, start, stop, eventTime,
				                               null, false) as TimelineEventLongoMatch;
			play.Teams = new ObservableCollection<Team> (teamtagger.SelectedTeams);
			if (selectedPlayers != null) {
				play.Players = new ObservableCollection<Player> (selectedPlayers);
			} else {
				play.Players = new ObservableCollection<Player> (); 
			}
			if (tags != null) {
				play.Tags = new ObservableCollection <Tag> (tags);
			} else {
				play.Tags = new ObservableCollection<Tag> ();
			}
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

		void HandlePlayersSubstitutionEvent (SportsTeam team, PlayerLongoMatch p1, PlayerLongoMatch p2,
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
			if (e.TimerButton is TimerButton)
				timeline.AddTimerNode (((TimerButton)e.TimerButton).Timer, e.TimeNode);
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
