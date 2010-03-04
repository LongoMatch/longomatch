﻿// MainWindow.cs
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
using LongoMatch.Updates;
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

			playlistwidget2.SetPlayer(playerbin1);

			localplayerslisttreewidget.Team = Team.LOCAL;
			visitorplayerslisttreewidget.Team = Team.VISITOR;
		}

		#endregion


		
		#region Private Methods
		private void SetProject(Project project, ProjectType projectType) {
			CloseActualProyect();
			openedProject = project;
			this.projectType = projectType;
			eManager.OpenedProject = project;
			eManager.OpenedProjectType = projectType;
			if (project!=null) {
				if (projectType == ProjectType.NewFileProject){
					// Check if the file associated to the project exists
					if (!File.Exists(project.File.FilePath)) {
						MessagePopup.PopupMessage(this, MessageType.Warning,
						                          Catalog.GetString("The file associated to this project doesn't exist.")+"\n"+Catalog.GetString("If the location of the file has changed try to edit it with the database manager."));
						CloseActualProyect();
					}
					else {
						Title = System.IO.Path.GetFileNameWithoutExtension(project.File.FilePath) + " - LongoMatch";
						try {
							playerbin1.Open(project.File.FilePath);							
						}
						catch (GLib.GException ex) {
							MessagePopup.PopupMessage(this, MessageType.Error,
							                          Catalog.GetString("An error occurred opening this project:")+"\n"+ex.Message);
							CloseActualProyect();
							return;
						}
						if (project.File.HasVideo)
							playerbin1.LogoMode = true;
						else
							playerbin1.LogoMode = false;							
						if (project.File.HasVideo)
							playerbin1.LogoMode = false;
						timelinewidget1.Project = project;
					} 
				}else {
					Title = "LongoMatch";
					playerbin1.Visible = false;					
					capturerBin = new CapturerBin();
					eManager.Capturer = capturerBin;
					hbox2.Add(capturerBin);
					(capturerBin).Show();	
				}
				
				playlistwidget2.Stop();
				treewidget1.Project=project;
				localplayerslisttreewidget.SetTeam(project.LocalTeamTemplate,project.GetLocalTeamModel());
				visitorplayerslisttreewidget.SetTeam(project.VisitorTeamTemplate,project.GetVisitorTeamModel());
				tagstreewidget1.Project = project;				
				buttonswidget1.Sections = project.Sections;
				MakeActionsSensitive(true);
				ShowWidgets();
				hkManager.Sections=project.Sections;
				KeyPressEvent += hotkeysListener;
			}
		}

		private void CloseActualProyect() {
			bool playlistVisible = playlistwidget2.Visible;
			
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
			if (projectType != ProjectType.NewFileProject){
				playerbin1.Visible = true;
				eManager.Capturer = null;
				if (capturerBin != null)
					capturerBin.Destroy();			
			}
			else {
				playerbin1.Close();
				playerbin1.LogoMode = true;
			}
			playlistwidget2.Visible = playlistVisible;
			rightvbox.Visible = playlistVisible;
			noteswidget1.Visible = false;
			SaveDB();
			selectedTimeNode = null;
			MakeActionsSensitive(false);
			hkManager.Sections = null;
			KeyPressEvent -= hotkeysListener;
		}

		private void MakeActionsSensitive(bool sensitive) {
			CloseProjectAction.Sensitive=sensitive;
			SaveProjectAction.Sensitive = sensitive;
			CaptureModeAction.Sensitive = sensitive;
			AnalyzeModeAction.Sensitive = sensitive;
			ExportProjectToCSVFileAction.Sensitive = sensitive;
			HideAllWidgetsAction.Sensitive=sensitive;
		}

		private void ShowWidgets() {
			leftbox.Show();
			if (CaptureModeAction.Active)
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

		private void SaveDB() {
			if (openedProject != null && projectType == ProjectType.NewFileProject) {
				MainClass.DB.UpdateProject(OpenedProject());
			}
		}
		
		private bool FinishCapture(){
			bool res=false;
			
			if (projectType == ProjectType.None || 
			    projectType == ProjectType.NewFileProject)
				return true;
			MessageDialog md = new MessageDialog((Gtk.Window)this.Toplevel, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo,
			                                     Catalog.GetString("A capture project is actually running."+
			                                                       "This action will stop the ongoing capture and save the project"+"\n"+
			                                                       "Do you want to proceed?"));
			if (md.Run() == (int)ResponseType.Yes){
				CloseActualProyect();
				res = true;
			}
			md.Destroy();			
			return res;
		}

		#endregion

		#region Public Methods
		public static Project OpenedProject() {
			return openedProject;
		}

		#endregion

		#region Callbacks

		protected virtual void OnUnrealized(object sender, System.EventArgs e) {
			Destroy();
			Application.Quit();
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

		protected virtual void OnOpenActivated(object sender, System.EventArgs e)
		{
			if (!FinishCapture())
				return;
			
			ProjectDescription project=null;
			OpenProjectDialog opd = new OpenProjectDialog();
			opd.TransientFor = this;

			if (opd.Run() == (int)ResponseType.Ok)
				project = opd.GetSelection();
			opd.Destroy();
			if (project != null)
				SetProject(MainClass.DB.GetProject(project.File), ProjectType.NewFileProject);
		}

		protected virtual void OnNewActivated(object sender, System.EventArgs e)
		{
			Project project;
			ProjectType type;
			ProjectSelectionDialog psd;
			NewProjectDialog npd;
			
			if (!FinishCapture())
				return;
			
			// Show the project selection dialog
			psd = new ProjectSelectionDialog();
			psd.TransientFor = this;
			if (psd.Run() != (int)ResponseType.Ok){		
				psd.Destroy();
				return;
			}
			type = psd.Type;
			psd.Destroy();
			
			// Show the new project dialog and wait the get a valid project or for the 
			// the user cancelling the creation of a new project;
			npd = new NewProjectDialog();
			npd.TransientFor = this;
			npd.Use = type;
			int response = npd.Run();
			while (response == (int)ResponseType.Ok && npd.GetProject() == null) {
				MessagePopup.PopupMessage(this, MessageType.Info,
				                          Catalog.GetString("Please, select a video file."));
				response=npd.Run();
			}
			npd.Destroy();
			// Si se cumplen las condiciones y se ha pulsado el botón aceptar continuamos
			if (response ==(int)ResponseType.Ok) {
				project = npd.GetProject();
				if (type == ProjectType.NewFileProject) {
					try {
						MainClass.DB.AddProject(project);
					}
					catch {
						MessagePopup.PopupMessage(this, MessageType.Error,
						                          Catalog.GetString("This file is already used in a Project.")+"\n"+Catalog.GetString("Open the project, please."));
						return;
					}					
				}
				SetProject(project, type);				
			}
		}

		protected virtual void OnCloseActivated(object sender, System.EventArgs e)
		{
			if (FinishCapture())
				CloseActualProyect();
		}
		
		protected virtual void OnImportProjectActionActivated (object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Import Project"),
			                (Gtk.Window)Toplevel,
			                FileChooserAction.Open,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-open",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.HomeDir());
			FileFilter filter = new FileFilter();
			filter.Name = "LongoMatch Project";
			filter.AddPattern("*.lpr");

			fChooser.AddFilter(filter);
			if (fChooser.Run() == (int)ResponseType.Accept) {
				Project project;
				try{
					project = Project.Import(fChooser.Filename);
					if (!MainClass.DB.Exists(project)){
						MainClass.DB.AddProject(project);
						MessagePopup.PopupMessage(this, MessageType.Info, 
						                          Catalog.GetString("Project successfully imported."));
					}
					else{
						MessageDialog md = new MessageDialog((Gtk.Window)this.Toplevel,
						                       DialogFlags.Modal,
						                       MessageType.Question,
						                       Gtk.ButtonsType.YesNo,
						                       Catalog.GetString("A project already exists for the file:")+project.File.FilePath+
						                       "\n"+Catalog.GetString("Do you want to overwrite it?"));
						md.Icon=Stetic.IconLoader.LoadIcon(this, "longomatch", Gtk.IconSize.Dialog, 48);
						if (md.Run() == (int)ResponseType.Yes){
							MainClass.DB.UpdateProject(project);
						}
						md.Destroy();						
					}
				}
				catch (Exception ex){
					MessagePopup.PopupMessage(this, MessageType.Error, ex.Message);
					fChooser.Destroy();
					return;
				}
			}
			fChooser.Destroy();
			
		}

		protected virtual void OnDatabaseManagerActivated(object sender, System.EventArgs e)
		{
			ProjectsManager pm = new ProjectsManager();
			pm.TransientFor = this;
			pm.Show();
		}

		protected virtual void OnTimeprecisionadjustwidget1SizeRequested(object o, Gtk.SizeRequestedArgs args)
		{
			if (args.Requisition.Width>= hpaned.Position)
				hpaned.Position = args.Requisition.Width;
		}

		protected virtual void OnDeleteEvent(object o, Gtk.DeleteEventArgs args)
		{
			if (!FinishCapture())
				return;
			playlistwidget2.StopEdition();
			SaveDB();
			// We never know...
			System.Threading.Thread.Sleep(1000);
			playerbin1.Dispose();
			Application.Quit();
		}

		protected virtual void OnQuitActivated(object sender, System.EventArgs e)
		{
			if (!FinishCapture())
				return;
			playlistwidget2.StopEdition();
			SaveDB();
			// We never know...
			System.Threading.Thread.Sleep(1000);
			playerbin1.Dispose();
			Application.Quit();
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

		protected virtual void OnOpenPlaylistActionActivated(object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Open playlist"),
			                (Gtk.Window)Toplevel,
			                FileChooserAction.Open,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-open",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.PlayListDir());
			FileFilter filter = new FileFilter();
			filter.Name = "LGM playlist";
			filter.AddPattern("*.lgm");

			fChooser.AddFilter(filter);
			if (fChooser.Run() == (int)ResponseType.Accept) {
				if (openedProject != null)
					CloseActualProyect();
				playlistwidget2.Load(fChooser.Filename);
				PlaylistAction.Active = true;
			}
			fChooser.Destroy();
		}

		protected virtual void OnPlayerbin1Error(object o,LongoMatch.Video.Handlers.ErrorArgs args)
		{
			MessagePopup.PopupMessage(this, MessageType.Info,
			                          Catalog.GetString("The actual project will be closed due to an error in the media player:")+"\n" +args.Message);
			CloseActualProyect();
		}

		protected virtual void OnCaptureModeActionToggled(object sender, System.EventArgs e)
		{
			if (((Gtk.ToggleAction)sender).Active) {
				buttonswidget1.Show();
				timelinewidget1.Hide();
			}
			else {
				buttonswidget1.Hide();
				timelinewidget1.Show();
			}
		}

		protected virtual void OnFullScreenActionToggled(object sender, System.EventArgs e)
		{
			playerbin1.FullScreen = ((Gtk.ToggleAction)sender).Active;
		}

		protected virtual void OnSaveProjectActionActivated(object sender, System.EventArgs e)
		{
			SaveDB();
		}

		protected override bool OnKeyPressEvent(EventKey evnt)
		{
			if (openedProject != null && evnt.State == ModifierType.None) {
				Gdk.Key key = evnt.Key;
				if (key == Gdk.Key.z)
					playerbin1.SeekToPreviousFrame(selectedTimeNode != null);
				if (key == Gdk.Key.x)
					playerbin1.SeekToNextFrame(selectedTimeNode != null);
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

		protected virtual void OnAboutActionActivated(object sender, System.EventArgs e)
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			Gtk.AboutDialog about = new AboutDialog();
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				about.ProgramName = "LongoMatch";
			about.Version = String.Format("{0}.{1}.{2}",version.Major,version.Minor,version.Build);
			about.Copyright = "Copyright ©2007-2009 Andoni Morales Alastruey";
			about.Website = "http://www.longomatch.ylatuya.es";
			about.License =
			        @"This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.";
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

		protected virtual void OnExportProjectToCSVFileActionActivated(object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Select Export File"),
			                (Gtk.Window)Toplevel,
			                FileChooserAction.Save,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.HomeDir());
			fChooser.DoOverwriteConfirmation = true;
			FileFilter filter = new FileFilter();
			filter.Name = "CSV File";
			filter.AddPattern("*.csv");
			fChooser.AddFilter(filter);
			if (fChooser.Run() == (int)ResponseType.Accept) {
				string outputFile=fChooser.Filename;
				outputFile = System.IO.Path.ChangeExtension(outputFile,"csv");
				CSVExport export = new CSVExport(openedProject, outputFile);
				export.WriteToFile();
			}
			fChooser.Destroy();

		}

		protected override bool OnConfigureEvent(Gdk.EventConfigure evnt)
		{
			return base.OnConfigureEvent(evnt);
		}

		protected virtual void OnHideAllWidgetsActionToggled(object sender, System.EventArgs e)
		{
			if (openedProject != null) {
				leftbox.Visible = !((Gtk.ToggleAction)sender).Active;
				timelinewidget1.Visible = !((Gtk.ToggleAction)sender).Active && AnalyzeModeAction.Active;
				buttonswidget1.Visible = !((Gtk.ToggleAction)sender).Active && CaptureModeAction.Active;
				if (((Gtk.ToggleAction)sender).Active)
					rightvbox.Visible = false;
				else if (!((Gtk.ToggleAction)sender).Active && (playlistwidget2.Visible || noteswidget1.Visible))
					rightvbox.Visible = true;
			}
		}
		protected virtual void OnHelpAction1Activated(object sender, System.EventArgs e)
		{
			try {
				System.Diagnostics.Process.Start("http://www.longomatch.ylatuya.es/documentation/manual.html");
			} catch {}
		}

		#endregion}
	}
}
