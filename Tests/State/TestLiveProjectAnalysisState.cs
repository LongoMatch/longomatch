//
//  Copyright (C) 2017 FLUENDO S.A.
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
using LongoMatch;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Common;
using VAS.Core.Store;

namespace Tests.State
{
	public class TestLiveProjectAnalysisState
	{
		Mock<IMultimediaToolkit> mtkMock;
		Mock<ICapturerBin> capturerMock;
		LiveProjectAnalysisState state;
		LMProject project;
		LMProjectVM projectVM;
		LMProjectAnalysisVM analysisVM;

		[TestFixtureSetUp]
		public void SetupOnce ()
		{
			var hotkeysMock = new Mock<IHotkeysService> ();
			var playerMock = new Mock<IVideoPlayer> ();
			mtkMock = new Mock<IMultimediaToolkit> ();
			var framesCapturerMock = new Mock<IFramesCapturer> ();
			capturerMock = new Mock<ICapturerBin> ();
			mtkMock.Setup (m => m.GetFramesCapturer ()).Returns (framesCapturerMock.Object);
			mtkMock.Setup (m => m.GetPlayer ()).Returns (playerMock.Object);
			App.Current.MultimediaToolkit = mtkMock.Object;
			App.Current.HotkeysService = hotkeysMock.Object;
		}

		[SetUp]
		public void Setup ()
		{
			state = new LiveProjectAnalysisState ();
			project = Utils.CreateProject ();
			projectVM = new LMProjectVM { Model = project };
			analysisVM = new LMProjectAnalysisVM { Project = projectVM };
			var panel = new Mock<Utils.IDummyCapturerPanel> ();
			panel.Setup (p => p.Capturer).Returns (capturerMock.Object);
			state.Panel = panel.Object;
		}

		[Test]
		public async void LoadState_AllGood_TransitionOK ()
		{
			// Act
			bool ret = await state.LoadState (analysisVM);

			// Assert
			capturerMock.Verify (c => c.Run (It.IsAny<CaptureSettings> (), It.IsAny<MediaFile> ()), Times.Once);
			Assert.AreNotEqual (projectVM, state.ViewModel.Project);
			Assert.AreEqual (project, state.ViewModel.Project.Model);
			Assert.IsTrue (ret);
		}

		[Test]
		public async void LoadState_VideoPlayerException_TransitionCancelled ()
		{
			capturerMock.Setup (p => p.Run (It.IsAny<CaptureSettings> (), It.IsAny<MediaFile> ())).
						Throws<Exception> ();

			// Act
			bool ret = await state.LoadState (analysisVM);

			// Assert
			Assert.IsFalse (ret);
		}
	}
}
