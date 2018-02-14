//
//  Copyright (C) 2018 Fluendo S.A.
//
//
using System;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
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
		}
	}
}
