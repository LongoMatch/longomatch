//
//  Copyright (C) 2015 vguzman
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
using System.Linq;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Services;
using Moq;
using NUnit.Framework;

namespace Tests.Services
{
	[TestFixture ()]
	public class TestPlaylistManager
	{

		PlaylistManager plmanager;
		Mock<IGUIToolkit> mockGuiToolkit;
		Mock<IRenderingJobsManager> mockVideoRenderer;
		Mock<IAnalysisWindow> mockAnalysisWindow;
		Mock<IPlayerController> mockPlayerController;

		bool eventLoaded;
		bool playlistElementSelected;


		[TestFixtureSetUp ()]
		public void FixtureSetup ()
		{
			Config.EventsBroker = new EventsBroker ();
			mockAnalysisWindow = new Mock<IAnalysisWindow> ();
			mockPlayerController = new Mock<IPlayerController> ();
			mockAnalysisWindow.SetupGet (m => m.Player).Returns (mockPlayerController.Object);
			mockVideoRenderer = new Mock<IRenderingJobsManager> ();

			Config.EventsBroker.EventLoadedEvent += (TimelineEvent evt) => eventLoaded = true;
			Config.EventsBroker.PlaylistElementSelectedEvent += (Playlist playlist, IPlaylistElement element) => playlistElementSelected = true;
		}

		[SetUp ()]
		public void Setup ()
		{
			mockGuiToolkit = new Mock<IGUIToolkit> ();
			Config.GUIToolkit = mockGuiToolkit.Object;
			Config.RenderingJobsManger = mockVideoRenderer.Object; 

			plmanager = new PlaylistManager ();
			plmanager.Start ();

			OpenProject (new Project ());
			eventLoaded = false;
			playlistElementSelected = false;

		}

		[TearDown ()]
		public void TearDown ()
		{
			plmanager.Stop ();
			mockGuiToolkit.ResetCalls ();
			mockPlayerController.ResetCalls ();
		}

		void OpenProject (Project project = null, ProjectType projectType = ProjectType.FileProject)
		{
			if (project != null) {
				project.Description = new ProjectDescription ();
				project.Description.FileSet = new MediaFileSet ();
			}
			Config.EventsBroker.EmitOpenedProjectChanged (project, projectType, new EventsFilter (project), mockAnalysisWindow.Object);
		}

		[Test ()]
		public void TestNewPlaylist ()
		{
			string name = "name";
			bool changedEvent = false;
			Config.EventsBroker.PlaylistsChangedEvent += (object sender) => changedEvent = true;
			mockGuiToolkit.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ())).Returns (Task.Factory.StartNew (() => name));

			Project project = new Project ();
			Config.EventsBroker.EmitNewPlaylist (project);

			mockGuiToolkit.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());

			Assert.IsTrue (changedEvent);
			Assert.AreEqual (1, project.Playlists.Count);
			Assert.AreEqual (name, project.Playlists [0].Name);

		}

		[Test ()]
		public void TestNewPlaylistNull ()
		{
			// We DON'T Setup the QueryMessage, it will return null, and continue without creating the playlist
			bool changedEvent = false;
			Config.EventsBroker.PlaylistsChangedEvent += (object sender) => changedEvent = true;

			Project project = new Project ();
			Config.EventsBroker.EmitNewPlaylist (project);

			mockGuiToolkit.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());

			Assert.IsFalse (changedEvent);
			Assert.AreEqual (0, project.Playlists.Count);

		}

		[Test ()]
		public void TestNewPlaylistRepeatName ()
		{
			bool changedEvent = false;
			Config.EventsBroker.PlaylistsChangedEvent += (object sender) => changedEvent = true;
			bool called = false;
			string name = "name";
			string differentName = "different name";
			mockGuiToolkit.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()))
				.Returns (() => Task.Factory.StartNew (() => {
				if (called) {
					return differentName;
				} else {
					called = true;
					return name;
				}
			}));

			Project project = new Project ();
			Config.EventsBroker.EmitNewPlaylist (project);
			called = false;
			Config.EventsBroker.EmitNewPlaylist (project);

			mockGuiToolkit.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Exactly (3));

			Assert.IsTrue (changedEvent);
			Assert.AreEqual (2, project.Playlists.Count);
			Assert.AreEqual (name, project.Playlists [0].Name);
			Assert.AreEqual (differentName, project.Playlists [1].Name);

		}

		[Test ()]
		public void TestAddPlaylistElement ()
		{
			bool changedEvent = false;
			Config.EventsBroker.PlaylistsChangedEvent += (object sender) => changedEvent = true;

			var playlist = new Playlist { Name = "name" };
			IPlaylistElement element = new PlaylistPlayElement (new TimelineEvent ());
			var elementList = new List<IPlaylistElement> ();
			elementList.Add (element);

			Config.EventsBroker.EmitAddPlaylistElement (playlist, elementList);

			Assert.IsTrue (changedEvent);
			Assert.AreEqual (elementList, playlist.Elements.ToList ());


		}

		[Test ()]
		public void TestAddPlaylistElementNewPlaylist ()
		{
			mockGuiToolkit.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ())).Returns (Task.Factory.StartNew (() => "name"));

			bool changedEvent = false;
			Config.EventsBroker.PlaylistsChangedEvent += (object sender) => changedEvent = true;
			var elementList = new List<IPlaylistElement> ();

			Config.EventsBroker.EmitAddPlaylistElement (null, elementList);

			mockGuiToolkit.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());

			Assert.IsTrue (changedEvent);


		}

		[Test ()]
		public void TestAddPlaylistElementNullPlaylist ()
		{
			// We DON'T Setup the QueryMessage, it will return null, and continue without creating the playlist
			bool changedEvent = false;
			Config.EventsBroker.PlaylistsChangedEvent += (object sender) => changedEvent = true;
			var elementList = new List<IPlaylistElement> ();

			Config.EventsBroker.EmitAddPlaylistElement (null, elementList);

			mockGuiToolkit.Verify (guitoolkit => guitoolkit.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());

			Assert.IsFalse (changedEvent);
		}

		[Test ()]
		public void TestLoadPlayEvent ()
		{
			TimelineEvent element = new TimelineEvent ();

			Config.EventsBroker.EmitLoadEvent (element);

			mockPlayerController.Verify (player => player.LoadEvent (It.IsAny<MediaFileSet> (), element, element.Start, true), Times.Once ());

			Assert.IsTrue (eventLoaded);

		}

		[Test ()]
		public void TestLoadPlayEventNull ()
		{
			

			TimelineEvent element = null;

			Config.EventsBroker.EmitLoadEvent (element);

			mockPlayerController.Verify (player => player.UnloadCurrentEvent (), Times.Once ());

			Assert.IsTrue (eventLoaded);

		}

		[Test ()]
		public void TestLoadPlayEventSubs ()
		{
			TimelineEvent element = new SubstitutionEvent ();

			Config.EventsBroker.EmitLoadEvent (element);

			mockPlayerController.Verify (player => player.Seek (element.EventTime, true, false, false), Times.Once ());
			mockPlayerController.Verify (player => player.Play (false), Times.Once ());

			Assert.IsTrue (eventLoaded);

		}

		[Test ()]
		public void TestLoadPlayEventFake ()
		{
			var project = new Project ();
			OpenProject (project, ProjectType.FakeCaptureProject);

			TimelineEvent element = new SubstitutionEvent ();

			Config.EventsBroker.EmitLoadEvent (element);

			mockPlayerController.Verify (player => player.Seek (It.IsAny<Time> (), It.IsAny<bool> (), It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());
			mockPlayerController.Verify (player => player.Play (false), Times.Never ());

			Assert.IsFalse (eventLoaded);

		}

		[Test ()]
		public void TestPrev ()
		{
			TimelineEvent element = new TimelineEvent ();
			Config.EventsBroker.EmitLoadEvent (element);
			// loadedPlay != null
			mockPlayerController.ResetCalls ();

			Config.EventsBroker.EmitPreviousPlaylistElement (null);

			mockPlayerController.Verify (player => player.Seek (element.EventTime, true, false, false), Times.Once ());
			Assert.IsFalse (playlistElementSelected);
		}

		[Test ()]
		public void TestPrev2 ()
		{
			// loadedPlay == null

			Config.EventsBroker.EmitPreviousPlaylistElement (null);

			mockPlayerController.Verify (player => player.Seek (It.IsAny<Time> (), It.IsAny<bool> (), It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());
			Assert.IsFalse (playlistElementSelected);
		}


		[Test ()]
		public void TestPrev3 ()
		{
			mockPlayerController.SetupGet (player => player.CurrentTime).Returns (new Time (1000));

			Playlist playlist = new Playlist ();
			PlaylistPlayElement element = new PlaylistPlayElement (new TimelineEvent ());
			element.Play.Start = new Time (0);
			playlist.Elements.Add (element);
			Config.EventsBroker.EmitPlaylistElementSelected (playlist, element);
			mockPlayerController.ResetCalls ();
			playlistElementSelected = false;

			Config.EventsBroker.EmitPreviousPlaylistElement (playlist);

			mockPlayerController.Verify (player => player.Seek (element.Play.Start, true, false, false), Times.Once ());
			Assert.IsFalse (playlistElementSelected);
		}

		[Test ()]
		public void TestPrev4 ()
		{
			mockPlayerController.SetupGet (player => player.CurrentTime).Returns (new Time (499));

			Playlist playlist = new Playlist ();
			PlaylistPlayElement element0 = new PlaylistPlayElement (new TimelineEvent ());
			PlaylistPlayElement element = new PlaylistPlayElement (new TimelineEvent ());
			element.Play.Start = new Time (0);
			playlist.Elements.Add (element0);
			playlist.Elements.Add (element);
			Config.EventsBroker.EmitPlaylistElementSelected (playlist, element);
			mockPlayerController.ResetCalls ();
			playlistElementSelected = false;

			Config.EventsBroker.EmitPreviousPlaylistElement (playlist);

			Assert.IsTrue (playlistElementSelected);
			Assert.AreSame (element0, playlist.Selected);
		}

		[Test ()]
		public void TestPrev5 ()
		{
			Playlist playlist = new Playlist ();
			IPlaylistElement element = new PlaylistImage (new Image (1, 1), new Time (10));
			playlist.Elements.Add (element);
			Config.EventsBroker.EmitPlaylistElementSelected (playlist, element);
			mockPlayerController.ResetCalls ();
			playlistElementSelected = false;

			Config.EventsBroker.EmitPreviousPlaylistElement (playlist);

			Assert.IsFalse (playlistElementSelected);
			mockPlayerController.Verify (player => player.Seek (It.IsAny<Time> (), It.IsAny<bool> (), It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());
		}

		[Test ()]
		public void TestOpenPresentation ()
		{
			Playlist presentation = new Playlist ();
			IPlaylistElement element = new PlaylistPlayElement (new TimelineEvent ());
			IPlaylistElement element2 = new PlaylistPlayElement (new TimelineEvent ());
			presentation.Elements.Add (element);
			presentation.Elements.Add (element2);

			IPlayerController playercontroller = mockPlayerController.Object;
			mockPlayerController.ResetCalls ();

			Config.EventsBroker.EmitOpenedPresentationChanged (presentation, playercontroller);

			Assert.AreSame (playercontroller, plmanager.Player);

			Config.EventsBroker.EmitPlaylistElementSelected (presentation, element);
			mockPlayerController.Verify (player => player.LoadPlaylistEvent (presentation, element), Times.Once ());
		}

		[Test ()]
		public void TestOpenNullPresentation ()
		{
			Playlist presentation = new Playlist ();
			IPlaylistElement element = new PlaylistPlayElement (new TimelineEvent ());
			IPlaylistElement element2 = new PlaylistPlayElement (new TimelineEvent ());
			presentation.Elements.Add (element);
			presentation.Elements.Add (element2);

			IPlayerController playercontroller = mockPlayerController.Object;
			mockPlayerController.ResetCalls ();

			Config.EventsBroker.EmitOpenedPresentationChanged (null, playercontroller);

			Assert.AreSame (playercontroller, plmanager.Player);

			Config.EventsBroker.EmitPlaylistElementSelected (presentation, element);
			mockPlayerController.Verify (player => player.LoadPlaylistEvent (presentation, element), Times.Once ());
		}

		[Test ()]
		public void TestOpenPresentationNullPlayer ()
		{
			Playlist presentation = new Playlist ();
			IPlaylistElement element = new PlaylistPlayElement (new TimelineEvent ());
			IPlaylistElement element2 = new PlaylistPlayElement (new TimelineEvent ());
			presentation.Elements.Add (element);
			presentation.Elements.Add (element2);

			IPlayerController playercontroller = mockPlayerController.Object;
			mockPlayerController.ResetCalls ();

			Config.EventsBroker.EmitOpenedPresentationChanged (presentation, null);

			Config.EventsBroker.EmitPlaylistElementSelected (presentation, element);

			mockPlayerController.Verify (player => player.LoadPlaylistEvent (presentation, element), Times.Never ());
		}

		[Test ()]
		public void TestOpenNullPresentationNullPlayer ()
		{
			Playlist presentation = new Playlist ();
			IPlaylistElement element = new PlaylistPlayElement (new TimelineEvent ());
			IPlaylistElement element2 = new PlaylistPlayElement (new TimelineEvent ());
			presentation.Elements.Add (element);
			presentation.Elements.Add (element2);

			mockPlayerController.ResetCalls ();

			Config.EventsBroker.EmitOpenedPresentationChanged (null, null);

			Config.EventsBroker.EmitPlaylistElementSelected (presentation, element);

			mockPlayerController.Verify (player => player.LoadPlaylistEvent (presentation, element), Times.Never ());
		}
	}
}
