//
//  Copyright (C) 2016 Fluendo S.A.
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
using System.Linq;
using System.Threading.Tasks;
using LongoMatch.Core.Events;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.State;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Core.Interfaces.MVVMC;
using VAS.Services.ViewModel;
using System.Collections.Generic;
using VAS.Core;

namespace LongoMatch.Services.ViewModel
{
	[ViewAttribute ("ProjectsManager")]
	public class SportsProjectsManagerVM : ProjectsManagerVM<LMProject, LMProjectVM>
	{
		CountLimitationBarChartVM limitationChart;

		public SportsProjectsManagerVM ()
		{
			ResyncCommand = new LimitationAsyncCommand (VASFeature.OpenMultiCamera.ToString (), Resync,
														() => LoadedProject.FileSet.Count () > 1);
			DeleteCommand = new AsyncCommand<LMProjectVM> (Delete, (arg) => Selection.Any () || arg != null) { IconName = "vas-delete" };
			DetailsCommand = new AsyncCommand<LMProjectVM> (Details, (arg) => Selection.Any () || arg != null) { IconName = "lm-box" };
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			if (LimitationChart != null) {
				LimitationChart.Dispose ();
				LimitationChart = null;
			}
		}

		/// <summary>
		/// ViewModel for the Bar chart used to display count limitations in the Limitation Widget
		/// </summary>
		public CountLimitationBarChartVM LimitationChart {
			get {
				return limitationChart;
			}

			set {
				limitationChart = value;
				Limitation = limitationChart?.Limitation;
			}
		}

		[PropertyChanged.DoNotNotify]
		public LimitationAsyncCommand ResyncCommand {
			get;
			protected set;
		}

		protected override async Task Open (LMProjectVM viewModel)
		{
			await Save (false);
			await base.Open (viewModel);
		}

		protected async Task Resync ()
		{
			await App.Current.EventsBroker.Publish (new ResyncEvent ());
		}

		protected virtual async Task Details (LMProjectVM viewModel)
		{
			LoadedProject = viewModel;
			await App.Current.StateController.MoveTo (ProjectDetailsState.NAME, this);
		}

		/// <summary>
		/// Command to delete the selected projects.
		/// </summary>
		protected virtual async Task Delete (LMProjectVM viewModel)
		{
			if (viewModel != null) {
				await App.Current.EventsBroker.Publish (new DeleteEvent<LMProject> { Object = viewModel.Model });
			} else {
				foreach (LMProject project in Selection.Select (vm => vm.Model).ToList ()) {
					await App.Current.EventsBroker.Publish (new DeleteEvent<LMProject> { Object = project });
				}
			}
		}

		protected override MenuVM CreateMenu (IViewModel viewModel)
		{
			MenuVM menu = new MenuVM ();
			menu.ViewModels.AddRange (new List<MenuNodeVM> {
				new MenuNodeVM (DeleteCommand, viewModel, Catalog.GetString ("Delete")) { ActiveColor = App.Current.Style.ColorAccentError },
				new MenuNodeVM (DetailsCommand, viewModel, Catalog.GetString ("Project Details")) { ActiveColor = App.Current.Style.TextBase },
			});

			return menu;
		}
	}
}

