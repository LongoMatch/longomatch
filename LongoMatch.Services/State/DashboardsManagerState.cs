//
//  Copyright (C) 2016 Andoni Morales Alastruey
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
using System.Threading.Tasks;
using LongoMatch.Core.Common;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Store.Templates;
using VAS.Services.State;

namespace LongoMatch.Services.States
{
	public class DashboardsManagerState : ScreenState<DashboardsManagerVM>
	{
		public const string NAME = "DashboardsManager";

		public override string Name {
			get {
				return NAME;
			}
		}

		public override Task<bool> HideState ()
		{
			ViewModel.SaveCommand.Execute (false);
			return base.HideState ();
		}

		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new DashboardsManagerVM ();
			var templates = App.Current.CategoriesTemplatesProvider.Templates;
			ViewModel.Model = new RangeObservableCollection<Dashboard> (App.Current.CategoriesTemplatesProvider.Templates);
			if (App.Current.LicenseLimitationsService != null)
			{
				ViewModel.LimitationChart = App.Current.LicenseLimitationsService.CreateBarChartVM (
				LongoMatchCountLimitedObjects.Dashboard.ToString ());
			}
		}

	}
}
