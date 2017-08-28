//
//  Copyright (C) 2017 Fluendo S.A.
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
	/// <summary>
	/// LM Team Tagger Controller, is the responsible of tag (select) players and teams,
	/// emit substitutions events, update the players in the field/bench based on the current
	/// playing time, formation, etc.
	/// </summary>
	[Controller (NewProjectState.NAME)]
	[Controller (TeamsManagerState.NAME)]
	[Controller (ProjectAnalysisState.NAME)]
	[Controller (LiveProjectAnalysisState.NAME)]
	[Controller (FakeLiveProjectAnalysisState.NAME)]
	[Controller (SubstitutionsEditorState.NAME)]
	[Controller (LightLiveProjectState.NAME)]
	[Controller (PlayEditorState.NAME)]
	[Controller (LMDrawingToolState.NAME)]
	public class LMTeamTaggerController : ControllerBase
	{
		LMTeamTaggerVM teamTagger;
		LMProjectVM project;
		VideoPlayerVM videoPlayer;
		bool isAnalysis;
		KeyValuePair<PlayerVM, TeamVM> substitutionPlayer;
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
			TeamTagger = ((ILMTeamTaggerDealer)viewModel).TeamTagger;
			var analysisVM = viewModel as IAnalysisViewModel;
			if (analysisVM != null) {
				isAnalysis = true;
				Project = (LMProjectVM)analysisVM.Project;
				VideoPlayer = analysisVM.VideoPlayer;
			} else {
				Project = (LMProjectVM)(viewModel as IProjectDealer)?.Project;
			}
		}

		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.Subscribe<TagPlayerEvent> (HandleTagPlayerEvent);
			App.Current.EventsBroker.Subscribe<UpdateLineup> (HandleUpdateLineup);
			App.Current.EventsBroker.Subscribe<EventsDeletedEvent> (HandleEventsDeletedEvent);
			App.Current.EventsBroker.Subscribe<EventEditedEvent> (HandleEventEdited);
			substitutionPlayer = new KeyValuePair<PlayerVM, TeamVM> (null, null);
			lastTime = null;
			UpdateLineup ();
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			App.Current.EventsBroker.Unsubscribe<TagPlayerEvent> (HandleTagPlayerEvent);
			App.Current.EventsBroker.Unsubscribe<UpdateLineup> (HandleUpdateLineup);
			App.Current.EventsBroker.Unsubscribe<EventsDeletedEvent> (HandleEventsDeletedEvent);
			App.Current.EventsBroker.Unsubscribe<EventEditedEvent> (HandleEventEdited);
		}

		void HandleTagPlayerEvent (TagPlayerEvent e)
		{
			if (teamTagger.SubstitutionMode) {
				if (substitutionPlayer.Key == null) {
					substitutionPlayer = new KeyValuePair<PlayerVM, TeamVM> (e.Player, e.Team);
					e.Player.Tagged = true;
				} else if (substitutionPlayer.Key == e.Player) {
					ClearSelection ();
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
				if (Project != null) {
					UpdateLineup ();
				} else {
					ChangeLineUp ((LMTeamVM)sender);
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
			} else if (teamTagger.CurrentTime != lastTime && Project != null) {
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
			return Project.Timeline.Model.OfType<SubstitutionEvent> ().
				Count (s => s.EventTime > start && s.EventTime <= stop) > 0;
		}

		void SetLineup ()
		{
			homeBenchPlayers.Clear ();
			homeStartingPlayers.Clear ();
			awayStartingPlayers.Clear ();
			awayBenchPlayers.Clear ();

			foreach (var player in Project.Model.Lineup.HomeStartingPlayers) {
				homeStartingPlayers.Add (Project.Players.FirstOrDefault (
					p => p.Model == player) as LMPlayerVM);
			}
			foreach (var player in Project.Model.Lineup.HomeBenchPlayers) {
				homeBenchPlayers.Add (Project.Players.FirstOrDefault (
					p => p.Model == player) as LMPlayerVM);
			}
			foreach (var player in Project.Model.Lineup.AwayStartingPlayers) {
				awayStartingPlayers.Add (Project.Players.FirstOrDefault (
					p => p.Model == player) as LMPlayerVM);
			}
			foreach (var player in Project.Model.Lineup.AwayBenchPlayers) {
				awayBenchPlayers.Add (Project.Players.FirstOrDefault (
					p => p.Model == player) as LMPlayerVM);
			}
		}

		void UpdatePlayersPosition ()
		{
			if (Project != null) {
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

		void ChangeLineUp (LMTeamVM team, IEnumerable<LMPlayerVM> players = null)
		{
			if (players == null) {
				players = team.CalledPlayersList;
			}
			var fieldPlayers = players.Take (team.Model.StartingPlayers);
			foreach (var player in fieldPlayers) {
				player.Playing = true;
			}
			var benchPlayers = players.Except (fieldPlayers);
			foreach (var player in benchPlayers) {
				player.Playing = false;
			}
			team.FieldPlayersList = fieldPlayers;
			team.BenchPlayersList = benchPlayers;
		}

		void UpdateLineup ()
		{
			if (Project == null) {
				return;
			}

			List<LMPlayerVM> initialHomePlayerList, initialAwayPlayerList;

			SetLineup ();
			initialHomePlayerList = homeStartingPlayers.Concat (homeBenchPlayers).ToList ();
			initialAwayPlayerList = awayStartingPlayers.Concat (awayBenchPlayers).ToList ();

			foreach (var ev in Project.Timeline.Model.OfType<SubstitutionEvent> ().
					 Where (e => e.EventTime <= teamTagger.CurrentTime)) {
				if (ev.In != null && ev.Out != null) {
					if (ev.Teams.Contains (Project.Model.LocalTeamTemplate)) {
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
				ChangeLineUp (teamTagger.HomeTeam, initialHomePlayerList);
			}
			if (teamTagger.AwayTeam != null) {
				ChangeLineUp (teamTagger.AwayTeam, initialAwayPlayerList);
			}
		}

		void HandleUpdateLineup (UpdateLineup e)
		{
			UpdatePlayersPosition ();
		}

		void HandleEventsDeletedEvent (EventsDeletedEvent e)
		{
			if (e.TimelineEvents.Count (s => s is SubstitutionEvent) != 0) {
				UpdatePlayersPosition ();
			}
		}

		void HandleEventEdited (EventEditedEvent e)
		{
			if (e.TimelineEvent is SubstitutionEvent || e.TimelineEvent is LineupEvent) {
				UpdatePlayersPosition ();
			}
		}
	}
}
