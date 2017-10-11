//
//  Copyright (C) 2017 Fluendo S.A.

using VAS.Core.Filters;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Services.Controller;
using LongoMatch.Services.State;
using Predicate = VAS.Core.Filters.Predicate<VAS.Core.ViewModel.TimelineEventVM>;
using LongoMatch.Core.ViewModel;
using VAS.Core;
using LongoMatch.Core.Store;
using System.Collections.Specialized;
using System.Threading.Tasks;
using VAS.Core.Events;
using System.Linq;

namespace LongoMatch.Services.Controller
{
	[Controller (ProjectAnalysisState.NAME)]
	/// <summary>
	/// Events filter controller.
	/// This controller manages events filters and filtering for views where projects are used.
	/// </summary>
	public class LMEventsFilterController : EventsFilterController
	{
		protected override void InitializePredicates ()
		{
			ViewModel.Filters.IgnoreEvents = true;
			ViewModel.Filters.Clear ();
			ViewModel.EventsPredicate.Clear ();
			UpdateTeamsPredicates ();
			UpdatePeriodsPredicates ();
			UpdateTimersPredicates ();
			UpdateCommonTagsPredicates ();
			UpdateEventTypesPredicates ();

			ViewModel.EventsPredicate.Add (ViewModel.PeriodsPredicate);
			ViewModel.EventsPredicate.Add (ViewModel.TimersPredicate);
			ViewModel.EventsPredicate.Add (ViewModel.CommonTagsPredicate);
			ViewModel.EventsPredicate.Add (ViewModel.EventTypesPredicate);
			ViewModel.Filters.Add (ViewModel.EventsPredicate);
			ViewModel.Filters.Add (ViewModel.TeamsPredicate);
			ViewModel.Filters.IgnoreEvents = false;
			ViewModel.Filters.EmitPredicateChanged ();
		}

		protected override void UpdateTeamsPredicates ()
		{
			bool oldIgnoreEvents = ViewModel.Filters.IgnoreEvents;
			ViewModel.Filters.IgnoreEvents = true;
			ViewModel.TeamsPredicate.Clear ();

			LMTimelineVM lmTimeline = (LMTimelineVM)ViewModel;
			LMTeamVM homeTeamVM = (LMTeamVM)lmTimeline.HomeTeamTimelineVM.Team;
			LMTeamVM awayTeamVM = (LMTeamVM)lmTimeline.AwayTeamTimelineVM.Team;

			ViewModel.TeamsPredicate.Add (new Predicate {
				Name = Catalog.GetString ("No team / player tagged"),
				Expression = ev =>
					(!(ev.Model as LMTimelineEvent).Teams.Any () &&
					 !(ev.Model as LMTimelineEvent).Players.Any ())
			});

			foreach (var team in new LMTeamVM [] { homeTeamVM, awayTeamVM }) {
				var teamPredicate = new OrPredicate<TimelineEventVM> {
					Name = team.Name,
				};
				teamPredicate.Add (new Predicate {
					Name = Catalog.GetString ("Team tagged"),
					Expression = ev =>
						(ev.Model as LMTimelineEvent).Teams.Contains (team.Model)
				});
				foreach (var player in team) {
					teamPredicate.Add (new Predicate {
						Name = player.Model.Name,
						Expression = ev =>
							(ev.Model as LMTimelineEvent).Players.Contains (player.Model)
					});
				}
				ViewModel.TeamsPredicate.Add (teamPredicate);
			}
			ViewModel.Filters.IgnoreEvents = oldIgnoreEvents;
			if (!ViewModel.Filters.IgnoreEvents) {
				ViewModel.Filters.EmitPredicateChanged ();
			}
		}
	}
}
