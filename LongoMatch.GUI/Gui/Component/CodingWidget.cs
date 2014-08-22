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
using Gtk;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Common;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Drawing.Cairo;
using LongoMatch.Gui.Helpers;
using Mono.Unix;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CodingWidget : Gtk.Bin
	{
		TeamTagger teamtagger;
		ProjectType projectType;
		List<Player> selectedPlayers;
		Play loadedPlay;
		List<Window> activeWindows;
		int currentPage;
		Gdk.Pixbuf timelineIco, timelineActiveIco;
		Gdk.Pixbuf posIco, posAtiveIco;
		Gdk.Pixbuf dashboardIco, dashboardActiveIco;

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
			notebook.Page = currentPage = 0;

			teamtagger = new TeamTagger (new WidgetWrapper (teamsdrawingarea));
			teamtagger.SelectionMode = MultiSelectionMode.Multiple;
			teamtagger.PlayersSelectionChangedEvent += HandlePlayersSelectionChangedEvent;

			teamsdrawingarea.HeightRequest = 200;
			teamsdrawingarea.WidthRequest = 300;
			timeline.HeightRequest = 200;
			playspositionviewer1.HeightRequest = 200;
			
			Config.EventsBroker.Tick += HandleTick;
			Config.EventsBroker.PlaySelected += HandlePlaySelected;
			LongoMatch.Gui.Helpers.Misc.DisableFocus (this);
			
			buttonswidget.Mode = TagMode.Free;
			buttonswidget.FitMode = FitMode.Fit;
		}

		protected override void OnDestroyed ()
		{
			foreach (Window w in activeWindows) {
				w.Destroy ();
			}
			Config.EventsBroker.Tick -= HandleTick;
			Config.EventsBroker.PlaySelected -= HandlePlaySelected;
			buttonswidget.Destroy ();
			timeline.Destroy ();
			playspositionviewer1.Destroy ();
			base.OnDestroyed ();
		}

		public void SetProject (Project project, ProjectType projectType, PlaysFilter filter)
		{
			this.projectType = projectType;
			autoTaggingMode.Active = true;
			buttonswidget.Visible = true;
			if (project != null) {
				buttonswidget.Template = project.Categories;
			}
			teamtagger.LoadTeams (project.LocalTeamTemplate, project.VisitorTeamTemplate,
			                      project.Categories.FieldBackground);
			if (projectType != ProjectType.FileProject) {
				timelineMode.Visible = false;
			} else {
				timelineMode.Visible = true;
				timeline.SetProject (project, filter);
			}
			playspositionviewer1.LoadProject (project);
		}

		public void AddPlay (Play play)
		{
			if (projectType == ProjectType.FileProject) {
				timeline.AddPlay (play);
			}
			playspositionviewer1.AddPlay (play);
		}

		public void DeletePlays (List<Play> plays)
		{
			if (projectType == ProjectType.FileProject) {
				timeline.RemovePlays (plays);
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
 
			timelineIco = IconTheme.Default.LoadIcon ("longomatch-tab-timeline", s, f);
			timelineActiveIco = IconTheme.Default.LoadIcon ("longomatch-tab-active-timeline", s, f);
			dashboardIco = IconTheme.Default.LoadIcon ("longomatch-tab-dashboard", s, f);
			dashboardActiveIco = IconTheme.Default.LoadIcon ("longomatch-tab-active-dashboard", s, f);
			posIco = IconTheme.Default.LoadIcon ("longomatch-tab-position", s, f);
			posAtiveIco = IconTheme.Default.LoadIcon ("longomatch-tab-active-position", s, f);
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
			} else {
				return;
			}
			img.Pixbuf = icon;
			notebook.SetTabDetachable (widget, true);
		}

		Notebook CreateNewWindow (Notebook source, Widget page, int x, int y)
		{
			Window window;
			Notebook notebook;

			window = new Window (WindowType.Toplevel);
			if (page == timeline) {
				window.Title = Catalog.GetString ("Timeline");
			} else if (page == dashboardhpaned) {
				window.Title = Catalog.GetString ("Analysis dashboard");
			} else if (page == playspositionviewer1) {
				window.Title = Catalog.GetString ("Zonal tags viewer");
			}
			window.Icon = Stetic.IconLoader.LoadIcon (this, "longomatch", IconSize.Menu);
			notebook = new Notebook ();
			notebook.ShowTabs = false;
			//notebook.Group = source.Group;
			window.Add (notebook);
			window.SetDefaultSize (300, 300);
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

		void HandlePlaySelected (Play play)
		{
			loadedPlay = play;
			timeline.SelectedTimeNode = play;
		}

		void HandleTick (Time currentTime)
		{
			timeline.CurrentTime = currentTime;
		}

		void HandlePlayersSelectionChangedEvent (List<Player> players)
		{
			if (loadedPlay != null) {
				loadedPlay.Players = players;
				Config.EventsBroker.EmitTeamTagsChanged ();
			} else {
				selectedPlayers = players;
			}
		}
	}
}

