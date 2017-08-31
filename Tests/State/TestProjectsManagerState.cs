﻿//
//  Copyright (C) 2017 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Store;
using LongoMatch.Services.State;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;

namespace Tests.State
{
	[TestFixture]
	public class TestProjectsManagerState
	{
		ProjectsManagerState state;
		LMProject pastProject, nowProject, futureProject;
		Mock<IStorage> storageMock;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			futureProject = Utils.CreateProject ();
			futureProject.CreationDate = DateTime.Parse ("12/12/9999");
			pastProject = Utils.CreateProject ();
			pastProject.CreationDate = DateTime.Parse ("12/12/2000");
			nowProject = Utils.CreateProject ();
			nowProject.CreationDate = DateTime.Now;

			var projectList = new List<LMProject>{
				futureProject,
				pastProject,
				nowProject
			};



			storageMock = new Mock<IStorage> ();
			storageMock.Setup (s => s.RetrieveAll<LMProject> ()).Returns (projectList);
			App.Current.DatabaseManager = Mock.Of<IStorageManager> ();
			App.Current.DatabaseManager.ActiveDB = storageMock.Object;
		}

		[SetUp]
		public void SetUp ()
		{
			state = new ProjectsManagerState ();
			state.Panel = Mock.Of<IPanel> ();
		}

		[TearDown]
		public void TearDown ()
		{
			state.Dispose ();
			state = null;
		}

		[Test]
		public async Task LoadState_WithProjects_ProjectsLoadedInCreationOrder ()
		{
			var sortedProjectList = new RangeObservableCollection<LMProject>{
				futureProject,
				nowProject,
				pastProject,
			};

			await state.LoadState (null);

			CollectionAssert.AreEqual (sortedProjectList, state.ViewModel.Model);
		}

		[Test]
		public async Task ShowState_WithProjects_FirstSelected ()
		{
			await state.LoadState (null);

			await state.ShowState ();

			Assert.AreEqual (futureProject, state.ViewModel.Selection.First ().Model);
			Assert.AreEqual (futureProject, state.ViewModel.LoadedProject.Model);
		}

		[Test]
		public async Task ShowState_WithoutProjects_FirstSelected ()
		{
			storageMock.Setup (s => s.RetrieveAll<LMProject> ()).Returns (Enumerable.Empty<LMProject> ());
			await state.LoadState (null);

			await state.ShowState ();

			Assert.IsFalse (state.ViewModel.Selection.Any ());
			Assert.IsNull (state.ViewModel.LoadedProject.Model);
		}
	}
}
