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
using VAS.Core.Common;
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

		static IEnumerable<string> featureList = Enum.GetValues (typeof (LongoMatchFeature)).Cast<LongoMatchFeature> ().Select (e => e.ToString ())
													 .Concat (Enum.GetValues (typeof (VASFeature)).Cast<VASFeature> ().Select (e => e.ToString ())).ToList ();
		static IEnumerable<string> countList = Enum.GetValues (typeof (LongoMatchCountLimitedObjects)).Cast<LongoMatchCountLimitedObjects> ().Select (e => e.ToString ()).ToList ();

		static IEnumerable<string> basicLimitations = new List<string> { };
		static IEnumerable<string> starterLimitations = new List<string> {
			LongoMatchFeature.ExcelExport.ToString (),
			LongoMatchFeature.XMlImportExport.ToString (),
			VASFeature.Zoom.ToString(),
			VASFeature.OpenMultiCamera.ToString(),
			VASFeature.CreateMultiCamera.ToString()
		};

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
			foreach (var limitation in featureList.Union (countList)) {
				yield return limitation;
			}
		}

		static IEnumerable<string> GetStarterLimitations ()
		{
			foreach (var limitation in starterLimitations) {
				yield return limitation;
			}
		}

		static IEnumerable<string> GetBasicLimitations ()
		{
			basicLimitations = GetAllLimitations ().Except (GetStarterLimitations ());
			foreach (var limitation in basicLimitations) {
				yield return limitation;
			}
		}

		[Test, Combinatorial]
		public async Task LMLicenseLimitationsService_ProPlanInitialized_AllLimitationsDisabled (
			[Values (LMDummyWibuManager.PRO_PRODUCT_TEXT)] string productText,
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
		public async Task LMLicenseLimitationsService_StarterPlanInitialized_StarterLimitationsEnabled (
			[Values (LMDummyWibuManager.STARTER_PRODUCT_TEXT)] string productText,
			[ValueSource ("GetStarterLimitations")]string limitationName)
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

		[Test, Combinatorial]
		public async Task LMLicenseLimitationsService_StarterPlanInitialized_BasicLimitationsDisabled (
			[Values (LMDummyWibuManager.STARTER_PRODUCT_TEXT)] string productText,
			[ValueSource ("GetBasicLimitations")]string limitationName)
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

		[Test, Combinatorial]
		public async Task LMLicenseLimitationsService_LicenseChangeEventProToBasic_AllLimitationsEnabled (
			[ValueSource ("GetAllLimitations")]string limitationName)
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.PRO_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			service.Start ();
			var featureLimitation = service.Get<FeatureLimitationVM> (limitationName);
			Assert.IsFalse (featureLimitation.Enabled);

			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.BASIC_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			await App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			Assert.IsTrue (featureLimitation.Enabled);
			service.Stop ();
		}

		[Test, Combinatorial]
		public async Task LMLicenseLimitationsService_LicenseChangeEventProToStarter_AllStarterLimitationsEnabled (
			[ValueSource ("GetStarterLimitations")]string limitationName)
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.PRO_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			service.Start ();
			var featureLimitation = service.Get<FeatureLimitationVM> (limitationName);
			Assert.IsFalse (featureLimitation.Enabled);

			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.STARTER_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			await App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			Assert.IsTrue (featureLimitation.Enabled);
			service.Stop ();
		}

		[Test, Combinatorial]
		public async Task LMLicenseLimitationsService_LicenseChangeEventProToStarter_AllBasicLimitationsDisabled (
			[ValueSource ("GetBasicLimitations")]string limitationName)
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.PRO_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			service.Start ();
			var featureLimitation = service.Get<FeatureLimitationVM> (limitationName);
			Assert.IsFalse (featureLimitation.Enabled);

			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.STARTER_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			await App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			Assert.IsFalse (featureLimitation.Enabled);
			service.Stop ();
		}

		[Test, Combinatorial]
		public async Task LMLicenseLimitationsService_LicenseChangeEventBasicToPro_AllLimitationsDisabled (
			[ValueSource ("GetAllLimitations")]string limitationName)
		{
			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.BASIC_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			service = new LMLicenseLimitationsService ();
			service.Start ();
			var featureLimitation = service.Get<FeatureLimitationVM> (limitationName);
			Assert.IsTrue (featureLimitation.Enabled);

			wibuManager = new LMDummyWibuManager (LMDummyWibuManager.PRO_PRODUCT_TEXT);
			App.Current.LicenseManager = wibuManager;
			await App.Current.LicenseManager.Init ();
			await App.Current.EventsBroker.Publish (new LicenseChangeEvent ());

			Assert.IsFalse (featureLimitation.Enabled);
			service.Stop ();
		}
	}
}
