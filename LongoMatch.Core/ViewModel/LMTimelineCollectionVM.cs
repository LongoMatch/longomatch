//
//  Copyright (C) 2016 Fluendo S.A.
using System;
using LongoMatch.Core.Store;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace LongoMatch.Core.ViewModel
{
	public class LMTimelineCollectionVM : LimitedCollectionViewModel<TimelineEvent, TimelineEventVM>
	{
		public LMTimelineCollectionVM () : base (false)
		{
		}

		protected override TimelineEventVM CreateInstance (TimelineEvent model)
		{
			var viewModel = base.CreateInstance (model);
			if (model is LineupEvent) {
				StaticViewModels.Add (viewModel);
			}
			return viewModel;
		}
	}
}
