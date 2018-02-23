//
//  Copyright (C) 2018 Fluendo S.A.

using LongoMatch.Core.Common;
using LongoMatch.Core.ViewModel;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;

namespace LongoMatch.Core.Interfaces.Services
{
	public interface ILMEventsService : IController
	{
		void ShowProjectStats (LMProjectVM project);

		void CreatePlayerSubstitutionEvent (LMTeamVM team, LMPlayerVM player1, LMPlayerVM player2, SubstitutionReason substitutionReason, Time time);

		void SetDefaultCallbacks (LMProjectAnalysisVM projectAnalysisVM);
	}
}
