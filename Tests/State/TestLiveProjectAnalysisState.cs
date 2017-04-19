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

namespace Tests.State
{
	public class TestLiveProjectAnalysisState
	{
		Mock<IMultimediaToolkit> mtkMock;

		[SetUp]
		public void Setup ()
		{
			var hotkeysMock = new Mock<IHotkeysService> ();
			var playerMock = new Mock<IVideoPlayer> ();
			mtkMock = new Mock<IMultimediaToolkit> ();
			var capturerMock = new Mock<IFramesCapturer> ();
			mtkMock.Setup (m => m.GetFramesCapturer ()).Returns (capturerMock.Object);
			mtkMock.Setup (m => m.GetPlayer ()).Returns (playerMock.Object);
			App.Current.MultimediaToolkit = mtkMock.Object;
			App.Current.HotkeysService = hotkeysMock.Object;
		}

		[Test]
		public void LoadState_ViewModelFromPropDisposed_AssignOnlyModel ()
		{
			// Arrange
			LiveProjectAnalysisState state = new LiveProjectAnalysisState ();
			LMProject project = Utils.CreateProject ();
			LMProjectVM projectVM = new LMProjectVM { Model = project };
			LMProjectAnalysisVM analysisVM = new LMProjectAnalysisVM { Project = projectVM };

			var panel = new Mock<Utils.IDummyCapturerPanel> ();
			panel.Setup (p => p.Capturer).Returns (new Mock<ICapturerBin> ().Object);
			state.Panel = panel.Object;

			// Act
			state.LoadState (analysisVM);

			// Assert
			Assert.AreNotEqual (projectVM, state.ViewModel.Project);
			Assert.AreEqual (project, state.ViewModel.Project.Model);
		}
	}
}
