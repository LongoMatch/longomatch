//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Dynamic;
using LongoMatch;
using LongoMatch.Core;
using LongoMatch.License;
using LongoMatch.Services.State;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.License;
using VAS.Core.License;
using VAS.Core.ViewModel;

namespace Tests.State
{
	[TestFixture]
	public class TestLMUpgradeLimitationState
	{
		LMUpgradeLimitationState state;
		ILicenseManager currentLicenseManager;

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

		[SetUp]
		public void SetUp ()
		{
			state = new LMUpgradeLimitationState ();
			App.Current.LicenseManager = new LMDummyWibuManager (LMDummyWibuManager.BASIC_PRODUCT_TEXT);
			state.Panel = new Mock<IPanel> ().Object;
		}

		[Test]
		public void StateLoad_FeatureLimitation_ConfiguredWithFeatureName ()
		{
			dynamic properties = new ExpandoObject ();
			var limitation = new FeatureLicenseLimitation ();
			limitation.FeatureName = "test";
			limitation.RegisterName = "register_name";
			properties.limitationVM = new FeatureLimitationVM {
				Model = limitation
			};
			state.LoadState (properties);

			Assert.IsTrue (state.ViewModel.Header.Contains (limitation.FeatureName));
		}

		[Test]
		public void StateLoad_FeatureLimitation_ConfiguredWithCountLimitation ()
		{
			dynamic properties = new ExpandoObject ();
			var limitation = new CountLicenseLimitation ();
			limitation.RegisterName = "register_name";
			properties.limitationVM = new CountLimitationVM {
				Model = limitation
			};
			state.LoadState (properties);

			Assert.IsTrue (state.ViewModel.Header == Catalog.GetString ("Unlock your team's potential with LongoMatch PRO"));
		}
	}
}
