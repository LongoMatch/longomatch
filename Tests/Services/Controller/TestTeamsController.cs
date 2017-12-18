//
//  Copyright (C) 2017 Fluendo S.A.
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluVAS.License;
using LongoMatch;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store.Templates;
using LongoMatch.License;
using LongoMatch.Services.Controller;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.License;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using VAS.Tests;


namespace Tests.Services.Controller
{
	[TestFixture]
	public class TestTeamsController
	{
		LMDummyWibuManager wibuManager;
		Mock<IGUIToolkit> mockGuiToolkit;
		Mock<ILicenseLimitationsService> mockLimitationService;
		Mock<IDialogs> mockDialogs;
		Mock<ITeamTemplatesProvider> mockProvider;
		TeamsController controller;
		CountLimitationVM countLimitationVM;

		[OneTimeSetUp]
		public void FixtureSetUp ()
		{
			WibuInit.InitDependencies ();

			DummyTeam team = new DummyTeam { Name = "Team 1" };

			mockGuiToolkit = new Mock<IGUIToolkit> ();
			mockDialogs = new Mock<IDialogs> ();
			mockProvider = new Mock<ITeamTemplatesProvider> ();

			mockGuiToolkit.Setup (x => x.CreateNewTemplate<Team> (It.IsAny<IList<Team>> (),
																	   It.IsAny<string> (),
																	   It.IsAny<string> (),
																	   It.IsAny<string> (),
																	   It.IsAny<CreateEvent<Team>> ()
																	  )).ReturnsAsync (true);
			mockDialogs.Setup (x => x.OpenFile (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
												It.IsAny<string> (), It.IsAny<string []> ())).Returns ("Team.ltt");
			mockDialogs.Setup (m => m.QueryMessage (It.IsAny<string> (), null, It.IsAny<string> (),
													 It.IsAny<object> ())).Returns (AsyncHelpers.Return ("team_copy"));
			mockDialogs.Setup (x => x.QuestionMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()))
					   .ReturnsAsync (true);
			mockProvider.Setup (x => x.LoadFile (It.IsAny<string> ())).Returns (team);
			mockProvider.Setup (x => x.Templates).Returns (new List<Team> ());

			App.Current.GUIToolkit = mockGuiToolkit.Object;
			App.Current.Dialogs = mockDialogs.Object;
			App.Current.TeamTemplatesProvider = mockProvider.Object;

			mockLimitationService = new Mock<ILicenseLimitationsService> ();
			countLimitationVM = new CountLimitationVM () {
				Model = new CountLicenseLimitation {
					RegisterName = LongoMatchCountLimitedObjects.Team.ToString (),
					Enabled = true,
					Maximum = 2
				}
			};
			mockLimitationService.Setup (x => x.Get<CountLimitationVM> (It.IsAny<string> ())).Returns (countLimitationVM);
			App.Current.LicenseLimitationsService = mockLimitationService.Object;

			controller = new TeamsController ();
		}

		[SetUp]
		public async Task TestInit ()
		{
			await controller.Start ();
			controller.SetViewModel (new DummyTeamManagerVM (new TeamVM ()));
		}

		[TearDown]
		public async Task TestEnd ()
		{
			await controller.Stop ();
			mockLimitationService.ResetCalls ();
			mockProvider.ResetCalls ();
		}

		[Test ()]
		public async Task SaveStaticTeam_LimitationDisabled_CanSaveCopy ()
		{
			// Arrange
			mockLimitationService.Setup (s => s.CanExecute (LongoMatchCountLimitedObjects.Team.ToString ())).Returns (true);

			// Action
			LMTeam team = new LMTeam { Static = true };
			await App.Current.EventsBroker.Publish (new UpdateEvent<Team> {
				Object = team
			});

			// Assert
			mockLimitationService.Verify (s => s.MoveToUpgradeDialog (LongoMatchCountLimitedObjects.Team.ToString ()), Times.Never);
			mockProvider.Verify (m => m.Save (It.IsAny<Team> ()), Times.Once);
		}

		[Test ()]
		public async Task SaveStaticTeam_LimitationEnabled_CannotSaveCopy ()
		{
			// Arrange
			mockLimitationService.Setup (s => s.CanExecute (LongoMatchCountLimitedObjects.Team.ToString ())).Returns (false);

			// Action
			LMTeam team = new LMTeam { Static = true };
			await App.Current.EventsBroker.Publish (new UpdateEvent<Team> {
				Object = team
			});

			// Assert
			mockLimitationService.Verify (s => s.MoveToUpgradeDialog (LongoMatchCountLimitedObjects.Team.ToString ()), Times.Once);
		}

		[TestCase (LMDummyWibuManager.BASIC_PRODUCT_TEXT)]
		[TestCase (LMDummyWibuManager.STARTER_PRODUCT_TEXT)]
		[TestCase (LMDummyWibuManager.PRO_PRODUCT_TEXT)]
		public async Task NewTeam_TwoStored_CanAdd (string version)
		{
			// Arrange
			await SetWibuManager (version, 2, true);

			// Action
			var t1 = new CreateEvent<Team> { Name = "Team 1" };
			await App.Current.EventsBroker.Publish (t1);

			// Assert
			Assert.IsTrue (t1.ReturnValue);
		}

		[TestCase (LMDummyWibuManager.BASIC_PRODUCT_TEXT)]
		public async Task NewTeam_FourStored_CannotAdd (string version)
		{
			// Arrange
			await SetWibuManager (version, 4, false);

			// Action
			var t1 = new CreateEvent<Team> { Name = "Team 1" };
			await App.Current.EventsBroker.Publish (t1);

			// Assert
			Assert.IsFalse (t1.ReturnValue);
		}

		[TestCase (LMDummyWibuManager.STARTER_PRODUCT_TEXT)]
		[TestCase (LMDummyWibuManager.PRO_PRODUCT_TEXT)]
		public async Task NewTeam_FourStored_CanAdd (string version)
		{
			// Arrange
			await SetWibuManager (version, 4, true);

			// Action
			var t1 = new CreateEvent<Team> { Name = "Team 1" };
			await App.Current.EventsBroker.Publish (t1);

			// Assert
			Assert.IsTrue (t1.ReturnValue);
		}

		[TestCase (LMDummyWibuManager.BASIC_PRODUCT_TEXT)]
		[TestCase (LMDummyWibuManager.STARTER_PRODUCT_TEXT)]
		[TestCase (LMDummyWibuManager.PRO_PRODUCT_TEXT)]
		public async Task ImportTeam_TwoStored_CanAdd (string version)
		{
			// Arrange
			await SetWibuManager (version, 2, true);

			// Action
			var t1 = new ImportEvent<Team> ();
			await App.Current.EventsBroker.Publish (t1);

			// Assert
			Assert.IsTrue (t1.ReturnValue);
		}

		[TestCase (LMDummyWibuManager.BASIC_PRODUCT_TEXT)]
		public async Task ImportTeam_FourStored_CannotAdd (string version)
		{
			// Arrange
			await SetWibuManager (version, 4, false);

			// Action
			var t1 = new ImportEvent<Team> ();
			await App.Current.EventsBroker.Publish (t1);

			// Assert
			Assert.IsFalse (t1.ReturnValue);
		}

		[TestCase (LMDummyWibuManager.STARTER_PRODUCT_TEXT)]
		[TestCase (LMDummyWibuManager.PRO_PRODUCT_TEXT)]
		public async Task ImportTeam_FourStored_CanAdd (string version)
		{
			// Arrange
			await SetWibuManager (version, 4, true);

			// Action
			var t1 = new ImportEvent<Team> ();
			await App.Current.EventsBroker.Publish (t1);

			// Assert
			Assert.IsTrue (t1.ReturnValue);
		}

		async Task SetWibuManager (string version, int existingTeams, bool canExecute)
		{
			wibuManager = new LMDummyWibuManager (version);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();

			// Exclude the static teams
			countLimitationVM.Count = existingTeams - 2;
			mockLimitationService.Setup (x => x.CanExecute (It.IsAny<string> ())).Returns (canExecute);
		}
	}
}
