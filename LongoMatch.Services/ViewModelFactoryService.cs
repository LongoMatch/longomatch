//
//  Copyright (C) 2018 Fluendo S.A.
using System;
using VAS.Core.Interfaces;
using LongoMatch.Core.ViewModel;
using LongoMatch.Core.Store;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace LongoMatch.Services
{
	/// <summary>
	/// View model factory service, creates correctly typed ViewModel instances based on it's model.
	/// This is useful to work with base classes and create child viewmodels without knowing the reference on it.
	/// </summary>
	public class ViewModelFactoryService : IViewModelFactoryService
	{
		/// <summary>
		/// Gets the level.
		/// </summary>
		/// <value>The level.</value>
		public int Level => 40;

		/// <summary>
		/// Gets the name of the service
		/// </summary>
		/// <value>The name of the service</value>
		public string Name => "ViewModelFactoryService";

		/// <summary>
		/// Creates a TimelineEventVM based on a TimelineEvent model
		/// </summary>
		/// <returns>The TimelineEventVM</returns>
		/// <param name="timelineEvent">the timeline event model</param>
		public TimelineEventVM CreateTimelineEventVM (TimelineEvent timelineEvent)
		{
			if (timelineEvent is LMTimelineEvent lmEvent) {
				return new LMTimelineEventVM {
					Model = lmEvent
				};
			}

			throw new InvalidOperationException ("TimelineEvent seems to have an incorrect type");
		}

		/// <summary>
		/// Start this service
		/// </summary>
		/// <returns>true if started correctly, false otherwise</returns>
		public bool Start ()
		{
			return true;
		}

		/// <summary>
		/// Stop this service
		/// </summary>
		/// <returns>true if stopped correctly, false otherwise</returns>
		public bool Stop ()
		{
			return true;
		}
	}
}
