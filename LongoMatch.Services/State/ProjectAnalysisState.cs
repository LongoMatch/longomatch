//
//  Copyright (C) 2016 Fluendo S.A.
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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Threading.Tasks;
using LongoMatch.Core;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.ViewModel;
using VAS.Services;
using VAS.Services.Controller;

namespace LongoMatch.Services.State
{
	/// <summary>
	/// A state for post-match analysis of projects.
	/// </summary>
	public class ProjectAnalysisState : AnalysisStateBase
	{
		public const string NAME = "ProjectAnalysis";

		public override string Name {
			get {
				return NAME;
			}
		}

		protected override async Task<bool> LoadProject ()
		{
			ProjectVM project = ViewModel.Project;

			if (project.Model.IsFakeCapture) {
				/* If it's a fake live project prompt for a video file and
			 	* create a new PreviewMediaFile for this project and recreate the thumbnails */
				Log.Debug ("Importing fake live project");
				await App.Current.StateController.MoveTo (NewProjectState.NAME, project);
				return false;
			}

			// Check if the file associated to the project exists
			if (!project.FileSet.Model.CheckFiles ()) {
				if (!App.Current.GUIToolkit.SelectMediaFiles (project.FileSet.Model)) {
					return false;
				}
			}

			if (project.FileSet.Duration == null) {
				Log.Warning ("The selected project is empty. Rediscovering files");
				for (int i = 0; i < project.Model.FileSet.Count; i++) {
					project.Model.FileSet [i] = App.Current.MultimediaToolkit.DiscoverFile (project.Model.FileSet [i].FilePath);
				}
			}

			project.Model.CleanupTimers ();
			project.Model.UpdateEventTypesAndTimers ();

			try {
				ViewModel.VideoPlayer.OpenFileSet (project.FileSet);
			} catch (Exception ex) {
				Log.Exception (ex);
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("An error occurred opening this project:") + "\n" + ex.Message);
				return false;
			}

			return true;
		}

		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new LMProjectAnalysisVM ();
			ViewModel.Project.Model = data.Project.Model;
			ViewModel.VideoPlayer = new VideoPlayerVM ();
			ViewModel.VideoPlayer.ShowCenterPlayHeadButton = false;
			ViewModel.VideoPlayer.ViewMode = PlayerViewOperationMode.Analysis;
		}

		protected override void CreateControllers (dynamic data)
		{
			var playerController = new VideoPlayerController ();
			playerController.SetViewModel (ViewModel.VideoPlayer);
			Controllers.Add (playerController);
			Controllers.Add (new CoreEventsController ());
		}
	}
}
