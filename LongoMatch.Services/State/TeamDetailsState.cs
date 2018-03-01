//
//  Copyright (C) 2018 
//
//
using System;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
using VAS.Services.State;

namespace LongoMatch.Services.State
{
	/// <summary>
	/// This state shows the team information in different detail areas
	/// </summary>
	public class TeamDetailsState : ScreenState<TeamsManagerVM>
	{
		public const string NAME = nameof (TeamDetailsState);

		public override string Name {
			get {
				return NAME;
			}
		}

		protected override void CreateViewModel (dynamic data)
		{
			var vm = data.viewModel as TeamsManagerVM;
			ViewModel = vm;
		}
	}
}
