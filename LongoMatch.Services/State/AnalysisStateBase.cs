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
using System.Threading.Tasks;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;
using VAS.Services.State;

namespace LongoMatch.Services.State
{
	/// <summary>
	/// Base state for the project analysis states where the common load and hide logic is available
	/// </summary>
	public abstract class AnalysisStateBase : ScreenState<LMProjectAnalysisVM>
	{
		/// <summary>
		/// Unloads the state before leaving it
		/// </summary>
		/// <returns>The state.</returns>
		public override async Task<bool> HideState ()
		{
			// prompt before executing the close operation
			if (!await App.Current.EventsBroker.PublishWithReturn (new CloseEvent<LMProjectVM> { Object = ViewModel.Project })) {
				return false;
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
					VASCountLimitedObjects.TimelineEvents.ToString (), 9, App.Current.Style.PaletteBackground);
			}
		}
	}
}
