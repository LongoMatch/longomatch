//
//  Copyright (C) 2018 Fluendo S.A.
//
//
using VAS.Services.State;
using VAS.Core.Common;
using LongoMatch.Services.ViewModel;

namespace LongoMatch.Services.State
{
	/// <summary>
	/// This state shows dashboard information in different detail areas
	/// </summary>
	public class DashboardDetailsState : ScreenState<DashboardsManagerVM>
	{
		public const string NAME = "DashboardDetailsState";

		public override string Name {
			get {
				return NAME;
			}
		}

		protected override void CreateViewModel (dynamic data)
		{
			var vm = data.viewModel as DashboardsManagerVM;
			vm.Dashboard.Mode = DashboardMode.Code;
			vm.Dashboard.FitMode = FitMode.Fit;

			ViewModel = vm;
		}
	}
}
