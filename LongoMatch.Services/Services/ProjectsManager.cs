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
		IMainController mainController;
		IAnalysisWindow analysisWindow;
		IProjectOptionsController projectOptionsController;
		TemplatesService ts;
		
		public ProjectsManager (IGUIToolkit guiToolkit, IMultimediaToolkit multimediaToolkit,
		                       TemplatesService ts) {
			this.multimediaToolkit = multimediaToolkit;
			this.guiToolkit = guiToolkit;
			this.ts =ts;
			mainController = guiToolkit.MainController;
			ConnectSignals();
		}

		public void ConnectSignals() {
			mainController.NewProjectEvent += NewProject;
			mainController.OpenProjectEvent += OpenProject;
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
				OpenedProjectChanged (OpenedProject, OpenedProjectType, PlaysFilter,
				                      analysisWindow, projectOptionsController);
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

		private void SaveCaptureProject(Project project) {
			string filePath = project.Description.File.FilePath;

			/* scan the new file to build a new PreviewMediaFile with all the metadata */
			try {
				Log.Debug ("Saving capture project: " + project);
			
				RemuxOutputFile (Capturer.CaptureProperties.EncodingSettings);
			
				Log.Debug("Reloading saved file: " + filePath);
				project.Description.File = multimediaToolkit.DiscoverFile(filePath);
				foreach (Play play in project.AllPlays ()) {
					play.Fps = project.Description.File.Fps;
				}
				Core.DB.AddProject(project);
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
			/* we need to set the opened project to null to avoid calling again CloseOpendProject() */
			OpenedProject = null;
			SetProject(project, ProjectType.FileProject, new CaptureSettings());
		}
	
		private bool SetProject(Project project, ProjectType projectType, CaptureSettings props)
		{
			if (OpenedProject != null) {
				CloseOpenedProject(true);
			}
				
			PlaysFilter = new PlaysFilter(project);
			guiToolkit.OpenProject (project, projectType, props, PlaysFilter,
			                        out analysisWindow, out projectOptionsController);
			Player = analysisWindow.Player;
			Capturer = analysisWindow.Capturer;
			projectOptionsController.CloseOpenedProjectEvent += () => {PromptCloseProject ();};
			projectOptionsController.SaveProjectEvent += SaveProject;
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
				if(projectType == ProjectType.CaptureProject ||
				   projectType == ProjectType.URICaptureProject) {
					Capturer.CaptureProperties = props;
					try {
						Capturer.Type = CapturerType.Live;
					} catch(Exception ex) {
						guiToolkit.ErrorMessage(ex.Message);
						CloseOpenedProject (false);
						return false;
					}
				} else
					Capturer.Type = CapturerType.Fake;
				Capturer.Run();
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

		private bool PromptCloseProject() {
			int res;
			//EndCaptureDialog dialog;

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
			}

			res = 0;
			/* Capture project */
			/*dialog = new EndCaptureDialog();
			dialog.TransientFor = (Gtk.Window)this.Toplevel;
			
			res = dialog.Run();
			dialog.Destroy();

			/* Close project wihtout saving */
			if(res == (int)EndCaptureResponse.Quit) {
				CloseOpenedProject (false);
				return true;
			} else if(res == (int)EndCaptureResponse.Save) {
				/* Close and save project */
				CloseOpenedProject (true);
				return true;
			} else
				/* Continue with the current project */
				return false;
		}

		private void CloseOpenedProject (bool save) {
			if(OpenedProject == null)
				return;
				
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
			guiToolkit.CloseProject ();
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
			} else if (projectType == ProjectType.CaptureProject ||
			           projectType == ProjectType.URICaptureProject) {
				SaveCaptureProject(project);
			}
		}
		
		protected virtual void NewProject() {
			Project project;
			ProjectType projectType;
			CaptureSettings captureSettings = new CaptureSettings();

			Log.Debug("Creating new project");
			
			if (!PromptCloseProject ()) {
				return;
			}
			
			/* Show the project selection dialog */
			projectType = guiToolkit.SelectNewProjectType();
			
			if(projectType == ProjectType.CaptureProject) {
				List<Device> devices = multimediaToolkit.VideoDevices;
				if(devices.Count == 0) {
					guiToolkit.ErrorMessage(Catalog.GetString("No capture devices were found."));
					return;
				}
				project = guiToolkit.NewCaptureProject(Core.DB, ts, devices, out captureSettings);
			} else if (projectType == ProjectType.FakeCaptureProject) {
				project = guiToolkit.NewFakeProject(Core.DB, ts);
			} else if (projectType == ProjectType.FileProject) {
				project = guiToolkit.NewFileProject(Core.DB, ts);
				if (project != null)
					Core.DB.AddProject(project);
			} else if (projectType == ProjectType.URICaptureProject) {
				project = guiToolkit.NewURICaptureProject(Core.DB, ts, out captureSettings);
			} else {
				project = null;
			}
			
			if (project != null)
				SetProject(project, projectType, captureSettings);
		}
		
		protected void OpenProject() {
			Project project = null;
			ProjectDescription projectDescription = null;
			
			if (!PromptCloseProject ()) {
				return;
			}
			
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
					"\n" + Catalog.GetString("Select a video file in the next step."));
				
				project = guiToolkit.EditFakeProject(Core.DB, project, ts);
				if (project == null)
					return;
				ToolsManager.CreateThumbnails(project, guiToolkit, multimediaToolkit.GetFramesCapturer());
			}
			SetProject(project, ProjectType.FileProject, new CaptureSettings());
		}
	}
}
