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
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;

namespace LongoMatch.Services.ViewModel
{
	public class SportsProjectsManagerVM : ProjectsManagerVM<LMProject, LMProjectVM>
	{
		CountLimitationBarChartVM limitationChart;

		public SportsProjectsManagerVM ()
		{
			ResyncCommand = new LimitationAsyncCommand (VASFeature.OpenMultiCamera.ToString (), Resync,
														() => LoadedProject.FileSet.Count () > 1);
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
	}
}

