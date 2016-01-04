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
using LongoMatch;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;
using LongoMatch.Services;
using Moq;
using NUnit.Framework;

namespace Tests.Services
{
	[TestFixture ()]
	public class TestProjectsManager
	{
		Mock<IMultimediaToolkit> mtkMock;
		Mock<IGUIToolkit> gtkMock;
		Mock<IAnalysisWindow> winMock;
		Mock<ICapturer> capturerMock;
		Mock<ICapturerBin> capturerBinMock;
		PlayerController player;
		ProjectsManager projectsManager;
		Project project;
		CaptureSettings settings;

		List<Mock> mockList;

		[TestFixtureSetUp ()]
		public void FixtureSetup ()
		{
			mockList = new List<Mock> ();
			settings = new CaptureSettings ();
			settings.EncodingSettings = new EncodingSettings ();
			settings.EncodingSettings.EncodingProfile = EncodingProfiles.MP4;

			var playerMock = new Mock<IPlayer> ();
			playerMock.SetupAllProperties ();
			mockList.Add (playerMock);

			capturerMock = new Mock<ICapturer> ();
			capturerMock.SetupAllProperties ();
			mockList.Add (capturerMock);

			winMock = new Mock<IAnalysisWindow> ();
			winMock.SetupAllProperties ();
			IAnalysisWindow win = winMock.Object;
			mockList.Add (winMock);

			mtkMock = new Mock<IMultimediaToolkit> ();
			mtkMock.Setup (m => m.GetPlayer ()).Returns (playerMock.Object);
			mtkMock.Setup (m => m.GetMultiPlayer ()).Throws (new Exception ());
			mtkMock.Setup (m => m.GetCapturer ()).Returns (capturerMock.Object);
			mtkMock.Setup (m => m.DiscoverFile (It.IsAny<string> (), It.IsAny<bool> ()))
				.Returns ((string s, bool b) => new MediaFile { FilePath = s });
			Config.MultimediaToolkit = mtkMock.Object;
			mockList.Add (mtkMock);

			gtkMock = new Mock<IGUIToolkit> ();
			gtkMock.Setup (m => m.Invoke (It.IsAny<EventHandler> ())).Callback<EventHandler> (e => e (null, null));
			gtkMock.Setup (m => m.OpenProject (It.IsAny<Project> (), It.IsAny<ProjectType> (),
				It.IsAny<CaptureSettings> (), It.IsAny<EventsFilter> (), out win));
			gtkMock.Setup (g => g.RemuxFile (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<VideoMuxerType> ()))
				.Returns (() => settings.EncodingSettings.OutputFile)
				.Callback ((string s, string d, VideoMuxerType m) => File.Copy (s, d));
			gtkMock.Setup (g => g.EndCapture (true)).Returns (EndCaptureResponse.Save);
			Config.GUIToolkit = gtkMock.Object;
			mockList.Add (gtkMock);

			capturerBinMock = new Mock<ICapturerBin> ();
			capturerBinMock.Setup (w => w.Capturer).Returns (capturerMock.Object);
			capturerBinMock.Setup (w => w.CaptureSettings).Returns (() => settings);
			capturerBinMock.Setup (w => w.Periods).Returns (() => new List<Period> ());
			mockList.Add (capturerBinMock);
			player = new PlayerController (); 
			winMock.Setup (w => w.Capturer).Returns (capturerBinMock.Object);
			winMock.Setup (w => w.Player).Returns (player);
		}

		[SetUp ()]
		public void Setup ()
		{
			Config.EventsBroker = new EventsBroker ();
			Config.DatabaseManager = new LocalDatabaseManager ();
			projectsManager = new ProjectsManager ();
			projectsManager.Start ();
			project = Utils.CreateProject ();
			settings.EncodingSettings.OutputFile = Path.GetTempFileName ();
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
		public void TestLoadCaptureProject ()
		{
			bool projectOpened = false;

			Config.EventsBroker.OpenedProjectChanged += (p, pt, f, a) => {
				Assert.AreEqual (project, p);
				Assert.AreEqual (ProjectType.CaptureProject, pt);
				projectOpened = true;
			};

			Config.EventsBroker.EmitOpenNewProject (project, ProjectType.CaptureProject, settings);
			Assert.AreEqual (1, Config.DatabaseManager.ActiveDB.Count<Project> ());
			Assert.AreEqual (project, projectsManager.OpenedProject);
			Assert.AreEqual (ProjectType.CaptureProject, projectsManager.OpenedProjectType);
			Assert.AreEqual (player, projectsManager.Player);
			Assert.AreEqual (capturerBinMock.Object, projectsManager.Capturer);
			Assert.IsTrue (projectOpened);
			capturerBinMock.Verify (c => c.Run (settings, project.Description.FileSet.First ()), Times.Once ());
		}

		[Test ()]
		public void TestCaptureProjectError ()
		{
			int projectOpened = 0;
			Project testedProject = null;

			Config.EventsBroker.OpenedProjectChanged += (p, pt, f, a) => {
				Assert.AreEqual (testedProject, p);
				projectOpened++;
			};

			testedProject = project;
			Config.EventsBroker.EmitOpenNewProject (project, ProjectType.CaptureProject, settings);
			Assert.AreEqual (1, projectOpened);
			testedProject = null;
			Config.EventsBroker.EmitCaptureError (null, "Error!");
			/* Errors during a capture project should be handled gracefully
			 * closing the current capture project and saving a copy of the
			 * captured video and coded data, without loosing anything */
			Assert.AreEqual (1, Config.DatabaseManager.ActiveDB.Count<Project> ());
			Assert.AreEqual (2, projectOpened);
		}

		[Test ()]
		public void TestCaptureFinished ()
		{
			Config.EventsBroker.CaptureFinished += (c) => {
			};

			Config.EventsBroker.EmitOpenNewProject (project, ProjectType.CaptureProject, settings);
			Config.EventsBroker.EmitCaptureFinished (true);
			Assert.AreEqual (0, Config.DatabaseManager.ActiveDB.Count<Project> ());
			Assert.AreEqual (null, projectsManager.OpenedProject);
			capturerBinMock.Verify (c => c.Close (), Times.Once ());
			capturerBinMock.ResetCalls ();
			gtkMock.Verify (g => g.CloseProject (), Times.Once ());
			gtkMock.ResetCalls ();
			Utils.DeleteProject (project);

			project = Utils.CreateProject ();
			Config.EventsBroker.EmitOpenNewProject (project, ProjectType.CaptureProject, settings);
			Config.EventsBroker.EmitCaptureFinished (false);
			capturerBinMock.Verify (c => c.Close (), Times.Once ());
			/* We are not prompted to quit the capture */
			gtkMock.Verify (g => g.EndCapture (true), Times.Never ());
			gtkMock.Verify (g => g.CloseProject (), Times.Once ());
			gtkMock.Verify (g => g.RemuxFile (It.IsAny<string> (),
				settings.EncodingSettings.OutputFile, VideoMuxerType.Mp4));
			Assert.AreEqual (1, Config.DatabaseManager.ActiveDB.Count<Project> ());
			Assert.AreEqual (project, projectsManager.OpenedProject);
			Assert.AreEqual (ProjectType.FileProject, projectsManager.OpenedProjectType);
		}

		[Test ()]
		public void TestCloseCaptureProject ()
		{
			int projectChanged = 0;

			Config.EventsBroker.OpenedProjectChanged += (p, pt, f, aw) => projectChanged++;
			Config.EventsBroker.EmitCloseOpenedProject ();
			Assert.AreEqual (0, projectChanged);

			Config.EventsBroker.EmitOpenNewProject (project, ProjectType.CaptureProject, settings);
			projectChanged = 0;

			gtkMock.Setup (g => g.EndCapture (false)).Returns (EndCaptureResponse.Return);
			Config.EventsBroker.EmitCloseOpenedProject ();
			Assert.AreEqual (project, projectsManager.OpenedProject);
			Assert.AreEqual (ProjectType.CaptureProject, projectsManager.OpenedProjectType);
			Assert.AreEqual (0, projectChanged);

			gtkMock.Setup (g => g.EndCapture (false)).Returns (EndCaptureResponse.Quit);
			Config.EventsBroker.EmitCloseOpenedProject ();
			Assert.AreEqual (null, projectsManager.OpenedProject);
			Assert.AreEqual (0, Config.DatabaseManager.ActiveDB.Count<Project> ());
			Assert.AreEqual (1, projectChanged);

			Config.EventsBroker.EmitOpenNewProject (project, ProjectType.CaptureProject, settings);
			projectChanged = 0;
			gtkMock.Setup (g => g.EndCapture (false)).Returns (EndCaptureResponse.Save);
			Config.EventsBroker.EmitCloseOpenedProject ();
			Assert.AreEqual (project, projectsManager.OpenedProject);
			Assert.AreEqual (ProjectType.FileProject, projectsManager.OpenedProjectType);
			Assert.AreEqual (1, Config.DatabaseManager.ActiveDB.Count<Project> ());
			Assert.AreEqual (2, projectChanged);
		}

		[Test ()]
		public void TestOpenBadProject ()
		{
			// Test to try opening a project with duration = null
			Assert.Greater (project.Description.FileSet.Count, 0);
			foreach (var file in project.Description.FileSet) {
				file.Duration = null;
			}

			Config.DatabaseManager.ActiveDB.Store<Project> (project);

			Config.EventsBroker.EmitOpenProjectID (project.ID, project);

			mtkMock.Verify (g => g.DiscoverFile (It.IsAny<string> (), true), Times.Exactly (project.Description.FileSet.Count));

			IAnalysisWindow win = winMock.Object;
			gtkMock.Verify (g => g.OpenProject (project, It.IsAny<ProjectType> (),
				It.IsAny<CaptureSettings> (), It.IsAny<EventsFilter> (), out win), Times.Once ());

		}
	}
}

