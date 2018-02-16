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
using LongoMatch.Core.ViewModel;
using LongoMatch.License;
using LongoMatch.Services.Controller;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.License;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using DashboardDummy = VAS.Tests.Utils.DashboardDummy;

namespace Tests.Services.Controller
{
	public class TestDashboardsController
	{
		LMDummyWibuManager wibuManager;
		Mock<IGUIToolkit> mockGuiToolkit;
		Mock<ILicenseLimitationsService> mockLimitationService;
		Mock<IDialogs> mockDialogs;
		Mock<ICategoriesTemplatesProvider> mockProvider;
		DashboardsController controller;
		CountLimitationVM countLimitationVM;

		[OneTimeSetUp]
		public void FixtureSetUp ()
		{
			WibuInit.InitDependencies ();

			DashboardDummy dashboard = new DashboardDummy { Name = "Dashboard 1" };

			mockGuiToolkit = new Mock<IGUIToolkit> ();
			mockDialogs = new Mock<IDialogs> ();
			mockProvider = new Mock<ICategoriesTemplatesProvider> ();

			mockGuiToolkit.SetupGet (x => x.DeviceScaleFactor).Returns (1.0f);
			mockGuiToolkit.Setup (x => x.CreateNewTemplate<Dashboard> (It.IsAny<IList<Dashboard>> (),
																	   It.IsAny<string> (),
																	   It.IsAny<string> (),
																	   It.IsAny<string> (),
																	   It.IsAny<CreateEvent<Dashboard>> ()
																	  )).ReturnsAsync (true);
			mockDialogs.Setup (x => x.OpenFile (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
												It.IsAny<string> (), It.IsAny<string []> ())).Returns ("Dashboard.lct");
			mockDialogs.Setup (x => x.QuestionMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()))
					   .ReturnsAsync (true);
			mockDialogs.Setup (m => m.QueryMessage (It.IsAny<string> (), null, It.IsAny<string> (),
													 It.IsAny<object> ())).Returns (AsyncHelpers.Return ("dashboard_copy"));
			mockProvider.Setup (x => x.LoadFile (It.IsAny<string> ())).Returns (dashboard);
			mockProvider.Setup (x => x.Templates).Returns (new List<Dashboard> { dashboard });

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

			App.Current.GUIToolkit = mockGuiToolkit.Object;
			App.Current.Dialogs = mockDialogs.Object;
			App.Current.CategoriesTemplatesProvider = mockProvider.Object;

			controller = new DashboardsController ();
		}

		[SetUp]
		public async Task TestInit ()
		{
			await controller.Start ();
			controller.SetViewModel (new DashboardsManagerVM ());
		}

		[TearDown]
		public async Task TestEnd ()
		{
			await controller.Stop ();
			mockLimitationService.ResetCalls ();
			mockProvider.ResetCalls ();
		}

		[TestCase (LMDummyWibuManager.BASIC_PRODUCT_TEXT)]
		[TestCase (LMDummyWibuManager.STARTER_PRODUCT_TEXT)]
		[TestCase (LMDummyWibuManager.PRO_PRODUCT_TEXT)]
		public async Task NewDashboard_OneStored_CanAdd (string version)
		{
			// Arrange
			await SetWibuManager (version, 1, true);

			// Action
			var d1 = new CreateEvent<Dashboard> { Name = "Dashboard 1" };
			await App.Current.EventsBroker.Publish (d1);

			// Assert
			Assert.IsTrue (d1.ReturnValue);
		}

		[Test ()]
		public async Task SaveStaticDashboard_LimitationDisabled_CanSaveCopy ()
		{
			// Arrange
			mockLimitationService.Setup (s => s.CanExecute (LongoMatchCountLimitedObjects.Dashboard.ToString ())).Returns (true);

			// Action
			int i = App.Current.CategoriesTemplatesProvider.Templates.Count;
			LMDashboard dashboard = new LMDashboard { Static = true };
			await App.Current.EventsBroker.Publish (new UpdateEvent<Dashboard> {
				Object = dashboard
			});
			i = App.Current.CategoriesTemplatesProvider.Templates.Count;
			// Assert
			mockLimitationService.Verify (s => s.MoveToUpgradeDialog (LongoMatchCountLimitedObjects.Dashboard.ToString ()), Times.Never);
			mockProvider.Verify (m => m.Save (It.IsAny<Dashboard> ()), Times.Once);
		}

		[TestCase (LMDummyWibuManager.BASIC_PRODUCT_TEXT)]
		public async Task NewDashboard_TwoStored_CannotAdd (string version)
		{
			// Arrange
			await SetWibuManager (version, 2, false);

			// Action
			var d1 = new CreateEvent<Dashboard> { Name = "Dashboard 1" };
			await App.Current.EventsBroker.Publish (d1);

			// Assert
			Assert.IsFalse (d1.ReturnValue);
		}

		[Test ()]
		public async Task SaveStaticDashboard_LimitationEnabled_CannotSaveCopy ()
		{
			// Arrange
			mockLimitationService.Setup (s => s.CanExecute (LongoMatchCountLimitedObjects.Dashboard.ToString ())).Returns (false);

			// Action
			LMDashboard dashboard = new LMDashboard { Static = true };
			await App.Current.EventsBroker.Publish (new UpdateEvent<Dashboard> {
				Object = dashboard
			});

			// Assert
			mockLimitationService.Verify (s => s.MoveToUpgradeDialog (LongoMatchCountLimitedObjects.Dashboard.ToString ()), Times.Once);
		}

		[TestCase (LMDummyWibuManager.STARTER_PRODUCT_TEXT)]
		[TestCase (LMDummyWibuManager.PRO_PRODUCT_TEXT)]
		public async Task NewDashboard_TwoStored_CanAdd (string version)
		{
			// Arrange
			await SetWibuManager (version, 2, true);

			// Action
			var d1 = new CreateEvent<Dashboard> { Name = "Dashboard 1" };
			await App.Current.EventsBroker.Publish (d1);

			// Assert
			Assert.IsTrue (d1.ReturnValue);
		}

		[TestCase (LMDummyWibuManager.BASIC_PRODUCT_TEXT)]
		[TestCase (LMDummyWibuManager.STARTER_PRODUCT_TEXT)]
		[TestCase (LMDummyWibuManager.PRO_PRODUCT_TEXT)]
		public async Task ImportDashboard_OneStored_CanAdd (string version)
		{
			// Arrange
			await SetWibuManager (version, 1, true);

			// Action
			var d1 = new ImportEvent<Dashboard> ();
			await App.Current.EventsBroker.Publish (d1);

			// Assert
			Assert.IsTrue (d1.ReturnValue);
		}

		[TestCase (LMDummyWibuManager.BASIC_PRODUCT_TEXT)]
		public async Task ImportDashboard_TwoStored_CannotAdd (string version)
		{
			// Arrange
			await SetWibuManager (version, 2, false);

			// Action
			var d1 = new ImportEvent<Dashboard> ();
			await App.Current.EventsBroker.Publish (d1);

			// Assert
			Assert.IsFalse (d1.ReturnValue);
		}

		[TestCase (LMDummyWibuManager.STARTER_PRODUCT_TEXT)]
		[TestCase (LMDummyWibuManager.PRO_PRODUCT_TEXT)]
		public async Task ImportDashboard_TwoStored_CanAdd (string version)
		{
			// Arrange
			await SetWibuManager (version, 2, true);

			// Action
			var d1 = new ImportEvent<Dashboard> ();
			await App.Current.EventsBroker.Publish (d1);

			// Assert
			Assert.IsTrue (d1.ReturnValue);
		}

		async Task SetWibuManager (string version, int existingDashboards, bool canExecute)
		{
			wibuManager = new LMDummyWibuManager (version);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();

			// Exclude the system dashboard
			countLimitationVM.Count = existingDashboards - 1;
			mockLimitationService.Setup (x => x.CanExecute (It.IsAny<string> ())).Returns (canExecute);
		}
	}
}
