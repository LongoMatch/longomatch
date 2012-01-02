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
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Interfaces.Multimedia;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Services
{


	public class ProjectsManager
	{
		public event OpenedProjectChangedHandler OpenedProjectChanged;

		IGUIToolkit guiToolkit;
		IMultimediaToolkit multimediaToolkit;
		IMainWindow mainWindow;
		
		public ProjectsManager(IGUIToolkit guiToolkit, IMultimediaToolkit multimediaToolkit) {
			this.multimediaToolkit = multimediaToolkit;
			this.guiToolkit = guiToolkit;
			mainWindow = guiToolkit.MainWindow;
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
		
		public ICapturer Capturer {
			set;
			get;
		}
		
		public IPlayer Player {
			get;
			set;
		}
		
		private void EmitProjectChanged() {
			if (OpenedProjectChanged != null)
				OpenedProjectChanged(OpenedProject, OpenedProjectType);
		}
		
		private void SaveCaptureProject(Project project) {
			string filePath = project.Description.File.FilePath;

			Log.Debug ("Saving capture project: " + project);
			
			/* FIXME: Show message */
			//guiToolkit.InfoMessage(Catalog.GetString("Loading newly created project..."));

			/* scan the new file to build a new PreviewMediaFile with all the metadata */
			try {
				Log.Debug("Reloading saved file: " + filePath);
				project.Description.File = multimediaToolkit.DiscoverFile(filePath);
				Core.DB.AddProject(project);
			} catch(Exception ex) {
				Log.Exception(ex);
				Log.Debug ("Backing up project to file");
				string projectFile = filePath + "-" + DateTime.Now;
				projectFile = projectFile.Replace("-", "_").Replace(" ", "_").Replace(":", "_");
				Project.Export(OpenedProject, projectFile);
				guiToolkit.ErrorMessage(Catalog.GetString("An error occured saving the project:\n")+ex.Message+ "\n\n"+
					Catalog.GetString("The video file and a backup of the project has been "+
					"saved. Try to import it later:\n")+
					filePath+"\n"+projectFile);
			}
			/* we need to set the opened project to null to avoid calling again CloseOpendProject() */
			/* FIXME: */
			//project = null;
			SetProject(project, ProjectType.FileProject, new CaptureSettings());
		}
	
		private void ImportProject() {
			Project project;
			string fileName;

			Log.Debug("Importing project");
			/* Show a file chooser dialog to select the file to import */
			fileName = guiToolkit.OpenFile(Catalog.GetString("Import Project"), null,
				Config.HomeDir(), Constants.PROJECT_NAME, "*lpr");
				
			if(fileName == null)
				return;

			/* try to import the project and show a message error is the file
			 * is not a valid project */
			try {
				project = Project.Import(fileName);
			}
			catch(Exception ex) {
				guiToolkit.ErrorMessage(Catalog.GetString("Error importing project:") +
					"\n"+ex.Message);
				Log.Exception(ex);
				return;
			}

			/* If the project exists ask if we want to overwrite it */
			if(Core.DB.Exists(project)) {
				var res = guiToolkit.QuestionMessage(Catalog.GetString("A project already exists for the file:") +
					project.Description.File.FilePath+ "\n" +
					Catalog.GetString("Do you want to overwrite it?"), null);
				if(!res)
					return;
				Core.DB.UpdateProject(project);
			} else {
				Core.DB.AddProject(project);
			}

			guiToolkit.InfoMessage(Catalog.GetString("Project successfully imported."));
		}
	
		private bool SetProject(Project project, ProjectType projectType, CaptureSettings props)
		{
			if(OpenedProject != null)
				CloseOpenedProject(true);

			if(projectType == ProjectType.FileProject) {
				// Check if the file associated to the project exists
				if(!File.Exists(project.Description.File.FilePath)) {
					guiToolkit.WarningMessage(Catalog.GetString("The file associated to this project doesn't exist.") + "\n"
						+ Catalog.GetString("If the location of the file has changed try to edit it with the database manager."));
					CloseOpenedProject(true);
					return false;
				}
				try {
					Player.Open(project.Description.File.FilePath);
				}
				catch(Exception ex) {
					guiToolkit.ErrorMessage(Catalog.GetString("An error occurred opening this project:") + "\n" + ex.Message);
					CloseOpenedProject(true);
					return false;
				}

			} else {
				if(projectType == ProjectType.CaptureProject) {
					Capturer.CaptureProperties = props;
					try {
						Capturer.Type = CapturerType.Live;
					} catch(Exception ex) {
						guiToolkit.ErrorMessage(ex.Message);
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
			IFramesCapturer capturer;
			IBusyDialog dialog;

			dialog = guiToolkit.BusyDialog(Catalog.GetString("Creating video thumbnails. This can take a while."));
			dialog.Show();
			dialog.Pulse();

			/* Create all the thumbnails */
			capturer = multimediaToolkit.GetFramesCapturer();
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
				try {
					Core.DB.AddProject(project);
				} catch (Exception e) {
					Log.Exception(e);
				}
			} else if (projectType == ProjectType.CaptureProject) {
				SaveCaptureProject(project);
			}
		}
		
		protected virtual void NewProject() {
			Project project;
			ProjectType projectType;
			CaptureSettings captureSettings = new CaptureSettings();

			Log.Debug("Creating new project");
			
			/* Show the project selection dialog */
			projectType = guiToolkit.SelectNewProjectType();
			
			if(projectType == ProjectType.CaptureProject) {
				List<Device> devices = multimediaToolkit.VideoDevices;
				if(devices.Count == 0) {
					guiToolkit.ErrorMessage(Catalog.GetString("No capture devices were found."));
					return;
				}
				project = guiToolkit.NewCaptureProject(Core.DB, Core.TemplatesService, devices,
					out captureSettings);
			} else if (projectType == ProjectType.FakeCaptureProject) {
				project = guiToolkit.NewFakeProject(Core.DB, Core.TemplatesService);
			} else if (projectType == ProjectType.FileProject) {
				project = guiToolkit.NewFileProject(Core.DB, Core.TemplatesService);
				Core.DB.AddProject(project);
			} else {
				project = null;
			}
			
			if (project != null)
				SetProject(project, projectType, captureSettings);
		}
		
		protected void OpenProject() {
			Project project = null;
			ProjectDescription projectDescription = null;
			
			projectDescription = guiToolkit.SelectProject(Core.DB.GetAllProjects());
			if (projectDescription == null)
				return;

			project = Core.DB.GetProject(projectDescription.UUID);

			if (project.Description.File.FilePath == Constants.FAKE_PROJECT) {
				/* If it's a fake live project prompt for a video file and
				 * create a new PreviewMediaFile for this project and recreate the thumbnails */
				Log.Debug ("Importing fake live project");
				project.Description.File = null;
				
				guiToolkit.InfoMessage(
					Catalog.GetString("You are opening a live project without any video file associated yet.") + 
					"\n" + Catalog.GetString("Select a video file in the step."));
				
				project = guiToolkit.EditFakeProject(Core.DB, project, Core.TemplatesService);
				if (project == null)
					return;
				CreateThumbnails(project);
			}
			SetProject(project, ProjectType.FileProject, new CaptureSettings());
		}
		
		protected void ExportProject() {
			if (OpenedProject == null) {
				Log.Warning("Opened project is null and can't be exported");
			}
			
			string filename = guiToolkit.SaveFile(Catalog.GetString("Save project"), null,
				Config.HomeDir(), Constants.PROJECT_NAME, Constants.PROJECT_EXT);
			
			if (filename == null)
				return;
			
			System.IO.Path.ChangeExtension(filename, Constants.PROJECT_EXT);
			
			try {
				Project.Export(OpenedProject, filename);
				guiToolkit.InfoMessage(Catalog.GetString("Project exported successfully"));
			}catch (Exception ex) {
				guiToolkit.ErrorMessage(Catalog.GetString("Error exporting project"));
				Log.Exception(ex);
			}
		}
		
		protected void OpenCategoriesTemplatesManager()
		{
			guiToolkit.OpenCategoriesTemplatesManager (Core.TemplatesService);
		}

		protected void OpenTeamsTemplatesManager()
		{
			guiToolkit.OpenTeamsTemplatesManager (Core.TemplatesService.TeamTemplateProvider);
		}
		
		protected void OpenProjectsManager()
		{
			guiToolkit.OpenProjectsManager(OpenedProject, Core.DB, Core.TemplatesService);
		}

	}
}
