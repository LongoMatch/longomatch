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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch.Core.Events;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;

namespace LongoMatch.Services.ViewModel
{
	[ViewAttribute ("ProjectsManager")]
	public class SportsProjectsManagerVM : ProjectsManagerVM<LMProject, LMProjectVM>
	{
		CountLimitationBarChartVM limitationChart;

		public SportsProjectsManagerVM ()
		{
			ResyncCommand = new LimitationAsyncCommand (VASFeature.OpenMultiCamera.ToString (), Resync, () => LoadedProject.FileSet.Count () > 1);
			ProjectMenu = CreateProjectMenu ();
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

		/// <summary>
		/// Gets or sets the type of the sort.
		/// </summary>
		/// <value>The type of the sort.</value>
		public ProjectSortType SortType {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the filter text.
		/// </summary>
		/// <value>The filter text.</value>
		public string FilterText {
			get;
			set;
		} = "";

		[PropertyChanged.DoNotNotify]
		public LimitationAsyncCommand ResyncCommand {
			get;
			protected set;
		}

		/// <summary>
		/// Gets the project menu.
		/// </summary>
		/// <value>The project menu.</value>
		public MenuVM ProjectMenu { get; private set; }

		protected override async Task Open (LMProjectVM viewModel)
		{
			await Save (false);
			await base.Open (viewModel);
		}

		protected async Task Resync ()
		{
			await App.Current.EventsBroker.Publish (new ResyncEvent ());
		}

		protected MenuVM CreateProjectMenu ()
		{
			MenuVM menu = new MenuVM ();
			menu.ViewModels.AddRange (new List<MenuNodeVM> {
				new MenuNodeVM (DeleteCommand, null, Catalog.GetString("Delete")) { Color = App.Current.Style.ColorAccentError },
			});

			return menu;
		}
	}
}

