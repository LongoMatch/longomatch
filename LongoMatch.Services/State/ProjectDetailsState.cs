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

		public ProjectDetailsState ()
		{
			ViewModelOwner = false;
		}

		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = data;
			ViewModel.LoadedProject.Model.Load ();
		}
	}
}
