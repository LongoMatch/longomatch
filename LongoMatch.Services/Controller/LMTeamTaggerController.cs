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
	[Controller (LMDrawingToolState.NAME)]
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
				if (project != null) {
					SetLineup ();
					UpdateLineup ();
				}
			}
		}

		LMTeamTaggerVM TeamTagger {
			get {
				return teamTagger;
			}
			set {
				if (teamTagger != null) {
					teamTagger.PropertyChanged -= HandleTeamTaggerPropertyChanged;
					if (teamTagger.HomeTeam != null) {
						teamTagger.HomeTeam.PropertyChanged -= HandleTeamPropertyChanged;
					}
					if (teamTagger.AwayTeam != null) {
						teamTagger.AwayTeam.PropertyChanged -= HandleTeamPropertyChanged;
					}
				}
				teamTagger = value;
				if (teamTagger != null) {
					teamTagger.PropertyChanged += HandleTeamTaggerPropertyChanged;
					if (teamTagger.HomeTeam != null) {
						teamTagger.HomeTeam.PropertyChanged += HandleTeamPropertyChanged;
					}
					if (teamTagger.AwayTeam != null) {
						teamTagger.AwayTeam.PropertyChanged += HandleTeamPropertyChanged;
					}
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
			} else {
				Project = (viewModel as ILMProjectVM)?.Project;
			}
		}

		public override void Start ()
		{
			base.Start ();
			App.Current.EventsBroker.Subscribe<TagPlayerEvent> (HandleTagPlayerEvent);
			App.Current.EventsBroker.Subscribe<UpdateLineup> (HandleUpdateLineup);
			UpdateLineup ();
		}

		public override void Stop ()
		{
			base.Stop ();
			App.Current.EventsBroker.Unsubscribe<TagPlayerEvent> (HandleTagPlayerEvent);
			App.Current.EventsBroker.Unsubscribe<UpdateLineup> (HandleUpdateLineup);
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
					(teamTagger.SelectionMode == MultiSelectionMode.Single || e.Modifier == ButtonModifier.None)) {
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
				ChangeLineUp (team);
			}
			ClearSelection ();
		}

		void ClearSelection ()
		{
			substitutionPlayer = new KeyValuePair<PlayerVM, TeamVM> (null, null);

			if (teamTagger.HomeTeam != null) {
				foreach (var player in teamTagger.HomeTeam.ViewModels.Where (p => p.Tagged)) {
					player.Tagged = false;
				}
				if (teamTagger.HomeTeam.Selection.Any ()) {
					teamTagger.HomeTeam.Selection.Clear ();
				}
				teamTagger.HomeTeam.Tagged = false;
			}

			if (teamTagger.AwayTeam != null) {
				foreach (var player in teamTagger.AwayTeam.ViewModels.Where (p => p.Tagged)) {
					player.Tagged = false;
				}
				if (teamTagger.AwayTeam.Selection.Any ()) {
					teamTagger.AwayTeam.Selection.Clear ();
				}
				teamTagger.AwayTeam.Tagged = false;
			}
		}

		void HandleTeamPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Formation") {
				if (project != null) {
					UpdateLineup ();
				} else {
					ChangeLineUp (sender as LMTeamVM);
				}
			}
		}

		void HandleTeamTaggerPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != null) {
				if (teamTagger.NeedsSync (e.PropertyName, nameof (teamTagger.SubstitutionMode), sender, teamTagger)) {
					ClearSelection ();
				}

				if (teamTagger.NeedsSync (e.PropertyName, nameof (teamTagger.CurrentTime), sender, teamTagger)) {
					CurrentTimeUpdate ();
				}
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
			homeBenchPlayers.Clear ();
			homeStartingPlayers.Clear ();
			awayStartingPlayers.Clear ();
			awayBenchPlayers.Clear ();

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
		}

		void ChangeLineUp (LMTeamVM team)
		{
			if (team != null) {
				team.PlayingPlayersList = team.ViewModels.OfType<LMPlayerVM> ().
					Take (team.Model.StartingPlayers);

				team.StartingPlayersList = team.PlayingPlayersList;

				team.BenchPlayersList = team.OfType<LMPlayerVM> ().
					Except (team.PlayingPlayersList);
			}
		}

		void UpdateLineup ()
		{
			if (project == null) {
				return;
			}

			List<LMPlayerVM> initialHomePlayerList, initialAwayPlayerList;
			if (isAnalysis) {
				SetLineup ();
			}
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
			if (teamTagger.HomeTeam != null) {
				teamTagger.HomeTeam.PlayingPlayersList = initialHomePlayerList.Take (teamTagger.HomeTeam.Model.StartingPlayers);
				teamTagger.HomeTeam.BenchPlayersList = teamTagger.HomeTeam.OfType<LMPlayerVM> ().Except (
					teamTagger.HomeTeam.PlayingPlayersList);
			}
			if (teamTagger.AwayTeam != null) {
				teamTagger.AwayTeam.PlayingPlayersList = initialAwayPlayerList.Take (teamTagger.AwayTeam.Model.StartingPlayers);
				teamTagger.AwayTeam.BenchPlayersList = teamTagger.AwayTeam.OfType<LMPlayerVM> ().Except (
					teamTagger.AwayTeam.PlayingPlayersList);
			}
		}

		void HandleUpdateLineup (UpdateLineup e)
		{
			if (project != null) {
				UpdateLineup ();
			} else {
				if (teamTagger.HomeTeam != null) {
					ChangeLineUp (teamTagger.HomeTeam);
				}
				if (teamTagger.AwayTeam != null) {
					ChangeLineUp (teamTagger.AwayTeam);
				}
			}
		}
	}
}
