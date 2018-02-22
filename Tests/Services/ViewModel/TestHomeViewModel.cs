//
//  Copyright (C) 2018 Fluendo S.A.
using System;
using LongoMatch;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Services;

namespace Tests.Services.ViewModel
{
	[TestFixture]
	public class TestHomeViewModel
	{
		HomeViewModel viewModel;
		Mock<IStateController> mockStateController;

		[SetUp]
		public void SetUp ()
		{
			viewModel = new HomeViewModel ();
			mockStateController = new Mock<IStateController> ();
			App.Current.StateController = mockStateController.Object;
		}

		[Test]
		public void HomeViewModel_ExecutePreferences_MoveToPreferences ()
		{
			viewModel.PreferencesCommand.Execute ();

			mockStateController.Verify (sc => sc.MoveTo (PreferencesState.NAME, null, false, false), Times.Once);
		}
	}
}
