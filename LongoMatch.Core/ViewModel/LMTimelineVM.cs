//  Copyright (C) 2016 Fluendo S.A.
using System.Linq;
using VAS.Core;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

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
			ParentEventsPredicate.Add (PeriodsPredicate);
			ParentEventsPredicate.Add (TimersPredicate);
			ParentEventsPredicate.Add (CommonTagsPredicate);
			ParentEventsPredicate.Add (CategoriesPredicate);
			Filters.Add (ParentEventsPredicate);
			Filters.Add (TeamsPredicate);
			Filters.IgnoreEvents = false;
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

		protected override LimitedCollectionViewModel<TimelineEvent, TimelineEventVM> CreateFullTimeline ()
		{
			return new LMTimelineCollectionVM ();
		}
	}
}
