//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using VAS.Core.ViewModel;
using LongoMatch.Core.Store;
namespace LongoMatch.Core.ViewModel
{
	public class PenaltyCardButtonVM : EventButtonVM
	{
		/// <summary>
		/// Gets the typed model
		/// </summary>
		/// <value>The typed model.</value>
		public PenaltyCardButton TypedModel {
			get {
				return (PenaltyCardButton)base.Model;
			}
		}

		/// <summary>
		/// Gets the view.
		/// </summary>
		/// <value>The view.</value>
		public override string View {
			get {
				return "PenaltyCardButtonView";
			}
		}
	}
}
