//
//  Copyright (C) 2017 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Services.State;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.License;
using VAS.Core.ViewModel;
using VAS.Core.ViewModel.Statistics;

namespace Tests.State
{
	[TestFixture]
	public class TestProjectsManagerState
	{
		ProjectsManagerState state;
		LMProject pastProject, nowProject, futureProject;
		Mock<IStorage> storageMock;
		List<LMProject> projectList;
		Mock<ILicenseLimitationsService> licenseLimitationMock;
		CountLicenseLimitation notLimitedLimitation;
		CountLicenseLimitation limitedLimitation;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			SetupClass.SetUp ();
			futureProject = Utils.CreateProject ();
			futureProject.CreationDate = DateTime.Parse ("12/12/9999");
			pastProject = Utils.CreateProject ();
			pastProject.CreationDate = DateTime.Parse ("12/12/2000");
			nowProject = Utils.CreateProject ();
			nowProject.CreationDate = DateTime.Now;

			projectList = new List<LMProject>{
				futureProject,
				pastProject,
				nowProject
			};

			notLimitedLimitation = new CountLicenseLimitation {
				Count = 0,
				Enabled = false,
				DisplayName = "NotLimited"
			};

			limitedLimitation = new CountLicenseLimitation {
				Count = 0,
				Enabled = true,
				Maximum = 1,
				DisplayName = "Limited"
			};

			storageMock = new Mock<IStorage> ();
			App.Current.DatabaseManager = Mock.Of<IStorageManager> ();
			App.Current.DatabaseManager.ActiveDB = storageMock.Object;
		}

		[SetUp]
		public void SetUp ()
		{
			licenseLimitationMock = new Mock<ILicenseLimitationsService> ();
			licenseLimitationMock.Setup (lim => lim.Get<CountLimitationVM> (
				VASCountLimitedObjects.Projects.ToString ())).Returns (
					new CountLimitationVM {
						Model = notLimitedLimitation
					});
			App.Current.LicenseLimitationsService = licenseLimitationMock.Object;

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
		public async Task ShowState_WithProjects_ProjectsLoadedInCreationOrder ()
		{
			storageMock.Setup (s => s.RetrieveAll<LMProject> ()).Returns (projectList);
			var sortedProjectList = new RangeObservableCollection<LMProject>{
				futureProject,
				nowProject,
				pastProject,
			};
			await state.LoadState (null);

			await state.ShowState ();

			CollectionAssert.AreEqual (sortedProjectList, state.ViewModel.Model);
		}

		[Test]
		public async Task ShowState_WithProjects_FirstSelected ()
		{
			storageMock.Setup (s => s.RetrieveAll<LMProject> ()).Returns (projectList);
			await state.LoadState (null);

			await state.ShowState ();

			Assert.AreEqual (futureProject, state.ViewModel.Selection.First ().Model);
		}

		[Test]
		public async Task ShowState_WithoutProjects_FirstSelected ()
		{
			storageMock.Setup (s => s.RetrieveAll<LMProject> ()).Returns (Enumerable.Empty<LMProject> ());
			await state.LoadState (null);

			await state.ShowState ();

			Assert.IsFalse (state.ViewModel.Selection.Any ());
		}

		[Test]
		public async Task ShowState_NotLimitation_AllProjectsLoadedInCreationOrder ()
		{
			storageMock.Setup (s => s.RetrieveAll<LMProject> ()).Returns (projectList);
			var sortedProjectList = new RangeObservableCollection<LMProject>{
				futureProject,
				nowProject,
				pastProject,
			};
			await state.LoadState (null);

			await state.ShowState ();

			CollectionAssert.AreEqual (sortedProjectList, state.ViewModel.ViewModels.Select (p => p.Model));
		}

		[Test]
		public async Task ShowState_Limitation_LimitedLoadedProjects ()
		{
			licenseLimitationMock.Reset ();

			CountLimitationVM countLimitation = new CountLimitationVM {
				Model = limitedLimitation,
			};
			licenseLimitationMock.Setup (ls => ls.CreateBarChartVM (It.IsAny<string> (), -1, null)).Returns (
				new CountLimitationBarChartVM {
					Limitation = countLimitation,
					BarChart = new TwoBarChartVM (new SeriesVM (), new SeriesVM (), 4)
				}
			);

			storageMock.Setup (s => s.RetrieveAll<LMProject> ()).Returns (projectList);
			var sortedProjectList = new RangeObservableCollection<LMProject>{
				futureProject,
				nowProject,
				pastProject,
			};
			await state.LoadState (null);

			await state.ShowState ();

			CollectionAssert.AreNotEqual (sortedProjectList, state.ViewModel.ViewModels.Select (p => p.Model));
			Assert.AreNotEqual (limitedLimitation.Maximum, state.ViewModel.Model.Count ());
			Assert.AreEqual (limitedLimitation.Maximum, state.ViewModel.ViewModels.Count ());
		}
	}
}
