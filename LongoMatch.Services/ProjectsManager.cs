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
using LongoMatch.Core.Events;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using Constants = LongoMatch.Core.Common.Constants;
using LMCommon = LongoMatch.Core.Common;

namespace LongoMatch.Services
{
	public class ProjectsManager: IService
	{
		IGUIToolkit guiToolkit;
		IMultimediaToolkit multimediaToolkit;
		IAnalysisWindowBase analysisWindow;

		public ProjectsManager ()
		{
		}

		public ProjectLongoMatch OpenedProject {
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
			App.Current.EventsBroker.Publish<OpenedProjectEvent> (
				new OpenedProjectEvent {
					Project = OpenedProject,
					ProjectType = OpenedProjectType,
					Filter = PlaysFilter,
					AnalysisWindow = analysisWindow
				}
			);
		}

		void RemuxOutputFile (EncodingSettings settings)
		{
			VideoMuxerType muxer;
				
			/* We need to remux to the original format */
			muxer = settings.EncodingProfile.Muxer;
			if (muxer == VideoMuxerType.Avi || muxer == VideoMuxerType.Mp4) {
				string outFile = settings.OutputFile;
				string tmpFile = settings.OutputFile;
				
				while (File.Exists (tmpFile)) {
					tmpFile = tmpFile + ".tmp";
				}
				
				Log.Debug ("Remuxing file tmp: " + tmpFile + " out: " + outFile);
				
				try {
					File.Move (outFile, tmpFile);
				} catch (Exception ex) {
					/* Try to fix "Sharing violation on path" in windows
					 * wait a bit more until the file lock is released */
					Log.Exception (ex);
					System.Threading.Thread.Sleep (5 * 1000);
					try {
						File.Move (outFile, tmpFile);
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

		bool SaveCaptureProject (ProjectLongoMatch project)
		{
			Guid projectID = project.ID;
			// FIXME
			string filePath = project.Description.FileSet.First ().FilePath;

			/* scan the new file to build a new PreviewMediaFile with all the metadata */
			try {
				Log.Debug ("Saving capture project: " + project.ID);
			
				#if !OSTYPE_ANDROID && !OSTYPE_IOS
				RemuxOutputFile (Capturer.CaptureSettings.EncodingSettings);
				#endif
			
				Log.Debug ("Reloading saved file: " + filePath);
				project.Description.FileSet [0] = multimediaToolkit.DiscoverFile (filePath);
				project.Periods = new ObservableCollection<Period> (Capturer.Periods);
				App.Current.DatabaseManager.ActiveDB.Store<ProjectLongoMatch> (project);
				return true;
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
				App.Current.DatabaseManager.ActiveDB.Delete<Project> (project);
				return false;
			}
		}

		bool SetProject (ProjectLongoMatch project, ProjectType projectType, CaptureSettings props)
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
					if (!guiToolkit.SelectMediaFiles (project.Description.FileSet)) {
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
		bool PromptCloseProject (CloseOpenedProjectEvent e)
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
					CaptureFinished (true, true, false);
					return true;
				} else if (res == EndCaptureResponse.Save) {
					CaptureFinished (false, false, true);
					return true;
				} else {
					/* Continue with the current project */
					return false;
				}
			}
		}

		bool CloseOpenedProject (bool save)
		{
			if (OpenedProject == null)
				return false;
				
			Log.Debug ("Closing project " + OpenedProject.ID);
			if (Capturer != null) {
				Capturer.Close ();
			}
			if (Player != null) {
				Player.Dispose ();
			}

			bool saveOk = true;
			if (save) {
				saveOk = SaveProject (OpenedProject, OpenedProjectType);
			}

			OpenedProject = null;
			OpenedProjectType = ProjectType.None;
			guiToolkit.CloseProject ();
			EmitProjectChanged ();
			return saveOk;
		}

		bool UpdateProject (ProjectLongoMatch project)
		{
			try {
				App.Current.DatabaseManager.ActiveDB.Store<Project> (project);
				return true;
			} catch (Exception ex) {
				Log.Exception (ex);
				guiToolkit.ErrorMessage (Catalog.GetString ("An error occured saving the project:\n") + ex.Message);
				return false;
			}
		}

		protected virtual void NewProject (NewProjectEvent e)
		{
			Log.Debug ("Creating new project");
			
			if (!PromptCloseProject (new CloseOpenedProjectEvent ())) {
				return;
			}
			
			guiToolkit.CreateNewProject (e.Project);
		}

		protected virtual void HandleSaveProject (SaveProjectEvent e)
		{
			SaveProject (e.Project as ProjectLongoMatch, e.ProjectType);
		}

		bool SaveProject (ProjectLongoMatch project, ProjectType projectType)
		{
			if (project == null)
				return false;

			Log.Debug (String.Format ("Saving project {0} type: {1}", project.ID, projectType));
			if (projectType == ProjectType.FileProject) {
				return UpdateProject (project);
			} else if (projectType == ProjectType.FakeCaptureProject) {
				project.Periods = new ObservableCollection<Period> (Capturer.Periods);
				return UpdateProject (project);
			} else if (projectType == ProjectType.CaptureProject ||
			           projectType == ProjectType.URICaptureProject) {
				return SaveCaptureProject (project);
			} else {
				return false;
			}
		}

		void OpenNewProject (OpenNewProjectEvent e)
		{
			if (e.Project != null) {
				try {
					App.Current.DatabaseManager.ActiveDB.Store<Project> (e.Project, true);
					SetProject (e.Project, e.ProjectType, e.CaptureSettings);
				} catch (Exception ex) {
					Log.Exception (ex);
					guiToolkit.ErrorMessage (Catalog.GetString ("An error occured saving the project:\n") + ex.Message);
				}
			}
		}

		void OpenProject (OpenProjectEvent e)
		{
			if (!PromptCloseProject (new CloseOpenedProjectEvent ())) {
				return;
			}
			guiToolkit.SelectProject (App.Current.DatabaseManager.ActiveDB.RetrieveAll<ProjectLongoMatch> ().Cast<Project> ().ToList ());
		}

		void Save (Project project)
		{
			if (App.Current.Config.AutoSave) {
				App.Current.DatabaseManager.ActiveDB.Store (project);
			}
		}



		void OpenProjectID (OpenProjectIDEvent e)
		{
			if (e.Project == null) {
				try {
					e.Project = App.Current.DatabaseManager.ActiveDB.Retrieve<ProjectLongoMatch> (e.ProjectID);
				} catch (Exception ex) {
					Log.Exception (ex);
					guiToolkit.ErrorMessage (ex.Message);
					return;
				}
			}

			if (!e.Project.IsLoaded) {
				try {
					IBusyDialog busy = App.Current.GUIToolkit.BusyDialog (Catalog.GetString ("Loading project..."), null);
					busy.ShowSync (e.Project.Load);
				} catch (Exception ex) {
					Log.Exception (ex);
					guiToolkit.ErrorMessage (Catalog.GetString ("Could not load project:") + "\n" + ex.Message);
					return;
				}
			}

			// FIXME
			if (e.Project.IsFakeCapture) {
				/* If it's a fake live project prompt for a video file and
				 * create a new PreviewMediaFile for this project and recreate the thumbnails */
				Log.Debug ("Importing fake live project");
				App.Current.EventsBroker.Publish<NewProjectEvent> (new NewProjectEvent { Project = e.Project });
				return;
			}

			if (e.Project.Description.FileSet.Duration == null) {
				Log.Warning ("The selected project is empty. Rediscovering files");
				for (int i = 0; i < e.Project.Description.FileSet.Count; i++) {
					e.Project.Description.FileSet [i] = App.Current.MultimediaToolkit.DiscoverFile (e.Project.Description.FileSet [i].FilePath);
				}
			}

			e.Project.UpdateEventTypesAndTimers ();
			SetProject (e.Project, ProjectType.FileProject, new CaptureSettings ());
		}

		void CaptureFinished (bool cancel, bool delete, bool reopen)
		{
			ProjectLongoMatch project = OpenedProject;
			ProjectType type = OpenedProjectType;
			if (delete) {
				if (type != ProjectType.FakeCaptureProject) {
					try {
						File.Delete (Capturer.CaptureSettings.EncodingSettings.OutputFile);
					} catch (Exception ex1) {
						Log.Exception (ex1);
					}
				}
				try {
					App.Current.DatabaseManager.ActiveDB.Delete<Project> (OpenedProject);
				} catch (StorageException ex) {
					Log.Exception (ex);
					App.Current.GUIToolkit.ErrorMessage (ex.Message);
				}
			}
			bool closeOk = CloseOpenedProject (!cancel);
			if (closeOk && reopen && !cancel && type != ProjectType.FakeCaptureProject) {
				OpenProjectID (new OpenProjectIDEvent { ProjectID = project.ID, Project = project });					
			}
		}

		void HandleMultimediaError (MultimediaErrorEvent e)
		{
			guiToolkit.ErrorMessage (Catalog.GetString ("The following error happened and" +
			" the current project will be closed:") + "\n" + e.Message);
			CloseOpenedProject (true);
		}

		void HandleCaptureFinished (CaptureFinishedEvent e)
		{
			CaptureFinished (e.Cancel, e.Cancel, e.Reopen);
		}

		void HandleCaptureError (CaptureErrorEvent e)
		{
			guiToolkit.ErrorMessage (Catalog.GetString ("The following error happened and" +
			" the current capture will be closed:") + "\n" + e.Message);
			CaptureFinished (true, false, false);
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
			multimediaToolkit = App.Current.MultimediaToolkit;
			guiToolkit = App.Current.GUIToolkit;
			App.Current.EventsBroker.Subscribe<NewProjectEvent> (NewProject);
			App.Current.EventsBroker.Subscribe<OpenProjectEvent> (OpenProject);
			App.Current.EventsBroker.Subscribe<OpenProjectIDEvent> (OpenProjectID);
			App.Current.EventsBroker.Subscribe<OpenNewProjectEvent> (OpenNewProject);
			closeOpenedProjectEventToken = App.Current.EventsBroker.Subscribe<CloseOpenedProjectEvent> ((e) => {
				PromptCloseProject (new CloseOpenedProjectEvent ());
			});
			App.Current.EventsBroker.Subscribe<SaveProjectEvent> (HandleSaveProject);
			App.Current.EventsBroker.Subscribe<CaptureErrorEvent> (HandleCaptureError);
			App.Current.EventsBroker.Subscribe<CaptureFinishedEvent> (HandleCaptureFinished);
			App.Current.EventsBroker.Subscribe<MultimediaErrorEvent> (HandleMultimediaError);
			return true;
		}

		public bool Stop ()
		{
			multimediaToolkit = null;
			guiToolkit = null;
			App.Current.EventsBroker.Unsubscribe<NewProjectEvent> (NewProject);
			App.Current.EventsBroker.Unsubscribe<OpenProjectEvent> (OpenProject);
			App.Current.EventsBroker.Unsubscribe<OpenProjectIDEvent> (OpenProjectID);
			App.Current.EventsBroker.Unsubscribe<OpenNewProjectEvent> (OpenNewProject);
			App.Current.EventsBroker.Unsubscribe<CloseOpenedProjectEvent> (closeOpenedProjectEventToken);
			App.Current.EventsBroker.Unsubscribe<SaveProjectEvent> (HandleSaveProject);
			App.Current.EventsBroker.Unsubscribe<CaptureErrorEvent> (HandleCaptureError);
			App.Current.EventsBroker.Unsubscribe<CaptureFinishedEvent> (HandleCaptureFinished);
			App.Current.EventsBroker.Unsubscribe<MultimediaErrorEvent> (HandleMultimediaError);
			return true;
		}

		#endregion

		EventToken closeOpenedProjectEventToken;
	}
}
