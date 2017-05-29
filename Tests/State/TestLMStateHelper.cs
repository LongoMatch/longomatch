//
//  Copyright (C) 2017 Andoni Morales Alastruey
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
using LongoMatch.Services.State;
using Moq;
using NUnit.Framework;
using VAS;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Store;

namespace Tests.State
{
	[TestFixture]
	public class TestLMStateHelper
	{
		Mock<IStateController> stateControllerMock;

		[SetUp]
		public void SetUp ()
		{
			stateControllerMock = new Mock<IStateController> ();
			App.Current.StateController = stateControllerMock.Object;
		}

		[Test]
		public void OpenProject_NewFakeLive_NavigateToAnalyisWindow ()
		{
			var project = new LMProject ();
			var projectVM = new LMProjectVM { Model = project };
			project.ProjectType = ProjectType.FakeCaptureProject;

			project.Description = new ProjectDescription ();
			project.Description.FileSet = new MediaFileSet ();
			project.Description.FileSet.Add (new MediaFile ());
			project.Description.FileSet [0].FilePath = Constants.FAKE_PROJECT;

			LMStateHelper.OpenProject (projectVM);

			stateControllerMock.Verify (s => s.MoveTo (FakeLiveProjectAnalysisState.NAME, It.IsAny<object> (), true, false));
		}

		[Test]
		public void OpenProject_NewLive_NavigateToAnalyisWindow ()
		{
			var project = new LMProject ();
			var projectVM = new LMProjectVM { Model = project };
			project.ProjectType = ProjectType.CaptureProject;

			LMStateHelper.OpenProject (projectVM);

			stateControllerMock.Verify (s => s.MoveTo (LiveProjectAnalysisState.NAME, It.IsAny<object> (), true, false));
		}

		[Test]
		public void OpenProject_ExistingProject_NavigateToAnalyisWindow ()
		{
			var project = new LMProject ();
			var projectVM = new LMProjectVM { Model = project };
			project.ProjectType = ProjectType.FileProject;

			LMStateHelper.OpenProject (projectVM);

			stateControllerMock.Verify (s => s.MoveTo (ProjectAnalysisState.NAME, It.IsAny<object> (), true, false));
		}

		[Test]
		public void OpenProject_ExistingFakeLive_NavigateToAnalyisWindow ()
		{
			var project = new LMProject ();
			var projectVM = new LMProjectVM { Model = project };
			project.ProjectType = ProjectType.FileProject;
			project.Description = new ProjectDescription ();
			project.Description.FileSet = new MediaFileSet ();
			project.Description.FileSet.Add (new MediaFile ());
			project.Description.FileSet [0].FilePath = Constants.FAKE_PROJECT;

			LMStateHelper.OpenProject (projectVM);

			stateControllerMock.Verify (s => s.MoveTo (NewProjectState.NAME, projectVM, false, false));
		}
	}
}
