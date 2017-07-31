//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Common;
using LongoMatch.License;
using LongoMatch.Services;
using Moq;
using NUnit.Framework;
using VAS.Core.Events;
using VAS.Core.Interfaces.License;
using VAS.Core.ViewModel;

namespace Tests.Services
{
	[TestFixture]
	public class TestLMLicenseLimitationsService
	{
		LMLicenseLimitationsService service;
		ILicenseManager currentLicenseManager;
		LMDummyWibuManager wibuManager;

		[TestFixtureSetUp]
		public void TestFixtureSetUp ()
		{
			currentLicenseManager = App.Current.LicenseManager;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown ()
		{
			App.Current.LicenseManager = currentLicenseManager;
		}

		[Test]
		public async Task LMLicenseLimitationsService_ProPanInitialized_DatabaseManagerLimitationDisabled ()
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.PRO_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			var featureLimitation = service.Get<FeatureLimitationVM> (LongoMatchFeature.DatabaseManager.ToString ());

			Assert.IsNotNull (featureLimitation);
			Assert.AreEqual (LongoMatchFeature.DatabaseManager.ToString (), featureLimitation.RegisterName);
			Assert.IsFalse (featureLimitation.Enabled);
		}

		[Test]
		public async Task LMLicenseLimitationsService_StarterPanInitialized_DatabaseManagerLimitationDisabled ()
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.STARTER_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			var featureLimitation = service.Get<FeatureLimitationVM> (LongoMatchFeature.DatabaseManager.ToString ());

			Assert.IsNotNull (featureLimitation);
			Assert.AreEqual (LongoMatchFeature.DatabaseManager.ToString (), featureLimitation.RegisterName);
			Assert.IsFalse (featureLimitation.Enabled);
		}

		[Test]
		public async Task LMLicenseLimitationsService_BasicPlanInitialized_DatabaseManagerLimitationEnabled ()
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.BASIC_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			var featureLimitation = service.Get<FeatureLimitationVM> (LongoMatchFeature.DatabaseManager.ToString ());

			Assert.IsNotNull (featureLimitation);
			Assert.AreEqual (LongoMatchFeature.DatabaseManager.ToString (), featureLimitation.RegisterName);
			Assert.IsTrue (featureLimitation.Enabled);
		}

		[Test]
		public async Task LMLicenseLimitationsService_LicenseChangeEventPro_DatabaseManagerLimitationDisabled ()
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.BASIC_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			service.Start ();
			var featureLimitation = service.Get<FeatureLimitationVM> (LongoMatchFeature.DatabaseManager.ToString ());
			Assert.IsTrue (featureLimitation.Enabled);

			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.PRO_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			await App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			Assert.IsFalse (featureLimitation.Enabled);
			service.Stop ();
		}

		[Test]
		public async Task LMLicenseLimitationsService_LicenseChangeEventStarter_DatabaseManagerLimitationDisabled ()
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.BASIC_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			service.Start ();
			var featureLimitation = service.Get<FeatureLimitationVM> (LongoMatchFeature.DatabaseManager.ToString ());
			Assert.IsTrue (featureLimitation.Enabled);

			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.STARTER_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			await App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			Assert.IsFalse (featureLimitation.Enabled);
			service.Stop ();
		}

		[Test]
		public async Task LMLicenseLimitationsService_LicenseChangeEventBasic_DatabaseManagerLimitationDisabled ()
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.PRO_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			service.Start ();
			var featureLimitation = service.Get<FeatureLimitationVM> (LongoMatchFeature.DatabaseManager.ToString ());
			Assert.IsFalse (featureLimitation.Enabled);

			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.BASIC_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			await App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			Assert.IsTrue (featureLimitation.Enabled);
			service.Stop ();
		}
	}
}
