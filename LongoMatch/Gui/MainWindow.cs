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
using Gtk;
using Mono.Unix;
using System.IO;
using GLib;
using System.Threading;
using Gdk;
using LongoMatch.Common;
using LongoMatch.DB;
using LongoMatch.TimeNodes;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Popup;
using LongoMatch.Video;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Player;
using LongoMatch.Video.Utils;
using LongoMatch.Updates;
using LongoMatch.Utils;
using LongoMatch.IO;
using LongoMatch.Handlers;
using System.Reflection;



namespace LongoMatch.Gui
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class MainWindow : Gtk.Window
	{
		private static Project openedProject;
		private ProjectType projectType;
		private TimeNode selectedTimeNode;

		private EventsManager eManager;
		private HotKeysManager hkManager;
		private KeyPressEventHandler hotkeysListener;
		
		private CapturerBin capturerBin;		
		
		#region Constructors
		public MainWindow() :
		base("LongoMatch")
		{
			this.Build();

			/*Updater updater = new Updater();
			updater.NewVersion += new LongoMatch.Handlers.NewVersionHandler(OnUpdate);
			updater.Run();*/
			
			projectType = ProjectType.None;

			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				DrawingToolAction.Visible = false;
				DrawingToolAction.DisconnectAccelerator();
			}

			eManager = new EventsManager(treewidget1,
			                             localplayerslisttreewidget,
			                             visitorplayerslisttreewidget,
			                             tagstreewidget1,
			                             buttonswidget1,
			                             playlistwidget2,
			                             playerbin1,
			                             timelinewidget1,
			                             videoprogressbar,
			                             noteswidget1);

			hkManager = new HotKeysManager();
			// Listenning only when a project is loaded
			hotkeysListener = new KeyPressEventHandler(hkManager.KeyListener);
			// Forward the event to the events manager
			hkManager.newMarkEvent += new NewMarkEventHandler(eManager.OnNewMark);

			DrawingManager dManager = new DrawingManager(drawingtoolbox1,playerbin1.VideoWidget);
			//Forward Key and Button events to the Drawing Manager
			KeyPressEvent += new KeyPressEventHandler(dManager.OnKeyPressEvent);

			playerbin1.SetLogo(System.IO.Path.Combine(MainClass.ImagesDir(),"background.png"));
			playerbin1.LogoMode = true;
			
			buttonswidget1.Mode = TagMode.Predifined;

			playlistwidget2.SetPlayer(playerbin1);

			localplayerslisttreewidget.Team = Team.LOCAL;
			visitorplayerslisttreewidget.Team = Team.VISITOR;
		}

		#endregion
		
		#region Private Methods
		private void SetProject(Project project, ProjectType projectType) {
			if (project == null)
				return;
			
			CloseOpenedProject(true);
			openedProject = project;
			this.projectType = projectType;
			eManager.OpenedProject = project;
			eManager.OpenedProjectType = projectType;
			
			if (projectType == ProjectType.FileProject){
				// Check if the file associated to the project exists
				if (!File.Exists(project.File.FilePath)) {
					MessagePopup.PopupMessage(this, MessageType.Warning,
					                          Catalog.GetString("The file associated to this project doesn't exist.")+"\n"
					                          +Catalog.GetString("If the location of the file has changed try to edit it with the database manager."));
					CloseOpenedProject(true);
					return;
				} else {
					Title = System.IO.Path.GetFileNameWithoutExtension(project.File.FilePath) + " - LongoMatch";
					try {
						playerbin1.Open(project.File.FilePath);							
					}
					catch (GLib.GException ex) {
						MessagePopup.PopupMessage(this, MessageType.Error,
						                          Catalog.GetString("An error occurred opening this project:")+"\n"+ex.Message);
						CloseOpenedProject(true);
						return;
					}
					if (project.File.HasVideo)
						playerbin1.LogoMode = true;
					else
						playerbin1.LogoMode = false;							
					if (project.File.HasVideo)
						playerbin1.LogoMode = false;
					timelinewidget1.Project = project;
					treewidget1.ProjectIsLive = false;
					localplayerslisttreewidget.ProjectIsLive = false;
					visitorplayerslisttreewidget.ProjectIsLive = false;
					tagstreewidget1.ProjectIsLive = false;						
				} 
			}else {
				Title = "LongoMatch";
				playerbin1.Visible = false;					
				capturerBin = new CapturerBin();
				capturerBin.Logo = System.IO.Path.Combine(MainClass.ImagesDir(),"background.png");
				capturerBin.CaptureFinished += delegate {
					CloseOpenedProject(true);	
				};
				eManager.Capturer = capturerBin;
				hbox2.Add(capturerBin);
				(capturerBin).Show();	
				treewidget1.ProjectIsLive = true;
				localplayerslisttreewidget.ProjectIsLive = true;
				visitorplayerslisttreewidget.ProjectIsLive = true;
				tagstreewidget1.ProjectIsLive = true;	
				CaptureModeAction.Active = true;
			}
			
			playlistwidget2.Stop();
			treewidget1.Project=project;
			localplayerslisttreewidget.SetTeam(project.LocalTeamTemplate,project.GetLocalTeamModel());
			visitorplayerslisttreewidget.SetTeam(project.VisitorTeamTemplate,project.GetVisitorTeamModel());
			tagstreewidget1.Project = project;				
			buttonswidget1.Sections = project.Sections;
			MakeActionsSensitive(true,projectType);
			ShowWidgets();
			hkManager.Sections=project.Sections;
			KeyPressEvent += hotkeysListener;			
		}
		
		private void CloseOpenedProject(bool save) {
			bool playlistVisible = playlistwidget2.Visible;			
			
			if (save)
				SaveProject();
			
			if (projectType != ProjectType.FileProject){
				playerbin1.Visible = true;
				eManager.Capturer = null;
				if (capturerBin != null)
					capturerBin.Destroy();
			} else {
				playerbin1.Close();
				playerbin1.LogoMode = true;
			}
			Title = "LongoMatch";
			ClearWidgets();
			HideWidgets();	
			
			if (openedProject != null) {
				openedProject.Clear();
				openedProject = null;
				projectType = ProjectType.None;
				eManager.OpenedProject = null;
				eManager.OpenedProjectType = ProjectType.None;				
			}			
			playlistwidget2.Visible = playlistVisible;
			rightvbox.Visible = playlistVisible;
			noteswidget1.Visible = false;			
			selectedTimeNode = null;
			MakeActionsSensitive(false, projectType);
			hkManager.Sections = null;
			KeyPressEvent -= hotkeysListener;
		}

		private void MakeActionsSensitive(bool sensitive, ProjectType projectType) {
			bool sensitive2 = sensitive && projectType == ProjectType.FileProject;
			CloseProjectAction.Sensitive=sensitive;
			SaveProjectAction.Sensitive = sensitive;
			CaptureModeAction.Sensitive = sensitive2;
			FreeCaptureModeAction.Sensitive = sensitive2;
			AnalyzeModeAction.Sensitive = sensitive2;
			ExportProjectToCSVFileAction.Sensitive = sensitive2;
			HideAllWidgetsAction.Sensitive=sensitive2;
		}

		private void ShowWidgets() {
			leftbox.Show();
			if (CaptureModeAction.Active || FreeCaptureModeAction.Active)
				buttonswidget1.Show();
			else
				timelinewidget1.Show();
		}

		private void HideWidgets() {
			leftbox.Hide();
			rightvbox.Hide();
			buttonswidget1.Hide();
			timelinewidget1.Hide();
		}

		private void ClearWidgets() {
			buttonswidget1.Sections = null;
			treewidget1.Project = null;
			tagstreewidget1.Clear();
			timelinewidget1.Project = null;
			localplayerslisttreewidget.Clear();
			visitorplayerslisttreewidget.Clear();
		}

		private void SaveProject() {
			if (openedProject != null && projectType == ProjectType.FileProject) {
				MainClass.DB.UpdateProject(openedProject);
			} else if (projectType == ProjectType.FakeCaptureProject)
				ProjectUtils.SaveFakeLiveProject(openedProject, this);
		}
		
		private bool PromptCloseProject(){
			int res;
			EndCaptureDialog dialog;
			
			if (openedProject == null)
				return true;
			
			if (projectType == ProjectType.FileProject){
				MessageDialog md = new MessageDialog(this, DialogFlags.Modal, 
				                                     MessageType.Question, ButtonsType.OkCancel,
				                                     Catalog.GetString("Do you want to close the current project?"));
				res = md.Run();
				md.Destroy();
				if (res == (int)ResponseType.Ok){
					CloseOpenedProject(true);
					return true;
				}
				return false;
			}
			
			dialog = new EndCaptureDialog();
			dialog.TransientFor = (Gtk.Window)this.Toplevel;			
			res = dialog.Run();
			dialog.Destroy();			
			
			/* Close project wihtout saving */
			if (res == (int)EndCaptureResponse.Quit){
				CloseOpenedProject(false);
				return true;
			} else if (res == (int)EndCaptureResponse.Save){
				/* Close and save project */
				CloseOpenedProject(true);
				return true;
			} else
				/* Continue with the current project */
				return false;			
		}
		
		private void CloseAndQuit(){
			if (!PromptCloseProject())
				return;
			playlistwidget2.StopEdition();
			SaveProject();
			playerbin1.Dispose();
			Application.Quit();
		}
		#endregion	

		#region Callbacks
		#region File
		protected virtual void OnNewActivated(object sender, System.EventArgs e)
		{
			Project project;
			ProjectType projectType;
			
			if (!PromptCloseProject())
				return;
			
			ProjectUtils.CreateNewProject(this, out project, out projectType);	
			if (project != null)
				SetProject(project, projectType);
		}
		
		protected virtual void OnOpenActivated(object sender, System.EventArgs e)
		{
			if (!PromptCloseProject())
				return;
			
			ProjectDescription project=null;
			OpenProjectDialog opd = new OpenProjectDialog();
			opd.TransientFor = this;

			if (opd.Run() == (int)ResponseType.Ok)
				project = opd.GetSelection();
			opd.Destroy();
			if (project != null)
				SetProject(MainClass.DB.GetProject(project.File), ProjectType.FileProject);
		}
		
		protected virtual void OnSaveProjectActionActivated(object sender, System.EventArgs e)
		{
			SaveProject();
		}
		
		protected virtual void OnCloseActivated(object sender, System.EventArgs e)
		{
			PromptCloseProject();
		}
		
		protected virtual void OnImportProjectActionActivated (object sender, System.EventArgs e)
		{
			ProjectUtils.ImportProject(this);
		}
		
		protected virtual void OnQuitActivated(object sender, System.EventArgs e)
		{
			CloseAndQuit();
		}	
		#endregion
		#region Tools
		protected virtual void OnDatabaseManagerActivated(object sender, System.EventArgs e)
		{
			ProjectsManager pm = new ProjectsManager(openedProject);
			pm.TransientFor = this;
			pm.Show();
		}
		
		protected virtual void OnSectionsTemplatesManagerActivated(object sender, System.EventArgs e)
		{
			TemplatesManager tManager = new TemplatesManager(TemplatesManager.UseType.SectionsTemplate);
			tManager.TransientFor = this;
			tManager.Show();
		}

		protected virtual void OnTeamsTemplatesManagerActionActivated(object sender, System.EventArgs e)
		{
			TemplatesManager tManager = new TemplatesManager(TemplatesManager.UseType.TeamTemplate);
			tManager.TransientFor = this;
			tManager.Show();
		}
		
		protected virtual void OnExportProjectToCSVFileActionActivated(object sender, System.EventArgs e)
		{
			ProjectUtils.ExportToCSV(this, openedProject);
		}
		#endregion
		#region View
		protected virtual void OnFullScreenActionToggled(object sender, System.EventArgs e)
		{
			playerbin1.FullScreen = ((Gtk.ToggleAction)sender).Active;
		}
		
		protected virtual void OnPlaylistActionToggled(object sender, System.EventArgs e)
		{
			bool visible = ((Gtk.ToggleAction)sender).Active;
			playlistwidget2.Visible=visible;
			treewidget1.PlayListLoaded=visible;
			localplayerslisttreewidget.PlayListLoaded=visible;
			visitorplayerslisttreewidget.PlayListLoaded=visible;

			if (!visible && !noteswidget1.Visible) {
				rightvbox.Visible = false;
			}
			else if (visible) {
				rightvbox.Visible = true;
			}
		}	
		
		protected virtual void OnHideAllWidgetsActionToggled(object sender, System.EventArgs e)
		{
			if (openedProject != null) {
				leftbox.Visible = !((Gtk.ToggleAction)sender).Active;
				timelinewidget1.Visible = !((Gtk.ToggleAction)sender).Active && AnalyzeModeAction.Active;
				buttonswidget1.Visible = !((Gtk.ToggleAction)sender).Active && 
					(CaptureModeAction.Active || CaptureModeAction.Active);
				if (((Gtk.ToggleAction)sender).Active)
					rightvbox.Visible = false;
				else if (!((Gtk.ToggleAction)sender).Active && (playlistwidget2.Visible || noteswidget1.Visible))
					rightvbox.Visible = true;
			}
		}
		
		protected virtual void OnViewToggled(object sender, System.EventArgs e)
		{
			/* this callback is triggered by Capture and Free Capture */
			ToggleAction view = (Gtk.ToggleAction)sender;
			buttonswidget1.Visible = view.Active;
			timelinewidget1.Visible = !view.Active;
			if (view == FreeCaptureModeAction)
				buttonswidget1.Mode = TagMode.Free;
			else 
				buttonswidget1.Mode = TagMode.Predifined;			
		}	
		#endregion
		#region Help
		protected virtual void OnHelpAction1Activated(object sender, System.EventArgs e)
		{
			try {
				System.Diagnostics.Process.Start("http://www.longomatch.ylatuya.es/documentation/manual.html");
			} catch {}
		}
		
		protected virtual void OnAboutActionActivated(object sender, System.EventArgs e)
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			Gtk.AboutDialog about = new AboutDialog();
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				about.ProgramName = "LongoMatch";
			about.Version = String.Format("{0}.{1}.{2}",version.Major,version.Minor,version.Build);
			about.Copyright = Constants.COPYRIGHT;
			about.Website = Constants.WEBSITE;
			about.License = Constants.LICENSE;
			about.Authors = new string[] {"Andoni Morales Alastruey"};
			about.Artists = new string[] {"Bencomo González Marrero"};
			about.TranslatorCredits = Constants.TRANSLATORS;
			about.TransientFor = this;
			Gtk.AboutDialog.SetUrlHook(delegate(AboutDialog dialog,string url) {
				try {
					System.Diagnostics.Process.Start(url);
				} catch {}
			});
			about.Run();
			about.Destroy();

		}
		#endregion			

		protected virtual void OnTimeprecisionadjustwidget1SizeRequested(object o, Gtk.SizeRequestedArgs args)
		{
			if (args.Requisition.Width>= hpaned.Position)
				hpaned.Position = args.Requisition.Width;
		}		

		protected virtual void OnPlayerbin1Error(object o,LongoMatch.Video.Handlers.ErrorArgs args)
		{
			MessagePopup.PopupMessage(this, MessageType.Info,
			                          Catalog.GetString("The actual project will be closed due to an error in the media player:")+"\n" +args.Message);
			CloseOpenedProject(true);
		}

		protected override bool OnKeyPressEvent(EventKey evnt)
		{
			if (openedProject != null && evnt.State == ModifierType.None) {
				Gdk.Key key = evnt.Key;
				switch (key){
					case Constants.PREV_FRAME:
						playerbin1.SeekToPreviousFrame(selectedTimeNode != null);
						break;
					case Constants.NEXT_FRAME:
						playerbin1.SeekToNextFrame(selectedTimeNode != null);
						break;
					case Constants.STEP_FORWARD:
						playerbin1.StepForward();
						break;
					case Constants.STEP_BACKWARD:
						playerbin1.StepBackward();
						break;
					case Constants.FRAMERATE_UP:
						playerbin1.FramerateUp();
						break;
					case Constants.FRAMERATE_DOWN:
						playerbin1.FramerateDown();
						break;
					case Constants.TOGGLE_PLAY:
						playerbin1.TogglePlay();
						break;			
				}					
			}
			return base.OnKeyPressEvent(evnt);
		}

		protected virtual void OnTimeNodeSelected(LongoMatch.TimeNodes.MediaTimeNode tNode)
		{
			rightvbox.Visible=true;
		}

		protected virtual void OnSegmentClosedEvent()
		{
			if (!playlistwidget2.Visible)
				rightvbox.Visible=false;
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

		protected override bool OnDeleteEvent (Gdk.Event evnt)
		{
			CloseAndQuit();	
			return true;
		}

		#endregion			}
	}
}