//
//  Copyright (C) 2017 Fluendo S.A.
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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

		public override void SetViewModel (IViewModel viewModel)
		{
			playEditorVM = (PlayEditorVM)viewModel;
			teamTagger = playEditorVM.TeamTagger;
		}

		public override async Task Start ()
		{
			await base.Start ();
			teamTagger.PropertyChanged += HandleTeamTaggerPropertyChanged;
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			teamTagger.PropertyChanged -= HandleTeamTaggerPropertyChanged;
		}

		void HandleTeamTaggerPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (teamTagger.NeedsSync (e.PropertyName, "Collection_Selection", sender, teamTagger.HomeTeam) ||
				teamTagger.NeedsSync (e.PropertyName, "Collection_Selection", sender, teamTagger.AwayTeam)) {
				playEditorVM.Play.Players.Reset (teamTagger.HomeTeam.Selection.Select (p => p.Model)
												   .Concat (teamTagger.AwayTeam.Selection.Select (p => p.Model)));
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
			//FIXME: this is using playEditorVM.Play as Model, it should use TimelineEventVM,
			//when the PlayEditor view is fully migrated to MVVM
			if (team.Tagged && !playEditorVM.Play.Teams.Contains (team.Model)) {
				playEditorVM.Play.Teams.Add (team.Model);
			} else if (!team.Tagged && playEditorVM.Play.Teams.Contains (team.Model)) {
				playEditorVM.Play.Teams.Remove (team.Model);
			}
		}
	}
}
