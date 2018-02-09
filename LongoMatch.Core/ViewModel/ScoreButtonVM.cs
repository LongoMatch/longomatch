//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using VAS.Core.ViewModel;
using LongoMatch.Core.Store;

namespace LongoMatch.Core.ViewModel
{
	public class ScoreButtonVM : EventButtonVM
	{
		/// <summary>
		/// Gets or sets the typed model.
		/// </summary>
		/// <value>The model.</value>
		public ScoreButton TypedModel {
			get {
				return (ScoreButton)base.Model;
			}
		}

		/// <summary>
		/// Gets the view.
		/// </summary>
		/// <value>The view.</value>
		public override string View {
			get {
				return "ScoreButtonView";
			}
		}

	}
}
