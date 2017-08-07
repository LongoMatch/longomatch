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
using System.Collections.ObjectModel;
using LongoMatch;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace Tests.Controller
{
	[TestFixture]
	public class TestLMEventsController
	{
		LMEventsController controller;
		Mock<IVideoPlayerController> playerController;
		TimelineEventVM evVM1, evVM2;
		LMProjectVM projectVM;
		VideoPlayerVM videoPlayer;
		Mock<IGUIToolkit> mockToolkit;

		[SetUp]
		public void SetUp ()
		{
			mockToolkit = new Mock<IGUIToolkit> ();
			mockToolkit.SetupGet (o => o.DeviceScaleFactor).Returns (1.0f);
			App.Current.GUIToolkit = mockToolkit.Object;

			controller = new LMEventsController ();
			playerController = new Mock<IVideoPlayerController> ();
			videoPlayer = new VideoPlayerVM {
				Player = playerController.Object,
				CamerasConfig = new ObservableCollection<CameraConfig> ()
			};
			Mock<IVideoPlayer> playerMock = new Mock<IVideoPlayer> ();
			playerMock.SetupAllProperties ();

			projectVM = new LMProjectVM { Model = Utils.CreateProject (true) };
			controller.SetViewModel (new LMProjectAnalysisVM {
				Project = projectVM,
				VideoPlayer = videoPlayer
			});

			var mtkMock = new Mock<IMultimediaToolkit> ();
			mtkMock.Setup (m => m.GetPlayer ()).Returns (playerMock.Object);
			App.Current.MultimediaToolkit = mtkMock.Object;

			controller.Start ();

		}

		[Test]
		public void LoadEvent_LoadedOK ()
		{
			var ev1 = new TimelineEvent ();
			ev1.Start = new Time (0);
			ev1.Stop = new Time (1000);

			App.Current.EventsBroker.Publish (
					new LoadTimelineEventEvent<TimelineEvent> {
						Object = ev1,
						Playing = true,
					}
				);

			playerController.Verify (p => p.LoadEvent (ev1, ev1.Start, true));
		}
	}
}
