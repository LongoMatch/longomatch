//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using LongoMatch.Core.Store;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace LongoMatch.Core.ViewModel
{

	/// <summary>
	/// A collection of <see cref="LMPlayer"/> players to be used in a <see cref="Team"/>
	/// </summary>
	public class LMPlayersCollectionVM : CollectionViewModel<Player, PlayerVM>
	{
		public LMPlayersCollectionVM ()
		{
			TypeMappings.Add (typeof (LMPlayerVM), typeof (LMPlayer));
		}
		protected override PlayerVM CreateInstance (Player model)
		{
			return new LMPlayerVM { Model = (LMPlayer)model };
		}
	}
}
