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
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		public new PenaltyCardButton Model {
			get {
				return (PenaltyCardButton)base.Model;
			}
			set {
				base.Model = value;
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
