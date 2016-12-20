//
//  Copyright (C) 2016 Fluendo S.A.
using System;
using LongoMatch.Core.Store;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace LongoMatch.Core.ViewModel
{
	public class LMTimelineCollectionVM : CollectionViewModel<TimelineEvent, TimelineEventVM>
	{
		protected override TimelineEventVM CreateInstance (TimelineEvent model)
		{
			return new LMTimelineEventVM { Model = (LMTimelineEvent)model };
		}
	}
}
