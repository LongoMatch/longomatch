//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.ComponentModel;
using System.Linq;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;

namespace LongoMatch.Services.Controller
{
	/// <summary>
	/// Play editor controller. Is the responsible to edit a timeline event in the playEditor state
	/// </summary>
	[Controller (PlayEditorState.NAME)]
	public class PlayEditorController : ControllerBase
	{
		PlayEditorVM playEditorVM;
		LMTeamTaggerVM teamTagger;

		LMTeamTaggerVM TeamTagger {
			get {
				return teamTagger;
			}
			set {
				if (teamTagger != null) {
					teamTagger.PropertyChanged -= HandleTeamTaggerPropertyChanged;
				}
				teamTagger = value;
				if (teamTagger != null) {
					teamTagger.PropertyChanged += HandleTeamTaggerPropertyChanged;
				}
			}
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			playEditorVM = (PlayEditorVM)viewModel;
			TeamTagger = playEditorVM.TeamTagger;
		}

		void HandleTeamTaggerPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (TeamTagger.NeedsSync (e.PropertyName, "Collection_Selection", sender, TeamTagger.HomeTeam) ||
				TeamTagger.NeedsSync (e.PropertyName, "Collection_Selection", sender, TeamTagger.AwayTeam)) {
				playEditorVM.Play.Players.Replace (TeamTagger.HomeTeam.Selection.Select (p => p.Model)
												   .Concat (TeamTagger.AwayTeam.Selection.Select (p => p.Model)));
			}

			if (teamTagger.NeedsSync (e.PropertyName, nameof (teamTagger.HomeTeam.Tagged), sender, teamTagger.HomeTeam)) {
				UpdatePlayTeams (teamTagger.HomeTeam);
			}

			if (teamTagger.NeedsSync (e.PropertyName, nameof (teamTagger.AwayTeam.Tagged), sender, teamTagger.AwayTeam)) {
				UpdatePlayTeams (teamTagger.AwayTeam);
			}
		}

		void UpdatePlayTeams (LMTeamVM team)
		{
			if (team.Tagged && !playEditorVM.Play.Teams.Contains (team.Model)) {
				playEditorVM.Play.Teams.Add (team.Model);
			} else if (!team.Tagged && playEditorVM.Play.Teams.Contains (team.Model)) {
				playEditorVM.Play.Teams.Remove (team.Model);
			}
		}
	}
}
