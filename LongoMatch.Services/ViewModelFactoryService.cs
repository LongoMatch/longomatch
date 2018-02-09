//
//  Copyright (C) 2018 Fluendo S.A.
using System;
using LongoMatch.Core.ViewModel;
using LongoMatch.Core.Store;
using VAS.Services;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.Services
{
	/// <summary>
	/// View model factory service, creates correctly typed ViewModel instances based on it's model.
	/// This is useful to work with base classes and create child viewmodels without knowing the reference on it.
	/// </summary>
	public class ViewModelFactoryService : ViewModelFactoryBaseService
	{
		public ViewModelFactoryService ()
		{
			TypeMappings.Add (typeof (LMTimelineEvent), typeof (LMTimelineEventVM));
			TypeMappings.Add (typeof (ScoreButton), typeof (ScoreButtonVM));
			TypeMappings.Add (typeof (PenaltyCardButton), typeof (PenaltyCardButtonVM));
			TypeMappings.Add (typeof (LMPlayer), typeof (LMPlayerVM));
			TypeMappings.Add (typeof (LMTeam), typeof (LMTeamVM));
			TypeMappings.Add (typeof (LMProject), typeof (LMProjectVM));
		}
	}
}
