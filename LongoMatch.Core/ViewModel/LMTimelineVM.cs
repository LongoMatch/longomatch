//
//  Copyright (C) 2016 Fluendo S.A.
using System.Linq;
using LongoMatch.Core.Store;
using VAS.Core.Filters;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using Predicate = VAS.Core.Filters.Predicate<VAS.Core.ViewModel.TimelineEventVM>;

namespace LongoMatch.Core.ViewModel
{
	public class LMTimelineVM : TimelineVM
	{
		LMTeamVM homeTeamVM, awayTeamVM;

		public LMTimelineVM (LMTeamVM homeTeam, LMTeamVM awayTeam)
		{
			homeTeamVM = homeTeam;
			awayTeamVM = awayTeam;
			Filters.IgnoreEvents = true;
			CategoriesPredicate = new OrPredicate<TimelineEventVM> {
				Name = Catalog.GetString ("Categories")
			};
			TeamsPredicate = new OrPredicate<TimelineEventVM> {
				Name = Catalog.GetString ("Teams"),
			};

			EventTypesTimeline.ViewModels.CollectionChanged += (sender, e) => UpdateEventTypesPredicates ();
			Filters.Add (CategoriesPredicate);
			Filters.Add (TeamsPredicate);
			Filters.IgnoreEvents = false;
		}

		/// <summary>
		/// Gets or sets the categories predicate.
		/// </summary>
		/// <value>The categories predicate.</value>
		public OrPredicate<TimelineEventVM> CategoriesPredicate {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the teams predicate.
		/// </summary>
		/// <value>The teams predicate.</value>
		public OrPredicate<TimelineEventVM> TeamsPredicate {
			get;
			private set;
		}

		public TeamTimelineVM HomeTeamTimelineVM {
			get {
				return TeamsTimeline.First ();
			}
		}

		public TeamTimelineVM AwayTeamTimelineVM {
			get {
				return TeamsTimeline.Last ();
			}
		}

		protected override CollectionViewModel<TimelineEvent, TimelineEventVM> CreateFullTimeline ()
		{
			return new LMTimelineCollectionVM ();
		}

		internal void UpdatePredicates ()
		{
			UpdateTeamsPredicates ();
			UpdateEventTypesPredicates ();
		}

		void UpdateTeamsPredicates ()
		{
			Filters.IgnoreEvents = true;
			TeamsPredicate.Clear ();

			foreach (var team in new LMTeamVM [] { homeTeamVM, awayTeamVM }) {
				var teamPredicate = new OrPredicate<TimelineEventVM> {
					Name = team.Name,
				};
				teamPredicate.Add (new Predicate {
					Name = Catalog.GetString ("Team"),
					Expression = ev =>
						(ev.Model as LMTimelineEvent).TaggedTeams.Contains (team.Model)
				});
				foreach (var player in team) {
					teamPredicate.Add (new Predicate {
						Name = player.Model.Name,
						Expression = ev =>
							(ev.Model as LMTimelineEvent).Players.Contains (player.Model)
					});
				}
				TeamsPredicate.Add (teamPredicate);
			}
			Filters.IgnoreEvents = false;
			RaisePropertyChanged ("Collection", this);
		}

		void UpdateEventTypesPredicates ()
		{
			Filters.IgnoreEvents = true;
			CategoriesPredicate.Clear ();

			foreach (var eventType in EventTypesTimeline) {
				var predicate = new Predicate {
					Name = eventType.EventTypeVM.Name,
					Expression = ev => ev.Model.EventType == eventType.Model
				};
				CategoriesPredicate.Add (predicate);
			}
			Filters.IgnoreEvents = false;
			RaisePropertyChanged ("Collection", this);
		}

		void UpdatePeriodsPredicates ()
		{
		}
	}
}
