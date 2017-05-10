//
//  Copyright (C) 2017 Fluendo S.A.
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace LongoMatch.Services.ViewModel
{
	/// <summary>
	/// ViewModel used in LMTeamTaggerView, it has a HomeTeam and AwayTeam in order to
	/// render teams in views. It has properties to configure correctly the view.
	/// </summary>
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

		/// <summary>
		/// Method to click a LMPlayerVM
		/// </summary>
		/// <param name="player">Player.</param>
		/// <param name="modifier">Modifier.</param>
		public void PlayerClick (LMPlayerVM player, ButtonModifier modifier)
		{
			App.Current.EventsBroker.Publish (new TagPlayerEvent {
				Player = player,
				Team = GetTeam (player),
				Modifier = modifier,
				Sender = player
			});
		}

		/// <summary>
		/// Resets the team tagger based on a ProjectVM
		/// </summary>
		/// <param name="project">Project View Model</param>
		public void ResetTeamTagger (LMProjectVM project)
		{
			AwayTeam = project.AwayTeam;
			HomeTeam = project.HomeTeam;
			Background = project.Dashboard.Model?.FieldBackground;
		}

		LMTeamVM GetTeam (LMPlayerVM player)
		{
			if (HomeTeam != null) {
				if (HomeTeam.ViewModels.Contains (player)) {
					return HomeTeam;
				}
			}
			if (AwayTeam != null) {
				if (AwayTeam.ViewModels.Contains (player)) {
					return AwayTeam;
				}
			}
			return null;
		}
	}
}
