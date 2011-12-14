// MainWindow.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Collections.Generic;
using System.IO;
using Gdk;
using GLib;
using Gtk;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Gui.Dialog;
using LongoMatch.Handlers;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Video.Common;
using LongoMatch.Gui.Component;


namespace LongoMatch.Gui
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class MainWindow : Gtk.Window, IMainWindow
	{
	
		/* Tags */
		public event NewTagHandler NewTagEvent;
		public event NewTagStartHandler NewTagStartEvent;
		public event NewTagStopHandler NewTagStopEvent;
		public event PlaySelectedHandler PlaySelectedEvent;
		public event NewTagAtFrameHandler NewTagAtFrameEvent;
		public event TagPlayHandler TagPlayEvent;
		public event PlaysDeletedHandler PlaysDeletedEvent;
		public event TimeNodeChangedHandler TimeNodeChanged;
		
		/* Playlist */
		public event RenderPlaylistHandler RenderPlaylistEvent;
		public event PlayListNodeAddedHandler PlayListNodeAddedEvent;
		public event PlayListNodeSelectedHandler PlayListNodeSelectedEvent;
		public event OpenPlaylistHandler OpenPlaylistEvent;
		public event NewPlaylistHandler NewPlaylistEvent;
		public event SavePlaylistHandler SavePlaylistEvent; 
		
		/* Snapshots */
		public event SnapshotSeriesHandler SnapshotSeriesEvent;
		
		/* Projects */
		public event SaveProjectHandler SaveProjectEvent;
		public event NewProjectHandler NewProjectEvent;
		public event OpenProjectHandler OpenProjectEvent;
		public event CloseOpenendProjectHandler CloseOpenedProjectEvent;
		public event ImportProjectHandler ImportProjectEvent;
		public event ExportProjectHandler ExportProjectEvent;
		
		/* Managers */
		public event ManageJobsHandler ManageJobsEvent; 
		public event ManageTeamsHandler ManageTeamsEvent;
		public event ManageCategoriesHandler ManageCategoriesEvent;
		public event ManageProjects ManageProjectsEvent;
		public event ApplyCurrentRateHandler ApplyRateEvent;
		
		/* Game Units events */
		public event GameUnitHandler GameUnitEvent;
		public event UnitChangedHandler UnitChanged;
		public event UnitSelectedHandler UnitSelected;
		public event UnitsDeletedHandler UnitDeleted;
		public event UnitAddedHandler UnitAdded;

		private static Project openedProject;
		private ProjectType projectType;
		private TimeNode selectedTimeNode;
		TimeLineWidget timeline;
		bool gameUnitsActionVisible;
		GameUnitsTimelineWidget guTimeline;

		#region Constructors
		public MainWindow() :
		base("LongoMatch")
		{
			this.Build();

			projectType = ProjectType.None;

			timeline = new TimeLineWidget();
			downbox.PackStart(timeline, true, true, 0);
			
			guTimeline = new GameUnitsTimelineWidget ();
			downbox.PackStart(guTimeline, true, true, 0);
			
			player.SetLogo(System.IO.Path.Combine(Config.ImagesDir(),"background.png"));
			player.LogoMode = true;
			player.Tick += OnTick;

			capturer.Visible = false;
			capturer.Logo = System.IO.Path.Combine(Config.ImagesDir(),"background.png");
			capturer.CaptureFinished += (sender, e) => {CloseCaptureProject();};
			
			buttonswidget.Mode = TagMode.Predifined;
			localPlayersList.Team = Team.LOCAL;
			visitorPlayersList.Team = Team.VISITOR;
			
			ConnectSignals();
			ConnectMenuSignals();
			
			if (!Config.useGameUnits)
				GameUnitsViewAction.Visible = false;
		}

		#endregion
		
		#region Plubic Methods
		public void AddPlay(Play play) {
			playsList.AddPlay(play);
			tagsList.AddPlay(play);
			timeline.AddPlay(play);
			/* FIXME: Check performance */
			UpdateTeamsModels();
			timeline.QueueDraw();
		}
		
		public void UpdateSelectedPlay (Play play) {
			timeline.SelectedTimeNode = play;
			notes.Visible = true;
			notes.Play= play;
		}

		public void UpdateCategories (Categories categories) {
			buttonswidget.Categories = openedProject.Categories;
		}
		
		public void DeletePlays (List<Play> plays) {
			playsList.RemovePlays(plays);
			timeline.RemovePlays(plays);
			tagsList.RemovePlays(plays);
			UpdateTeamsModels();
			timeline.QueueDraw();
		}
		
		public IRenderingStateBar RenderingStateBar{
			get {
				return renderingstatebar1;
			}
		}
		
		public IPlayer Player{
			get {
				return player;
			}
		}
		
		public ICapturer Capturer{
			get {
				return capturer;
			}
		}
		
		public IPlaylistWidget Playlist{
			get {
				return playlist;
			}
		}
		
		public void UpdateGameUnits (GameUnitsList gameUnits) {
			gameUnitsActionVisible = gameUnits != null && gameUnits.Count > 0;
			GameUnitsViewAction.Sensitive = gameUnitsActionVisible;
			if (gameUnits == null) {
				gameunitstaggerwidget1.Visible = false;
				return;
			}
			gameunitstaggerwidget1.Visible = true;
			gameunitstaggerwidget1.GameUnits = gameUnits;
		}
		
		#endregion
		
		#region Private Methods
		
		private void ConnectSignals() {
			/* Adding Handlers for each event */

			/* Connect new mark event */
			buttonswidget.NewMarkEvent += EmitNewTag;;
			buttonswidget.NewMarkStartEvent += EmitNewTagStart;
			buttonswidget.NewMarkStopEvent += EmitNewTagStop;
			timeline.NewMarkEvent += EmitNewTagAtFrame;

			/* Connect TimeNodeChanged events */
			playsList.TimeNodeChanged += EmitTimeNodeChanged;
			localPlayersList.TimeNodeChanged += EmitTimeNodeChanged;
			visitorPlayersList.TimeNodeChanged += EmitTimeNodeChanged;
			tagsList.TimeNodeChanged += EmitTimeNodeChanged;
			timeline.TimeNodeChanged += EmitTimeNodeChanged;
			notes.TimeNodeChanged += EmitTimeNodeChanged;

			/* Connect TimeNodeDeleted events */
			playsList.TimeNodeDeleted += EmitPlaysDeleted;
			timeline.TimeNodeDeleted += EmitPlaysDeleted;

			/* Connect TimeNodeSelected events */
			playsList.TimeNodeSelected += OnTimeNodeSelected;
			localPlayersList.TimeNodeSelected += OnTimeNodeSelected;
			visitorPlayersList.TimeNodeSelected += OnTimeNodeSelected;
			tagsList.TimeNodeSelected += OnTimeNodeSelected;
			timeline.TimeNodeSelected += OnTimeNodeSelected;

			/* Connect playlist events */
			playlist.PlayListNodeSelected += EmitPlayListNodeSelected;
			playlist.ApplyCurrentRate += EmitApplyRate;
			playlist.NewPlaylistEvent += EmitNewPlaylist;
			playlist.OpenPlaylistEvent += EmitOpenPlaylist;
			playlist.SavePlaylistEvent += EmitSavePlaylist;

			/* Connect PlayListNodeAdded events */
			playsList.PlayListNodeAdded += OnPlayListNodeAdded;
			localPlayersList.PlayListNodeAdded += OnPlayListNodeAdded;
			visitorPlayersList.PlayListNodeAdded += OnPlayListNodeAdded;
			tagsList.PlayListNodeAdded += OnPlayListNodeAdded;

			/* Connect tags events */
			playsList.TagPlay += EmitTagPlay;

			/* Connect SnapshotSeries events */
			playsList.SnapshotSeriesEvent += EmitSnapshotSeries;
			localPlayersList.SnapshotSeriesEvent += EmitSnapshotSeries;
			visitorPlayersList.SnapshotSeriesEvent += EmitSnapshotSeries;
			tagsList.SnapshotSeriesEvent += EmitSnapshotSeries;

			playlist.RenderPlaylistEvent += EmitRenderPlaylist;
			playsList.RenderPlaylistEvent += EmitRenderPlaylist;
			
			renderingstatebar1.ManageJobs += (e, o) => {EmitManageJobs();};
			
			openAction.Activated += (sender, e) => {EmitSaveProject();};
			
			/* Game Units event */
			gameunitstaggerwidget1.GameUnitEvent += EmitGameUnitEvent;
			guTimeline.UnitAdded += EmitUnitAdded;;
			guTimeline.UnitDeleted += EmitUnitDeleted;
			guTimeline.UnitSelected += EmitUnitSelected;
			guTimeline.UnitChanged += EmitUnitChanged;
		}
		
		private void ConnectMenuSignals() {
			SaveProjectAction.Activated += (o, e) => {EmitSaveProject();};
			CloseProjectAction.Activated += (o, e) => {PromptCloseProject();};
			ImportProjectAction.Activated += (o, e) => {EmitImportProject();};
			ExportProjectToCSVFileAction.Activated += (o, e) => {EmitExportProject();};
			QuitAction.Activated += (o, e) => {CloseAndQuit();};
			CategoriesTemplatesManagerAction.Activated += (o, e) => {EmitManageCategories();};
			TeamsTemplatesManagerAction.Activated += (o, e) => {EmitManageTeams();};
			ProjectsManagerAction.Activated += (o, e) => {EmitManageProjects();};
		}
		
		private void UpdateTeamsModels() {
			localPlayersList.SetTeam(openedProject.LocalTeamTemplate, openedProject.AllPlays());
			visitorPlayersList.SetTeam(openedProject.VisitorTeamTemplate, openedProject.AllPlays());
		}

		public void SetProject(Project project, ProjectType projectType, CaptureSettings props)
		{
			bool isLive = false;
			
			/* Update tabs labels */
			var desc = project.Description;
			visitorteamlabel.Text = desc.VisitorName;
			localteamlabel.Text = desc.LocalName;
			
			if(projectType == ProjectType.FileProject) {
				Title = System.IO.Path.GetFileNameWithoutExtension(desc.File.FilePath) +
				        " - " + Constants.SOFTWARE_NAME;
				player.LogoMode = false;
				timeline.Project = project;
				guTimeline.Project = project;

			} else {
				Title = Constants.SOFTWARE_NAME;
				isLive = true;
				if(projectType == ProjectType.FakeCaptureProject)
					capturer.Type = CapturerType.Fake;
				player.Visible = false;
				capturer.Visible = true;
				TaggingViewAction.Active = true;
			}
			
			openedProject = project;
			this.projectType = projectType;
			
			playsList.ProjectIsLive = isLive;
			localPlayersList.ProjectIsLive = isLive;
			visitorPlayersList.ProjectIsLive = isLive;
			tagsList.ProjectIsLive = isLive;
			playsList.Project=project;
			tagsList.Project = project;
			UpdateTeamsModels();
			buttonswidget.Categories = project.Categories;
			MakeActionsSensitive(true,projectType);
			ShowWidgets();
		}
		
		private void CloseCaptureProject() {
			if(projectType == ProjectType.CaptureProject) {
				capturer.Close();
				player.Visible = true;
				capturer.Visible = false;;
				EmitSaveProject();
			} else if(projectType == ProjectType.FakeCaptureProject) {
				EmitCloseOpenedProject(true);
			}
		}

		private void ResetGUI() {
			bool playlistVisible = playlist.Visible;
			Title = Constants.SOFTWARE_NAME;
			player.Visible = true;
			player.LogoMode = true;
			capturer.Visible = false;
			ClearWidgets();
			HideWidgets();
			playlist.Visible = playlistVisible;
			rightvbox.Visible = playlistVisible;
			notes.Visible = false;
			selectedTimeNode = null;
			MakeActionsSensitive(false, projectType);
		}

		private void MakeActionsSensitive(bool sensitive, ProjectType projectType) {
			bool sensitive2 = sensitive && projectType == ProjectType.FileProject;
			CloseProjectAction.Sensitive=sensitive;
			SaveProjectAction.Sensitive = sensitive;
			TaggingViewAction.Sensitive = sensitive2;
			ManualTaggingViewAction.Sensitive = sensitive2;
			GameUnitsViewAction.Sensitive = sensitive2 && gameUnitsActionVisible;
			TimelineViewAction.Sensitive = sensitive2;
			ExportProjectToCSVFileAction.Sensitive = sensitive2;
			HideAllWidgetsAction.Sensitive=sensitive2;
		}

		private void ShowWidgets() {
			leftbox.Show();
			if(TaggingViewAction.Active || ManualTaggingViewAction.Active) {
				buttonswidget.Show();
				gameunitstaggerwidget1.Show();
			} else if (TimelineViewAction.Active) {
				timeline.Show();
			} else if (GameUnitsViewAction.Active) {
				gameunitstaggerwidget1.Show();
				guTimeline.Show();
			}
		}

		private void HideWidgets() {
			leftbox.Hide();
			rightvbox.Hide();
			buttonswidget.Hide();
			timeline.Hide();
			gameunitstaggerwidget1.Hide();
			guTimeline.Hide();
		}

		private void ClearWidgets() {
			buttonswidget.Categories = null;
			playsList.Project = null;
			tagsList.Clear();
			timeline.Project = null;
			localPlayersList.Clear();
			visitorPlayersList.Clear();
		}

		private bool PromptCloseProject() {
			int res;
			EndCaptureDialog dialog;

			if(openedProject == null)
				return true;

			if(projectType == ProjectType.FileProject) {
				MessageDialog md = new MessageDialog(this, DialogFlags.Modal,
				                                     MessageType.Question, ButtonsType.OkCancel,
				                                     Catalog.GetString("Do you want to close the current project?"));
				res = md.Run();
				md.Destroy();
				if(res == (int)ResponseType.Ok) {
					EmitCloseOpenedProject(true);
					return true;
				}
				return false;
			}

			/* Capture project */
			dialog = new EndCaptureDialog();
			dialog.TransientFor = (Gtk.Window)this.Toplevel;
			res = dialog.Run();
			dialog.Destroy();

			/* Close project wihtout saving */
			if(res == (int)EndCaptureResponse.Quit) {
				EmitCloseOpenedProject(false);
				return true;
			} else if(res == (int)EndCaptureResponse.Save) {
				/* Close and save project */
				EmitCloseOpenedProject(true);
				return true;
			} else
				/* Continue with the current project */
				return false;
		}

		private void CloseAndQuit() {
			if(!PromptCloseProject())
				return;
			EmitSaveProject();
			player.Dispose();
			Application.Quit();
		}
		
		#endregion

		#region Callbacks
		#region File
		protected virtual void OnNewActivated(object sender, System.EventArgs e)
		{
			if(!PromptCloseProject())
				return;
			EmitNewProject();
		}

		protected virtual void OnOpenActivated(object sender, System.EventArgs e)
		{
			if(!PromptCloseProject())
				return;
			EmitOpenProject();
		}
		#endregion
		
		#region View
		protected virtual void OnFullScreenActionToggled(object sender, System.EventArgs e)
		{
			player.FullScreen = (sender as Gtk.ToggleAction).Active;
		}

		protected virtual void OnPlaylistActionToggled(object sender, System.EventArgs e)
		{
			bool visible = (sender as Gtk.ToggleAction).Active;
			playlist.Visible=visible;
			playsList.PlayListLoaded=visible;
			localPlayersList.PlayListLoaded=visible;
			visitorPlayersList.PlayListLoaded=visible;

			if(!visible && !notes.Visible) {
				rightvbox.Visible = false;
			} else if(visible) {
				rightvbox.Visible = true;
			}
		}

		protected virtual void OnHideAllWidgetsActionToggled(object sender, System.EventArgs e)
		{
			ToggleAction action = sender as ToggleAction;
			
			if(openedProject == null)
				return;
			
			leftbox.Visible = !action.Active;
			timeline.Visible = !action.Active && TimelineViewAction.Active;
			buttonswidget.Visible = !action.Active &&
				(TaggingViewAction.Active || ManualTaggingViewAction.Active);
			if (Config.useGameUnits) {
				guTimeline.Visible = !action.Visible && GameUnitsViewAction.Active;
				gameunitstaggerwidget1.Visible = !action.Active && (GameUnitsViewAction.Active || 
					TaggingViewAction.Active || ManualTaggingViewAction.Active);
			}
			if(action.Active)
				rightvbox.Visible = false;
			else if(!action.Active && (playlist.Visible || notes.Visible))
				rightvbox.Visible = true;
		}

		protected virtual void OnViewToggled(object sender, System.EventArgs e)
		{
			ToggleAction action = sender as Gtk.ToggleAction;
			
			if (!action.Active)
				return;
			
			buttonswidget.Visible = action == ManualTaggingViewAction || sender == TaggingViewAction;
			timeline.Visible = action == TimelineViewAction;
			if (Config.useGameUnits) {
				guTimeline.Visible = action == GameUnitsViewAction;
				gameunitstaggerwidget1.Visible = buttonswidget.Visible || guTimeline.Visible;
			}
			if(action == ManualTaggingViewAction)
				buttonswidget.Mode = TagMode.Free;
			else
				buttonswidget.Mode = TagMode.Predifined;
		}
		#endregion
		#region Help
		protected virtual void OnHelpAction1Activated(object sender, System.EventArgs e)
		{
			try {
				System.Diagnostics.Process.Start(Constants.MANUAL);
			} catch {}
		}

		protected virtual void OnAboutActionActivated(object sender, System.EventArgs e)
		{
			var about = new LongoMatch.Gui.Dialog.AboutDialog();
			about.TransientFor = this;
			about.Run();
			about.Destroy();
		}
		#endregion

		protected virtual void OnPlayerbin1Error(object o, string message)
		{
			MessagePopup.PopupMessage(this, MessageType.Info,
			                          Catalog.GetString("The actual project will be closed due to an error in the media player:")+"\n" + message);
			EmitCloseOpenedProject(true);
		}

		protected override bool OnKeyPressEvent(EventKey evnt)
		{
			Gdk.Key key = evnt.Key;
			Gdk.ModifierType modifier = evnt.State;
			bool ret;

			ret = base.OnKeyPressEvent(evnt);

			if(openedProject == null && !player.Opened)
				return ret;

			if(projectType != ProjectType.CaptureProject &&
			                projectType != ProjectType.FakeCaptureProject) {
				switch(key) {
				case Constants.SEEK_FORWARD:
					if(modifier == Constants.STEP)
						player.StepForward();
					else
						player.SeekToNextFrame(selectedTimeNode != null);
					break;
				case Constants.SEEK_BACKWARD:
					if(modifier == Constants.STEP)
						player.StepBackward();
					else
						player.SeekToPreviousFrame(selectedTimeNode != null);
					break;
				case Constants.FRAMERATE_UP:
					player.FramerateUp();
					break;
				case Constants.FRAMERATE_DOWN:
					player.FramerateDown();
					break;
				case Constants.TOGGLE_PLAY:
					player.TogglePlay();
					break;
				}
			} else {
				switch(key) {
				case Constants.TOGGLE_PLAY:
					capturer.TogglePause();
					break;
				}
			}
			return ret;
		}

		protected virtual void OnTimeNodeSelected(Play play)
		{
			rightvbox.Visible=true;
			if (PlaySelectedEvent != null)
				PlaySelectedEvent(play);
		}

		protected virtual void OnSegmentClosedEvent()
		{
			if(!playlist.Visible)
				rightvbox.Visible=false;
			timeline.SelectedTimeNode = null;
			notes.Visible = false;
		}
		
		protected virtual void OnTick(object o, long currentTime, long streamLength,
			float currentPosition, bool seekable)
		{
			if(currentTime != 0 && timeline != null && openedProject != null)
				timeline.CurrentFrame=(uint)(currentTime *
				                             openedProject.Description.File.Fps / 1000);
			gameunitstaggerwidget1.CurrentTime = new Time{MSeconds = (int)currentTime};
		}

		
		protected virtual void OnUpdate(Version version, string URL) {
			LongoMatch.Gui.Dialog.UpdateDialog updater = new LongoMatch.Gui.Dialog.UpdateDialog();
			updater.Fill(version, URL);
			updater.TransientFor = this;
			updater.Run();
			updater.Destroy();
		}

		protected virtual void OnDrawingToolActionToggled(object sender, System.EventArgs e)
		{
			drawingtoolbox1.Visible = DrawingToolAction.Active;
			drawingtoolbox1.DrawingVisibility = DrawingToolAction.Active;
		}

		protected override bool OnDeleteEvent(Gdk.Event evnt)
		{
			CloseAndQuit();
			return true;
		}

		protected virtual void OnCapturerBinError(object o, string message)
		{
			MessagePopup.PopupMessage(this, MessageType.Info,
				Catalog.GetString("An error occured in the video capturer and" +
				" the current project will be closed:")+"\n" + message);
			EmitCloseOpenedProject(true);
		}
		#endregion
		
		#region Events
		private void EmitPlaySelected(Play play)
		{
			if (PlaySelectedEvent != null)
				PlaySelectedEvent(play);
		}

		private void EmitTimeNodeChanged(TimeNode tNode, object val)
		{
			if (TimeNodeChanged != null)
				TimeNodeChanged(tNode, val);
		}

		private void EmitPlaysDeleted(List<Play> plays)
		{
			if (PlaysDeletedEvent != null)
				PlaysDeletedEvent(plays);
		}

		private void OnPlayListNodeAdded(Play play)
		{
			if (PlayListNodeAddedEvent != null)
				PlayListNodeAddedEvent(play);
		}

		private void EmitPlayListNodeSelected(PlayListPlay plNode)
		{
			if (PlayListNodeSelectedEvent != null)
				PlayListNodeSelectedEvent(plNode);
		}

		private void EmitSnapshotSeries(Play play) {
			if (SnapshotSeriesEvent != null)
				SnapshotSeriesEvent (play);
		}

		private void EmitNewTagAtFrame(Category category, int frame) {
			if (NewTagAtFrameEvent != null)
				NewTagAtFrameEvent(category, frame);
		}

		private void EmitNewTag(Category category) {
			if (NewTagEvent != null)
				NewTagEvent(category);
		}

		private void EmitNewTagStart() {
			if (NewTagStartEvent != null)
				NewTagStartEvent ();
		}

		private void EmitNewTagStop(Category category) {
			if (NewTagStopEvent != null)
				NewTagStopEvent (category);
		}
		
		private void EmitRenderPlaylist(IPlayList playlist) {
			if (RenderPlaylistEvent != null)
				RenderPlaylistEvent(playlist);
		}
		
		private void EmitApplyRate(PlayListPlay plNode) {
			if (ApplyRateEvent != null)
				ApplyRateEvent (plNode);
		}

		private void EmitTagPlay(Play play) {
			if (TagPlayEvent != null)
				TagPlayEvent (play);
		}
		
		private void EmitSaveProject() {
			if (SaveProjectEvent != null)
				SaveProjectEvent(openedProject, projectType);
		}
		
		private void EmitNewProject() {
			if (NewProjectEvent != null)
				NewProjectEvent();
		}

		private void EmitCloseOpenedProject(bool save) {
			if (CloseOpenedProjectEvent != null)
				CloseOpenedProjectEvent(save);
			openedProject = null;
			projectType = ProjectType.None;
			ResetGUI();
		}
		
		private void EmitImportProject() {
			if (ImportProjectEvent != null)
				ImportProjectEvent();
		}
		
		private void EmitOpenProject() {
			if(OpenProjectEvent != null)
				OpenProjectEvent();
		}
		
		private void EmitExportProject() {
			if(ExportProjectEvent != null)
				ExportProjectEvent();
		}
		
		private void EmitManageJobs() {
			if(ManageJobsEvent != null)
				ManageJobsEvent();
		}
		
		private void EmitManageTeams() {
			if(ManageTeamsEvent != null)
				ManageTeamsEvent();
		}
		
		private void EmitManageCategories() {
			if(ManageCategoriesEvent != null)
				ManageCategoriesEvent();
		}
		
		private void EmitManageProjects()
		{
			if (ManageProjectsEvent != null)
				ManageProjectsEvent();
		}
		
		private void EmitNewPlaylist() {
			if (NewPlaylistEvent != null)
				NewPlaylistEvent();
		}
		
		private void EmitOpenPlaylist() {
			if (OpenPlaylistEvent != null)
				OpenPlaylistEvent();
		}
		
		private void EmitSavePlaylist() {
			if (SavePlaylistEvent != null)
				SavePlaylistEvent();
		}
		
		private void EmitGameUnitEvent(GameUnit gameUnit, GameUnitEventType eType) {
			if (GameUnitEvent != null)
				GameUnitEvent(gameUnit, eType);
		}
		
		private void EmitUnitAdded(GameUnit gameUnit, int frame) {
			if (UnitAdded != null)
				UnitAdded(gameUnit, frame);
		}
		
		private void EmitUnitDeleted(GameUnit gameUnit, List<TimelineNode> units) {
			if (UnitDeleted != null)
				UnitDeleted(gameUnit, units);
		}
		
		private void EmitUnitSelected(GameUnit gameUnit, TimelineNode unit) {
			if (UnitSelected != null)
				UnitSelected(gameUnit, unit);
		}
		
		private void EmitUnitChanged(GameUnit gameUnit, TimelineNode unit, Time time) {
			if (UnitChanged != null)
				UnitChanged(gameUnit, unit, time);
		}
		#endregion
	}
}
