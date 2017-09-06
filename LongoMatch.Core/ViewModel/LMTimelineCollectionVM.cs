//
//  Copyright (C) 2016 Fluendo S.A.
using System;
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace LongoMatch.Core.ViewModel
{
	public class LMTimelineCollectionVM : LimitedCollectionViewModel<TimelineEvent, TimelineEventVM>
	{
		protected override TimelineEventVM CreateInstance (TimelineEvent model)
		{
			var viewModel = new LMTimelineEventVM { Model = (LMTimelineEvent)model };
			if (model is LineupEvent) {
				StaticViewModels.Add (viewModel);
			}
			return viewModel;
		}
	}
}
