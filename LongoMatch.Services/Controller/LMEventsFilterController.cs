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
		protected override void UpdatePredicates ()
		{
			ViewModel.Filters.IgnoreEvents = true;
			UpdateTeamsPredicates ();
			UpdatePeriodsPredicates ();
			UpdateTimersPredicates ();
			UpdateCommonTagsPredicates ();
			UpdateEventTypesPredicates ();
			ViewModel.Filters.IgnoreEvents = false;
			ViewModel.Filters.EmitPredicateChanged ();
		}

		protected override void UpdateTeamsPredicates ()
		{
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
			ViewModel.Filters.IgnoreEvents = false;
			ViewModel.Filters.EmitPredicateChanged ();
		}
	}
}
