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

		[OneTimeSetUp]
		public void TestFixtureSetUp ()
		{
			currentLicenseManager = App.Current.LicenseManager;
		}

		[OneTimeTearDown]
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
		public void StateLoad_FeatureLimitation_ConfiguredWithDisplayName ()
		{
			dynamic properties = new ExpandoObject ();
			var limitation = new FeatureLicenseLimitation ();
			limitation.DisplayName = "test";
			limitation.RegisterName = "register_name";
			properties.limitationVM = new FeatureLimitationVM {
				Model = limitation
			};
			state.LoadState (properties);

			Assert.IsTrue (state.ViewModel.Header.Contains (limitation.DisplayName));
		}

		[Test]
		public void StateLoad_FeatureLimitationNoDisplay_ConfiguredWithRegisterName ()
		{
			dynamic properties = new ExpandoObject ();
			var limitation = new FeatureLicenseLimitation ();
			limitation.RegisterName = "register_name";
			properties.limitationVM = new FeatureLimitationVM {
				Model = limitation
			};
			state.LoadState (properties);

			Assert.IsTrue (state.ViewModel.Header.Contains (limitation.RegisterName));
		}

		[Test]
		public void StateLoad_CountLimitation_ConfiguredWithDisplayName ()
		{
			dynamic properties = new ExpandoObject ();
			var limitation = new CountLicenseLimitation ();
			limitation.DisplayName = "test";
			limitation.RegisterName = "register_name";
			properties.limitationVM = new CountLimitationVM {
				Model = limitation
			};
			state.LoadState (properties);

			Assert.IsTrue (state.ViewModel.Header.Contains (limitation.DisplayName));
		}

		[Test]
		public void StateLoad_CountLimitationNoDisplay_ConfiguredWithRegisterName ()
		{
			dynamic properties = new ExpandoObject ();
			var limitation = new CountLicenseLimitation ();
			limitation.RegisterName = "register_name";
			properties.limitationVM = new CountLimitationVM {
				Model = limitation
			};
			state.LoadState (properties);

			Assert.IsTrue (state.ViewModel.Header.Contains (limitation.RegisterName));
		}

		[Test]
		public void StateLoad_Limitation_ConfiguredWithGenericMessage ()
		{
			dynamic properties = new ExpandoObject ();
			var limitation = new LicenseLimitation ();
			limitation.DisplayName = "test";
			limitation.RegisterName = "register_name";
			properties.limitationVM = new LimitationVM {
				Model = limitation
			};
			state.LoadState (properties);

			Assert.IsTrue (state.ViewModel.Header == Catalog.GetString ("Unlock your team's potential with LongoMatch PRO"));
		}

		[Test]
		public void StateLoad_NoLimitation_ConfiguredWithGenericMessage ()
		{
			dynamic properties = new ExpandoObject ();
			properties.limitationVM = null;
			state.LoadState (properties);

			Assert.IsTrue (state.ViewModel.Header == Catalog.GetString ("Unlock your team's potential with LongoMatch PRO"));
		}
	}
}
