//
//  Copyright (C) 2018 
using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.Controller;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace Tests.Services
{
	[TestFixture]
	public class TestPlayEditorState
	{
		PlayEditorState state;
		PlayEditorVM viewModel;
		Mock<ILocator<IView>> mockViewLocator;
		Mock<IPanel> mockView;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			SetupClass.SetUp ();
			Scanner.ScanReferencedControllers (App.Current.ControllerLocator);
			mockViewLocator = new Mock<ILocator<IView>> ();
			mockView = new Mock<IPanel> ();
			App.Current.ViewLocator = mockViewLocator.Object;
			mockViewLocator.Setup (vl => vl.Retrieve (PlayEditorState.NAME)).Returns (mockView.Object);
		}

		[SetUp]
		public void SetUp ()
		{
			mockView.ResetCalls ();
			mockViewLocator.ResetCalls ();
			state = new PlayEditorState ();
		}

		[TearDown]
		public async Task TearDown ()
		{
			try {
				await state.HideState ();
			} catch (InvalidOperationException) {
				// Ignore already stopped error.
			}
			try {
				await state.UnloadState ();
			} catch (InvalidOperationException) {
				// Ignore already stopped error.
			}
		}

		[Test]
		public async Task LoadState_ViewModelCreated ()
		{
			// Arrange
			dynamic data = new ExpandoObject ();
			data.project = new LMProjectVM {
				Model = new LMProject ()
			};
			data.settings = new PlayEventEditionSettings ();
			data.play = new LMTimelineEventVM { Model = new LMTimelineEvent { Name = "Event1" } };

			// Act
			await state.LoadState (data);

			// Assert
			Assert.AreEqual (data.play, state.ViewModel.Play);
			Assert.AreEqual (2, state.Controllers.Count);
			Assert.That (state.Controllers.Any (c => c.GetType () == typeof (LMTeamTaggerController)));
			Assert.That (state.Controllers.Any (c => c.GetType () == typeof (PlayEditorController)));
			mockViewLocator.Verify (vl => vl.Retrieve (PlayEditorState.NAME), Times.Once ());
			mockView.Verify (v => v.SetViewModel (It.IsAny<PlayEditorVM> ()), Times.Once ());
		}

		[Test]
		public async Task ClickPlayer_FirstHomePlayer_PlayerTagged ()
		{
			// Arrange
			await SetUpState ();
			LMPlayerVM clickedPlayer = (LMPlayerVM)viewModel.TeamTagger.HomeTeam.First ();

			// Act
			viewModel.TeamTagger.PlayerClick (clickedPlayer, ButtonModifier.None);

			// Assert
			Assert.AreEqual (1, viewModel.TeamTagger.HomeTeam.Selection.Count ());
			Assert.AreEqual (clickedPlayer, viewModel.TeamTagger.HomeTeam.Selection.First ());
			Assert.IsTrue (clickedPlayer.Tagged);
		}

		[Test]
		public async Task ClickPlayer_AlreadyTagged_PlayerUntagged ()
		{
			// Arrange
			await SetUpState ();
			LMPlayerVM clickedPlayer = (LMPlayerVM)viewModel.TeamTagger.HomeTeam.First ();
			viewModel.TeamTagger.PlayerClick (clickedPlayer, ButtonModifier.None);

			// Act
			viewModel.TeamTagger.PlayerClick (clickedPlayer, ButtonModifier.None);

			// Assert
			Assert.IsFalse (viewModel.TeamTagger.HomeTeam.Selection.Any ());
			Assert.IsFalse (clickedPlayer.Tagged);
		}

		async Task SetUpState ()
		{
			dynamic data = new ExpandoObject ();
			var projectVM = new LMProjectVM {
				Model = new LMProject ()
			};
			projectVM.Model.LocalTeamTemplate = LMTeam.DefaultTemplate (10);
			projectVM.Model.VisitorTeamTemplate = LMTeam.DefaultTemplate (10);

			data.project = projectVM;
			data.settings = new PlayEventEditionSettings ();
			data.play = new LMTimelineEventVM { Model = new LMTimelineEvent { Name = "Event1" } };

			await state.LoadState (data);
			await state.ShowState ();

			viewModel = state.ViewModel;
		}
	}
}
