//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Common;
using LongoMatch.License;
using LongoMatch.Services.Controller;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core.Events;
using VAS.Core.Interfaces.License;

namespace Tests.Controller
{
	[TestFixture]
	public class TestHomeController
	{
		HomeController controller;
		HomeViewModel viewModel;
		ILicenseManager currentManager;
		LMDummyWibuManager wibuManager;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			currentManager = App.Current.LicenseManager;
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			App.Current.LicenseManager = currentManager;
		}

		[SetUp]
		public void SetUp ()
		{
			viewModel = new HomeViewModel ();
			controller = new HomeController ();
			controller.SetViewModel (viewModel);
		}


		[Test]
		public async Task HomeController_StartController_SetsCorrectIcon ()
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.STARTER_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			await controller.Start ();
			Assert.AreEqual (Constants.LOGO_STARTER_ICON, App.Current.SoftwareIconName);
		}

		[Test]
		public async Task HomeController_LicenseChangeEvent_ControllerChangesIcon ()
		{
			wibuManager = new LMDummyWibuManager ("");
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			await controller.Start ();
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.PRO_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();

			Assert.AreEqual (Constants.LOGO_ICON, App.Current.SoftwareIconName);
			await App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			Assert.AreEqual (Constants.LOGO_PRO_ICON, App.Current.SoftwareIconName);
		}
	}
}
