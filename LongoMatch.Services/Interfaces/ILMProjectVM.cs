//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;

namespace LongoMatch.Services.Interfaces
{
	public interface ILMProjectVM
	{
		/// <summary>
		/// Gets or sets the play.
		/// </summary>
		/// <value>The play.</value>
		LMProjectVM Project { get; }
	}
}
