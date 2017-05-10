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
				return Model.Number;
			}
			set {
				Model.Number = value;
			}
		}

		/// <summary>
		/// Gets the player.
		/// </summary>
		/// <value>The player.</value>
		public new LMPlayer Model {
			get {
				return (LMPlayer)base.Model;
			}
			set {
				base.Model = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:LongoMatch.Core.ViewModel.LMPlayerVM"/> is called.
		/// Is In the Field or in the Bench
		/// </summary>
		/// <value><c>true</c> if called; otherwise, <c>false</c>.</value>
		public bool Called {
			get {
				return Model.Playing;
			}
			set {
				Model.Playing = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:LongoMatch.Core.ViewModel.LMPlayerVM"/> is playing.
		/// Is in the Field
		/// </summary>
		/// <value><c>true</c> if playing; otherwise, <c>false</c>.</value>
		[PropertyChanged.DoNotNotify]
		public bool Playing {
			get;
			set;
		}
	}
}
