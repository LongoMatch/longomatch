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

			var lmProjectVM = new LMProjectVM () {
				Model = new Core.Store.LMProject () {
					Description = new Core.Store.ProjectDescription () {
						LocalGoals = 1,
						VisitorGoals = 2,
						LocalName = "Boca Juniors",
						VisitorName = "River Plate",
						LocalShield = App.Current.ResourcesLocator.LoadIcon ("lma-default"),
						VisitorShield = App.Current.ResourcesLocator.LoadIcon ("lma-default"),
						Competition = "Primera División",
						Description = "Boca-River Bombonera 2017",
						Season = "2017",
						MatchDate = DateTime.Now,
					},
				},
			};

			lmProjectVM.Dashboard.Model = LMDashboard.DefaultTemplate (5);
			lmProjectVM.HomeTeam.Model = LMTeam.DefaultTemplate (11);
			lmProjectVM.AwayTeam.Model = LMTeam.DefaultTemplate (11);
			lmProjectVM.HomeTeam.TeamName = "Boca Juniors";
			lmProjectVM.AwayTeam.TeamName = "River Plate";

		}
	}
}
