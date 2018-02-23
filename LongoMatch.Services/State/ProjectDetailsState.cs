//
//  Copyright (C) 2018 Fluendo S.A.
//
//
using System.Linq;
using LongoMatch.Services.ViewModel;
using VAS.Core.Resources;
using VAS.Services.State;

namespace LongoMatch.Services.State
{
	public class ProjectDetailsState : ScreenState<SportsProjectsManagerVM>
	{
		public const string NAME = "ProjectDetailsState";

		public override string Name {
			get {
				return NAME;
			}
		}

		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = data;
			if (ViewModel.LoadedProject.Preview == null) {
				ViewModel.LoadedProject.Model.FileSet.FirstOrDefault ().Preview = App.Current.ResourcesLocator.LoadImage (Images.DefaultCardBackground);
			}
		}
	}
}
