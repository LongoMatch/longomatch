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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;

namespace LongoMatch.Services
{
	public class ProjectsManager: IService
	{
		IGUIToolkit guiToolkit;
		IMultimediaToolkit multimediaToolkit;
		IAnalysisWindow analysisWindow;

		public ProjectsManager ()
		{
		}

		public Project OpenedProject {
			set;
			get;
		}

		public ProjectType OpenedProjectType {
			set;
			get;
		}

		public EventsFilter PlaysFilter {
			get;
			set;
		}

		public ICapturerBin Capturer {
			set;
			get;
		}

		public IPlayerController Player {
			get;
			set;
		}

		void EmitProjectChanged ()
		{
			Config.EventsBroker.EmitOpenedProjectChanged (OpenedProject, OpenedProjectType,
				PlaysFilter, analysisWindow);
		}

		void RemuxOutputFile (EncodingSettings settings)
		{
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

		void SaveCaptureProject (Project project)
		{
			Guid projectID = project.ID;
			// FIXME
			string filePath = project.Description.FileSet.First ().FilePath;

			/* scan the new file to build a new PreviewMediaFile with all the metadata */
			try {
				Log.Debug ("Saving capture project: " + project.ID);
			
				RemuxOutputFile (Capturer.CaptureSettings.EncodingSettings);
			
				Log.Debug ("Reloading saved file: " + filePath);
				project.Description.FileSet [0] = multimediaToolkit.DiscoverFile (filePath);
				project.Periods = new ObservableCollection<Period> (Capturer.Periods);
				Config.DatabaseManager.ActiveDB.Store<Project> (project);
			} catch (Exception ex) {
				Log.Exception (ex);
				Log.Debug ("Backing up project to file");

				string filePathNoExtension = Path.GetDirectoryName (filePath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension (filePath);
				string projectFile = DateTime.Now.ToString ().Replace ("-", "_");
				projectFile = projectFile.Replace (":", "_");
				projectFile = projectFile.Replace (" ", "_");
				projectFile = projectFile.Replace ("/", "_");
				projectFile = filePathNoExtension + "_" + projectFile;
				Project.Export (OpenedProject, projectFile);
				guiToolkit.ErrorMessage (Catalog.GetString ("An error occured saving the project:\n") + ex.Message + "\n\n" +
				Catalog.GetString ("The video file and a backup of the project has been " +
				"saved. Try to import it later:\n") +
				filePath + "\n" + projectFile + Constants.PROJECT_EXT);

			}
		}

		bool SetProject (Project project, ProjectType projectType, CaptureSettings props)
		{
			if (OpenedProject != null) {
				CloseOpenedProject (true);
			}
			
			Log.Debug ("Loading project " + project.ID + " " + projectType);
				
			PlaysFilter = new EventsFilter (project);
			project.CleanupTimers ();
			project.ProjectType = projectType;
			guiToolkit.OpenProject (project, projectType, props, PlaysFilter,
				out analysisWindow);
			Player = analysisWindow.Player;
			Capturer = analysisWindow.Capturer;
			OpenedProject = project;
			OpenedProjectType = projectType;
		
			if (projectType == ProjectType.FileProject) {
				// Check if the file associated to the project exists
				if (!project.Description.FileSet.CheckFiles ()) {
					if (!guiToolkit.SelectMediaFiles (project)) {
						CloseOpenedProject (true);
						return false;
					}
				}
				try {
					Player.Open (project.Description.FileSet);
				} catch (Exception ex) {
					Log.Exception (ex);
					guiToolkit.ErrorMessage (Catalog.GetString ("An error occurred opening this project:") + "\n" + ex.Message);
					CloseOpenedProject (false);
					return false;
				}

			} else if (projectType == ProjectType.CaptureProject ||
			           projectType == ProjectType.URICaptureProject ||
			           projectType == ProjectType.FakeCaptureProject) {
				try {
					Capturer.Run (props, project.Description.FileSet.First ());
				} catch (Exception ex) {
					Log.Exception (ex);
					guiToolkit.ErrorMessage (ex.Message);
					CloseOpenedProject (false);
					return false;
				}
			}

			EmitProjectChanged ();
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
		bool PromptCloseProject ()
		{
			if (OpenedProject == null)
				return true;

			if (OpenedProjectType == ProjectType.FileProject) {
				bool ret;
				ret = guiToolkit.QuestionMessage (
					Catalog.GetString ("Do you want to close the current project?"), null).Result;
				if (ret) {
					CloseOpenedProject (true);
					return true;
				}
				return false;
			} else {
				EndCaptureResponse res;

				// Check if we need to show or not the stop and save button
				bool isCapturing;
				if (Capturer.Periods == null || Capturer.Periods.Count == 0)
					isCapturing = false;
				else
					isCapturing = true;

				res = guiToolkit.EndCapture (isCapturing);

				/* Close project wihtout saving */
				if (res == EndCaptureResponse.Quit) {
					CaptureFinished (true, true);
					return true;
				} else if (res == EndCaptureResponse.Save) {
					CaptureFinished (false, false);
					return true;
				} else {
					/* Continue with the current project */
					return false;
				}
			}
		}

		void CloseOpenedProject (bool save)
		{
			if (OpenedProject == null)
				return;
				
			Log.Debug ("Closing project " + OpenedProject.ID);
			if (Capturer != null) {
				Capturer.Close ();
			}
			if (Player != null) {
				Player.Dispose ();
			}

			if (save)
				SaveProject (OpenedProject, OpenedProjectType);

			if (OpenedProject != null)
				OpenedProject.Clear ();
			OpenedProject = null;
			OpenedProjectType = ProjectType.None;
			guiToolkit.CloseProject ();
			EmitProjectChanged ();
		}

		protected virtual void SaveProject (Project project, ProjectType projectType)
		{
			if (project == null)
				return;
			
			Log.Debug (String.Format ("Saving project {0} type: {1}", project.ID, projectType));
			if (projectType == ProjectType.FileProject) {
				try {
					Config.DatabaseManager.ActiveDB.Store<Project> (project);
				} catch (Exception ex) {
					Log.Exception (ex);
					guiToolkit.ErrorMessage (Catalog.GetString ("An error occured saving the project:\n") + ex.Message);
				}
			} else if (projectType == ProjectType.FakeCaptureProject) {
				project.Periods = new ObservableCollection<Period> (Capturer.Periods);
				try {
					Config.DatabaseManager.ActiveDB.Store<Project> (project);
				} catch (Exception ex) {
					Log.Exception (ex);
					guiToolkit.ErrorMessage (Catalog.GetString ("An error occured saving the project:\n") + ex.Message);
				}
			} else if (projectType == ProjectType.CaptureProject ||
			           projectType == ProjectType.URICaptureProject) {
				SaveCaptureProject (project);
			}
		}

		protected virtual void NewProject (Project project)
		{
			Log.Debug ("Creating new project");
			
			if (!PromptCloseProject ()) {
				return;
			}
			
			guiToolkit.CreateNewProject (project);
		}

		void OpenNewProject (Project project, ProjectType projectType,
		                     CaptureSettings captureSettings)
		{
			if (project != null) {
				try {
					Config.DatabaseManager.ActiveDB.Store<Project> (project);
					SetProject (project, projectType, captureSettings);
				} catch (Exception ex) {
					Log.Exception (ex);
					guiToolkit.ErrorMessage (Catalog.GetString ("An error occured saving the project:\n") + ex.Message);
				}
			}
		}

		void OpenProject ()
		{
			if (!PromptCloseProject ()) {
				return;
			}
			guiToolkit.SelectProject (Config.DatabaseManager.ActiveDB.RetrieveAll<Project> ().ToList ());
		}

		void OpenProjectID (Guid projectID, Project project)
		{
			if (project == null) {
				try {
					project = Config.DatabaseManager.ActiveDB.Retrieve<Project> (projectID);
				} catch (Exception ex) {
					Log.Exception (ex);
					guiToolkit.ErrorMessage (ex.Message);
					return;
				}
			}

			if (!project.IsLoaded) {
				try {
					IBusyDialog busy = Config.GUIToolkit.BusyDialog (Catalog.GetString ("Loading project..."), null);
					busy.ShowSync (project.Load);
				} catch (Exception ex) {
					Log.Exception (ex);
					guiToolkit.ErrorMessage (Catalog.GetString ("Could not load project:") + "\n" + ex.Message);
					return;
				}
			}

			// FIXME
			if (project.IsFakeCapture) {
				/* If it's a fake live project prompt for a video file and
				 * create a new PreviewMediaFile for this project and recreate the thumbnails */
				Log.Debug ("Importing fake live project");
				Config.EventsBroker.EmitNewProject (project);
				return;
			}

			if (project.Description.FileSet.Duration == null) {
				Log.Warning ("The selected project is empty. Rediscovering files");
				for (int i = 0; i < project.Description.FileSet.Count; i++) {
					project.Description.FileSet [i] = Config.MultimediaToolkit.DiscoverFile (project.Description.FileSet [i].FilePath);
				}
			}

			project.UpdateEventTypesAndTimers ();
			SetProject (project, ProjectType.FileProject, new CaptureSettings ());
		}

		void CaptureFinished (bool cancel, bool delete)
		{
			Project project = OpenedProject;
			ProjectType type = OpenedProjectType;
			if (delete) {
				try {
					Config.DatabaseManager.ActiveDB.Delete<Project> (OpenedProject);
				} catch (StorageException ex) {
					Log.Exception (ex);
					Config.GUIToolkit.ErrorMessage (ex.Message);
				}
			}
			CloseOpenedProject (!cancel);
			if (!cancel && type != ProjectType.FakeCaptureProject) {
				OpenProjectID (project.ID, project);
			}
		}

		void HandleMultimediaError (object sender, string message)
		{
			guiToolkit.ErrorMessage (Catalog.GetString ("The following error happened and" +
			" the current project will be closed:") + "\n" + message);
			CloseOpenedProject (true);
		}

		void HandleCaptureFinished (bool cancel)
		{
			CaptureFinished (cancel, cancel);
		}

		void HandleCaptureError (object sender, string message)
		{
			guiToolkit.ErrorMessage (Catalog.GetString ("The following error happened and" +
			" the current capture will be closed:") + "\n" + message);
			CaptureFinished (true, false);
		}

		#region IService

		public int Level {
			get {
				return 40;
			}
		}

		public string Name {
			get {
				return "Projects";
			}
		}

		public bool Start ()
		{
			multimediaToolkit = Config.MultimediaToolkit;
			guiToolkit = Config.GUIToolkit;
			Config.EventsBroker.NewProjectEvent += NewProject;
			Config.EventsBroker.OpenProjectEvent += OpenProject;
			Config.EventsBroker.OpenProjectIDEvent += OpenProjectID;
			Config.EventsBroker.OpenNewProjectEvent += OpenNewProject;
			Config.EventsBroker.CloseOpenedProjectEvent += PromptCloseProject;
			Config.EventsBroker.SaveProjectEvent += SaveProject;
			Config.EventsBroker.CaptureError += HandleCaptureError;
			Config.EventsBroker.CaptureFinished += HandleCaptureFinished;
			Config.EventsBroker.MultimediaError += HandleMultimediaError;
			return true;
		}

		public bool Stop ()
		{
			multimediaToolkit = null;
			guiToolkit = null;
			Config.EventsBroker.NewProjectEvent -= NewProject;
			Config.EventsBroker.OpenProjectEvent -= OpenProject;
			Config.EventsBroker.OpenProjectIDEvent -= OpenProjectID;
			Config.EventsBroker.OpenNewProjectEvent -= OpenNewProject;
			Config.EventsBroker.CloseOpenedProjectEvent -= PromptCloseProject;
			Config.EventsBroker.SaveProjectEvent -= SaveProject;
			Config.EventsBroker.CaptureError -= HandleCaptureError;
			Config.EventsBroker.CaptureFinished -= HandleCaptureFinished;
			Config.EventsBroker.MultimediaError -= HandleMultimediaError;
			return true;
		}

		#endregion
	}
}
