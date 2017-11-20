//
//  Copyright (C) 2017 Fluendo S.A.
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Events;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services;
using LongoMatch.Services.Controller;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;

namespace Tests.Controller
{
	public class TestLMTeamTaggerController
	{
		LMTeamTaggerController controller;
		LMTaggingController taggingController;
		LMEventsController eventsController;
		LMTeamTaggerVM teamTagger;
		VideoPlayerVM videoPlayer;
		LMProjectVM project;
		LMProjectVM projectVM;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			SetupClass.SetUp ();
			Mock<ILicenseLimitationsService> mockLicenseLimitation = new Mock<ILicenseLimitationsService> ();
			mockLicenseLimitation.Setup (license => license.CanExecute (It.IsAny<string> ())).Returns (true);
			App.Current.LicenseLimitationsService = mockLicenseLimitation.Object;
		}

		async Task ControllerSetUp (IViewModel viewModel)
		{
			controller = new LMTeamTaggerController ();
			controller.SetViewModel (viewModel);
			await controller.Start ();
		}

		async Task SimpleSetUpAsync ()
		{
			var viewModel = new LMDrawingToolVM ();
			teamTagger = viewModel.TeamTagger;
			viewModel.Project = Utils.CreateProject ();
			await ControllerSetUp (viewModel);
		}

		async Task ProjectSetUpAsync ()
		{
			var viewModel = new PlayEditorVM ();
			teamTagger = viewModel.TeamTagger;
			viewModel.Project = new LMProjectVM { Model = Utils.CreateProject () };
			project = viewModel.Project;
			await ControllerSetUp (viewModel);
		}

		async Task AnalysisSetUpAsync ()
		{
			var viewModel = new LMProjectAnalysisVM ();
			viewModel.VideoPlayer = new VideoPlayerVM ();
			videoPlayer = viewModel.VideoPlayer;
			teamTagger = viewModel.TeamTagger;
			projectVM = new LMProjectVM { Model = Utils.CreateProject () };
			viewModel.Project = projectVM;
			await ControllerSetUp (viewModel);
			eventsController = new LMEventsController ();
			eventsController.SetViewModel (viewModel);
			await eventsController.Start ();


			taggingController = new LMTaggingController ();
			taggingController.SetViewModel (new ProjectAnalysisVM<LMProjectVM> { VideoPlayer = videoPlayer, Project = projectVM });
			await taggingController.Start ();
		}

		[TearDown]
		public async Task TearDownAsync ()
		{
			await controller.Stop ();
			eventsController?.Stop ();
			eventsController = null;
			taggingController?.Stop ();
			taggingController = null;
		}

		[Test]
		public async Task TestTagPlayerSingleModeSubstitutionModeFalseAsync ()
		{
			await SimpleSetUpAsync ();
			teamTagger.SelectionMode = MultiSelectionMode.Single;
			teamTagger.SubstitutionMode = false;
			var clickedPlayer1 = teamTagger.HomeTeam.ViewModels.FirstOrDefault () as LMPlayerVM;
			var clickedPlayer2 = teamTagger.HomeTeam.ViewModels [1] as LMPlayerVM;

			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			Assert.IsTrue (clickedPlayer1.Tagged);
			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.Shift);
			Assert.IsTrue (clickedPlayer1.Tagged);
			teamTagger.PlayerClick (clickedPlayer2, ButtonModifier.None);

			Assert.IsFalse (clickedPlayer1.Tagged);
			Assert.IsTrue (clickedPlayer2.Tagged);
			Assert.AreEqual (1, teamTagger.HomeTeam.Selection.Count);
			Assert.AreSame (clickedPlayer2, teamTagger.HomeTeam.Selection.First ());
		}

		[Test]
		public async Task TestTagPlayerMultipleModeSubstitutionModeFalse ()
		{
			await SimpleSetUpAsync ();
			teamTagger.SelectionMode = MultiSelectionMode.Multiple;
			teamTagger.SubstitutionMode = false;
			var clickedPlayer1 = teamTagger.HomeTeam.ViewModels.FirstOrDefault () as LMPlayerVM;
			var clickedPlayer2 = teamTagger.HomeTeam.ViewModels [1] as LMPlayerVM;

			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			Assert.IsTrue (clickedPlayer1.Tagged);
			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			Assert.IsFalse (clickedPlayer1.Tagged);
			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			teamTagger.PlayerClick (clickedPlayer2, ButtonModifier.None);

			Assert.IsTrue (clickedPlayer1.Tagged);
			Assert.IsTrue (clickedPlayer2.Tagged);
			Assert.AreEqual (2, teamTagger.HomeTeam.Selection.Count);
			Assert.AreSame (clickedPlayer1, teamTagger.HomeTeam.Selection.First ());
			Assert.AreSame (clickedPlayer2, teamTagger.HomeTeam.Selection.ElementAt (1));
		}

		[Test]
		public async Task TestTagPlayerMultipleWithModifierModeSubstitutionModeFalse ()
		{
			await SimpleSetUpAsync ();
			teamTagger.SelectionMode = MultiSelectionMode.MultipleWithModifier;
			teamTagger.SubstitutionMode = false;
			var clickedPlayer1 = teamTagger.HomeTeam.ViewModels.FirstOrDefault () as LMPlayerVM;
			var clickedPlayer2 = teamTagger.HomeTeam.ViewModels [1] as LMPlayerVM;

			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			Assert.IsTrue (clickedPlayer1.Tagged);
			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.Shift);
			Assert.IsFalse (clickedPlayer1.Tagged);
			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			Assert.IsTrue (clickedPlayer1.Tagged);
			teamTagger.PlayerClick (clickedPlayer2, ButtonModifier.None);
			Assert.IsFalse (clickedPlayer1.Tagged);
			Assert.IsTrue (clickedPlayer2.Tagged);
			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.Shift);

			Assert.IsTrue (clickedPlayer1.Tagged);
			Assert.IsTrue (clickedPlayer2.Tagged);
			Assert.AreEqual (2, teamTagger.HomeTeam.Selection.Count);
			Assert.AreSame (clickedPlayer2, teamTagger.HomeTeam.Selection.First ());
			Assert.AreSame (clickedPlayer1, teamTagger.HomeTeam.Selection.ElementAt (1));
		}

		[Test]
		public async Task TestSubstitutionsNoAnalysisVM ()
		{
			bool emitedPropertyChange = false;
			int times = 0;
			await SimpleSetUpAsync ();
			teamTagger.SubstitutionMode = true;
			var clickedPlayer1 = teamTagger.HomeTeam.FieldPlayersList.FirstOrDefault ();
			var clickedPlayer2 = teamTagger.HomeTeam.BenchPlayersList.FirstOrDefault ();
			teamTagger.PropertyChanged += (sender, e) => {
				if (sender == teamTagger.HomeTeam && e.PropertyName == nameof (teamTagger.HomeTeam.FieldPlayersList)) {
					emitedPropertyChange = true;
					times++;
				}
			};

			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			Assert.IsTrue (clickedPlayer1.Tagged);
			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			Assert.IsFalse (clickedPlayer1.Tagged);

			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			teamTagger.PlayerClick (clickedPlayer2, ButtonModifier.None);

			Assert.IsFalse (clickedPlayer1.Tagged);
			Assert.IsFalse (clickedPlayer2.Tagged);
			Assert.IsFalse (teamTagger.HomeTeam.Selection.Any ());
			Assert.AreSame (clickedPlayer2, teamTagger.HomeTeam.ViewModels.First ());
			Assert.AreSame (clickedPlayer1, teamTagger.HomeTeam.ViewModels.Last ());
			Assert.AreSame (clickedPlayer2, teamTagger.HomeTeam.FieldPlayersList.First ());
			Assert.AreSame (clickedPlayer1, teamTagger.HomeTeam.BenchPlayersList.First ());
			Assert.IsTrue (emitedPropertyChange);
			Assert.AreEqual (1, times);
		}

		[Test]
		public async Task TestSubstitutionsIsNotEmmitedClickSamePlayer ()
		{
			bool substEventEmitted = false;
			bool emitedPropertyChange = false;
			int times = 0;
			await AnalysisSetUpAsync ();
			teamTagger.SubstitutionMode = true;
			var clickedPlayer1 = teamTagger.HomeTeam.FieldPlayersList.FirstOrDefault ();
			App.Current.EventsBroker.Subscribe<PlayerSubstitutionEvent> ((ev) => substEventEmitted = true);
			teamTagger.PropertyChanged += (sender, e) => {
				if (sender == teamTagger.HomeTeam && e.PropertyName == nameof (teamTagger.HomeTeam.FieldPlayersList)) {
					emitedPropertyChange = true;
					times++;
				}
			};

			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);

			Assert.IsFalse (clickedPlayer1.Tagged);
			Assert.IsFalse (teamTagger.HomeTeam.Selection.Any ());
			Assert.IsFalse (emitedPropertyChange);
			Assert.IsFalse (substEventEmitted);
			Assert.AreEqual (0, times);
		}

		[Test]
		public async Task Substitution_IsEmitted_WithAbsoluteCurrentTime ()
		{
			bool substEventEmitted = false;
			bool emitedPropertyChange = false;
			Time substTime = new Time (0);
			int times = 0;
			await AnalysisSetUpAsync ();
			teamTagger.SubstitutionMode = true;
			videoPlayer.AbsoluteCurrentTime = new Time (20000);
			videoPlayer.CurrentTime = new Time (10000);
			var clickedPlayer1 = teamTagger.HomeTeam.FieldPlayersList.FirstOrDefault ();
			var clickedPlayer2 = teamTagger.HomeTeam.BenchPlayersList.FirstOrDefault ();
			App.Current.EventsBroker.Subscribe<PlayerSubstitutionEvent> ((ev) => {
				substEventEmitted = true;
				substTime = ev.Time;
			});
			teamTagger.PropertyChanged += (sender, e) => {
				if (sender == teamTagger.HomeTeam && e.PropertyName == nameof (teamTagger.HomeTeam.FieldPlayersList)) {
					emitedPropertyChange = true;
					times++;
				}
			};

			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			teamTagger.PlayerClick (clickedPlayer2, ButtonModifier.None);

			Assert.IsFalse (clickedPlayer1.Tagged);
			Assert.IsFalse (clickedPlayer2.Tagged);
			Assert.IsFalse (teamTagger.HomeTeam.Selection.Any ());
			Assert.AreSame (clickedPlayer2, teamTagger.HomeTeam.FieldPlayersList.First ());
			Assert.AreSame (clickedPlayer1, teamTagger.HomeTeam.BenchPlayersList.First ());
			Assert.IsTrue (emitedPropertyChange);
			Assert.IsTrue (substEventEmitted);
			Assert.AreEqual (1, times);
			Assert.AreEqual (videoPlayer.AbsoluteCurrentTime, substTime);
		}

		[Test]
		public async Task TaggingController_VideoPlayerTimeChangeOnFileProject_DashboardTimeEqualsAbsoluteCurrentTime ()
		{
			// Arrange
			await AnalysisSetUpAsync ();
			teamTagger.SubstitutionMode = true;
			videoPlayer.CurrentTime = new Time (0);
			projectVM.Dashboard.CurrentTime = new Time (0);

			// Action
			videoPlayer.AbsoluteCurrentTime = new Time (15000);
			videoPlayer.CurrentTime = new Time (10000);

			// Assert
			Assert.AreEqual (new Time (10000), videoPlayer.CurrentTime);
			Assert.AreEqual (videoPlayer.AbsoluteCurrentTime, projectVM.Dashboard.CurrentTime);
		}

		[Test]
		public async Task TaggingController_VideoPlayerTimeChangeOnCaptureProject_DoesNotAffectToDashboardTime ()
		{
			// Arrange
			await AnalysisSetUpAsync ();
			projectVM.ProjectType = ProjectType.CaptureProject;
			teamTagger.SubstitutionMode = true;
			videoPlayer.CurrentTime = new Time (0);
			projectVM.Dashboard.CurrentTime = new Time (0);

			// Action
			videoPlayer.AbsoluteCurrentTime = new Time (10000);
			videoPlayer.CurrentTime = new Time (10000);

			// Assert
			Assert.AreEqual (new Time (10000), videoPlayer.CurrentTime);
			Assert.AreEqual (new Time (0), projectVM.Dashboard.CurrentTime);
		}

		[Test]
		public async Task TaggingController_LineupChangeOnCaptureProject_EventTaggedOnDashboardTime ()
		{
			// Arrange
			await AnalysisSetUpAsync ();
			projectVM.ProjectType = ProjectType.CaptureProject;
			teamTagger.SubstitutionMode = true;
			videoPlayer.CurrentTime = new Time (0);
			var clickedPlayer1 = teamTagger.HomeTeam.FieldPlayersList.FirstOrDefault ();
			var clickedPlayer2 = teamTagger.HomeTeam.BenchPlayersList.FirstOrDefault ();

			// Action
			await App.Current.EventsBroker.Publish (new CapturerTickEvent { Time = new Time (10000) });
			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			teamTagger.PlayerClick (clickedPlayer2, ButtonModifier.None);

			// Assert
			Assert.AreEqual (new Time (0), videoPlayer.CurrentTime);
			Assert.AreEqual (new Time (10000), projectVM.Timeline.FullTimeline.Model [4].EventTime);
		}

		[Test]
		public async Task CurrentTimeChanges_AnalysisMode_LineupChangesWithAbsoluteCurrentTime ()
		{
			await AnalysisSetUpAsync ();
			teamTagger.SubstitutionMode = true;
			videoPlayer.AbsoluteCurrentTime = new Time (20001);
			videoPlayer.CurrentTime = new Time (10000);
			App.Current.EventsBroker.Subscribe<PlayerSubstitutionEvent> ((ev) => Assert.AreEqual (20001, ev.Time.MSeconds));
			var clickedPlayer1 = teamTagger.HomeTeam.FieldPlayersList.FirstOrDefault ();
			var clickedPlayer2 = teamTagger.HomeTeam.BenchPlayersList.FirstOrDefault ();

			teamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			teamTagger.PlayerClick (clickedPlayer2, ButtonModifier.None);
			videoPlayer.AbsoluteCurrentTime = new Time (1000);
			videoPlayer.CurrentTime = new Time (0);

			Assert.AreSame (clickedPlayer1, teamTagger.HomeTeam.FieldPlayersList.FirstOrDefault ());
			Assert.AreSame (clickedPlayer2, teamTagger.HomeTeam.BenchPlayersList.FirstOrDefault ());

			videoPlayer.AbsoluteCurrentTime = new Time (30000);
			videoPlayer.CurrentTime = new Time (20000);

			Assert.AreSame (clickedPlayer2, teamTagger.HomeTeam.FieldPlayersList.FirstOrDefault ());
			Assert.AreSame (clickedPlayer1, teamTagger.HomeTeam.BenchPlayersList.FirstOrDefault ());
		}

		[Test]
		public async Task LineupChanges_WhenCurrentTimeChanges_WithProject ()
		{
			await ProjectSetUpAsync ();
			var outPlayer = teamTagger.HomeTeam.FieldPlayersList.FirstOrDefault ();
			var inPlayer = teamTagger.HomeTeam.BenchPlayersList.FirstOrDefault ();
			var substEvent = new SubstitutionEvent {
				In = inPlayer.Model,
				Out = outPlayer.Model,
				EventTime = new Time (10000),
				EventType = new SubstitutionEventType (),
				Name = "Test Subst"
			};
			substEvent.Teams.Add (teamTagger.HomeTeam.Model);
			project.Timeline.Model.Add (substEvent);

			teamTagger.CurrentTime = new Time (0);

			Assert.AreSame (outPlayer, teamTagger.HomeTeam.FieldPlayersList.FirstOrDefault ());
			Assert.AreSame (inPlayer, teamTagger.HomeTeam.BenchPlayersList.FirstOrDefault ());

			teamTagger.CurrentTime = new Time (20000);

			Assert.AreSame (inPlayer, teamTagger.HomeTeam.FieldPlayersList.FirstOrDefault ());
			Assert.AreSame (outPlayer, teamTagger.HomeTeam.BenchPlayersList.FirstOrDefault ());
		}

		[Test]
		public async Task TestLineupChangesWhenFormationChanges ()
		{
			await SimpleSetUpAsync ();

			Assert.AreEqual (4, teamTagger.HomeTeam.FieldPlayersList.Count ());
			Assert.AreEqual (1, teamTagger.HomeTeam.BenchPlayersList.Count ());

			teamTagger.HomeTeam.Model.FormationStr = "2-1";

			Assert.AreEqual (3, teamTagger.HomeTeam.FieldPlayersList.Count ());
			Assert.AreEqual (2, teamTagger.HomeTeam.BenchPlayersList.Count ());
		}

		[Test]
		public async Task TestLineupChangesWhenFormationChangesWithProject ()
		{
			await ProjectSetUpAsync ();

			Assert.AreEqual (4, teamTagger.HomeTeam.FieldPlayersList.Count ());
			Assert.AreEqual (1, teamTagger.HomeTeam.BenchPlayersList.Count ());

			teamTagger.HomeTeam.Model.FormationStr = "2-1";

			Assert.AreEqual (3, teamTagger.HomeTeam.FieldPlayersList.Count ());
			Assert.AreEqual (2, teamTagger.HomeTeam.BenchPlayersList.Count ());
		}
	}
}
