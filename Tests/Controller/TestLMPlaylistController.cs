//
//  Copyright (C) 2017 FLUENDO
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;

namespace Tests.Controller
{
	public class TestLMPlaylistController
	{
		Mock<IGUIToolkit> mockGuiToolkit;
		VideoPlayerVM videoPlayerVM;
		Mock<IStorageManager> storageManagerMock;
		Mock<IStorage> storageMock;
		Mock<IVideoPlayerController> videoPlayerController;
		LMPlaylistController sut;
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
		public async Task Setup ()
		{
			App.Current.GUIToolkit = mockGuiToolkit.Object;
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
			sut = new LMPlaylistController ();
			sut.SetViewModel (viewModel);
			await sut.Start ();
		}

		[TearDown]
		public async Task TearDown ()
		{
			await sut.Stop ();
			storageMock.ResetCalls ();
			storageManagerMock.ResetCalls ();
			mockGuiToolkit.ResetCalls ();
		}

		[Test]
		public void MoveElements_ToSamePosition1_NoCollectionOrderChange()
		{
			// Arrange
			var a = new PlaylistVM { Model = new Playlist { Name = "a" } };
			var b = new PlaylistVM { Model = new Playlist { Name = "b" } };
			sut.ViewModel.ViewModels.AddRange (new List<PlaylistVM> {a, b});

			// Act
			App.Current.EventsBroker.Publish (new MoveElementsEvent<PlaylistVM> { Index = 1, ElementToMove = a });

			// Assert
			Assert.AreEqual (sut.ViewModel.ViewModels [0].Name, "a");
			Assert.AreEqual (sut.ViewModel.ViewModels [1].Name, "b");
		}

		[Test]
		public void MoveElements_ToSamePosition2_CollectionOrderChange ()
		{
			// Arrange
			var a = new PlaylistVM { Model = new Playlist { Name = "a" } };
			var b = new PlaylistVM { Model = new Playlist { Name = "b" } };
			sut.ViewModel.ViewModels.AddRange (new List<PlaylistVM> { a, b });

			// Act
			App.Current.EventsBroker.Publish (new MoveElementsEvent<PlaylistVM> { Index = 1, ElementToMove = b });

			// Assert
			Assert.AreEqual (sut.ViewModel.ViewModels [0].Name, "a");
			Assert.AreEqual (sut.ViewModel.ViewModels [1].Name, "b");
		}

		[Test]
		public void MoveElements_ToNextPosition_CollectionOrderChange ()
		{
			// Arrange
			var a = new PlaylistVM { Model = new Playlist { Name = "a" } };
			var b = new PlaylistVM { Model = new Playlist { Name = "b" } };
			sut.ViewModel.ViewModels.AddRange (new List<PlaylistVM> { a, b });

			// Act
			App.Current.EventsBroker.Publish (new MoveElementsEvent<PlaylistVM> { Index = 2, ElementToMove = a });

			// Assert
			Assert.AreEqual (sut.ViewModel.ViewModels [0].Name, "b");
			Assert.AreEqual (sut.ViewModel.ViewModels [1].Name, "a");
		}

		[Test]
		public void MoveElements_ToPreviousPosition_CollectionOrderChange ()
		{
			// Arrange
			var a = new PlaylistVM { Model = new Playlist { Name = "a" } };
			var b = new PlaylistVM { Model = new Playlist { Name = "b" } };
			sut.ViewModel.ViewModels.AddRange (new List<PlaylistVM> { a, b });

			// Act
			App.Current.EventsBroker.Publish (new MoveElementsEvent<PlaylistVM> { Index = 0, ElementToMove = b });

			// Assert
			Assert.AreEqual (sut.ViewModel.ViewModels [0].Name, "b");
			Assert.AreEqual (sut.ViewModel.ViewModels [1].Name, "a");
		}
	}
}
