﻿//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core;
using LongoMatch.Core.Events;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Services.Interfaces;
using LongoMatch.Services.States;
using LongoMatch.Services.ViewModel;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace LongoMatch.Services.Controller
{
	[Controller (TeamsManagerState.NAME)]
	public class LMTeamEditorController : ControllerBase
	{
		LMTeamEditorVM teamEditor;

		public override void SetViewModel (IViewModel viewModel)
		{
			teamEditor = (viewModel as ILMTeamEditorVM)?.TeamEditor;
		}

		public override void Start ()
		{
			base.Start ();
			App.Current.EventsBroker.Subscribe<CreateEvent<LMPlayer>> (HandleCreatePlayer);
			App.Current.EventsBroker.Subscribe<DeleteEvent<LMPlayer>> (HandleDeletePlayers);
		}

		public override void Stop ()
		{
			base.Stop ();
			App.Current.EventsBroker.Subscribe<CreateEvent<LMPlayer>> (HandleCreatePlayer);
			App.Current.EventsBroker.Subscribe<DeleteEvent<LMPlayer>> (HandleDeletePlayers);
		}

		void HandleCreatePlayer (CreateEvent<LMPlayer> e)
		{
			LMTeam model = teamEditor.Team.Model as LMTeam;
			var player = model.AddDefaultItem (model.List.Count);
			var playerVM = teamEditor.Team.ViewModels.FirstOrDefault (p => p.Model == player);
			teamEditor.Team.SelectionReplace (new List<PlayerVM> { playerVM });
			foreach (var p in teamEditor.Team.ViewModels) {
				p.Tagged = false;
			}
			playerVM.Tagged = true;
			App.Current.EventsBroker.Publish (new UpdateLineup ());
		}

		void HandleDeletePlayers (DeleteEvent<LMPlayer> e)
		{
			foreach (var player in teamEditor.Team.Selection) {
				string msg = Catalog.GetString ("Do you want to delete player: ") + player.Name;
				if (App.Current.Dialogs.QuestionMessage (msg, null).Result) {
					teamEditor.Team.ViewModels.Remove (player);
				}
			}
			App.Current.EventsBroker.Publish (new UpdateLineup ());
		}

	}
}
