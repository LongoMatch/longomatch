//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Common;
using LongoMatch.License;
using LongoMatch.Services;
using Moq;
using NUnit.Framework;
using VAS.Core.Events;
using VAS.Core.Interfaces;
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
		static IEnumerable<string> featureList = Enum.GetValues (typeof (LongoMatchFeature)).Cast<LongoMatchFeature> ().Select (e => e.ToString ()).ToList ();
		//static IEnumerable<string> countList = Enum.GetValues (typeof (LongoMatchCountLimitedObjects)).Cast<LongoMatchCountLimitedObjects> ().Select (e => e.ToString ()).ToList ();

		[TestFixtureSetUp]
		public void TestFixtureSetUp ()
		{
			currentLicenseManager = App.Current.LicenseManager;

			var dbManager = new Mock<IStorageManager> ();
			var activeDbMock = new Mock<IStorage> ();
			dbManager.Setup (x => x.ActiveDB).Returns (activeDbMock.Object);
			App.Current.DatabaseManager = dbManager.Object;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown ()
		{
			App.Current.LicenseManager = currentLicenseManager;
		}

		static IEnumerable<string> GetAllLimitations ()
		{
			// FIXME: When we start adding limitations to starter
			// 		  this should be splitted in 3 methods (one for each plan) that query the actual WibuManager
			foreach (var limitation in featureList/*.Union (countList)*/) {
				yield return limitation;
			}
		}

		[Test, Combinatorial]
		public async Task LMLicenseLimitationsService_ProPlanInitialized_AllLimitationsDisabled (
			[Values (LMDummyWibuManager.PRO_PRODUCT_TEXT, LMDummyWibuManager.STARTER_PRODUCT_TEXT)] string productText,
			[ValueSource ("GetAllLimitations")]string limitationName)
		{
			wibuManager = new LMDummyWibuManager (productText);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			var limitation = service.Get<LimitationVM> (limitationName);

			Assert.IsNotNull (limitation);
			Assert.AreEqual (limitationName, limitation.RegisterName);
			Assert.IsFalse (limitation.Enabled);
		}

		[Test, Combinatorial]
		public async Task LMLicenseLimitationsService_BasicPlanInitialized_AllLimitationsEnabled (
			[Values (LMDummyWibuManager.BASIC_PRODUCT_TEXT)] string productText,
			[ValueSource ("GetAllLimitations")]string limitationName)
		{
			wibuManager = new LMDummyWibuManager (productText);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			var limitation = service.Get<LimitationVM> (limitationName);

			Assert.IsNotNull (limitation);
			Assert.AreEqual (limitationName, limitation.RegisterName);
			Assert.IsTrue (limitation.Enabled);
		}

		[Test]
		public async Task LMLicenseLimitationsService_BasicPlanInitialized_ExcelExportLimitationEnabled ()
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.BASIC_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			var featureLimitation = service.Get<FeatureLimitationVM> (LongoMatchFeature.ExcelExport.ToString ());

			Assert.IsNotNull (featureLimitation);
			Assert.AreEqual (LongoMatchFeature.ExcelExport.ToString (), featureLimitation.RegisterName);
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
		public async Task LMLicenseLimitationsService_LicenseChangeEventPro_ConversionLimitationDisabled ()
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.BASIC_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			service.Start ();
			var featureLimitation = service.Get<FeatureLimitationVM> (LongoMatchFeature.VideoConverter.ToString ());
			Assert.IsTrue (featureLimitation.Enabled);

			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.PRO_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			await App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			Assert.IsFalse (featureLimitation.Enabled);
			service.Stop ();
		}

		[Test]
		public async Task LMLicenseLimitationsService_LicenseChangeEventPro_ExcelExportLimitationDisabled ()
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.BASIC_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			service.Start ();
			var featureLimitation = service.Get<FeatureLimitationVM> (LongoMatchFeature.ExcelExport.ToString ());
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
		public async Task LMLicenseLimitationsService_LicenseChangeEventStarter_VideoConverterLimitationDisabled ()
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.BASIC_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			service.Start ();
			var featureLimitation = service.Get<FeatureLimitationVM> (LongoMatchFeature.VideoConverter.ToString ());
			Assert.IsTrue (featureLimitation.Enabled);

			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.STARTER_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			await App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			Assert.IsFalse (featureLimitation.Enabled);
			service.Stop ();
		}

		[Test]
		public async Task LMLicenseLimitationsService_LicenseChangeEventStarter_ExportExcelLimitationEnabled ()
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.PRO_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			service.Start ();
			var featureLimitation = service.Get<FeatureLimitationVM> (LongoMatchFeature.ExcelExport.ToString ());
			Assert.IsFalse (featureLimitation.Enabled);

			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.STARTER_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			await App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			Assert.IsTrue (featureLimitation.Enabled);
			service.Stop ();
		}

		[Test]
		public async Task LMLicenseLimitationsService_LicenseChangeEventBasic_DatabaseManagerLimitationEnabled ()
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

		[Test]
		public async Task LMLicenseLimitationsService_LicenseChangeEventBasic_ConversionLimitationEnabled ()
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.PRO_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			service.Start ();
			var featureLimitation = service.Get<FeatureLimitationVM> (LongoMatchFeature.VideoConverter.ToString ());
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
