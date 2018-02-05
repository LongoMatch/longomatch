//
//  Copyright (C) 2018 Fluendo S.A.
using System;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services;
using NUnit.Framework;
using VAS.Core.Store;

namespace Tests.Services
{
	[TestFixture]
	public class TestViewModelFactoryService
	{
		ViewModelFactoryService factoryService;

		[OneTimeSetUp]
		public void OneTimeSetup ()
		{
			factoryService = new ViewModelFactoryService ();
		}

		[Test]
		public void CreateTimelineVM_ModelIsRAExternalTimelineEvent_CreatesRAExternalTimelineEventVM ()
		{
			//Arrange
			TimelineEvent ev = new LMTimelineEvent ();

			//Act
			var timelineEventVM = factoryService.CreateTimelineEventVM (ev);

			//Assert
			Assert.IsTrue (timelineEventVM is LMTimelineEventVM);
		}

		[Test]
		public void CreateTimelineEventVM_ModelIsNotValid_ThrowsInvalidOperation ()
		{
			//Arrange
			TimelineEvent ev = new TestTimelineEvent ();

			//Act & Arrange
			Assert.Throws<InvalidOperationException> (() => factoryService.CreateTimelineEventVM (ev));
		}
	}

	class TestTimelineEvent : TimelineEvent
	{ }
}
