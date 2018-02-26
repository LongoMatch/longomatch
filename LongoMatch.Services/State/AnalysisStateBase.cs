//
//  Copyright (C) 2017 FLUENDO S.A
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch.Core.Events;
using LongoMatch.Core.Interfaces.Services;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.Controller;
using LongoMatch.Services.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Interfaces.Services;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Services.State;

namespace LongoMatch.Services.State
{
	/// <summary>
	/// Base state for the project analysis states where the common load and hide logic is available
	/// </summary>
	public abstract class AnalysisStateBase : ScreenState<LMProjectAnalysisVM>
	{
		IEventEditorService editorService;

		/// <summary>
		/// Unloads the state before leaving it
		/// </summary>
		/// <returns>The state.</returns>
		public override async Task<bool> HideState ()
		{
			// prompt before executing the close operation
			if (!ViewModel.Project.CloseHandled) {
				if (!await GetService<IProjectAnalysisService> ().Close ()) {
					return false;
				}
			}
			return await base.HideState ();
		}

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

			if (!await Initialize (data)) {
				return false;
			}

			return await LoadProject ();
		}

		/// <summary>
		/// Finishes loading the project. This function can be overriden by subclass to provide
		/// extra checks and loading logic for the project.
		/// </summary>
		/// <returns>The state result.</returns>
		protected virtual Task<bool> LoadProject ()
		{
			return AsyncHelpers.Return (true);
		}

		/// <summary>
		/// Creates the limitation view Model
		/// </summary>
		protected void CreateLimitation ()
		{
			if (App.Current.LicenseLimitationsService != null) {
				ViewModel.Timeline.LimitationChart = App.Current.LicenseLimitationsService.CreateBarChartVM (
					VASCountLimitedObjects.TimelineEvents.ToString (), 9, App.Current.Style.ScreenBase);
			}
		}

		protected override void SetCommands ()
		{
			GetService<IProjectAnalysisService> ().SetDefaultCallbacks (ViewModel);
			GetService<IEventEditorService> ().SetDefaultCallbacks (ViewModel.Project.Timeline);
			GetService<ILMEventsService> ().SetDefaultCallbacks (ViewModel);
			GetService<IPlaylistService> ().SetDefaultCallbacks (ViewModel.Playlists);

			ViewModel.ShowWarningLimitation.SetCallback (() => { });
			((LimitationCommand)ViewModel.ShowWarningLimitation).LimitationCondition = () => ViewModel.Project.FileSet.Count () > 1;

			/* FIXME: There are still some things missing here:
			 * DashboardVM has several commands to manage buttons, the dashboard mode, and some behaviour changes. All that logic should also be set from the state.
			 * PlaylistCollectionVM has some methods used from the View, without a command. They should be easy to migrate to this setup.
			 * LMTeamTaggerVM has a couple of methods that should also be commands.
			 * HotkeyVM (included in all controllers that set hotkeys, and in dashboardbuttons) has a command to update the key. It's not used here but is used in RiftAnalyst when reconfiguring a button hotkey.
			 * LMProjectVM has a command ShowMenu, used only in mobile. It won't be used in analysis.
			 * The CapturerBin is still not migrated to MVVM (it's a WIP in RA-1294), but for the moment it contains several other commands:
			 		public Command StartCommand { get; }
					public Command StopCommand { get; }
					public Command PauseClockCommand { get; }
					public Command ResumeClockCommand { get; }
					public Command SaveCommand { get; }
					public Command CancelCommand { get; }
					public Command ViewReadyCommand { get; }
					public Command PlayLastEventCommand { get; }
					public Command DeleteLastEventCommand { get; }
			 */
		}

		protected override void CreateControllers (dynamic data)
		{
			editorService = App.Current.DependencyRegistry.Retrieve<IEventEditorService> ();
			GetService<LMTeamTaggerController> ().Init (GetService<ILMEventsService> ());
		}
	}
}
