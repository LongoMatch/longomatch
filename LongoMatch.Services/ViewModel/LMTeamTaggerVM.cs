//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace LongoMatch.Services.ViewModel
{
	public class LMTeamTaggerVM : ViewModelBase
	{
		public LMTeamTaggerVM ()
		{
			HomeTeam = new LMTeamVM ();
			AwayTeam = new LMTeamVM ();
		}
		/// <summary>
		/// Gets or sets the home team.
		/// </summary>
		/// <value>The home team.</value>
		public LMTeamVM HomeTeam {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the away team.
		/// </summary>
		/// <value>The away team.</value>
		public LMTeamVM AwayTeam {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the background image
		/// </summary>
		/// <value>The background image</value>
		public Image Background {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:LongoMatch.Services.ViewModel.LMTeamTaggerVM"/> is compact.
		/// </summary>
		/// <value><c>true</c> if compact; otherwise, <c>false</c>.</value>
		public bool Compact {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the current time.
		/// </summary>
		/// <value>The current time.</value>
		public Time CurrentTime {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:LongoMatch.Services.ViewModel.LMTeamTaggerVM"/>
		/// substitution mode.
		/// </summary>
		/// <value><c>true</c> if substitution mode; otherwise, <c>false</c>.</value>
		public bool SubstitutionMode {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:LongoMatch.Services.ViewModel.LMTeamTaggerVM"/> show
		/// substitution buttons.
		/// </summary>
		/// <value><c>true</c> if show substitution buttons; otherwise, <c>false</c>.</value>
		public bool ShowSubstitutionButtons {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:LongoMatch.Services.ViewModel.LMTeamTaggerVM"/> show
		/// teams buttons.
		/// </summary>
		/// <value><c>true</c> if show teams buttons; otherwise, <c>false</c>.</value>
		public bool ShowTeamsButtons {
			get;
			set;
		}
	}
}
