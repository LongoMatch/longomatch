//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Services.State;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.License;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;

namespace Tests.State
{
	[TestFixture]
	public class TestNewProjectState
	{
		NewProjectState state;
		Mock<ICategoriesTemplatesProvider> mockDashboardsTemplates;
		Mock<ITeamTemplatesProvider> mockTeamTemplates;
		Mock<ILicenseLimitationsService> mockLimitationService;
		List<Team> teams;
		List<Dashboard> dashboards;

		[OneTimeSetUp]
		public void OneTimeSetUp () {
			SetupClass.SetUp ();
			Mock<IGUIToolkit> mockGui = new Mock<IGUIToolkit> ();
			mockGui.SetupGet (g => g.DeviceScaleFactor).Returns (1);
			App.Current.GUIToolkit = mockGui.Object;

			mockDashboardsTemplates = new Mock<ICategoriesTemplatesProvider> ();
			mockTeamTemplates = new Mock<ITeamTemplatesProvider> ();
			App.Current.CategoriesTemplatesProvider = mockDashboardsTemplates.Object;
			App.Current.TeamTemplatesProvider = mockTeamTemplates.Object;

			teams = new List<Team> {
				new LMTeam {
					Name = "1"
				},
				new LMTeam {
					Name = "2"
				},
				new LMTeam {
					Name = "3"
				},
				new LMTeam {
					Name = "4"
				},
				new LMTeam {
					Name = "5"
				}
			};

			dashboards = new List<Dashboard> {
				new LMDashboard {
					Name = "1"
				},
				new LMDashboard {
					Name = "2"
				},
				new LMDashboard {
					Name = "3"
				},
				new LMDashboard {
					Name = "4"
				},
				new LMDashboard {
					Name = "5"
				}
			};

			mockTeamTemplates.SetupGet (tm => tm.Templates).Returns (teams);
			mockDashboardsTemplates.SetupGet (d => d.Templates).Returns (dashboards);
		}

		[SetUp]
		public void SetUp ()
		{
			state = new NewProjectState ();
			state.Panel = Mock.Of<IPanel> ();
			mockLimitationService = new Mock<ILicenseLimitationsService> ();
			App.Current.LicenseLimitationsService = mockLimitationService.Object;
		}

		[Test]
		public async Task LoadState_NoLimited_ListAllDashboardsAndTeams ()
		{
			CountLimitationVM countLimitation = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					Enabled = false,
					Maximum = 3
				}
			};
			mockLimitationService.Setup (ls => ls.CreateBarChartVM (It.IsAny<string> (),-1,null)).Returns (
				new CountLimitationBarChartVM {
					Limitation = countLimitation
				}
			);

			await state.LoadState (null);

			Assert.AreEqual (teams.Count, state.ViewModel.Teams.ViewModels.Count);
			Assert.AreEqual (dashboards.Count, state.ViewModel.Dashboards.ViewModels.Count);
		}

		[Test]
		public async Task LoadState_Limited_ListMaximumDashboardsAndTeamsAllowed ()
		{
			CountLimitationVM countLimitation = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					Enabled = true,
					Maximum = 3
				}
			};
			mockLimitationService.Setup (ls => ls.CreateBarChartVM (It.IsAny<string> (), -1, null)).Returns (
				new CountLimitationBarChartVM {
					Limitation = countLimitation
				}
			);

			await state.LoadState (null);

			Assert.AreEqual (countLimitation.Maximum, state.ViewModel.Teams.ViewModels.Count);
			Assert.AreEqual (countLimitation.Maximum, state.ViewModel.Dashboards.ViewModels.Count);
		}
	}
}
