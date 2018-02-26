//
//  Copyright (C) 2015 Fluendo S.A.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Hotkeys;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services;

namespace Tests.Controller
{
	[TestFixture ()]
	public class TestProjectAnalysisController
	{
		Mock<IMultimediaToolkit> mtkMock;
		Mock<IGUIToolkit> gtkMock;
		Mock<ICapturer> capturerMock;
		//Mock<ICapturerBin> capturerBinMock;
		Mock<IStateController> stateControllerMock;
		VideoPlayerController player;
		ProjectAnalysisController projectsManager;
		LMProject project;
		LMProjectVM projectVM;
		VideoPlayerVM videoPlayerVM;
		CaptureSettings settings;

		List<Mock> mockList;

		Mock<ILicenseLimitationsService> mockLimitationService;
		ILicenseLimitationsService currentService;

		[OneTimeSetUp]
		public void FixtureSetup ()
		{
			mockList = new List<Mock> ();
			settings = new CaptureSettings ();
			settings.EncodingSettings = new EncodingSettings ();
			settings.EncodingSettings.EncodingProfile = EncodingProfiles.MP4;

			App.Current.HotkeysService = new HotkeysService ();
			GeneralUIHotkeys.RegisterDefaultHotkeys ();
			LMGeneralUIHotkeys.RegisterDefaultHotkeys ();

			var playerMock = new Mock<IVideoPlayer> ();
			playerMock.SetupAllProperties ();
			mockList.Add (playerMock);

			capturerMock = new Mock<ICapturer> ();
			capturerMock.SetupAllProperties ();
			mockList.Add (capturerMock);

			mtkMock = new Mock<IMultimediaToolkit> ();
			mtkMock.Setup (m => m.GetPlayer ()).Returns (playerMock.Object);
			mtkMock.Setup (m => m.GetMultiPlayer ()).Throws (new Exception ());
			mtkMock.Setup (m => m.GetCapturer ()).Returns (capturerMock.Object);
			mtkMock.Setup (m => m.DiscoverFile (It.IsAny<string> (), It.IsAny<bool> ()))
				.Returns ((string s, bool b) => new MediaFile { FilePath = s });
			App.Current.MultimediaToolkit = mtkMock.Object;
			mockList.Add (mtkMock);

			gtkMock = new Mock<IGUIToolkit> ();
			gtkMock.SetupGet (o => o.DeviceScaleFactor).Returns (1.0f);
			gtkMock.Setup (m => m.Invoke (It.IsAny<EventHandler> ())).Callback<EventHandler> (e => e (null, null));
			gtkMock.Setup (g => g.RemuxFile (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<VideoMuxerType> ()))
				.Returns (() => settings.EncodingSettings.OutputFile)
				.Callback ((string s, string d, VideoMuxerType m) => File.Copy (s, d));
			gtkMock.Setup (g => g.EndCapture (true)).Returns (EndCaptureResponse.Save);
			App.Current.GUIToolkit = gtkMock.Object;
			mockList.Add (gtkMock);

			//capturerBinMock = new Mock<ICapturerBin> ();
			//capturerBinMock.Setup (w => w.Capturer).Returns (capturerMock.Object);
			//capturerBinMock.Setup (w => w.CaptureSettings).Returns (() => settings);
			//capturerBinMock.Setup (w => w.Periods).Returns (() => new List<Period> ());
			//mockList.Add (capturerBinMock);

			player = new VideoPlayerController ();
			videoPlayerVM = new VideoPlayerVM ();
			videoPlayerVM.Player = player;
			player.SetViewModel (videoPlayerVM);

			currentService = App.Current.LicenseLimitationsService;

			stateControllerMock = new Mock<IStateController> ();
			App.Current.StateController = stateControllerMock.Object;
		}

		[SetUp ()]
		public void Setup ()
		{
			settings.EncodingSettings.OutputFile = Path.GetTempFileName ();
			App.Current.DatabaseManager = new LocalDatabaseManager ();

			project = Utils.CreateProject ();
			projectVM = new LMProjectVM { Model = project };
			LMProjectAnalysisVM viewModel = new LMProjectAnalysisVM ();
			viewModel.VideoPlayer = videoPlayerVM;
			viewModel.Project = projectVM;
			viewModel.SaveCommand.SetCallback (
				() => App.Current.EventsBroker.Publish (new SaveEvent<LMProjectVM> { Object = viewModel.Project }),
				() => viewModel.Project.Edited);
			viewModel.CloseCommand.SetCallback (
				async () => await App.Current.EventsBroker.Publish (
								new CloseEvent<LMProjectVM> { Object = viewModel.Project })
			   );
			viewModel.ShowWarningLimitation.SetCallback (() => { });
			((LimitationCommand)viewModel.ShowWarningLimitation).LimitationCondition = () => viewModel.Project.FileSet.Count () > 1;

			projectsManager = new ProjectAnalysisController ();
			projectsManager.SetViewModel (viewModel);
			projectsManager.Start ();

			KeyContext context = new KeyContext ();
			foreach (KeyAction action in projectsManager.GetDefaultKeyActions ()) {
				context.AddAction (action);
			}
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });
			mockLimitationService = new Mock<ILicenseLimitationsService> ();
			App.Current.LicenseLimitationsService = mockLimitationService.Object;
		}

		[TearDown ()]
		public void TearDown ()
		{
			try {
				projectsManager.Stop ();
			} catch {
			}
			Utils.DeleteProject (project);
			try {
				File.Delete (settings.EncodingSettings.OutputFile);
			} catch {
			}
			foreach (Mock mock in mockList) {
				mock.ResetCalls ();
			}

			App.Current.LicenseLimitationsService = currentService;
		}

		[Test ()]
		public void TestCaptureProjectError ()
		{
			projectsManager.ViewModel.Project.Model = project;
			projectsManager.ViewModel.CaptureSettings = settings;
			projectsManager.ViewModel.Project.ProjectType = ProjectType.CaptureProject;

			App.Current.EventsBroker.Publish<CaptureErrorEvent> (
				new CaptureErrorEvent {
					Sender = null,
					Message = "Error!"
				}
			);
			/* Errors during a capture project should be handled gracefully
			 * closing the current capture project and saving a copy of the
			 * captured video and coded data, without loosing anything */
			// SP-Question: The comment does not follow the actual code implementation
			// The project is not saved, what is the expected behavior?
			Assert.AreEqual (0, App.Current.DatabaseManager.ActiveDB.Count<LMProject> ());
		}

		[Test]
		public void CaptureFinished_Cancelled_ProjectNotSavedAndNavigatedHome ()
		{
			List<string> transitions = new List<string> ();
			App.Current.EventsBroker.Subscribe<NavigationEvent> ((obj) => {
				transitions.Add (obj.Name);
			});

			//projectsManager.Capturer = capturerBinMock.Object;
			projectsManager.ViewModel.Project.Model = project;
			projectsManager.ViewModel.CaptureSettings = settings;
			projectsManager.ViewModel.Project.ProjectType = ProjectType.CaptureProject;

			App.Current.EventsBroker.Publish (
				new CaptureFinishedEvent {
					Cancel = true,
					Reopen = false
				}
			);

			Assert.IsTrue (projectsManager.ViewModel.Project.CloseHandled);
			Assert.AreEqual (0, App.Current.DatabaseManager.ActiveDB.Count<LMProject> ());
			Assert.AreEqual (project, projectsManager.Project.Model);
			//capturerBinMock.Verify (c => c.Close (), Times.Once ());
			stateControllerMock.Verify (e => e.MoveToHome (false), Times.Once);
		}

		[Test]
		public void CaptureFinished_ProjectSaved_AskedToSaveAndProjectReopened ()
		{
			List<string> transitions = new List<string> ();
			App.Current.EventsBroker.Subscribe<NavigationEvent> ((obj) => {
				transitions.Add (obj.Name);
			});

			//projectsManager.Capturer = capturerBinMock.Object;
			projectsManager.ViewModel.Project.Model = project;
			projectsManager.ViewModel.CaptureSettings = settings;
			projectsManager.ViewModel.Project.ProjectType = ProjectType.CaptureProject;

			App.Current.EventsBroker.Publish (
				new CaptureFinishedEvent {
					Cancel = false,
					Reopen = true
				}
			);
			//capturerBinMock.Verify (c => c.Close (), Times.Once ());
			/* We are not prompted to quit the capture */
			gtkMock.Verify (g => g.EndCapture (true), Times.Never ());
			gtkMock.Verify (g => g.RemuxFile (It.IsAny<string> (),
				settings.EncodingSettings.OutputFile, VideoMuxerType.Mp4));
			Assert.AreEqual (1, App.Current.DatabaseManager.ActiveDB.Count<LMProject> ());
			Assert.AreEqual (project, projectsManager.Project.Model);
			stateControllerMock.Verify (e => e.MoveTo (ProjectAnalysisState.NAME, It.IsAny<object> (), true, false), Times.Once);
		}

		[Test ()]
		public async Task TestCloseCaptureProject ()
		{
			await App.Current.EventsBroker.Publish (new CloseOpenedProjectEvent ());

			//projectsManager.Capturer = capturerBinMock.Object;
			projectsManager.ViewModel.Project.Model = project;
			projectsManager.ViewModel.CaptureSettings = settings;
			projectsManager.ViewModel.Project.ProjectType = ProjectType.CaptureProject;

			gtkMock.Setup (g => g.EndCapture (false)).Returns (EndCaptureResponse.Return);
			await App.Current.EventsBroker.Publish (new CloseEvent<LMProjectVM> { Object = projectsManager.ViewModel.Project });
			Assert.AreEqual (project, projectsManager.Project.Model);

			gtkMock.Setup (g => g.EndCapture (false)).Returns (EndCaptureResponse.Quit);
			await App.Current.EventsBroker.Publish (new CloseEvent<LMProjectVM> { Object = projectsManager.ViewModel.Project });
			Assert.AreEqual (project, projectsManager.Project.Model);
			Assert.AreEqual (0, App.Current.DatabaseManager.ActiveDB.Count<LMProject> ());

			//projectsManager.Capturer = capturerBinMock.Object;
			projectsManager.ViewModel.Project.Model = project;
			projectsManager.ViewModel.CaptureSettings = settings;
			projectsManager.ViewModel.Project.ProjectType = ProjectType.CaptureProject;
			projectsManager.ViewModel.Project.CloseHandled = false;

			gtkMock.Setup (g => g.EndCapture (false)).Returns (EndCaptureResponse.Save);
			await App.Current.EventsBroker.Publish (new CloseEvent<LMProjectVM> { Object = projectsManager.ViewModel.Project });
			Assert.AreEqual (project, projectsManager.Project.Model);
			Assert.AreEqual (1, App.Current.DatabaseManager.ActiveDB.Count<LMProject> ());
		}

		[Test ()]
		public async Task HandleClose_PromtAlreadyDisplayed_ReturnTrue ()
		{
			// Arrange
			Mock<IDialogs> mockDialogs = new Mock<IDialogs> ();
			App.Current.Dialogs = mockDialogs.Object;

			//projectsManager.Capturer = capturerBinMock.Object;
			projectsManager.ViewModel.Project.Model = project;
			projectsManager.ViewModel.CaptureSettings = settings;
			projectsManager.ViewModel.Project.ProjectType = ProjectType.FileProject;
			projectsManager.ViewModel.Project.CloseHandled = true;

			// Act
			bool result = await App.Current.EventsBroker.PublishWithReturn (new CloseEvent<LMProjectVM> { Object = projectsManager.ViewModel.Project });

			// Assert
			Assert.IsTrue (result);
			Assert.IsTrue (projectsManager.ViewModel.Project.CloseHandled);
			mockDialogs.Verify (x => x.QuestionMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Never);
		}

		[Test ()]
		public async Task HandleClose_CloseRejected_AllowAskAgainForClosing ()
		{
			// Arrange
			Mock<IDialogs> mockDialogs = new Mock<IDialogs> ();
			App.Current.Dialogs = mockDialogs.Object;

			//projectsManager.Capturer = capturerBinMock.Object;
			projectsManager.ViewModel.Project.Model = project;
			projectsManager.ViewModel.CaptureSettings = settings;
			projectsManager.ViewModel.Project.ProjectType = ProjectType.FileProject;
			projectsManager.ViewModel.Project.CloseHandled = false;

			mockDialogs.Setup (x => x.QuestionMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ())).Returns (AsyncHelpers.Return (false));
			await App.Current.EventsBroker.PublishWithReturn (new CloseEvent<LMProjectVM> { Object = projectsManager.ViewModel.Project });

			// Act
			Assert.IsFalse (projectsManager.ViewModel.Project.CloseHandled);
			mockDialogs.Setup (x => x.QuestionMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ())).Returns (AsyncHelpers.Return (true));
			bool result = await App.Current.EventsBroker.PublishWithReturn (new CloseEvent<LMProjectVM> { Object = projectsManager.ViewModel.Project });

			// Assert
			Assert.IsTrue (result);
			Assert.IsTrue (projectsManager.ViewModel.Project.CloseHandled);
			mockDialogs.Verify (x => x.QuestionMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Exactly (2));
		}

		[Test]
		public void TestHotkey_Save_SaveCommandCalled ()
		{
			bool saveCalled = false;

			projectsManager.Stop ();
			App.Current.EventsBroker.Subscribe<SaveEvent<LMProjectVM>> ((obj) => saveCalled = true);
			App.Current.KeyContextManager.HandleKeyPressed (App.Current.Keyboard.ParseName ("<Primary>+s"));

			Assert.IsTrue (saveCalled);
		}

		[Test]
		public void TestHotkey_Close_CloseCommandCalled ()
		{
			bool closeCalled = false;

			projectsManager.Stop ();
			App.Current.EventsBroker.Subscribe<CloseEvent<LMProjectVM>> ((obj) => closeCalled = true);
			App.Current.KeyContextManager.HandleKeyPressed (App.Current.Keyboard.ParseName ("<Primary>+w"));

			Assert.IsTrue (closeCalled);
		}

		[Test]
		public void NavigationEvent_LimitationEnabled_ShowWarningLimitation ()
		{
			// Arrange
			mockLimitationService.Setup (sim => sim.CanExecute (VASFeature.OpenMultiCamera.ToString ())).Returns (false);

			// Act
			App.Current.EventsBroker.Publish (new NavigationEvent { Name = "ProjectAnalysis", IsModal = false });

			// Assert
			mockLimitationService.Verify (s => s.MoveToUpgradeDialog (VASFeature.OpenMultiCamera.ToString ()), Times.Once);
		}

		[Test]
		public void NavigationEvent_NoLimitationCanExecuteFeature_DoNothing ()
		{
			// Arrange
			mockLimitationService.Setup (sim => sim.CanExecute (VASFeature.OpenMultiCamera.ToString ())).Returns (true);

			// Act
			App.Current.EventsBroker.Publish (new NavigationEvent { Name = "ProjectAnalysis", IsModal = false });

			// Assert
			mockLimitationService.Verify (s => s.MoveToUpgradeDialog (VASFeature.OpenMultiCamera.ToString ()), Times.Never);
		}

		[Test]
		public void NavigationEvent_LimitationEnabledOnlyOneVideoFile_DoNothing ()
		{
			// Arrange
			mockLimitationService.Setup (sim => sim.CanExecute (VASFeature.OpenMultiCamera.ToString ())).Returns (false);
			project.FileSet.RemoveAt (0);

			// Act
			App.Current.EventsBroker.Publish (new NavigationEvent { Name = "ProjectAnalysis", IsModal = false });

			// Assert
			mockLimitationService.Verify (s => s.MoveToUpgradeDialog (VASFeature.OpenMultiCamera.ToString ()), Times.Never);
		}
	}
}

