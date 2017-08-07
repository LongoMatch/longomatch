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
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using Constants = LongoMatch.Core.Common.Constants;

namespace LongoMatch.Services
{
	[Controller (ProjectAnalysisState.NAME)]
	[Controller (LiveProjectAnalysisState.NAME)]
	[Controller (FakeLiveProjectAnalysisState.NAME)]
	public class ProjectAnalysisController : ControllerBase
	{
		LMProjectAnalysisVM viewModel;

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
				if (viewModel != null) {
					Project = viewModel.Project;
					VideoPlayer = viewModel.VideoPlayer;
					Capturer = viewModel.Capturer;
					Project.CloseHandled = false;
					Log.Debug ("Loading project " + viewModel.Project + " " + viewModel.Project.ProjectType);
				}

			}
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			ViewModel = (LMProjectAnalysisVM)viewModel;
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			var actions = new List<KeyAction> ();
			actions.Add (new KeyAction (App.Current.HotkeysService.GetByName (GeneralUIHotkeys.SAVE),
										() => ViewModel.SaveCommand.Execute ()));
			actions.Add (new KeyAction (App.Current.HotkeysService.GetByName (GeneralUIHotkeys.CLOSE),
										() => ViewModel.CloseCommand.Execute ()));
			return actions;
		}

		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.SubscribeAsync<CloseEvent<LMProjectVM>> (HandleClose);
			App.Current.EventsBroker.SubscribeAsync<SaveEvent<LMProjectVM>> (HandleSave);
			App.Current.EventsBroker.Subscribe<CaptureErrorEvent> (HandleCaptureError);
			App.Current.EventsBroker.Subscribe<CaptureFinishedEvent> (HandleCaptureFinished);
			App.Current.EventsBroker.Subscribe<MultimediaErrorEvent> (HandleMultimediaError);
			App.Current.EventsBroker.Subscribe<NavigationEvent> (HandleNavigation);
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			App.Current.EventsBroker.UnsubscribeAsync<CloseEvent<LMProjectVM>> (HandleClose);
			App.Current.EventsBroker.UnsubscribeAsync<SaveEvent<LMProjectVM>> (HandleSave);
			App.Current.EventsBroker.Unsubscribe<CaptureErrorEvent> (HandleCaptureError);
			App.Current.EventsBroker.Unsubscribe<CaptureFinishedEvent> (HandleCaptureFinished);
			App.Current.EventsBroker.Unsubscribe<MultimediaErrorEvent> (HandleMultimediaError);
			App.Current.EventsBroker.Unsubscribe<NavigationEvent> (HandleNavigation);
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
				if (!Project.CloseHandled) {
					Project.CloseHandled = true;
					e.ReturnValue = await PromptCloseProject ();
					if (!e.ReturnValue) {
						Project.CloseHandled = false;
					}
				} else {
					e.ReturnValue = true;
				}
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
				if (App.Current.GUIToolkit.RemuxFile (tmpFile, outFile, muxer) == outFile) {
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
				project.Description.FileSet [0] = App.Current.MultimediaToolkit.DiscoverFile (filePath);
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

				res = App.Current.GUIToolkit.EndCapture (isCapturing);

				/* Close project wihtout saving */
				if (res == EndCaptureResponse.Quit) {
					await CaptureFinished (true, true, false);
					return true;
				} else if (res == EndCaptureResponse.Save) {
					bool reopen = (Project.ProjectType == ProjectType.FakeCaptureProject) ? false : true;
					await CaptureFinished (false, false, reopen);
					return true;
				} else {
					/* Continue with the current project */
					return false;
				}
			}
		}

		async Task<bool> CloseOpenedProject (bool save, bool goHome = true)
		{
			if (Project == null)
				return false;

			Log.Debug ("Closing project " + Project.ShortDescription);

			if (Capturer != null) {
				Capturer.Close ();
			}

			bool saveOk = true;
			if (save) {
				saveOk = SaveProject ();
			}

			if (saveOk && goHome) {
				return await App.Current.StateController.MoveToHome ();
			}

			return saveOk;
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

			// if it comes from a cancel operation the close have been handled
			bool closeOk = await CloseOpenedProject (!cancel, !reopen);
			if (closeOk && reopen && !cancel && type != ProjectType.FakeCaptureProject) {
				Project.ProjectType = ProjectType.FileProject;
				Project.CloseHandled = true; // the reopen comes from a close operation avoid asking again
				LMStateHelper.OpenProject (Project);
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
			Project.CloseHandled = true;
			bool reopen = Project.ProjectType == ProjectType.FakeCaptureProject ? false : e.Reopen;
			await CaptureFinished (e.Cancel, e.Cancel, reopen);
		}

		async void HandleCaptureError (CaptureErrorEvent e)
		{
			App.Current.Dialogs.ErrorMessage (Catalog.GetString ("The following error happened and" +
			" the current capture will be closed:") + "\n" + e.Message);
			await CaptureFinished (true, false, false);
		}

		// Fixme: This event only is raised at the moment when the state is created but it will be better
		// if it is only related with the open operation itself.
		void HandleNavigation (NavigationEvent e)
		{
			if (e.Name == ProjectAnalysisState.NAME) {
				ViewModel.ShowWarningLimitation.Execute ();
			}
		}
	}
}
