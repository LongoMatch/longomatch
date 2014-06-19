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
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Common;
using System.Collections.Generic;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Drawing.Cairo;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CodingWidget : Gtk.Bin
	{
		VideoAnalysisMode analysisMode;
		TeamTagger teamtagger;
		Project project;
		
		public CodingWidget ()
		{
			this.Build ();

			autoTaggingMode.Activated += HandleViewToggled;
			timelineMode.Activated += HandleViewToggled;
			positionMode.Activated += HandleViewToggled;
			autoTaggingMode.Active = true;
			
			teamtagger = new TeamTagger (new WidgetWrapper (drawingarea1));
			teamtagger.HomeColor = Constants.HOME_COLOR;
			teamtagger.AwayColor = Constants.AWAY_COLOR;

			drawingarea1.HeightRequest = 200;
			drawingarea1.WidthRequest = 300;
			timeline.HeightRequest = 200;
			playspositionviewer1.HeightRequest = 200;
			
			Config.EventsBroker.Tick += HandleTick;
			Config.EventsBroker.PlaySelected += HandlePlaySelected;
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

		public void SetProject (Project project, bool isLive, PlaysFilter filter) {
			this.project = project;	
			autoTaggingMode.Active = true;
			timeline.Visible = false;
			buttonswidget.Visible = true;
			buttonswidget.Project = project;
			teamtagger.LoadTeams (project.LocalTeamTemplate, project.VisitorTeamTemplate,
			                      project.Categories.FieldBackground);
			timeline.SetProject (project, filter);
			playspositionviewer1.LoadProject (project);
		}
		
		public AnalysisComponent AnalysisComponentParent {
			set;
			protected get;
		}
		
		public void AddPlay(Play play) {
			timeline.AddPlay(play);
			playspositionviewer1.AddPlay (play);
		}
		
		public void DeletePlays (List<Play> plays) {
			timeline.RemovePlays(plays);
			playspositionviewer1.RemovePlays (plays);
		}

		public void UpdateCategories () {
			buttonswidget.Project = project;
		}
		
		public VideoAnalysisMode AnalysisMode {
			set {
				buttonswidget.Visible = (value == VideoAnalysisMode.ManualTagging) ||
					(value == VideoAnalysisMode.PredefinedTagging);
				drawingarea1.Visible = buttonswidget.Visible;
				timeline.Visible = value == VideoAnalysisMode.Timeline;
				if(value == VideoAnalysisMode.ManualTagging)
					buttonswidget.Mode = TagMode.Free;
				else if (value == VideoAnalysisMode.ManualTagging)
					buttonswidget.Mode = TagMode.Predifined;
				analysisMode = value;
				timeline.Visible = true;
			}
			protected get {
				return analysisMode;
			}
		}
		
		public bool WidgetsVisible {
			set {
				timeline.Visible = value && AnalysisMode == VideoAnalysisMode.Timeline;
				buttonswidget.Visible = value && (AnalysisMode == VideoAnalysisMode.ManualTagging ||
				                                  AnalysisMode == VideoAnalysisMode.PredefinedTagging);
				drawingarea1.Visible = buttonswidget.Visible;
			}
		}
		
		void HandleViewToggled (object sender, EventArgs e)
		{
			buttonswidget.Visible = autoTaggingMode.Active;
			drawingarea1.Visible = buttonswidget.Visible;
			timeline.Visible = timelineMode.Active;
			playspositionviewer1.Visible = positionMode.Active;
		}
		
		void HandlePlaySelected (Play play)
		{
			timeline.SelectedTimeNode = play;
		}

		void HandleTick (Time currentTime, Time streamLength, double currentPosition)
		{
			timeline.CurrentTime = currentTime;
		}
	}

}

