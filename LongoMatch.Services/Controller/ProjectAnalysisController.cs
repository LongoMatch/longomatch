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
using System.Linq;
using System.Threading.Tasks;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using Constants = LongoMatch.Core.Common.Constants;

namespace LongoMatch.Services
{
	[Controller (ProjectAnalysisState.NAME)]
	public class ProjectAnalysisController : DisposableBase, IController
	{
		LMProjectAnalysisVM viewModel;
		IGUIToolkit guiToolkit;
		IMultimediaToolkit multimediaToolkit;
		EventToken closeOpenedProjectEventToken;

		public ICapturerBin Capturer {
			set;
			get;
		}

		public LMProjectVM Project {
			set;
			get;
		}

		public VideoPlayerVM VideoPlayer {
			get;
			set;
		}

		public LMProjectAnalysisVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
			}
		}

		public void SetViewModel (IViewModel viewModel)
		{
			ViewModel = (LMProjectAnalysisVM)viewModel;
		}

		public IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		public void Start ()
		{
			multimediaToolkit = App.Current.MultimediaToolkit;
			guiToolkit = App.Current.GUIToolkit;
			Load (viewModel);
			App.Current.EventsBroker.SubscribeAsync<CloseEvent<LMProjectVM>> (HandleClose);
			App.Current.EventsBroker.SubscribeAsync<SaveEvent<LMProjectVM>> (HandleSave);
			App.Current.EventsBroker.Subscribe<CaptureErrorEvent> (HandleCaptureError);
			App.Current.EventsBroker.Subscribe<CaptureFinishedEvent> (HandleCaptureFinished);
			App.Current.EventsBroker.Subscribe<MultimediaErrorEvent> (HandleMultimediaError);
		}

		public void Stop ()
		{
			multimediaToolkit = null;
			guiToolkit = null;
			App.Current.EventsBroker.UnsubscribeAsync<CloseEvent<LMProjectVM>> (HandleClose);
			App.Current.EventsBroker.UnsubscribeAsync<SaveEvent<LMProjectVM>> (HandleSave);
			App.Current.EventsBroker.Unsubscribe<CaptureErrorEvent> (HandleCaptureError);
			App.Current.EventsBroker.Unsubscribe<CaptureFinishedEvent> (HandleCaptureFinished);
			App.Current.EventsBroker.Unsubscribe<MultimediaErrorEvent> (HandleMultimediaError);
		}

		protected Task HandleSave (SaveEvent<LMProjectVM> e)
		{
			if (e.Object == Project) {
				e.ReturnValue = SaveProject ();
			}
			return AsyncHelpers.Return ();
		}

		protected async Task HandleClose (CloseEvent<LMProjectVM> e)
		{
			if (e.Object == Project) {
				e.ReturnValue = await PromptCloseProject ();
			}
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

		bool SaveCaptureProject (LMProject project)
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
				project.Periods.Replace (Capturer.Periods);
				App.Current.DatabaseManager.ActiveDB.Store<LMProject> (project);
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
				VAS.Core.Store.Project.Export (Project.Model, projectFile);
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("An error occured saving the project:\n") + ex.Message + "\n\n" +
				Catalog.GetString ("The video file and a backup of the project has been " +
				"saved. Try to import it later:\n") +
				filePath + "\n" + projectFile + Constants.PROJECT_EXT);
				App.Current.DatabaseManager.ActiveDB.Delete<LMProject> (project);
				return false;
			}
		}

		async Task<bool> Load (LMProjectAnalysisVM analysisVM)
		{
			Project = analysisVM.Project;
			VideoPlayer = analysisVM.VideoPlayer;
			Capturer = analysisVM.Capturer;

			Log.Debug ("Loading project " + analysisVM.Project + " " + analysisVM.Project.ProjectType);

			Project.Model.CleanupTimers ();

			if (Project.ProjectType == ProjectType.FileProject) {
				// Check if the file associated to the project exists
				if (!Project.FileSet.Model.CheckFiles ()) {
					if (!guiToolkit.SelectMediaFiles (Project.FileSet.Model)) {
						await CloseOpenedProject (true);
						return false;
					}
				}
				try {
					VideoPlayer.OpenFileSet (Project.FileSet);
				} catch (Exception ex) {
					Log.Exception (ex);
					App.Current.Dialogs.ErrorMessage (Catalog.GetString ("An error occurred opening this project:") + "\n" + ex.Message);
					await CloseOpenedProject (false);
					return false;
				}

			} else if (Project.ProjectType == ProjectType.CaptureProject ||
					   Project.ProjectType == ProjectType.URICaptureProject ||
					   Project.ProjectType == ProjectType.FakeCaptureProject) {
				try {
					Capturer.Run (analysisVM.CaptureSettings, Project.FileSet.First ().Model);
				} catch (Exception ex) {
					Log.Exception (ex);
					App.Current.Dialogs.ErrorMessage (ex.Message);
					await CloseOpenedProject (false);
					return false;
				}
			}
			return true;
		}

		async Task<bool> PromptCloseProject ()
		{
			if (Project == null)
				return true;

			if (Project.ProjectType == ProjectType.FileProject) {
				if (await App.Current.Dialogs.QuestionMessage (
					Catalog.GetString ("Do you want to close the current project?"), null)) {
					await CloseOpenedProject (true);
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
					await CaptureFinished (true, true, false);
					return true;
				} else if (res == EndCaptureResponse.Save) {
					await CaptureFinished (false, false, true);
					return true;
				} else {
					/* Continue with the current project */
					return false;
				}
			}
		}

		async Task<bool> CloseOpenedProject (bool save)
		{
			if (Project == null)
				return false;

			Log.Debug ("Closing project " + Project.ShortDescription);
			if (Capturer != null) {
				Capturer.Close ();
			}
			if (VideoPlayer != null) {
				VideoPlayer.Dispose ();
			}

			bool saveOk = true;
			if (save) {
				saveOk = SaveProject ();
			}

			if (saveOk) {
				return await App.Current.StateController.MoveToHome ();
			}
			return false;
		}

		bool UpdateProject (LMProject project)
		{
			try {
				App.Current.DatabaseManager.ActiveDB.Store<LMProject> (project);
				return true;
			} catch (Exception ex) {
				Log.Exception (ex);
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("An error occured saving the project:\n") + ex.Message);
				return false;
			}
		}

		bool SaveProject ()
		{
			LMProject project = Project.Model;
			if (project == null)
				return false;

			Log.Debug (String.Format ("Saving project {0} type: {1}", project.ID, project.ProjectType));
			if (Project.ProjectType == ProjectType.FileProject) {
				return UpdateProject (project);
			} else if (Project.ProjectType == ProjectType.FakeCaptureProject) {
				project.Periods.Replace (Capturer.Periods);
				return UpdateProject (project);
			} else if (Project.ProjectType == ProjectType.CaptureProject ||
					   Project.ProjectType == ProjectType.URICaptureProject) {
				return SaveCaptureProject (project);
			} else {
				return false;
			}
		}

		void Save (Project project)
		{
			if (App.Current.Config.AutoSave) {
				App.Current.DatabaseManager.ActiveDB.Store (project);
			}
		}

		async Task<bool> CaptureFinished (bool cancel, bool delete, bool reopen)
		{
			LMProject project = Project.Model;
			ProjectType type = Project.ProjectType;
			if (delete) {
				if (type != ProjectType.FakeCaptureProject) {
					try {
						File.Delete (Capturer.CaptureSettings.EncodingSettings.OutputFile);
					} catch (Exception ex1) {
						Log.Exception (ex1);
					}
				}
				try {
					App.Current.DatabaseManager.ActiveDB.Delete<LMProject> (project);
				} catch (StorageException ex) {
					Log.Exception (ex);
					App.Current.Dialogs.ErrorMessage (ex.Message);
				}
			}
			bool closeOk = await CloseOpenedProject (!cancel);
			if (closeOk && reopen && !cancel && type != ProjectType.FakeCaptureProject) {
				return await App.Current.StateController.MoveTo (ProjectAnalysisState.NAME, Project);
			}
			return false;
		}

		async void HandleMultimediaError (MultimediaErrorEvent e)
		{
			App.Current.Dialogs.ErrorMessage (Catalog.GetString ("The following error happened and" +
			" the current project will be closed:") + "\n" + e.Message);
			await CloseOpenedProject (true);
		}

		async void HandleCaptureFinished (CaptureFinishedEvent e)
		{
			await CaptureFinished (e.Cancel, e.Cancel, e.Reopen);
		}

		async void HandleCaptureError (CaptureErrorEvent e)
		{
			App.Current.Dialogs.ErrorMessage (Catalog.GetString ("The following error happened and" +
			" the current capture will be closed:") + "\n" + e.Message);
			await CaptureFinished (true, false, false);
		}
	}
}
