//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Events;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.Interfaces;
using LongoMatch.Services.State;
using LongoMatch.Services.States;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace LongoMatch.Services.Controller
{
	[Controller (NewProjectState.NAME)]
	[Controller (TeamsManagerState.NAME)]
	[Controller (ProjectAnalysisState.NAME)]
	[Controller (LiveProjectAnalysisState.NAME)]
	[Controller (FakeLiveProjectAnalysisState.NAME)]
	[Controller (SubstitutionsEditorState.NAME)]
	[Controller (PlayEditorState.NAME)]
	public class LMTeamTaggerController : ControllerBase
	{
		LMTeamTaggerVM teamTagger;
		KeyValuePair<PlayerVM, TeamVM> substitutionPlayer;
		LMProjectVM project;
		VideoPlayerVM videoPlayer;
		bool isAnalysis;
		Time lastTime;

		List<LMPlayerVM> homeStartingPlayers = new List<LMPlayerVM> ();
		List<LMPlayerVM> homeBenchPlayers = new List<LMPlayerVM> ();
		List<LMPlayerVM> awayStartingPlayers = new List<LMPlayerVM> ();
		List<LMPlayerVM> awayBenchPlayers = new List<LMPlayerVM> ();

		LMProjectVM Project {
			get {
				return project;
			}
			set {
				project = value;
				SetLineup ();
			}
		}

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

		VideoPlayerVM VideoPlayer {
			get {
				return videoPlayer;
			}
			set {
				if (videoPlayer != null) {
					videoPlayer.PropertyChanged -= HandleVideoPlayerPropertyChanged;
				}
				videoPlayer = value;
				if (videoPlayer != null) {
					videoPlayer.PropertyChanged += HandleVideoPlayerPropertyChanged;
				}
			}
		}

		public LMTeamTaggerController ()
		{
			substitutionPlayer = new KeyValuePair<PlayerVM, TeamVM> ();
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			TeamTagger = (viewModel as ILMTeamTaggerVM)?.TeamTagger;
			var analysisVM = viewModel as IAnalysisViewModel;
			if (analysisVM != null) {
				isAnalysis = true;
				Project = analysisVM.Project as LMProjectVM;
				VideoPlayer = analysisVM.VideoPlayer;
			}
		}

		public override void Start ()
		{
			base.Start ();
			App.Current.EventsBroker.Subscribe<TagPlayerEvent> (HandleTagPlayerEvent);
		}

		public override void Stop ()
		{
			base.Stop ();
			App.Current.EventsBroker.Unsubscribe<TagPlayerEvent> (HandleTagPlayerEvent);
		}

		void HandleTagPlayerEvent (TagPlayerEvent e)
		{
			if (teamTagger.SubstitutionMode) {
				if (substitutionPlayer.Key == null) {
					substitutionPlayer = new KeyValuePair<PlayerVM, TeamVM> (e.Player, e.Team);
					e.Player.Tagged = true;
				} else if (substitutionPlayer.Value == e.Team) {
					e.Player.Tagged = true;
					EmitSubstitutionEvent (e.Player as LMPlayerVM, substitutionPlayer.Key as LMPlayerVM, e.Team as LMTeamVM);
				}
			} else {
				if (teamTagger.SelectionMode != MultiSelectionMode.Multiple &&
					(teamTagger.SelectionMode == MultiSelectionMode.Single || !e.HasModifier)) {
					ClearSelection ();
				}
				if (!e.Player.Tagged) {
					e.Team.Selection.Add (e.Player);
					e.Player.Tagged = true;
				} else {
					e.Team.Selection.Remove (e.Player);
					e.Player.Tagged = false;
				}
			}
		}

		void EmitSubstitutionEvent (LMPlayerVM player1, LMPlayerVM player2, LMTeamVM team)
		{
			if (isAnalysis) {
				SubstitutionReason reason;
				var player1Model = player1.Model;
				var player2Model = player2.Model;
				if (team.BenchPlayersList.Contains (player1) && team.BenchPlayersList.Contains (player2)) {
					reason = SubstitutionReason.BenchPositionChange;
				} else if (!team.BenchPlayersList.Contains (player1) && !team.BenchPlayersList.Contains (player2)) {
					reason = SubstitutionReason.PositionChange;
				} else if (team.BenchPlayersList.Contains (player1)) {
					reason = SubstitutionReason.PlayersSubstitution;
				} else {
					player1Model = player2.Model;
					player2Model = player1.Model;
					reason = SubstitutionReason.PlayersSubstitution;
				}
				App.Current.EventsBroker.Publish (new PlayerSubstitutionEvent {
					Team = team.Model,
					Player1 = player1Model,
					Player2 = player2Model,
					SubstitutionReason = reason,
					Time = videoPlayer.CurrentTime
				});
				UpdateLineup ();
			} else {
				team.SubViewModel.ViewModels.Swap (player1, player2);
			}
			ClearSelection ();
		}

		void ClearSelection ()
		{
			substitutionPlayer = new KeyValuePair<PlayerVM, TeamVM> (null, null);
			foreach (var player in teamTagger.HomeTeam.ViewModels.Where (p => p.Tagged)) {
				player.Tagged = false;
			}
			if (teamTagger.HomeTeam.Selection.Any ()) {
				teamTagger.HomeTeam.Selection.Clear ();
			}

			if (teamTagger.AwayTeam != null) {
				foreach (var player in teamTagger.AwayTeam.ViewModels.Where (p => p.Tagged)) {
					player.Tagged = false;
				}
				if (teamTagger.AwayTeam.Selection.Any ()) {
					teamTagger.HomeTeam.Selection.Clear ();
				}
			}
		}

		void HandleTeamTaggerPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (teamTagger.NeedsSync (e.PropertyName, nameof (teamTagger.SubstitutionMode), sender, teamTagger)) {
				ClearSelection ();
			}

			if (teamTagger.NeedsSync (e.PropertyName, nameof (teamTagger.CurrentTime), sender, teamTagger)) {
				//Handle the logic to change LineUp based on the Current Time
				CurrentTimeUpdate ();
			}
		}

		void HandleVideoPlayerPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (videoPlayer.NeedsSync (e.PropertyName, nameof (videoPlayer.CurrentTime), sender, videoPlayer)) {
				teamTagger.CurrentTime = videoPlayer.CurrentTime;
			}
		}

		void CurrentTimeUpdate ()
		{
			if (lastTime == null) {
				UpdateLineup ();
			} else if (teamTagger.CurrentTime != lastTime && project != null) {
				Time start, stop;
				if (lastTime < teamTagger.CurrentTime) {
					start = lastTime;
					stop = teamTagger.CurrentTime;
				} else {
					start = teamTagger.CurrentTime;
					stop = lastTime;
				}
				if (LineupChanged (start, stop)) {
					UpdateLineup ();
				}
			}
			lastTime = teamTagger.CurrentTime;
		}

		bool LineupChanged (Time start, Time stop)
		{
			return project.Timeline.Model.OfType<SubstitutionEvent> ().
				Count (s => s.EventTime > start && s.EventTime <= stop) > 0;
		}

		void SetLineup ()
		{
			foreach (var player in project.Model.Lineup.HomeStartingPlayers) {
				homeStartingPlayers.Add (project.Players.FirstOrDefault (
					p => p.Model == player) as LMPlayerVM);
			}
			foreach (var player in project.Model.Lineup.HomeBenchPlayers) {
				homeBenchPlayers.Add (project.Players.FirstOrDefault (
					p => p.Model == player) as LMPlayerVM);
			}
			foreach (var player in project.Model.Lineup.AwayStartingPlayers) {
				awayStartingPlayers.Add (project.Players.FirstOrDefault (
					p => p.Model == player) as LMPlayerVM);
			}
			foreach (var player in project.Model.Lineup.AwayBenchPlayers) {
				awayBenchPlayers.Add (project.Players.FirstOrDefault (
					p => p.Model == player) as LMPlayerVM);
			}

			teamTagger.HomeTeam.StartingPlayersList = homeStartingPlayers;
			teamTagger.HomeTeam.BenchPlayersList = homeBenchPlayers;
			teamTagger.AwayTeam.StartingPlayersList = awayStartingPlayers;
			teamTagger.AwayTeam.BenchPlayersList = awayBenchPlayers;
		}

		void UpdateLineup ()
		{
			if (project == null) {
				return;
			}

			List<LMPlayerVM> initialHomePlayerList, initialAwayPlayerList;

			initialHomePlayerList = homeStartingPlayers.Concat (homeBenchPlayers).ToList ();
			initialAwayPlayerList = awayStartingPlayers.Concat (awayBenchPlayers).ToList ();

			foreach (var ev in project.Timeline.Model.OfType<SubstitutionEvent> ().
					 Where (e => e.EventTime <= teamTagger.CurrentTime)) {
				if (ev.In != null && ev.Out != null) {
					if (ev.Teams.Contains (project.Model.LocalTeamTemplate)) {
						var playerInVM = initialHomePlayerList.FirstOrDefault (p => p.Model == ev.In);
						var playerOutVM = initialHomePlayerList.FirstOrDefault (p => p.Model == ev.Out);
						initialHomePlayerList.Swap (playerInVM, playerOutVM);
					} else {
						var playerInVM = initialAwayPlayerList.FirstOrDefault (p => p.Model == ev.In);
						var playerOutVM = initialAwayPlayerList.FirstOrDefault (p => p.Model == ev.Out);
						initialAwayPlayerList.Swap (playerInVM, playerOutVM);
					}
				}
			}

			teamTagger.HomeTeam.PlayingPlayersList = initialHomePlayerList.Take (teamTagger.HomeTeam.Model.StartingPlayers);
			teamTagger.HomeTeam.BenchPlayersList = teamTagger.HomeTeam.OfType<LMPlayerVM> ().Except (
				teamTagger.HomeTeam.PlayingPlayersList);
			teamTagger.AwayTeam.PlayingPlayersList = initialAwayPlayerList.Take (teamTagger.HomeTeam.Model.StartingPlayers);
			teamTagger.AwayTeam.BenchPlayersList = teamTagger.AwayTeam.OfType<LMPlayerVM> ().Except (
				teamTagger.AwayTeam.PlayingPlayersList);


			//homeFieldL.ForEach (p => p.Playing = true);
			//homeBenchL.ForEach (p => p.Playing = false);
			//awayFieldL.ForEach (p => p.Playing = true);
			//awayBenchL.ForEach (p => p.Playing = false);

			//teamTagger.HomeTeam.PlayingPlayersList = teamTagger.HomeTeam.OfType<LMPlayerVM> ().Where (p => p.Playing);
			//teamTagger.HomeTeam.BenchPlayersList = teamTagger.HomeTeam.OfType<LMPlayerVM> ().Except (
			//	teamTagger.HomeTeam.PlayingPlayersList);
			//teamTagger.AwayTeam.PlayingPlayersList = teamTagger.AwayTeam.OfType<LMPlayerVM> ().Where (p => p.Playing);
			//teamTagger.AwayTeam.BenchPlayersList = teamTagger.AwayTeam.OfType<LMPlayerVM> ().Except (
			//	teamTagger.AwayTeam.PlayingPlayersList);
		}
	}
}
