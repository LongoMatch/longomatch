//
//  Copyright (C) 2018 Fluendo S.A.
using System;
using System.Collections.Generic;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services;
using NUnit.Framework;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;

namespace Tests.Services
{
	[TestFixture]
	public class TestViewModelFactoryService
	{
		VMFactoryServiceTest factoryService;

		[OneTimeSetUp]
		public void OneTimeSetup ()
		{
			factoryService = new VMFactoryServiceTest ();
		}

		[Test]
		public void ViewModelFactoryService_TypeMappings_Initialized ()
		{
			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (ScoreButton)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (ScoreButton)], typeof (ScoreButtonVM));
			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (PenaltyCardButton)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (PenaltyCardButton)], typeof (PenaltyCardButtonVM));
			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (LMPlayer)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (LMPlayer)], typeof (LMPlayerVM));
			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (LMProject)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (LMProject)], typeof (LMProjectVM));
			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (LMTimelineEvent)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (LMTimelineEvent)], typeof (LMTimelineEventVM));
			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (LMTeam)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (LMTeam)], typeof (LMTeamVM));
			Assert.AreEqual (factoryService.TypeMappings.Count, 13);
		}

		[Test]
		public void ViewModelFactoryService_CreateScoreButtonVM ()
		{
			var model = new ScoreButton ();
			var vm = factoryService.CreateViewModel<DashboardButtonVM, DashboardButton> (model);

			Assert.IsTrue (vm is ScoreButtonVM);
		}

		[Test]
		public void ViewModelFactoryService_CreatePenaltyCardButtonVM ()
		{
			var model = new PenaltyCardButton ();
			var vm = factoryService.CreateViewModel<DashboardButtonVM, DashboardButton> (model);

			Assert.IsTrue (vm is PenaltyCardButtonVM);
		}

		[Test]
		public void ViewModelFactoryService_CreateLMPlayerVM ()
		{
			var model = new LMPlayer ();
			var vm = factoryService.CreateViewModel<PlayerVM, Player> (model);

			Assert.IsTrue (vm is LMPlayerVM);
		}

		[Test]
		public void ViewModelFactoryService_CreateLMProjectVM ()
		{
			var model = new LMProject ();
			var vm = factoryService.CreateViewModel<LMProjectVM, LMProject> (model);

			Assert.IsTrue (vm is LMProjectVM);
		}

		[Test]
		public void ViewModelFactoryService_CreateLMTeamVM ()
		{
			var model = new LMTeam ();
			var vm = factoryService.CreateViewModel<TeamVM, Team> (model);

			Assert.IsTrue (vm is LMTeamVM);
		}

		[Test]
		public void CreateTimelineVM_ModelIsLMTimelineEvent_CreatesLMTimelineEventVM ()
		{
			//Arrange
			TimelineEvent ev = new LMTimelineEvent ();

			//Act
			var timelineEventVM = factoryService.CreateViewModel<LMTimelineEventVM, TimelineEvent> (ev);

			//Assert
			Assert.IsTrue (timelineEventVM is LMTimelineEventVM);
		}

		[Test]
		public void CreateTimelineEventVM_ModelIsNotInTypeMappings_CreateBaseClassVM ()
		{
			//Arrange
			TimelineEvent ev = new TestTimelineEvent ();

			//Act
			var timelineEventVM = factoryService.CreateViewModel<TimelineEventVM, TimelineEvent> (ev);

			//Assert
			Assert.IsFalse (timelineEventVM is LMTimelineEventVM);
			Assert.IsTrue (timelineEventVM is TimelineEventVM);
		}
	}

	class TestTimelineEvent : TimelineEvent
	{ }

	/// <summary>
	/// VM Factory service test. Just to get the typeMappings in test
	/// </summary>
	class VMFactoryServiceTest : ViewModelFactoryService
	{
		public new Dictionary<Type, Type> TypeMappings {
			get {
				return base.TypeMappings;
			}
		}
	}
}
