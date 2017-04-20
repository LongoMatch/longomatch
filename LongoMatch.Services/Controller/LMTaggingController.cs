//
//  Copyright (C) 2017 Andoni Morales Alastruey
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using System.Collections.Generic;
using LongoMatch.Services.State;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.Controller;
using VKeyAction = VAS.Core.Hotkeys.KeyAction;
using LKeyAction = LongoMatch.Core.Common.KeyAction;
using VAS.Core.Hotkeys;
using LongoMatch.Core.ViewModel;
using System.Linq;
using LongoMatch.Services.ViewModel;
using LongoMatch.Services.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using LongoMatch.Core.Common;
using LongoMatch.Core.Events;
using Constants = VAS.Core.Common.Constants;
using VAS.Core.Events;

namespace LongoMatch.Services.Controller
{
	[Controller (ProjectAnalysisState.NAME)]
	[Controller (LiveProjectAnalysisState.NAME)]
	[Controller (FakeLiveProjectAnalysisState.NAME)]
	public class LMTaggingController : TaggingController
	{
		LMTeamTaggerVM teamTagger;
		LMPlayerVM substitutionPlayer;
		bool isAnalysis = false;

		public override void SetViewModel (IViewModel viewModel)
		{
			base.SetViewModel (viewModel);
			teamTagger = (viewModel as ILMTeamTaggerVM)?.TeamTagger;
			isAnalysis = (viewModel is IAnalysisViewModel);
		}

		public override IEnumerable<VKeyAction> GetDefaultKeyActions ()
		{
			List<VKeyAction> keyActions = (List<VKeyAction>)base.GetDefaultKeyActions ();

			VKeyAction action = new VKeyAction (new KeyConfig {
				Name = App.Current.Config.Hotkeys.ActionsDescriptions [LKeyAction.LocalPlayer],
				Key = App.Current.Config.Hotkeys.ActionsHotkeys [LKeyAction.LocalPlayer]
			}, () => HandleTeamTagging (teamTagger.HomeTeam, string.Empty));
			keyActions.Add (action);

			action = new VKeyAction (new KeyConfig {
				Name = App.Current.Config.Hotkeys.ActionsDescriptions [LKeyAction.VisitorPlayer],
				Key = App.Current.Config.Hotkeys.ActionsHotkeys [LKeyAction.VisitorPlayer]
			}, () => HandleTeamTagging (teamTagger.AwayTeam, string.Empty));
			keyActions.Add (action);

			return keyActions;
		}

		protected override void HandleClickedPCardEvent (ClickedPCardEvent e)
		{
			if (teamTagger.SelectionMode == MultiSelectionMode.Single || e.Modifier == ButtonModifier.None) {
				ClearSelection ();
			}
			base.HandleClickedPCardEvent (e);
			if (teamTagger.SubstitutionMode) {
				SubstitutePlayer (e.ClickedPlayer, GetTeam (e.ClickedPlayer as LMPlayerVM));
			}
		}

		protected override TimelineEvent CreateTimelineEvent (EventType type, Time start, Time stop, Time eventTime, Image miniature)
		{
			return project.Model.CreateEvent (type, start, stop, eventTime, miniature,
											  project.Model.EventsByType (type).Count + 1);
		}

		void HandleTeamTagging (LMTeamVM team, string taggedPlayer)
		{
			// limitation to the number of temporal contexts that can be created
			int position = taggedPlayer.Length;
			if (position == 3) {
				HandleTaggedPlayer (team, taggedPlayer);
			}

			KeyTemporalContext tempContext = new KeyTemporalContext { };
			for (int i = 0; i < 10; i++) {
				string newTaggedPlayer = taggedPlayer + i;
				VKeyAction action = new VKeyAction (new KeyConfig {
					Name = taggedPlayer,
					Key = App.Current.Keyboard.ParseName (i.ToString ())
				}, () => HandleTeamTagging (team, newTaggedPlayer));
				tempContext.AddAction (action);
			}
			tempContext.Duration = Constants.TEMP_TAGGING_DURATION;
			tempContext.ExpiredTimeAction = () => HandleTaggedPlayer (team, taggedPlayer);

			App.Current.KeyContextManager.AddContext (tempContext);
		}

		void HandleTaggedPlayer (LMTeamVM team, string taggedPlayer)
		{
			if (taggedPlayer != string.Empty) {
				PlayerVM player = team.ViewModels.FirstOrDefault (x => ((LMPlayerVM)x).Number == Convert.ToInt32 (taggedPlayer));
				if (player != null) {
					HandleClickedPCardEvent (new ClickedPCardEvent {
						ClickedPlayer = player,
						Modifier = ButtonModifier.None,
						Sender = player
					});
				}
			}
		}

		void SubstitutePlayer (PlayerVM player, LMTeamVM team)
		{
			if (teamTagger.SubstitutionMode) {
				if (substitutionPlayer == null) {
					substitutionPlayer = player as LMPlayerVM;
					player.Tagged = true;
				} else if (GetTeam (substitutionPlayer) == team) {
					player.Tagged = true;
					EmitSubstitutionEvent (player as LMPlayerVM, substitutionPlayer, team);
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
					Time = VideoPlayer.CurrentTime
				});
			} else {
				team.SubViewModel.ViewModels.Swap (player1, player2);
			}
			ClearSelection ();
		}

		LMTeamVM GetTeam (LMPlayerVM player)
		{
			if (teamTagger.HomeTeam.ViewModels.Contains (player)) {
				return teamTagger.HomeTeam;
			}
			if (teamTagger.AwayTeam.ViewModels.Contains (player)) {
				return teamTagger.AwayTeam;
			}
			return null;
		}

		void ClearSelection ()
		{
			substitutionPlayer = null;
			foreach (PlayerVM player in project.Players) {
				player.Tagged = false;
			}
		}
	}
}
