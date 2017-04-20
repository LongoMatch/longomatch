//
//  Copyright (C) 2017 Fluendo S.A.
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.Events;
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
			ShowSubstitutionButtons = true;
			SelectionMode = MultiSelectionMode.Single;
			CurrentTime = new Time (0);
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
		/// Gets or sets the selection mode.
		/// </summary>
		/// <value>The selection mode.</value>
		public MultiSelectionMode SelectionMode {
			set;
			get;
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

		public void PlayerClick (LMPlayerVM player, bool clickWithModif)
		{
			App.Current.EventsBroker.Publish (new TagPlayerEvent {
				Player = player,
				Team = GetTeam (player),
				HasModifier = clickWithModif,
				Sender = player
			});
		}

		LMTeamVM GetTeam (LMPlayerVM player)
		{
			if (HomeTeam.ViewModels.Contains (player)) {
				return HomeTeam;
			}
			if (AwayTeam.ViewModels.Contains (player)) {
				return AwayTeam;
			}
			return null;
		}
	}
}
