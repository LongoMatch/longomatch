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
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace Tests.Controller
{
	[TestFixture]
	public class TestPlaylistController
	{
		const string name = "name";

		Mock<IGUIToolkit> mockGuiToolkit;
		VideoPlayerVM videoPlayerVM;
		Mock<IDialogs> mockDialogs;
		Mock<IStorageManager> storageManagerMock;
		Mock<IStorage> storageMock;
		Mock<IVideoPlayerController> videoPlayerController;
		LMPlaylistController controller;
		PlaylistCollectionVM playlistCollectionVM;
		LMProjectVM projectVM;

		[OneTimeSetUp]
		public void FixtureSetup ()
		{
			mockGuiToolkit = new Mock<IGUIToolkit> ();
			mockGuiToolkit.SetupGet (o => o.DeviceScaleFactor).Returns (1.0f);

			storageManagerMock = new Mock<IStorageManager> ();
			storageManagerMock.SetupAllProperties ();
			storageMock = new Mock<IStorage> ();
			storageManagerMock.Object.ActiveDB = storageMock.Object;
			App.Current.DatabaseManager = storageManagerMock.Object;
		}

		[SetUp]
		public void Setup ()
		{
			mockDialogs = new Mock<IDialogs> ();
			App.Current.GUIToolkit = mockGuiToolkit.Object;
			App.Current.Dialogs = mockDialogs.Object;
			videoPlayerController = new Mock<IVideoPlayerController> ();
			videoPlayerVM = new VideoPlayerVM ();
			videoPlayerVM.Player = videoPlayerController.Object;
			LMProject project = Utils.CreateProject (true);
			project.ProjectType = ProjectType.FileProject;
			projectVM = new LMProjectVM { Model = project };
			playlistCollectionVM = projectVM.Playlists;
			LMProjectAnalysisVM viewModel = new LMProjectAnalysisVM ();
			viewModel.Project = projectVM;
			viewModel.VideoPlayer = videoPlayerVM;
			controller = new LMPlaylistController ();
			controller.SetViewModel (viewModel);
			controller.Start ();
		}

		[TearDown]
		public void TearDown ()
		{
			controller.Stop ();
			storageMock.ResetCalls ();
			storageManagerMock.ResetCalls ();
			mockGuiToolkit.ResetCalls ();
		}

		[Test]
		public void TestLoadPlayEvent ()
		{
			TimelineEvent element = new TimelineEvent { Start = new Time (0), Stop = new Time (5) };
			TimelineEventVM vm = new TimelineEventVM () { Model = element };

			App.Current.EventsBroker.Publish (new LoadEventEvent { TimelineEvent = vm });
			videoPlayerController.Verify (player => player.LoadEvent (vm, new Time (0), true),
				Times.Once ());
		}

		[Test]
		public void TestLoadPlayEventNull ()
		{
			TimelineEventVM element = null;

			App.Current.EventsBroker.Publish (new LoadEventEvent { TimelineEvent = element });
			videoPlayerController.Verify (player => player.UnloadCurrentEvent (), Times.Once ());
		}

		[Test]
		public void TestLoadPlayEventWithoutDuration ()
		{
			TimelineEvent element = new TimelineEvent { Start = new Time (0), Stop = new Time (0) };
			TimelineEventVM vm = new TimelineEventVM () { Model = element };

			App.Current.EventsBroker.Publish (new LoadEventEvent { TimelineEvent = vm });
			videoPlayerController.Verify (
				player => player.Seek (element.EventTime, true, false, false), Times.Once ());
			videoPlayerController.Verify (player => player.Play (false), Times.Once ());
		}

		[Test]
		public void TestLoadPlayEventFake ()
		{
			projectVM.Model.ProjectType = ProjectType.FakeCaptureProject;
			TimelineEvent element = new TimelineEvent ();
			TimelineEventVM vm = new TimelineEventVM () { Model = element };

			App.Current.EventsBroker.Publish (new LoadEventEvent { TimelineEvent = vm });
			videoPlayerController.Verify (player => player.Seek (It.IsAny<Time> (), It.IsAny<bool> (),
				It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());
			videoPlayerController.Verify (player => player.Play (false), Times.Never ());
		}

		[Test]
		public void TestPrev ()
		{
			TimelineEvent element = new TimelineEvent ();
			TimelineEventVM vm = new TimelineEventVM () { Model = element };

			App.Current.EventsBroker.Publish (new LoadEventEvent { TimelineEvent = vm });
			// loadedPlay != null
			videoPlayerController.ResetCalls ();

			App.Current.EventsBroker.Publish (new PreviousPlaylistElementEvent ());

			videoPlayerController.Verify (player => player.Previous (false), Times.Once ());
		}

		[Test]
		public void TestNext ()
		{
			TimelineEvent element = new TimelineEvent ();
			TimelineEventVM vm = new TimelineEventVM () { Model = element };

			App.Current.EventsBroker.Publish (new LoadEventEvent { TimelineEvent = vm });
			// loadedPlay != null
			videoPlayerController.ResetCalls ();

			App.Current.EventsBroker.Publish (new NextPlaylistElementEvent ());

			videoPlayerController.Verify (player => player.Next (), Times.Once ());
		}
	}
}
