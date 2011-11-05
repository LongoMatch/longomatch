//
//  Copyright (C) 2010 Andoni Morales Alastruey
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
using System.IO;
using Gtk;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Gui;
using LongoMatch.Gui.Dialog;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Video;
using LongoMatch.Video.Utils;
using LongoMatch.Video.Common;
using LongoMatch.Multimedia.Utils;
using LongoMatch.Multimedia.Interfaces;

namespace LongoMatch.Services
{


	public class ProjectsManager
	{
		public event OpenedProjectChangedHandler OpenedProjectChanged;

		MainWindow mainWindow;
		
		public ProjectsManager(MainWindow mainWindow) {
			this.mainWindow = mainWindow;
			Player = mainWindow.Player;
			Capturer = mainWindow.Capturer;
			ConnectSignals();
		}

		public void ConnectSignals() {
			mainWindow.NewProjectEvent += NewProject;
			mainWindow.OpenProjectEvent += OpenProject;
			mainWindow.CloseOpenedProjectEvent += CloseOpenedProject;
			mainWindow.SaveProjectEvent += SaveProject;
			mainWindow.ImportProjectEvent += ImportProject;
			mainWindow.ExportProjectEvent += ExportProject;
			mainWindow.ManageProjectsEvent += OpenProjectsManager;
			mainWindow.ManageCategoriesEvent += OpenCategoriesTemplatesManager;
			mainWindow.ManageTeamsEvent += OpenTeamsTemplatesManager;
		}
		
		public Project OpenedProject {
			set;
			get;
		}
		
		public ProjectType OpenedProjectType {
			set;
			get;
		}
		
		public CapturerBin Capturer {
			set;
			get;
		}
		
		public PlayerBin Player {
			get;
			set;
		}
		
		private void EmitProjectChanged() {
			if (OpenedProjectChanged != null)
				OpenedProjectChanged(OpenedProject, OpenedProjectType);
		}
		
		private void SaveCaptureProject(Project project) {
			MessageDialog md;
			string filePath = project.Description.File.FilePath;

			Log.Debug ("Saving capture project: " + project);
			md = new MessageDialog(mainWindow as Gtk.Window, DialogFlags.Modal,
			                       MessageType.Info, ButtonsType.None,
			                       Catalog.GetString("Loading newly created project..."));
			md.Show();

			/* scan the new file to build a new PreviewMediaFile with all the metadata */
			try {
				Log.Debug("Reloading saved file: " + filePath);
				project.Description.File = PreviewMediaFile.DiscoverFile(filePath);
				Core.DB.AddProject(project);
			} catch(Exception ex) {
				Log.Exception(ex);
				Log.Debug ("Backing up project to file");
				string projectFile = filePath + "-" + DateTime.Now;
				projectFile = projectFile.Replace("-", "_").Replace(" ", "_").Replace(":", "_");
				Project.Export(OpenedProject, projectFile);
				MessagePopup.PopupMessage(mainWindow, MessageType.Error,
				                          Catalog.GetString("An error occured saving the project:\n")+ex.Message+ "\n\n"+
				                          Catalog.GetString("The video file and a backup of the project has been "+
				                                            "saved. Try to import it later:\n")+
				                          filePath+"\n"+projectFile);
			}
			/* we need to set the opened project to null to avoid calling again CloseOpendProject() */
			/* FIXME: */
			//project = null;
			SetProject(project, ProjectType.FileProject, new CaptureSettings());
			md.Destroy();
		}
	
		private void SaveFakeLiveProject(Project project) {
			int response;
			MessageDialog md;
			FileFilter filter;
			FileChooserDialog fChooser;
			
			Log.Debug("Saving fake live project " + project);
			md = new MessageDialog(mainWindow, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
			                       Catalog.GetString("The project will be saved to a file. " +
			                                         "You can insert it later into the database using the "+
			                                         "\"Import project\" function once you copied the video file " +
			                                         "to your computer"));
			response = md.Run();
			md.Destroy();
			if(response == (int)ResponseType.Cancel)
				return;

			fChooser = new FileChooserDialog(Catalog.GetString("Save Project"),
			                                 mainWindow, FileChooserAction.Save,
			                                 "gtk-cancel",ResponseType.Cancel,
			                                 "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(Config.HomeDir());
			filter = new FileFilter();
			filter.Name = Constants.PROJECT_NAME;
			filter.AddPattern("*.lpr");
			fChooser.AddFilter(filter);
			if(fChooser.Run() == (int)ResponseType.Accept) {
				Project.Export(project, fChooser.Filename);
				MessagePopup.PopupMessage(mainWindow, MessageType.Info,
				                          Catalog.GetString("Project saved successfully."));
			}
			fChooser.Destroy();
		}

		private void ImportProject() {
			Project project;
			bool isFake, exists;
			int res;
			string fileName;
			FileFilter filter;
			NewProjectDialog npd;
			FileChooserDialog fChooser;

			Log.Debug("Importing project");
			/* Show a file chooser dialog to select the file to import */
			fChooser = new FileChooserDialog(Catalog.GetString("Import Project"),
			                                 mainWindow,
			                                 FileChooserAction.Open,
			                                 "gtk-cancel",ResponseType.Cancel,
			                                 "gtk-open",ResponseType.Accept);
			fChooser.SetCurrentFolder(Config.HomeDir());
			filter = new FileFilter();
			filter.Name = Constants.PROJECT_NAME;
			filter.AddPattern("*.lpr");
			fChooser.AddFilter(filter);


			res = fChooser.Run();
			fileName = fChooser.Filename;
			fChooser.Destroy();
			/* return if the user cancelled */
			if(res != (int)ResponseType.Accept)
				return;

			/* try to import the project and show a message error is the file
			 * is not a valid project */
			try {
				project = Project.Import(fileName);
			}
			catch(Exception ex) {
				MessagePopup.PopupMessage(mainWindow, MessageType.Error,
				                          Catalog.GetString("Error importing project:")+
				                          "\n"+ex.Message);
				Log.Exception(ex);
				return;
			}

			isFake = (project.Description.File.FilePath == Constants.FAKE_PROJECT);

			/* If it's a fake live project prompt for a video file and
			 * create a new PreviewMediaFile for this project */
			if(isFake) {
				Log.Debug ("Importing fake live project");
				project.Description.File = null;
				npd = new NewProjectDialog();
				npd.TransientFor = mainWindow;
				npd.Use = ProjectType.EditProject;
				npd.Project = project;
				int response = npd.Run();
				while(true) {
					if(response != (int)ResponseType.Ok) {
						npd.Destroy();
						return;
					} else if(npd.Project == null) {
						MessagePopup.PopupMessage(mainWindow, MessageType.Info,
						                          Catalog.GetString("Please, select a video file."));
						response=npd.Run();
					} else {
						project = npd.Project;
						npd.Destroy();
						break;
					}
				}
			}

			/* If the project exists ask if we want to overwrite it */
			if(Core.DB.Exists(project)) {
				MessageDialog md = new MessageDialog(mainWindow,
				                                     DialogFlags.Modal,
				                                     MessageType.Question,
				                                     Gtk.ButtonsType.YesNo,
				                                     Catalog.GetString("A project already exists for the file:")+
				                                     project.Description.File.FilePath+ "\n" +
				                                     Catalog.GetString("Do you want to overwrite it?"));
				md.Icon = Gtk.IconTheme.Default.LoadIcon("longomatch", 48, 0);
				res = md.Run();
				md.Destroy();
				if(res != (int)ResponseType.Yes)
					return;
				exists = true;
			} else
				exists = false;

			if(isFake)
				CreateThumbnails(project);
			if(exists)
				Core.DB.UpdateProject(project);
			else
				Core.DB.AddProject(project);


			MessagePopup.PopupMessage(mainWindow, MessageType.Info,
			                          Catalog.GetString("Project successfully imported."));
		}

		private void CreateNewProject(out Project project, out ProjectType projectType,
		                             out CaptureSettings captureSettings) {
			ProjectSelectionDialog psd;
			NewProjectDialog npd;
			List<Device> devices = null;
			int response;

			Log.Debug("Creating new project");
			/* The out parameters must be set before leaving the method */
			project = null;
			projectType = ProjectType.None;
			captureSettings = new CaptureSettings();

			/* Show the project selection dialog */
			psd = new ProjectSelectionDialog();
			psd.TransientFor = mainWindow;
			response = psd.Run();
			psd.Destroy();
			if(response != (int)ResponseType.Ok)
				return;
			projectType = psd.ProjectType;
			
			if(projectType == ProjectType.CaptureProject) {
				devices = VideoDevice.ListVideoDevices();
				if(devices.Count == 0) {
					MessagePopup.PopupMessage(mainWindow, MessageType.Error,
					                          Catalog.GetString("No capture devices were found."));
					return;
				}
			}

			/* Show the new project dialog and wait to get a valid project
			 * or quit if the user cancel it.*/
			npd = new NewProjectDialog();
			npd.TransientFor = mainWindow;
			npd.Use = projectType;
			npd.TemplatesService = Core.TemplatesService;
			if(projectType == ProjectType.CaptureProject)
				npd.Devices = devices;
			response = npd.Run();
			
			while(true) {
				/* User cancelled: quit */
				if(response != (int)ResponseType.Ok) {
					npd.Destroy();
					return;
				}
				/* No file chosen: display the dialog again */
				if(npd.Project == null)
					MessagePopup.PopupMessage(mainWindow, MessageType.Info,
					                          Catalog.GetString("Please, select a video file."));
				/* If a project with the same file path exists show a warning */
				else if(Core.DB.Exists(npd.Project))
					MessagePopup.PopupMessage(mainWindow, MessageType.Error,
					                          Catalog.GetString("This file is already used in another Project.")+"\n"+
					                          Catalog.GetString("Select a different one to continue."));

				else {
					/* We are now ready to create the new project */
					project = npd.Project;
					if(projectType == ProjectType.CaptureProject)
						captureSettings = npd.CaptureSettings;
					npd.Destroy();
					break;
				}
				response = npd.Run();
			}
			if(projectType == ProjectType.FileProject)
				/* We can safelly add the project since we already checked if
				 * it can can added */
				Core.DB.AddProject(project);
		}
		
		private bool SetProject(Project project, ProjectType projectType, CaptureSettings props)
		{
			if(OpenedProject != null)
				CloseOpenedProject(true);

			if(projectType == ProjectType.FileProject) {
				// Check if the file associated to the project exists
				if(!File.Exists(project.Description.File.FilePath)) {
					MessagePopup.PopupMessage(mainWindow, MessageType.Warning,
					                          Catalog.GetString("The file associated to this project doesn't exist.") + "\n"
					                          + Catalog.GetString("If the location of the file has changed try to edit it with the database manager."));
					CloseOpenedProject(true);
					return false;
				}
				try {
					Player.Open(project.Description.File.FilePath);
				}
				catch(GLib.GException ex) {
					MessagePopup.PopupMessage(mainWindow, MessageType.Error,
					                          Catalog.GetString("An error occurred opening this project:") + "\n" + ex.Message);
					CloseOpenedProject(true);
					return false;
				}

			} else {
				if(projectType == ProjectType.CaptureProject) {
					Capturer.CaptureProperties = props;
					try {
						Capturer.Type = CapturerType.Live;
					} catch(Exception ex) {
						MessagePopup.PopupMessage(mainWindow, MessageType.Error, ex.Message);
						CloseOpenedProject(false);
						return false;
					}
				} else
					Capturer.Type = CapturerType.Fake;
				Capturer.Run();
			}

			OpenedProject = project;
			OpenedProjectType = projectType;
			mainWindow.SetProject(project, projectType, props);
			EmitProjectChanged();
			return true;
		}
	
		/*
		public static void ExportToCSV(Project project) {
			FileChooserDialog fChooser;
			FileFilter filter;
			string outputFile;
			CSVExport export;

			fChooser = new FileChooserDialog(Catalog.GetString("Select Export File"),
			                                 window,
			                                 FileChooserAction.Save,
			                                 "gtk-cancel",ResponseType.Cancel,
			                                 "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.HomeDir());
			fChooser.DoOverwriteConfirmation = true;
			filter = new FileFilter();
			filter.Name = "CSV File";
			filter.AddPattern("*.csv");
			fChooser.AddFilter(filter);
			if(fChooser.Run() == (int)ResponseType.Accept) {
				outputFile=fChooser.Filename;
				outputFile = System.IO.Path.ChangeExtension(outputFile,"csv");
				export = new CSVExport(project, outputFile);
				export.WriteToFile();
			}
			fChooser.Destroy();
		}*/

		private void CreateThumbnails(Project project) {
			MultimediaFactory factory;
			IFramesCapturer capturer;
			BusyDialog dialog;

			dialog = new BusyDialog();
			dialog.TransientFor = mainWindow;
			dialog.Message = Catalog.GetString("Creating video thumbnails. This can take a while.");
			dialog.Show();
			dialog.Pulse();

			/* Create all the thumbnails */
			factory = new MultimediaFactory();
			capturer = factory.getFramesCapturer();
			capturer.Open(project.Description.File.FilePath);
			foreach(Play play in project.AllPlays()) {
				try {
					capturer.SeekTime(play.Start.MSeconds + ((play.Stop - play.Start).MSeconds/2),
					                  true);
					play.Miniature = capturer.GetCurrentFrame(Constants.THUMBNAIL_MAX_WIDTH,
					                 Constants.THUMBNAIL_MAX_HEIGHT);
					dialog.Pulse();

				} catch (Exception ex) {
					Log.Exception(ex);
				}
			}
			capturer.Dispose();
			dialog.Destroy();
		}
		
		private void CloseOpenedProject(bool save) {
			if (save)
				SaveProject(OpenedProject, OpenedProjectType);
			
			if(OpenedProjectType != ProjectType.FileProject)
				Capturer.Close();
			else
				Player.Close();

			if(OpenedProject != null)
				OpenedProject.Clear();
			OpenedProject = null;
			OpenedProjectType = ProjectType.None;
			EmitProjectChanged();
		}
		
		protected virtual void SaveProject(Project project, ProjectType projectType) {
			Log.Debug(String.Format("Saving project {0} type: {1}", project, projectType));
			if (project == null)
				return;
			
			if(projectType == ProjectType.FileProject) {
				try {
					Core.DB.UpdateProject(project);
				} catch(Exception e) {
					Log.Exception(e);
				}
			} else if (projectType == ProjectType.FakeCaptureProject) {
				SaveFakeLiveProject(project);
			} else if (projectType == ProjectType.CaptureProject) {
				SaveCaptureProject(project);
			}
		}
		
		protected virtual void NewProject() {
			Project project;
			ProjectType projectType;
			CaptureSettings captureSettings;
			
			CreateNewProject(out project, out projectType, out captureSettings);
			if(project != null)
				SetProject(project, projectType, captureSettings);
		}
		
		protected void OpenProject() {
			ProjectDescription project=null;
			OpenProjectDialog opd = new OpenProjectDialog();
			
			opd.Fill(Core.DB.GetAllProjects());	
			opd.TransientFor = mainWindow;
			if(opd.Run() == (int)ResponseType.Ok)
				project = opd.SelectedProject;
			opd.Destroy();
			if(project != null)
				SetProject(Core.DB.GetProject(project.UUID), ProjectType.FileProject, new CaptureSettings());
		}
		
		protected void ExportProject() {
			/* FIXME:
			 * ExportToCSV(this, openedProject);
			 * */
		}
		
		protected void OpenCategoriesTemplatesManager()
		{
			var tManager = new TemplatesManager<Categories, Category>(Core.TemplatesService.CategoriesTemplateProvider,
			                                                          Core.TemplatesService.GetTemplateEditor<Categories, Category>());
			tManager.TransientFor = mainWindow;
			tManager.Show();
		}

		protected void OpenTeamsTemplatesManager()
		{
			var tManager = new TemplatesManager<TeamTemplate, Player>(Core.TemplatesService.TeamTemplateProvider,
			                                                          Core.TemplatesService.GetTemplateEditor<TeamTemplate, Player>());
			tManager.TransientFor = mainWindow;
			tManager.Show();
		}
		
		protected void OpenProjectsManager()
		{
			Gui.Dialog.ProjectsManager pm = new Gui.Dialog.ProjectsManager(OpenedProject, Core.DB, Core.TemplatesService);
			pm.TransientFor = mainWindow;
			pm.Show();
		}

	}
}
