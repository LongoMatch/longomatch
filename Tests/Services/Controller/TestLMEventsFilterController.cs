//
//  Copyright (C) 2017 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.Controller;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.Controller;
using Predicate = VAS.Core.Filters.Predicate<VAS.Core.ViewModel.TimelineEventVM>;

namespace Tests.Services.Controller
{
	[TestFixture]
	public class TestLMEventsFilterController
	{
		LMTimelineVM timelineVM;
		LMTeamVM homeTeam;
		LMTeamVM awayTeam;
		LMEventsFilterController eventsFilterController;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			SetupClass.SetUp ();
			Mock<IGUIToolkit> mockGuiToolkit = new Mock<IGUIToolkit> ();
			mockGuiToolkit.Setup (m => m.DeviceScaleFactor).Returns (1);
			App.Current.GUIToolkit = mockGuiToolkit.Object;
			eventsFilterController = new LMEventsFilterController ();
			homeTeam = new LMTeamVM {
				Model = LMTeam.DefaultTemplate (2)
			};
			awayTeam = new LMTeamVM {
				Model = LMTeam.DefaultTemplate (3)
			};
		}

		[SetUp]
		public async Task SetUp ()
		{
			timelineVM = new LMTimelineVM (homeTeam, awayTeam);
			timelineVM.CreateTeamsTimelines (new List<TeamVM> { homeTeam, awayTeam });
			var projectvm = new LMProjectVM { Model = Utils.CreateProject () };
			timelineVM.CreateEventTypeTimelines (projectvm.EventTypes);
			timelineVM.Dashboard = projectvm.Dashboard;

			timelineVM.Project = projectvm;
			eventsFilterController.ViewModel = timelineVM;

			CreateEvents ();

			await eventsFilterController.Start ();
		}

		[TearDown]
		public async Task TearDown ()
		{
			try {
				await eventsFilterController.Stop ();
				eventsFilterController.ViewModel = null;
			} catch (InvalidOperationException) {
				// Ignore the already stopped error
			}
		}

		[Test]
		public async Task SetViewModel_EmptyFilters_TeamFiltersFilled ()
		{
			// Arrange
			try {
				await eventsFilterController.Stop ();
			} catch (InvalidOperationException) {
				// Ignore the already stopped error
			}

			// Act
			await eventsFilterController.Start ();

			// Assert
			// We have two teams, each with 2 & 3 players
			// The TeamsPredicate filter should contain:
			//   - 1 Predicate, for None specified
			//   - 2 OrPredicates, one for each team, with:
			//     - 1 Predicate for "Team tagged"
			//     - 1 Predicate for each player
			Assert.AreEqual (2, timelineVM.Filters.Count);
			Assert.AreEqual (timelineVM.TeamsPredicate, timelineVM.Filters.Elements [1]);
			Assert.AreEqual (3, timelineVM.TeamsPredicate.Count);
			Assert.AreEqual (Catalog.GetString ("No team / player tagged"), timelineVM.TeamsPredicate.Elements [0].Name);
			Assert.AreEqual (homeTeam.Name, timelineVM.TeamsPredicate.Elements [1].Name);
			Assert.AreEqual (3, ((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate.Elements [1]).Elements.Count);
			Assert.AreEqual (Catalog.GetString ("Team tagged"), ((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate.Elements [1]).Elements [0].Name);
			Assert.AreEqual (awayTeam.Name, timelineVM.TeamsPredicate.Elements [2].Name);
			Assert.AreEqual (4, ((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate.Elements [2]).Elements.Count);
			Assert.AreEqual (Catalog.GetString ("Team tagged"), ((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate.Elements [2]).Elements [0].Name);
		}

		[Test]
		public async Task SetViewModel_EmptyFilters_EventsFiltersFilled ()
		{
			// Arrange
			try {
				await eventsFilterController.Stop ();
			} catch (InvalidOperationException) {
				// Ignore the already stopped error
			}

			// Act
			await eventsFilterController.Start ();

			// Assert
			// We have the default dashboard, with a subcategory "Position" added to the first event type, with values "Left", "Center" and "Right"
			// The ParentEventsPredicate filter should contain:
			//   - One OrPredicate for Periods, with:
			//     - One Predicate for each period
			//   - One OrPredicate for Timers, with:
			//     - One Predicate for each timer
			//   - One OrPredicate for Common tags, with:
			//     - One OrPredicate for each tag group, with:
			//       - One Predicate for each tag
			//   - One OrPredicate for EventTypes, with:
			//     - One AndPredicate for each EventType with subcategories
			//     - One Predicate for each EventType without subcategories
			Assert.AreEqual (2, timelineVM.Filters.Count);
			Assert.AreEqual (timelineVM.ParentEventsPredicate, timelineVM.Filters.Elements [0]);
			Assert.AreEqual (4, timelineVM.ParentEventsPredicate.Count);

			Assert.AreEqual (3, timelineVM.PeriodsPredicate.Count);
			Assert.IsTrue (timelineVM.PeriodsPredicate.All (p => p is Predicate));

			Assert.AreEqual (3, timelineVM.TimersPredicate.Count);
			Assert.IsTrue (timelineVM.TimersPredicate.All (p => p is Predicate));

			Assert.AreEqual (2, timelineVM.CommonTagsPredicate.Count);
			Assert.IsTrue (timelineVM.CommonTagsPredicate.All (p => p is OrPredicate<TimelineEventVM>));
			Assert.AreEqual (3, ((OrPredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.First ()).Elements.Count);
			Assert.IsTrue (((OrPredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.First ()).All (p => p is Predicate));
			Assert.AreEqual (2, ((OrPredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.ElementAt (1)).Elements.Count);

			Assert.AreEqual (10, timelineVM.CategoriesPredicate.Elements.OfType<AndPredicate<TimelineEventVM>> ().Count ());
			Assert.AreEqual (5, timelineVM.CategoriesPredicate.Elements.OfType<Predicate> ().Count ());
		}

		[Test]
		public void ApplyFilter_AllFiltersActive_AllVisible ()
		{
			// Arrange

			// Act
			timelineVM.Filters.Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (e => e.Visible));
		}

		[Test]
		public void ApplyFilter_NoFiltersActive_AllVisible ()
		{
			// Arrange

			// Act
			timelineVM.Filters.Active = false;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (e => e.Visible));
		}

		[Test]
		public void ApplyFilter_SubcatAllFiltersActive_AllVisible ()
		{
			// Arrange

			// Act
			timelineVM.Filters.Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (e => e.Visible));
		}

		[Test]
		public void ApplyFilter_SubcatNoFiltersActive_AllVisible ()
		{
			// Arrange

			// Act
			timelineVM.Filters.Active = false;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (e => e.Visible));
		}

		[Test]
		public void ApplyFilter_NoTeamsTagged_OneVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.TeamsPredicate.Elements [0].Active = true;

			// Assert
			Assert.AreEqual (1, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
		}

		[Test]
		public void ApplyFilter_FirstTeamTaggedNoPlayers_ThreeVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate.Elements [1]).Elements [0].Active = true;

			// Assert
			Assert.AreEqual (3, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (5).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyFilter_SecondTeamTaggedNoPlayers_OneVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate.Elements [2]).Elements [0].Active = true;

			// Assert
			Assert.AreEqual (1, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
		}

		[Test]
		public void ApplyFilter_WholeFirstTeam_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.TeamsPredicate.Elements [1].Active = true;

			// Assert
			Assert.AreEqual (4, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (5).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyFilter_FirstPlayerFromSecondTeam_TwoVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate.Elements [2]).Elements [1].Active = true;

			// Assert
			Assert.AreEqual (2, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyFilter_SecondPlayerFromSecondTeam_NoneVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate.Elements [2]).Elements [2].Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (e => !e.Visible));
		}

		[Test]
		public void ApplyFilter_FirstTeamTaggedAndFirstPlayerFromSecondTeam_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate.Elements [1]).Elements [0].Active = true;
			((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate.Elements [2]).Elements [1].Active = true;

			// Assert
			Assert.AreEqual (4, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (5).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyFilter_AllWithPlayerTags_AllExceptOneVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = true;
			timelineVM.Filters.Elements [0].Active = false;

			// Act
			timelineVM.TeamsPredicate.Elements [0].Active = false;

			// Assert
			Assert.AreEqual (6, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsFalse (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (5).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeOne_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.CategoriesPredicate.Elements [0].Active = true;

			// Assert
			Assert.AreEqual (4, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeOneAndTwo_FiveVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.CategoriesPredicate.Elements [0].Active = true;
			timelineVM.CategoriesPredicate.Elements [1].Active = true;

			// Assert
			Assert.AreEqual (5, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeOneNoTags_OneVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			foreach (CompositePredicate<TimelineEventVM> tagGroup in timelineVM.CategoriesPredicate.Elements [0] as CompositePredicate<TimelineEventVM>) {
				tagGroup.Elements [0].Active = true;
			}

			// Assert
			Assert.AreEqual (1, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeOneBad_TwoVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			CompositePredicate<TimelineEventVM> firstEventTypePredicate = ((CompositePredicate<TimelineEventVM>)timelineVM.CategoriesPredicate.Elements [0]);
			CompositePredicate<TimelineEventVM> outcomePredicate = ((CompositePredicate<TimelineEventVM>)firstEventTypePredicate.Elements [0]);

			// Act
			outcomePredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (2, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeOneGoodBad_ThreeVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			CompositePredicate<TimelineEventVM> firstEventTypePredicate = ((CompositePredicate<TimelineEventVM>)timelineVM.CategoriesPredicate.Elements [0]);
			CompositePredicate<TimelineEventVM> outcomePredicate = ((CompositePredicate<TimelineEventVM>)firstEventTypePredicate.Elements [0]);

			// Act
			outcomePredicate.Elements [1].Active = true;
			outcomePredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (3, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeTwoBad_NoneVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			CompositePredicate<TimelineEventVM> secondEventTypePredicate = ((CompositePredicate<TimelineEventVM>)timelineVM.CategoriesPredicate.Elements [1]);
			CompositePredicate<TimelineEventVM> outcomePredicate = ((CompositePredicate<TimelineEventVM>)secondEventTypePredicate.Elements [0]);

			// Act
			outcomePredicate.Elements [2].Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (e => !e.Visible));
		}

		[Test]
		public void ApplyFilter_EventTypeOneGoodBadLeft_OneVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			CompositePredicate<TimelineEventVM> firstEventTypePredicate = ((CompositePredicate<TimelineEventVM>)timelineVM.CategoriesPredicate.Elements [0]);
			CompositePredicate<TimelineEventVM> outcomePredicate = ((CompositePredicate<TimelineEventVM>)firstEventTypePredicate.Elements [0]);
			CompositePredicate<TimelineEventVM> positionPredicate = ((CompositePredicate<TimelineEventVM>)firstEventTypePredicate.Elements [1]);

			// Act
			outcomePredicate.Elements [1].Active = true;
			outcomePredicate.Elements [2].Active = true;
			positionPredicate.Elements [1].Active = true;

			// Assert
			Assert.AreEqual (1, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyFilter_EventTypeTwoAndThree_TwoVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.CategoriesPredicate.Elements [1].Active = true;
			timelineVM.CategoriesPredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (2, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (5).Visible);
		}

		[Test]
		public void TeamsPredicate_ParentActive_AllActive ()
		{
			timelineVM.TeamsPredicate.Active = true;

			Assert.IsTrue (timelineVM.TeamsPredicate.All (p => p.Active));
			Assert.IsTrue (((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate [1]).All (p => p.Active));
			Assert.IsTrue (((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate [2]).All (p => p.Active));
		}

		[Test]
		public void TeamsPredicate_ParentNotActive_AllNotActive ()
		{
			timelineVM.TeamsPredicate.Active = false;

			Assert.IsFalse (timelineVM.TeamsPredicate.All (p => p.Active));
			Assert.IsFalse (((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate [1]).All (p => p.Active));
			Assert.IsFalse (((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate [2]).All (p => p.Active));
		}

		[Test]
		public void TeamsPredicate_OneChildActive_ParentActive ()
		{
			timelineVM.TeamsPredicate.Active = false;
			timelineVM.TeamsPredicate.Elements [1].Active = true;

			Assert.IsFalse (timelineVM.TeamsPredicate.Elements [0].Active);
			Assert.IsTrue (timelineVM.TeamsPredicate.Elements [1].Active);
			Assert.IsTrue (((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate [1]).All (p => p.Active));
			Assert.IsFalse (timelineVM.TeamsPredicate.Elements [2].Active);
			Assert.IsFalse (((CompositePredicate<TimelineEventVM>)timelineVM.TeamsPredicate [2]).All (p => p.Active));
		}

		[Test]
		public void ApplyPeriodsFilter_NoPeriods_OneVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.PeriodsPredicate.Elements [0].Active = true;

			// Assert
			Assert.AreEqual (1, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (5).Visible);
		}

		[Test]
		public void ApplyPeriodsFilter_FirstPeriod_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.PeriodsPredicate.Elements [1].Active = true;

			// Assert
			Assert.AreEqual (4, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyPeriodsFilter_SecondPeriod_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.PeriodsPredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (4, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyPeriodsFilter_FirstAndSecondPeriod_SixVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.PeriodsPredicate.Elements [1].Active = true;
			timelineVM.PeriodsPredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (6, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyPeriodsFilter_AllPeriods_AllVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.PeriodsPredicate.Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (ev => ev.Visible));
		}

		[Test]
		public void ApplyPeriodsFilter_NoPeriodActive_AllVisible ()
		{
			// Arrange

			// Act
			timelineVM.Filters.Active = false;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (ev => ev.Visible));
		}

		[Test]
		public void ApplyTimersFilter_NoTimers_TwoVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.TimersPredicate.Elements [0].Active = true;

			// Assert
			Assert.AreEqual (2, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (5).Visible);
		}

		[Test]
		public void ApplyTimersFilter_FirstTimer_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.TimersPredicate.Elements [1].Active = true;

			// Assert
			Assert.AreEqual (4, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyTimersFilter_SecondTimer_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.TimersPredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (4, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyTimersFilter_FirstAndSecondTimer_FiveVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.TimersPredicate.Elements [1].Active = true;
			timelineVM.TimersPredicate.Elements [2].Active = true;

			// Assert
			Assert.AreEqual (5, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyTimersFilter_AllTimers_AllVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.TimersPredicate.Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (ev => ev.Visible));
		}

		[Test]
		public void ApplyTimersFilter_NoTimerActive_AllVisible ()
		{
			// Arrange

			// Act
			timelineVM.Filters.Active = false;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (ev => ev.Visible));
		}

		[Test]
		public void ApplyCommonTagsFilter_AllCommonTags_AllVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.CommonTagsPredicate.Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (ev => ev.Visible));
		}

		[Test]
		public void ApplyCommonTagsFilter_GeneralTags_AllVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			timelineVM.CommonTagsPredicate.Elements [0].Active = true;

			// Assert
			Assert.IsTrue (timelineVM.FullTimeline.All (ev => ev.Visible));
		}

		[Test]
		public void ApplyCommonTagsFilter_NoneGeneralTags_FourVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.Elements [0]).Elements [0].Active = true;

			// Assert
			Assert.AreEqual (4, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (0).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (5).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (6).Visible);
		}

		[Test]
		public void ApplyCommonTagsFilter_AttackTag_TwoVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.Elements [0]).Elements [1].Active = true;

			// Assert
			Assert.AreEqual (2, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyCommonTagsFilter_DefenseTag_TwoVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.Elements [0]).Elements [2].Active = true;

			// Assert
			Assert.AreEqual (2, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyCommonTagsFilter_AttackDefenseTag_ThreeVisible ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.Elements [0]).Elements [1].Active = true;
			((CompositePredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.Elements [0]).Elements [2].Active = true;

			// Assert
			Assert.AreEqual (3, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (1).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (2).Visible);
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (3).Visible);
		}

		[Test]
		public void ApplyCommonTagsFilter_OtherTag_Thing ()
		{
			// Arrange
			timelineVM.Filters.Active = false;

			// Act
			((CompositePredicate<TimelineEventVM>)timelineVM.CommonTagsPredicate.Elements [1]).Elements [1].Active = true;

			// Assert
			Assert.AreEqual (1, timelineVM.FullTimeline.Count (e => e.Visible));
			Assert.IsTrue (timelineVM.FullTimeline.ElementAt (4).Visible);
		}

		void CreateEvents ()
		{
			AnalysisEventType firstEventType = ((AnalysisEventType)timelineVM.Dashboard.ViewModels.OfType<EventButtonVM> ().First ().EventType.Model);
			AnalysisEventType secondEventType = ((AnalysisEventType)timelineVM.Dashboard.ViewModels.OfType<EventButtonVM> ().ElementAt (1).EventType.Model);
			AnalysisEventType thirdEventType = ((AnalysisEventType)timelineVM.Dashboard.ViewModels.OfType<EventButtonVM> ().ElementAt (2).EventType.Model);
			AnalysisEventType fourthEventType = ((AnalysisEventType)timelineVM.Dashboard.ViewModels.OfType<EventButtonVM> ().ElementAt (3).EventType.Model);

			Dictionary<string, List<Tag>> commonTagsByGroup = timelineVM.Dashboard.Model.CommonTagsByGroup;

			Tag goodTag = firstEventType.TagsByGroup ["Outcome"].ElementAt (0);
			Tag badTag = firstEventType.TagsByGroup ["Outcome"].ElementAt (1);
			Tag leftTag = new Tag ("Left", "Position");
			Tag centerTag = new Tag ("Center", "Position");
			Tag rightTag = new Tag ("Right", "Position");
			firstEventType.Tags.Add (leftTag);
			firstEventType.Tags.Add (centerTag);
			firstEventType.Tags.Add (rightTag);

			Tag otherTag = new Tag ("tag value", "Other group");
			timelineVM.Dashboard.SubViewModel.Model.Add (new TagButton {
				Name = "Other tag",
				Tag = otherTag,
			});

			timelineVM.Project.Timers.First ().Model.Nodes.Add (new TimeNode {
				Start = new Time (0),
				Stop = new Time (10),
			});
			timelineVM.Project.Timers.First ().Model.Nodes.Add (new TimeNode {
				Start = new Time (31),
				Stop = new Time (60),
			});
			timelineVM.Project.Timers.First ().Model.Nodes.Add (new TimeNode {
				Start = new Time (80),
				Stop = new Time (100),
			});

			timelineVM.Project.Timers.Model.Add (new VAS.Core.Store.Timer {
				Name = "timer 2",
				Nodes = new RangeObservableCollection<TimeNode> {
					new TimeNode {
						Start = new Time (0),
						Stop = new Time (50),
					},
				}
			});

			timelineVM.FullTimeline.Model.Replace (new List<LMTimelineEvent> {
				new LMTimelineEvent {
					Name = "event 1, type 1, without player tags, first period left",
					EventType = firstEventType,
					Start = new Time (0), // it starts outside the period
					Stop = new Time (20), // it ends inside the period
				},
				new LMTimelineEvent {
					Name = "event 2, type 1 + Bad + Attack, first team tagged, first period",
					EventType = firstEventType,
					Start = new Time (11), // it starts inside the first period, but not the first timer
					Stop = new Time (30), // it ends inside the first period, but not in the second timer
				},
				new LMTimelineEvent {
					Name = "event 3, type 1 + Good + Defense, second team tagged, between first and second period",
					EventType = firstEventType,
					Start = new Time (40), // it starts inside the 1st period
					Stop = new Time (60), // it ends inside the 2nd period
				},
				new LMTimelineEvent {
					Name = "event 4, type 1 + Bad + Left + Attack + Defense, first player from first team tagged, second period",
					EventType = firstEventType,
					Start = new Time (60), // it starts inside the 2nd period
					Stop = new Time (80), // it ends inside the 2nd period
				},
				new LMTimelineEvent {
					Name = "event 5, type 2 + Good + Other Tag, first player from second team tagged, second period right",
					EventType = secondEventType,
					Start = new Time (80), // it starts inside the 2nd period
					Stop = new Time (100), // it ends outside the 2nd period
				},
				new LMTimelineEvent {
					Name = "event 6, type 3 + Bad, first team and first player from first team tagged, no periods",
					EventType = thirdEventType,
					Start = new Time (900), // It starts outside all periods
					Stop = new Time (1000), // It ends outside all periods, without touching them
				},
				new LMTimelineEvent {
					Name = "event 7, first team and first player from second team tagged, fill all periods",
					EventType = fourthEventType,
					Start = new Time (0), // It starts outside all periods
					Stop = new Time (100), // It ends outside all periods, touching them all
				},
			});
			// Subcats & Tags
			timelineVM.FullTimeline.ElementAt (1).Model.Tags.Add (badTag);
			timelineVM.FullTimeline.ElementAt (1).Model.Tags.Add (commonTagsByGroup [""].First ());
			timelineVM.FullTimeline.ElementAt (2).Model.Tags.Add (goodTag);
			timelineVM.FullTimeline.ElementAt (2).Model.Tags.Add (commonTagsByGroup [""].ElementAt (1));
			timelineVM.FullTimeline.ElementAt (3).Model.Tags.Add (badTag);
			timelineVM.FullTimeline.ElementAt (3).Model.Tags.Add (leftTag);
			timelineVM.FullTimeline.ElementAt (3).Model.Tags.Add (commonTagsByGroup [""].First ());
			timelineVM.FullTimeline.ElementAt (3).Model.Tags.Add (commonTagsByGroup [""].ElementAt (1));
			timelineVM.FullTimeline.ElementAt (4).Model.Tags.Add (goodTag);
			timelineVM.FullTimeline.ElementAt (4).Model.Tags.Add (otherTag);
			timelineVM.FullTimeline.ElementAt (5).Model.Tags.Add (badTag);

			// Teams & Players
			timelineVM.FullTimeline.ElementAt (1).Model.Teams.Add (homeTeam.Model);
			timelineVM.FullTimeline.ElementAt (2).Model.Teams.Add (awayTeam.Model);
			timelineVM.FullTimeline.ElementAt (3).Model.Players.Add (homeTeam.Model.Players [0]);
			timelineVM.FullTimeline.ElementAt (4).Model.Players.Add (awayTeam.Model.Players [0]);
			timelineVM.FullTimeline.ElementAt (5).Model.Teams.Add (homeTeam.Model);
			timelineVM.FullTimeline.ElementAt (5).Model.Players.Add (homeTeam.Model.Players [0]);
			timelineVM.FullTimeline.ElementAt (6).Model.Teams.Add (homeTeam.Model);
			timelineVM.FullTimeline.ElementAt (6).Model.Players.Add (awayTeam.Model.Players [0]);
		}
	}
}
