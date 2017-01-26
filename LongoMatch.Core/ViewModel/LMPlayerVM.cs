//
//  Copyright (C) 2016 Fluendo S.A.
using LongoMatch.Core.Store;
using VAS.Core.ViewModel;

namespace LongoMatch.Core.ViewModel
{
	/// <summary>
	/// A ViewModel for <see cref="LMPlayer"/>.
	/// </summary>
	public class LMPlayerVM : PlayerVM
	{
		/// <summary>
		/// Gets or sets the number of the player.
		/// </summary>
		/// <value>The number.</value>
		public int Number {
			get {
				return Player.Number;
			}
			set {
				Player.Number = value;
			}
		}

		/// <summary>
		/// Gets the player.
		/// </summary>
		/// <value>The player.</value>
		public LMPlayer Player {
			get {
				return Model as LMPlayer;
			}
		}
	}
}
