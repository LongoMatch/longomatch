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
using LongoMatch.Core.Store;
using LongoMatch.Services;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using VAS.Services;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
using VAS.Core.ViewModel;

namespace Tests.Controller
{
	[TestFixture ()]
	public class TestProjectAnalysisController
	{
		Mock<IMultimediaToolkit> mtkMock;
		Mock<IGUIToolkit> gtkMock;
		Mock<ICapturer> capturerMock;
		Mock<ICapturerBin> capturerBinMock;
		VideoPlayerController player;
		ProjectAnalysisController projectsManager;
		LMProject project;
		LMProjectVM projectVM;
		VideoPlayerVM videoPlayerVM;
		CaptureSettings settings;

		List<Mock> mockList;

		[TestFixtureSetUp ()]
		public void FixtureSetup ()
		{
			mockList = new List<Mock> ();
			settings = new CaptureSettings ();
			settings.EncodingSettings = new EncodingSettings ();
			settings.EncodingSettings.EncodingProfile = EncodingProfiles.MP4;

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
			gtkMock.Setup (m => m.Invoke (It.IsAny<EventHandler> ())).Callback<EventHandler> (e => e (null, null));
			gtkMock.Setup (g => g.RemuxFile (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<VideoMuxerType> ()))
				.Returns (() => settings.EncodingSettings.OutputFile)
				.Callback ((string s, string d, VideoMuxerType m) => File.Copy (s, d));
			gtkMock.Setup (g => g.EndCapture (true)).Returns (EndCaptureResponse.Save);
			App.Current.GUIToolkit = gtkMock.Object;
			mockList.Add (gtkMock);

			capturerBinMock = new Mock<ICapturerBin> ();
			capturerBinMock.Setup (w => w.Capturer).Returns (capturerMock.Object);
			capturerBinMock.Setup (w => w.CaptureSettings).Returns (() => settings);
			capturerBinMock.Setup (w => w.Periods).Returns (() => new List<Period> ());
			mockList.Add (capturerBinMock);

			player = new VideoPlayerController ();
			videoPlayerVM = new VideoPlayerVM ();
			videoPlayerVM.Player = player;
			player.SetViewModel (videoPlayerVM);
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

			projectsManager = new ProjectAnalysisController ();
			projectsManager.ViewModel = viewModel;
			projectsManager.Start ();
		}

		[TearDown ()]
		public void TearDown ()
		{
			projectsManager.Stop ();
			Utils.DeleteProject (project);
			try {
				File.Delete (settings.EncodingSettings.OutputFile);
			} catch {
			}
			foreach (Mock mock in mockList) {
				mock.ResetCalls ();
			}
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

		[Test ()]
		public void TestCaptureFinished ()
		{
			List<string> transitions = new List<string> ();
			App.Current.EventsBroker.Subscribe<NavigationEvent> ((obj) => {
				transitions.Add (obj.Name);
			});

			projectsManager.Capturer = capturerBinMock.Object;
			projectsManager.ViewModel.Project.Model = project;
			projectsManager.ViewModel.CaptureSettings = settings;
			projectsManager.ViewModel.Project.ProjectType = ProjectType.CaptureProject;

			App.Current.EventsBroker.Publish<CaptureFinishedEvent> (
				new CaptureFinishedEvent {
					Cancel = true,
					Reopen = true
				}
			);

			Assert.AreEqual (0, App.Current.DatabaseManager.ActiveDB.Count<LMProject> ());
			Assert.AreEqual (project, projectsManager.Project.Model); // SP-Remark: check that project is null has no sense here 
			capturerBinMock.Verify (c => c.Close (), Times.Once ());
			capturerBinMock.ResetCalls ();
			Assert.AreEqual ("Home", App.Current.StateController.Current); // replaced previous line for this one
			gtkMock.ResetCalls ();
			Utils.DeleteProject (project);

			project = Utils.CreateProject ();
			settings.EncodingSettings.OutputFile = project.Description.FileSet.FirstOrDefault ().FilePath;

			projectsManager.Capturer = capturerBinMock.Object;
			projectsManager.ViewModel.Project.Model = project;
			projectsManager.ViewModel.CaptureSettings = settings;
			projectsManager.ViewModel.Project.ProjectType = ProjectType.CaptureProject;

			App.Current.EventsBroker.Publish<CaptureFinishedEvent> (
				new CaptureFinishedEvent {
					Cancel = false,
					Reopen = true
				}
			);
			capturerBinMock.Verify (c => c.Close (), Times.Once ());
			/* We are not prompted to quit the capture */
			gtkMock.Verify (g => g.EndCapture (true), Times.Never ());
			gtkMock.Verify (g => g.RemuxFile (It.IsAny<string> (),
				settings.EncodingSettings.OutputFile, VideoMuxerType.Mp4));
			Assert.AreEqual ("Home", App.Current.StateController.Current);
			Assert.AreEqual (1, App.Current.DatabaseManager.ActiveDB.Count<LMProject> ());
			Assert.AreEqual (project, projectsManager.Project.Model);
			/*Assert.AreEqual (ProjectType.FileProject, projectsManager.OpenedProjectType);
			// Make sure the project is not cleared.
			Assert.IsNotEmpty (projectsManager.Project.Dashboard.List);
			Assert.IsNotEmpty (projectsManager.Project.LocalTeamTemplate.List);
			Assert.IsNotEmpty (projectsManager.Project.VisitorTeamTemplate.List);*/
		}

		[Test ()]
		public async Task TestCloseCaptureProject ()
		{
			await App.Current.EventsBroker.Publish (new CloseOpenedProjectEvent ());

			projectsManager.Capturer = capturerBinMock.Object;
			projectsManager.ViewModel.Project.Model = project;
			projectsManager.ViewModel.CaptureSettings = settings;
			projectsManager.ViewModel.Project.ProjectType = ProjectType.CaptureProject;

			gtkMock.Setup (g => g.EndCapture (false)).Returns (EndCaptureResponse.Return);
			await App.Current.EventsBroker.Publish (new CloseEvent<LMProjectVM> { Object = projectsManager.ViewModel.Project});
			Assert.AreEqual (project, projectsManager.Project.Model);

			gtkMock.Setup (g => g.EndCapture (false)).Returns (EndCaptureResponse.Quit);
			await App.Current.EventsBroker.Publish (new CloseEvent<LMProjectVM> { Object = projectsManager.ViewModel.Project });
			Assert.AreEqual (project, projectsManager.Project.Model);
			Assert.AreEqual (0, App.Current.DatabaseManager.ActiveDB.Count<LMProject> ());

			projectsManager.Capturer = capturerBinMock.Object;
			projectsManager.ViewModel.Project.Model = project;
			projectsManager.ViewModel.CaptureSettings = settings;
			projectsManager.ViewModel.Project.ProjectType = ProjectType.CaptureProject;

			gtkMock.Setup (g => g.EndCapture (false)).Returns (EndCaptureResponse.Save);
			await App.Current.EventsBroker.Publish (new CloseEvent<LMProjectVM> { Object = projectsManager.ViewModel.Project });
			Assert.AreEqual (project, projectsManager.Project.Model);
			Assert.AreEqual (1, App.Current.DatabaseManager.ActiveDB.Count<LMProject> ());
		}

		[Test ()]
		public void TestOpenBadProject ()
		{
			// Test to try opening a project with duration = null
			Assert.Greater (project.Description.FileSet.Count, 0);
			foreach (var file in project.Description.FileSet) {
				file.Duration = null;
			}

			App.Current.DatabaseManager.ActiveDB.Store<LMProject> (project);

			projectsManager.Capturer = capturerBinMock.Object;
			projectsManager.ViewModel.Project.Model = project;
			// projectsManager.ViewModel.Project.ProjectType = ProjectType.CaptureProject;
			/*App.Current.EventsBroker.Publish<OpenProjectIDEvent> (
				new OpenProjectIDEvent {
					ProjectID = project.ID,
					Project = project
				}
			);*/

			mtkMock.Verify (g => g.DiscoverFile (It.IsAny<string> (), true), Times.Exactly (project.Description.FileSet.Count));
		}
	}
}

