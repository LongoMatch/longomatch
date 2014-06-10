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
using LongoMatch.Interfaces;

namespace LongoMatch.Services
{


	public class ProjectsManager
	{
		IGUIToolkit guiToolkit;
		IMultimediaToolkit multimediaToolkit;
		IMainController mainController;
		IAnalysisWindow analysisWindow;
		IDatabase DB;
		
		public ProjectsManager (IGUIToolkit guiToolkit, IMultimediaToolkit multimediaToolkit,
		                       TemplatesService ts) {
			this.multimediaToolkit = multimediaToolkit;
			this.guiToolkit = guiToolkit;
			mainController = guiToolkit.MainController;
			DB = Config.DatabaseManager.ActiveDB;
			ConnectSignals();
		}

		public void ConnectSignals() {
			Config.EventsBroker.NewProjectEvent += NewProject;
			Config.EventsBroker.OpenProjectEvent += OpenProject;
			Config.EventsBroker.OpenProjectIDEvent += OpenProjectID;
			Config.EventsBroker.OpenNewProjectEvent += OpenNewProject;
			Config.EventsBroker.CloseOpenedProjectEvent += () => PromptCloseProject();
			Config.EventsBroker.SaveProjectEvent += SaveProject;
			Config.EventsBroker.KeyPressed += HandleKeyPressed;
		}

		public Project OpenedProject {
			set;
			get;
		}
		
		public ProjectType OpenedProjectType {
			set;
			get;
		}
		
		public PlaysFilter PlaysFilter {
			get;
			set;
		}
		
		public ICapturerBin Capturer {
			set;
			get;
		}
		
		public IPlayerBin Player {
			get;
			set;
		}
		
		void EmitProjectChanged() {
			Config.EventsBroker.EmitOpenedProjectChanged (OpenedProject, OpenedProjectType,
			                                              PlaysFilter, analysisWindow);
		}
		
		void RemuxOutputFile (EncodingSettings settings) {
			VideoMuxerType muxer;
				
			/* We need to remux to the original format */
			muxer = settings.EncodingProfile.Muxer;
			if (muxer == VideoMuxerType.Avi || muxer == VideoMuxerType.Mp4) {
				string outFile = settings.OutputFile;
				string tmpFile = settings.OutputFile;
				
				while (System.IO.File.Exists (tmpFile)) {
					tmpFile = tmpFile + ".tmp";
				}
				
				Log.Debug ("Remuxing file tmp: " + tmpFile + " out: " + outFile);
				
				try {
					System.IO.File.Move (outFile, tmpFile);
				} catch (Exception ex) {
					/* Try to fix "Sharing violation on path" in windows
					 * wait a bit more until the file lock is released */
					Log.Exception (ex);
					System.Threading.Thread.Sleep (5 * 1000);
					try {
						System.IO.File.Move (outFile, tmpFile);
					} catch (Exception ex2) {
						Log.Exception (ex2);
						/* It failed again, just skip remuxing */
						return;
					}
				}
				
				/* Remuxing suceed, delete old file */
				if (guiToolkit.RemuxFile (tmpFile, outFile, muxer) == outFile) {
					System.IO.File.Delete (tmpFile);
				} else {
					System.IO.File.Delete (outFile);
					System.IO.File.Move (tmpFile, outFile);
				}
			}
		}

		void SaveCaptureProject(Project project) {
			Guid projectID = project.UUID;
			string filePath = project.Description.File.FilePath;

			/* scan the new file to build a new PreviewMediaFile with all the metadata */
			try {
				Log.Debug ("Saving capture project: " + project.UUID);
			
				RemuxOutputFile (Capturer.CaptureSettings.EncodingSettings);
			
				Log.Debug("Reloading saved file: " + filePath);
				project.Description.File = multimediaToolkit.DiscoverFile(filePath);
				foreach (Play play in project.AllPlays ()) {
					play.Fps = project.Description.File.Fps;
				}
				DB.AddProject(project);
			} catch(Exception ex) {
				Log.Exception(ex);
				Log.Debug ("Backing up project to file");
				string projectFile = DateTime.Now.ToString().Replace("-", "_");
				projectFile = projectFile.Replace(":", "_");
				projectFile = projectFile.Replace(" ", "_");
				projectFile = projectFile.Replace("/", "_");
				projectFile = filePath + "_" + projectFile;
				Project.Export(OpenedProject, projectFile);
				guiToolkit.ErrorMessage(Catalog.GetString("An error occured saving the project:\n")+ex.Message+ "\n\n"+
					Catalog.GetString("The video file and a backup of the project has been "+
					"saved. Try to import it later:\n")+
					filePath+"\n"+projectFile);
			}
			CloseOpenedProject (false);
			OpenProjectID (projectID);
		}
	
		bool SetProject(Project project, ProjectType projectType, CaptureSettings props)
		{
			if (OpenedProject != null) {
				CloseOpenedProject(true);
			}
			
			Log.Debug ("Loading project " + project.UUID + " " + projectType);
				
			PlaysFilter = new PlaysFilter(project);
			guiToolkit.OpenProject (project, projectType, props, PlaysFilter,
			                        out analysisWindow);
			Player = analysisWindow.Player;
			Capturer = analysisWindow.Capturer;
			OpenedProject = project;
			OpenedProjectType = projectType;

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
					Log.Exception (ex);
					guiToolkit.ErrorMessage(Catalog.GetString("An error occurred opening this project:") + "\n" + ex.Message);
					CloseOpenedProject (false);
					return false;
				}

			} else {
				CapturerType type;
				if(projectType == ProjectType.CaptureProject ||
				   projectType == ProjectType.URICaptureProject) {
					type = CapturerType.Live;
				} else {
					type = CapturerType.Fake;
				}
				try {
					Capturer.Run(type, props);
				} catch(Exception ex) {
					guiToolkit.ErrorMessage(ex.Message);
					CloseOpenedProject (false);
					return false;
				}
			}

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

		bool PromptCloseProject() {
			if(OpenedProject == null)
				return true;

			if(OpenedProjectType == ProjectType.FileProject) {
				bool ret;
				ret = guiToolkit.QuestionMessage (
					Catalog.GetString("Do you want to close the current project?"), null);
				if (ret) {
					CloseOpenedProject (true);
					return true;
				}
				return false;
			} else {
				EndCaptureResponse res;
				
				res = guiToolkit.EndCapture (OpenedProject.Description.File.FilePath);

				/* Close project wihtout saving */
				if (res == EndCaptureResponse.Quit) {
					CloseOpenedProject (false);
					return true;
				} else if(res == EndCaptureResponse.Save) {
					/* Close and save project */
					CloseOpenedProject (true);
					return true;
				} else {
					/* Continue with the current project */
					return false;
				}
			}
		}

		void CloseOpenedProject (bool save) {
			if(OpenedProject == null)
				return;
				
			if (save)
				SaveProject(OpenedProject, OpenedProjectType);
			
			Log.Debug ("Closing project " + OpenedProject.UUID);
			if(OpenedProjectType != ProjectType.FileProject)
				Capturer.Close();
			else
				Player.Close();

			if(OpenedProject != null)
				OpenedProject.Clear();
			OpenedProject = null;
			OpenedProjectType = ProjectType.None;
			guiToolkit.CloseProject ();
			EmitProjectChanged();
		}
		
		protected virtual void SaveProject(Project project, ProjectType projectType) {
			Log.Debug(String.Format("Saving project {0} type: {1}", project, projectType));
			if (project == null)
				return;
			
			if(projectType == ProjectType.FileProject) {
				try {
					DB.UpdateProject(project);
				} catch(Exception e) {
					Log.Exception(e);
				}
			} else if (projectType == ProjectType.FakeCaptureProject) {
				try {
					DB.AddProject(project);
				} catch (Exception e) {
					Log.Exception(e);
				}
			} else if (projectType == ProjectType.CaptureProject ||
			           projectType == ProjectType.URICaptureProject) {
				SaveCaptureProject(project);
			}
		}
		
		protected virtual void NewProject() {
			Log.Debug("Creating new project");
			
			if (!PromptCloseProject ()) {
				return;
			}
			
			guiToolkit.CreateNewProject ();
		}
		
		void OpenNewProject (Project project, ProjectType projectType,
		                               CaptureSettings captureSettings)
		{
			if (project != null)
				SetProject(project, projectType, captureSettings);
		}
		
		void OpenProject() {
			if (!PromptCloseProject ()) {
				return;
			}
			guiToolkit.SelectProject(DB.GetAllProjects());
		}
		
		void OpenProjectID (Guid projectID) {
			Project project = null;
			
			project = DB.GetProject(projectID);

			if (project.Description.File.FilePath == Constants.FAKE_PROJECT) {
				/* If it's a fake live project prompt for a video file and
				 * create a new PreviewMediaFile for this project and recreate the thumbnails */
				Log.Debug ("Importing fake live project");
				project.Description.File = null;
				
				guiToolkit.InfoMessage(
					Catalog.GetString("You are opening a live project without any video file associated yet.") +
					"\n" + Catalog.GetString("Select a video file in the next step."));
				
				guiToolkit.CreateNewProject (project);
				if (project == null)
					return;
				ToolsManager.CreateThumbnails(project, guiToolkit, multimediaToolkit.GetFramesCapturer());
			}
			SetProject(project, ProjectType.FileProject, new CaptureSettings());
		}

		void HandleMultimediaError (string message)
		{
			guiToolkit.ErrorMessage (Catalog.GetString("The following error happened and" +
				" the current project will be closed:")+"\n" + message);
			CloseOpenedProject (true);
		}
		
		void HandleKeyPressed (object sender, int key, int modifier)
		{
			if(OpenedProject == null)
				return;

			if(OpenedProjectType != ProjectType.CaptureProject &&
			   OpenedProjectType != ProjectType.URICaptureProject &&
			   OpenedProjectType != ProjectType.FakeCaptureProject) {
				if (Player == null)
					return;

				switch(key) {
				case Constants.SEEK_FORWARD:
					if(modifier == Constants.STEP)
						Player.StepForward();
					else
						Player.SeekToNextFrame();
					break;
				case Constants.SEEK_BACKWARD:
					if(modifier == Constants.STEP)
						Player.StepBackward();
					else
						Player.SeekToPreviousFrame();
					break;
				case Constants.FRAMERATE_UP:
					Player.FramerateUp();
					break;
				case Constants.FRAMERATE_DOWN:
					Player.FramerateDown();
					break;
				case Constants.TOGGLE_PLAY:
					Player.TogglePlay();
					break;
				}
			} else {
				if (Capturer == null)
					return;
				switch(key) {
				case Constants.TOGGLE_PLAY:
					Capturer.TogglePause();
					break;
				}
			}
		}
	}
}
