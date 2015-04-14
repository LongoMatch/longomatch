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
using System.Collections.Generic;
using System.Linq;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Drawing.Cairo;
using LongoMatch.Drawing.Widgets;
using Mono.Unix;
using LongoMatch.GUI.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CodingWidget : Gtk.Bin
	{
		TeamTagger teamtagger;
		ProjectType projectType;
		Project project;
		List<Player> selectedPlayers;
		List<Window> activeWindows;
		int currentPage;
		Gdk.Pixbuf timelineIco, timelineActiveIco;
		Gdk.Pixbuf posIco, posAtiveIco;
		Gdk.Pixbuf dashboardIco, dashboardActiveIco;
		Gdk.Pixbuf listIco, listActiveIco;
		bool sizeAllocated;

		public CodingWidget ()
		{
			this.Build ();
			
			LoadIcons ();
			
			notebook.ShowBorder = false;
			notebook.Group = this.Handle;
			notebook.SwitchPage += HandleSwitchPage;
			Notebook.WindowCreationHook = CreateNewWindow;
			activeWindows = new List<Window> ();
			SetTabProps (dashboardhpaned, false);
			SetTabProps (timeline, false);
			SetTabProps (playspositionviewer1, false);
			SetTabProps (eventslistwidget, false);
			notebook.Page = currentPage = 0;

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
			
			Config.EventsBroker.PlayerTick += HandleTick;
			Config.EventsBroker.CapturerTick += HandleCapturerTick;
			Config.EventsBroker.EventLoadedEvent += HandlePlayLoaded;
			Config.EventsBroker.EventsDeletedEvent += HandleEventsDeletedEvent;
			Config.EventsBroker.TimerNodeAddedEvent += HandleTimerNodeAddedEvent;
			Config.EventsBroker.EventEditedEvent += HandleEventEdited;
			;
			LongoMatch.Gui.Helpers.Misc.SetFocus (this, false);
			
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
			Config.EventsBroker.PlayerTick -= HandleTick;
			Config.EventsBroker.CapturerTick -= HandleCapturerTick;
			Config.EventsBroker.EventLoadedEvent -= HandlePlayLoaded;
			Config.EventsBroker.TimerNodeAddedEvent -= HandleTimerNodeAddedEvent;
			Config.EventsBroker.EventEditedEvent -= HandleEventEdited;
			Config.EventsBroker.EventsDeletedEvent += HandleEventsDeletedEvent;
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

		public void TagPlayer (Player player)
		{
			teamtagger.Select (player);
		}

		public void TagTeam (TeamType team)
		{
			teamtagger.Select (team);
		}

		public void SetProject (Project project, ProjectType projectType, EventsFilter filter)
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

		public void AddPlay (TimelineEvent play)
		{
			if (projectType == ProjectType.FileProject) {
				timeline.AddPlay (play);
			} else if (projectType == ProjectType.FakeCaptureProject) {
				eventslistwidget.AddPlay (play);
			}
			playspositionviewer1.AddPlay (play);
		}

		public void DeletePlays (List<TimelineEvent> plays)
		{
			if (projectType == ProjectType.FileProject) {
				timeline.RemovePlays (plays);
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
			int s = StyleConf.NotebookTabIconSize;
			IconLookupFlags f = IconLookupFlags.ForceSvg;
 
			timelineIco = Helpers.Misc.LoadIcon ("longomatch-tab-timeline", s, f);
			timelineActiveIco = Helpers.Misc.LoadIcon ("longomatch-tab-active-timeline", s, f);
			dashboardIco = Helpers.Misc.LoadIcon ("longomatch-tab-dashboard", s, f);
			dashboardActiveIco = Helpers.Misc.LoadIcon ("longomatch-tab-active-dashboard", s, f);
			posIco = Helpers.Misc.LoadIcon ("longomatch-tab-position", s, f);
			posAtiveIco = Helpers.Misc.LoadIcon ("longomatch-tab-active-position", s, f);
			listIco = Helpers.Misc.LoadIcon ("longomatch-tab-dashboard", s, f);
			listActiveIco = Helpers.Misc.LoadIcon ("longomatch-tab-active-dashboard", s, f);
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

			if (widget == timeline) {
				icon = active ? timelineActiveIco : timelineIco;
			} else if (widget == dashboardhpaned) {
				icon = active ? dashboardActiveIco : dashboardIco;
			} else if (widget == playspositionviewer1) {
				icon = active ? posAtiveIco : posIco;
			} else if (widget == eventslistwidget) {
				icon = active ? listActiveIco : listIco;
			} else {
				return;
			}
			img.Pixbuf = icon;
			notebook.SetTabDetachable (widget, true);
		}

		Notebook CreateNewWindow (Notebook source, Widget page, int x, int y)
		{
			ExternalWindow window;
			EventBox box;
			Notebook notebook;

			window = new ExternalWindow ();
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
				source.AppendPage (pa, null);
				SetTabProps (pa, source.NPages == 0);
				notebook.Destroy ();
			};
			return notebook;
		}

		void HandleSwitchPage (object o, SwitchPageArgs args)
		{
			SetTabProps (notebook.GetNthPage (currentPage), false);
			SetTabProps (notebook.GetNthPage ((int)args.PageNum), true);
			currentPage = (int)args.PageNum;
		}

		void HandlePlayLoaded (TimelineEvent play)
		{
			timeline.LoadPlay (play);
		}

		void HandleCapturerTick (Time currentTime)
		{
			if (projectType != ProjectType.FileProject) {
				timeline.CurrentTime = currentTime;
				buttonswidget.CurrentTime = currentTime;
				teamtagger.CurrentTime = currentTime;
			}
		}

		void HandleTick (Time currentTime)
		{
			if (projectType == ProjectType.FileProject) {
				timeline.CurrentTime = currentTime;
				buttonswidget.CurrentTime = currentTime;
				teamtagger.CurrentTime = currentTime;
			}
		}

		void HandlePlayersSelectionChangedEvent (List<Player> players)
		{
			selectedPlayers = players.ToList ();
		}

		void HandleNewTagEvent (EventType eventType, List<Player> players, TeamType team, List<Tag> tags,
		                        Time start, Time stop, Time eventTime, Score score, PenaltyCard card, DashboardButton btn)
		{
			TimelineEvent play = project.AddEvent (eventType, start, stop, eventTime, null, score, card, false);
			play.Team = teamtagger.SelectedTeam;
			play.Players = selectedPlayers ?? new List<Player> ();
			play.Tags = tags ?? new List<Tag> ();
			teamtagger.ResetSelection ();
			selectedPlayers = null;
			Config.EventsBroker.EmitNewDashboardEvent (play, btn, true);
		}

		void HandlePlayersSubstitutionEvent (Team team, Player p1, Player p2,
		                                     SubstitutionReason reason, Time time)
		{
			Config.EventsBroker.EmitSubstitutionEvent (team, p1, p2, reason, time);
		}

		void HandleSizeAllocated (object o, SizeAllocatedArgs args)
		{
			if (!sizeAllocated) {
				dashboardhpaned.Position = dashboardhpaned.MaxPosition / 2;
				sizeAllocated = true;
			}
			
		}

		void HandleTimerNodeAddedEvent (TimerButton btn, TimeNode tn)
		{
			timeline.AddTimerNode (btn.Timer, tn);
		}

		void HandleEventEdited (TimelineEvent play)
		{
			if (play is SubstitutionEvent) {
				teamtagger.Reload ();
			}
		}

		void HandleEventsDeletedEvent (List<TimelineEvent> events)
		{
			if (events.Count (e => e is SubstitutionEvent) != 0) {
				teamtagger.Reload ();
			}
		}

	}
}

