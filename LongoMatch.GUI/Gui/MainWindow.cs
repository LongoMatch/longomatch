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
		
		public event KeyHandler KeyPressed;

		private static Project openedProject;
		private ProjectType projectType;
		private TimeNode selectedTimeNode;		

		TimeLineWidget timeline;
		bool gameUnitsActionVisible;
		GameUnitsTimelineWidget guTimeline;
		IGUIToolkit guiToolKit;
		Gtk.Window playerWindow;
		bool detachedPlayer;

		#region Constructors
		public MainWindow(IGUIToolkit guiToolkit) :
		base(Constants.SOFTWARE_NAME)
		{
			Screen screen;
			
			this.Build();
			Title = Constants.SOFTWARE_NAME;

			this.guiToolKit = guiToolkit;
			
			projectType = ProjectType.None;

			timeline = new TimeLineWidget();
			downbox.PackStart(timeline, true, true, 0);
			
			guTimeline = new GameUnitsTimelineWidget ();
			downbox.PackStart(guTimeline, true, true, 0);
			
			player.SetLogo(System.IO.Path.Combine(Config.ImagesDir(),"background.png"));
			player.LogoMode = true;
			player.Tick += OnTick;
			player.Detach += (sender, e) => DetachPlayer(true);

			capturer.Visible = false;
			capturer.Logo = System.IO.Path.Combine(Config.ImagesDir(),"background.png");
			capturer.CaptureFinished += (sender, e) => {CloseCaptureProject();};
			
			buttonswidget.Mode = TagMode.Predifined;
			ConnectSignals();
			ConnectMenuSignals();
			
			if (!Config.useGameUnits)
				GameUnitsViewAction.Visible = false;
			
			AddImportEntry(Catalog.GetString("Import file project"), "ImportFileProject",
			               Constants.PROJECT_NAME + " (" + Constants.PROJECT_EXT + ")",
			               "*" + Constants.PROJECT_EXT, Project.Import,
			               false);
			screen = Display.Default.DefaultScreen;
			this.Resize(screen.Width * 80 / 100, screen.Height * 80 / 100);
		}

		#endregion
		
		#region Plubic Methods
		public void AddPlay(Play play) {
			playsSelection.AddPlay(play);
			timeline.AddPlay(play);
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
			playsSelection.RemovePlays(plays);
			timeline.RemovePlays(plays);
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
		
		public ITemplatesService TemplatesService {
			set {
				playsSelection.TemplatesService = value;
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
		
		public void AddExportEntry (string name, string shortName, Action<Project, IGUIToolkit> exportAction) {
			MenuItem parent = (MenuItem) this.UIManager.GetWidget("/menubar1/ToolsAction/ExportProjectAction1");
			
			MenuItem item = new MenuItem(name);
			item.Activated += (sender, e) => (exportAction(openedProject, guiToolKit));
			item.Show();
			(parent.Submenu as Menu).Append(item);
		}
		
		public void AddImportEntry (string name, string shortName, string filterName,
		                            string filter, Func<string, Project> importFunc,
		                            bool requiresNewFile) {
			MenuItem parent = (MenuItem) this.UIManager.GetWidget("/menubar1/FileAction/ImportProjectAction");
			
			MenuItem item = new MenuItem(name);
			item.Activated += (sender, e) => (EmitImportProject(name, filterName, filter, importFunc, requiresNewFile));
			item.Show();
			(parent.Submenu as Menu).Append(item);
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
			timeline.TimeNodeChanged += EmitTimeNodeChanged;
			notes.TimeNodeChanged += EmitTimeNodeChanged;

			/* Connect TimeNodeDeleted events */
			playsSelection.PlaysDeleted += EmitPlaysDeleted;
			timeline.TimeNodeDeleted += EmitPlaysDeleted;

			/* Connect TimeNodeSelected events */
			playsSelection.PlaySelected += OnTimeNodeSelected;
			timeline.TimeNodeSelected += OnTimeNodeSelected;

			/* Connect playlist events */
			playlist.PlayListNodeSelected += EmitPlayListNodeSelected;
			playlist.ApplyCurrentRate += EmitApplyRate;
			playlist.NewPlaylistEvent += EmitNewPlaylist;
			playlist.OpenPlaylistEvent += EmitOpenPlaylist;
			playlist.SavePlaylistEvent += EmitSavePlaylist;

			/* Connect PlayListNodeAdded events */
			playsSelection.PlayListNodeAdded += OnPlayListNodeAdded;

			/* Connect tags events */
			playsSelection.TagPlay += EmitTagPlay;

			/* Connect SnapshotSeries events */
			playsSelection.SnapshotSeries += EmitSnapshotSeries;

			playlist.RenderPlaylistEvent += EmitRenderPlaylist;
			playsSelection.RenderPlaylist += EmitRenderPlaylist;
			
			renderingstatebar1.ManageJobs += (e, o) => {EmitManageJobs();};
			
			openAction.Activated += (sender, e) => {EmitSaveProject();};
			
			/* Game Units event */
			gameunitstaggerwidget1.GameUnitEvent += EmitGameUnitEvent;
			guTimeline.UnitAdded += EmitUnitAdded;;
			guTimeline.UnitDeleted += EmitUnitDeleted;
			guTimeline.UnitSelected += EmitUnitSelected;
			guTimeline.UnitChanged += EmitUnitChanged;
			
			KeyPressEvent += (o, args) => (EmitKeyPressed(o, (int)args.Event.Key, (int)args.Event.Key));
 		}
		
		private void ConnectMenuSignals() {
			SaveProjectAction.Activated += (o, e) => {EmitSaveProject();};
			CloseProjectAction.Activated += (o, e) => {PromptCloseProject();};
			ExportToProjectFileAction.Activated += (o, e) => {EmitExportProject();};
			QuitAction.Activated += (o, e) => {CloseAndQuit();};
			CategoriesTemplatesManagerAction.Activated += (o, e) => {EmitManageCategories();};
			TeamsTemplatesManagerAction.Activated += (o, e) => {EmitManageTeams();};
			ProjectsManagerAction.Activated += (o, e) => {EmitManageProjects();};
		}
		
		void DetachPlayer (bool detach) {
			if (detach == detachedPlayer)
				return;
				
			detachedPlayer = detach;
			
			if (detach) {
				EventBox box;
				Log.Debug("Detaching player");
				
				playerWindow = new Gtk.Window(Constants.SOFTWARE_NAME);
				playerWindow.Icon = Stetic.IconLoader.LoadIcon(this, "longomatch", IconSize.Button);
				playerWindow.DeleteEvent += (o, args) => DetachPlayer(false);
				box = new EventBox();
				
				box.KeyPressEvent += (o, args) => OnKeyPressEvent(args.Event);
				playerWindow.Add(box);
				
				box.Show();
				playerWindow.Show();
				
				player.Reparent(box);
				buttonswidget.Visible = true;
				timeline.Visible = true;
				if (Config.useGameUnits) {
					guTimeline.Visible = true;
					gameunitstaggerwidget1.Visible = true;
				}
			} else {
				ToggleAction action;
				
				Log.Debug("Attaching player again");
				player.Reparent(this.videowidgetsbox);
				playerWindow.Destroy();
				
				if (ManualTaggingViewAction.Active)
					action = ManualTaggingViewAction;
				else if (TimelineViewAction.Active)
					action = TimelineViewAction;
				else if (GameUnitsViewAction.Active)
					action = GameUnitsViewAction;
				else
					action = TaggingViewAction;
				OnViewToggled(action, new EventArgs());
			}
		}

		public void SetProject(Project project, ProjectType projectType, CaptureSettings props, PlaysFilter filter)
		{
			bool isLive = false;
			
			/* Update tabs labels */
			var desc = project.Description;
			
			ExportProjectAction1.Sensitive = true;
			
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
			
			filter.FilterUpdated += OnFilterUpdated;
			playsSelection.SetProject(project, isLive, filter);
			buttonswidget.Categories = project.Categories;
			MakeActionsSensitive(true,projectType);
			ShowWidgets();
		}
		
		private void CloseCaptureProject() {
			if(projectType == ProjectType.CaptureProject) {
				capturer.Close();
				player.Visible = true;
				capturer.Visible = false;
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
			if (detachedPlayer)
				DetachPlayer(false);
		}

		private void MakeActionsSensitive(bool sensitive, ProjectType projectType) {
			bool sensitive2 = sensitive && projectType == ProjectType.FileProject;
			CloseProjectAction.Sensitive=sensitive;
			SaveProjectAction.Sensitive = sensitive;
			TaggingViewAction.Sensitive = sensitive2;
			ManualTaggingViewAction.Sensitive = sensitive2;
			GameUnitsViewAction.Sensitive = sensitive2 && gameUnitsActionVisible;
			TimelineViewAction.Sensitive = sensitive2;
			ExportProjectAction1.Sensitive = sensitive2;
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
			playsSelection.Clear();
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
			playsSelection.PlayListLoaded=visible;

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
			var about = new LongoMatch.Gui.Dialog.AboutDialog(guiToolKit.Version);
			about.TransientFor = this;
			about.Run();
			about.Destroy();
		}
		
		protected void OnDialogInfoActionActivated (object sender, System.EventArgs e)
		{
			var info = new LongoMatch.Gui.Dialog.ShortcutsHelpDialog();
			info.TransientFor = this;
			info.Run();
			info.Destroy();
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
			if(currentTime != 0 && timeline != null && openedProject != null) {
				uint frame = (uint)(currentTime * openedProject.Description.File.Fps / 1000);
				timeline.CurrentFrame = frame;
				guTimeline.CurrentFrame = frame;
			}
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
		
		protected virtual void OnFilterUpdated()
		{
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
		
		private void EmitImportProject(string name, string filterName, string filter,
		                               Func<string, Project> func, bool requiresNewFile) {
			if (ImportProjectEvent != null)
				ImportProjectEvent(name, filterName, filter, func, requiresNewFile);
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
		
		private void EmitKeyPressed(object sender, int key, int modifier) {
			if (KeyPressed != null)
				KeyPressed(sender, key, modifier);
		}


		#endregion
	}
}
