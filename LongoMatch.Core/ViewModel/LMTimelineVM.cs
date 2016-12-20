//
//  Copyright (C) 2016 Fluendo S.A.
using System.Collections.Generic;
using System.Linq;
using VAS.Core.Events;
using VAS.Core.Filters;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using Predicate = VAS.Core.Filters.Predicate<LongoMatch.Core.ViewModel.LMTimelineEventVM>;

namespace LongoMatch.Core.ViewModel
{
	public class LMTimelineVM : TimelineVM
	{
		LMTeamVM homeTeamVM, awayTeamVM;

		public LMTimelineVM (LMTeamVM homeTeam, LMTeamVM awayTeam)
		{
			homeTeamVM = homeTeam;
			awayTeamVM = awayTeam;
			Filters = new AndPredicate<LMTimelineEventVM> ();
			Filters.IgnoreEvents = true;
			CategoriesPredicate = new OrPredicate<LMTimelineEventVM> {
				Name = Catalog.GetString ("Categories")
			};
			TeamsPredicate = new OrPredicate<LMTimelineEventVM> {
				Name = Catalog.GetString ("Teams"),
			};

			ViewModels.CollectionChanged += (sender, e) => UpdateEventTypesPredicates ();
			Filters.Add (CategoriesPredicate);
			Filters.Add (TeamsPredicate);
			Filters.IgnoreEvents = false;
		}

		/// <summary>
		/// Gets or sets the filters used in this timeline.
		/// </summary>
		/// <value>The filters.</value>
		public AndPredicate<LMTimelineEventVM> Filters {
			get;
			protected set;
		}

		/// <summary>
		/// Gets or sets the categories predicate.
		/// </summary>
		/// <value>The categories predicate.</value>
		OrPredicate<LMTimelineEventVM> CategoriesPredicate {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the teams predicate.
		/// </summary>
		/// <value>The teams predicate.</value>
		OrPredicate<LMTimelineEventVM> TeamsPredicate {
			get;
			set;
		}

		/// <summary>
		/// Load a TimelineEvent to the player to start playing it. The EventsController should be the responsible
		/// to Add the Events to the player
		/// </summary>
		/// <param name="vm">LMTimelineEventVM ViewModel</param>
		/// <param name="playing">If set to <c>true</c> playing. Else starts paused</param>
		public void LoadEvent (LMTimelineEventVM vm, bool playing)
		{
			App.Current.EventsBroker.Publish (new LoadTimelineEvent<TimelineEvent> { Object = vm.Model, Playing = playing });
		}

		/// <summary>
		/// Loads a List of Events to the player in order to start playing them, The EventsController should be the responsible
		/// to Add the Events to the player
		/// </summary>
		/// <param name="vm">A list of LMTimelineEventVM</param>
		/// <param name="playing">If set to <c>true</c> playing. Else starts paused</param>
		public void LoadEvent (IEnumerable<LMTimelineEventVM> vm, bool playing)
		{
			App.Current.EventsBroker.Publish (new LoadTimelineEvent<IEnumerable<TimelineEvent>> {
				Object = vm.Select (p => p.Model),
				Playing = playing
			});
		}

		/// <summary>
		/// Unloads the events from the player
		/// </summary>
		public void UnloadEvents ()
		{
			App.Current.EventsBroker.Publish (new LoadTimelineEvent<TimelineEvent> { Object = null, Playing = false });
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
			TeamsPredicate.Add (new Predicate {
				Name = "No team",
				Expression = ev =>
					!ev.Model.TaggedTeams.Any ()
			});
			TeamsPredicate.Add (new Predicate {
				Name = homeTeamVM.Name,
				Expression = ev => ev.Model.TaggedTeams.Contains (homeTeamVM.Model)
			});
			TeamsPredicate.Add (new Predicate {
				Name = awayTeamVM.Name,
				Expression = ev => ev.Model.TaggedTeams.Contains (awayTeamVM.Model)
			});
			Filters.IgnoreEvents = false;
			RaisePropertyChanged ("Collection", this);
		}

		void UpdateEventTypesPredicates ()
		{
			Filters.IgnoreEvents = true;
			CategoriesPredicate.Clear ();

			foreach (var eventType in this) {
				var predicate = new Predicate {
					Name = eventType.EventTypeVM.Name,
					Expression = ev => ev.Model.EventType == eventType.Model
				};
				CategoriesPredicate.Add (predicate);
			}
			Filters.IgnoreEvents = false;
			RaisePropertyChanged ("Collection", this);
		}
	}
}
