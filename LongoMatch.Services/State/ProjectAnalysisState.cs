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
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;
using VAS.Core.ViewModel;
using VAS.Services;
using VAS.Services.Controller;
using VAS.Services.State;

namespace LongoMatch.Services.State
{
	/// <summary>
	/// A state for post-match analysis of projects.
	/// </summary>
	public class ProjectAnalysisState : ScreenState<LMProjectAnalysisVM>
	{
		public const string NAME = "ProjectAnalysis";

		public override string Name {
			get {
				return NAME;
			}
		}

		/// <summary>
		/// Loads the project state using the project passed in data.
		/// </summary>
		/// <returns>The state.</returns>
		/// <param name="data">Data.</param>
		public override async Task<bool> LoadState (dynamic data)
		{
			LMProjectVM projectVM = data.Project;

			// FIXME: Load project asynchronously
			if (!projectVM.Model.IsLoaded) {
				try {
					IBusyDialog busy = App.Current.Dialogs.BusyDialog (Catalog.GetString ("Loading project..."), null);
					busy.ShowSync (() => {
						try {
							projectVM.Model.Load ();
						} catch (Exception ex) {
							Log.Exception (ex);
							throw;
						}
					});
				} catch (Exception ex) {
					Log.Exception (ex);
					App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Could not load project:") + "\n" + ex.Message);
					return false;
				}
			}

			if (projectVM.Model.IsFakeCapture) {
				/* If it's a fake live project prompt for a video file and
				 * create a new PreviewMediaFile for this project and recreate the thumbnails */
				Log.Debug ("Importing fake live project");
				await App.Current.StateController.MoveTo (NewProjectState.NAME, projectVM);
				return true;
			}

			if (projectVM.FileSet.Duration == null) {
				Log.Warning ("The selected project is empty. Rediscovering files");
				for (int i = 0; i < projectVM.Model.FileSet.Count; i++) {
					projectVM.Model.FileSet [i] = App.Current.MultimediaToolkit.DiscoverFile (projectVM.Model.FileSet [i].FilePath);
				}
			}

			projectVM.Model.UpdateEventTypesAndTimers ();
			return await Initialize (data);
		}

		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new LMProjectAnalysisVM ();
			ViewModel.Project = data.Project;
			ViewModel.VideoPlayer = new VideoPlayerVM ();
		}

		protected override void CreateControllers (dynamic data)
		{
			var playerController = new VideoPlayerController ();
			playerController.SetViewModel (ViewModel.VideoPlayer);
			Controllers.Add (playerController);
			Controllers.Add (new CoreEventsController ());
		}

		public override async Task<bool> HideState ()
		{
			// promt before executing the close operation
			if (!await App.Current.EventsBroker.PublishWithReturn (
					new CloseEvent<LMProjectVM> { Object = ViewModel })) 
			{
				return false;
			}

			return await base.HideState ();
		}
	}
}
