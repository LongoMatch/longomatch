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

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CodingWidget : Gtk.Bin
	{
		TeamTagger teamtagger;
		ProjectType projectType;
		List<Player> selectedPlayers;
		Play loadedPlay;
		
		public CodingWidget ()
		{
			this.Build ();
			
			notebook.ShowTabs = false;
			notebook.ShowBorder = false;

			autoTaggingMode.Activated += HandleViewToggled;
			timelineMode.Activated += HandleViewToggled;
			positionMode.Activated += HandleViewToggled;
			autoTaggingMode.Active = true;
			
			teamtagger = new TeamTagger (new WidgetWrapper (teamsdrawingarea));
			teamtagger.SelectionMode = MultiSelectionMode.Multiple;
			teamtagger.PlayersSelectionChangedEvent += HandlePlayersSelectionChangedEvent;

			teamsdrawingarea.HeightRequest = 200;
			teamsdrawingarea.WidthRequest = 300;
			timeline.HeightRequest = 200;
			playspositionviewer1.HeightRequest = 200;
			
			Config.EventsBroker.Tick += HandleTick;
			Config.EventsBroker.PlaySelected += HandlePlaySelected;
			LongoMatch.Gui.Helpers.Misc.DisableFocus (vbox);
			
			//buttonswidget.NewTagEvent += HandleNewTagEvent;
			buttonswidget.Mode = TagMode.Free;
		}

		protected override void OnDestroyed ()
		{
			Config.EventsBroker.Tick -= HandleTick;
			Config.EventsBroker.PlaySelected -= HandlePlaySelected;
			buttonswidget.Destroy ();
			timeline.Destroy ();
			playspositionviewer1.Destroy ();
			base.OnDestroyed ();
		}

		public void SetProject (Project project, ProjectType projectType, PlaysFilter filter) {
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
		
		public void AddPlay(Play play) {
			if (projectType == ProjectType.FileProject) {
				timeline.AddPlay(play);
			}
			playspositionviewer1.AddPlay (play);
		}
		
		public void DeletePlays (List<Play> plays) {
			if (projectType == ProjectType.FileProject) {
				timeline.RemovePlays(plays);
			}
			playspositionviewer1.RemovePlays (plays);
		}

		public void UpdateCategories () {
			buttonswidget.Refresh ();
		}
		
		void HandleViewToggled (object sender, EventArgs e)
		{
			if (!(sender as RadioAction).Active) {
				return;
			}
			if (autoTaggingMode.Active) {
				notebook.Page = 0;
			} else if (timelineMode.Active) {
				notebook.Page = 1;
			} else if (positionMode.Active) {
				notebook.CurrentPage = 2;
			}
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

		void HandleNewTagEvent (Category category, List<Player> players)
		{
			Config.EventsBroker.EmitNewTag (category, selectedPlayers);
			teamtagger.ClearSelection ();
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

