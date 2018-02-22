//
//  Copyright (C) 2018 Fluendo S.A.
using System;
using System.Threading.Tasks;
using LongoMatch.Core.ViewModel;
using VAS.Core.Interfaces.MVVMC;

namespace LongoMatch.Core.Interfaces.Services
{
	public interface IProjectAnalysisService : IController
	{
		Task<bool> Save ();

		Task<bool> Close ();

		void SetDefaultCallbacks (LMProjectAnalysisVM projectAnalysisVM);
	}
}
