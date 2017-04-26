//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.ComponentModel;
using System.Linq;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;

namespace LongoMatch.Services.Controller
{
	/// <summary>
	/// Substitutions editor controller. Is the responsible to edit a substituion or a lineup
	/// in the Substitution editor state.
	/// </summary>
	[Controller (SubstitutionsEditorState.NAME)]
	public class SubstitutionsEditorController : ControllerBase
	{
		SubstitutionsEditorVM viewModel;
		LMPlayerVM taggedPlayer;

		SubstitutionsEditorVM ViewModel {
			get {
				return viewModel;
			}

			set {
				viewModel = value;
				if (viewModel != null) {
					viewModel.PropertyChanged += HandleViewModelPropertyChanged;
				}
			}
		}

		public override void Start ()
		{
			base.Start ();
			App.Current.EventsBroker.Subscribe<UpdateEvent<SubstitutionEvent>> (HandleSaveSubstitutionEvent);
			App.Current.EventsBroker.Subscribe<UpdateEvent<LineupEvent>> (HandleSaveLineupEvent);
		}

		public override void Stop ()
		{
			base.Stop ();
			App.Current.EventsBroker.Unsubscribe<UpdateEvent<SubstitutionEvent>> (HandleSaveSubstitutionEvent);
			App.Current.EventsBroker.Unsubscribe<UpdateEvent<LineupEvent>> (HandleSaveLineupEvent);
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			ViewModel = (SubstitutionsEditorVM)viewModel;
		}

		void SwitchPlayers ()
		{
			LMPlayerVM inOutPlayer = null;
			if (ViewModel.InPlayer.Tagged) {
				inOutPlayer = ViewModel.InPlayer;
			}
			if (ViewModel.OutPlayer.Tagged) {
				inOutPlayer = ViewModel.OutPlayer;
			}

			if (inOutPlayer != null && taggedPlayer != null) {
				inOutPlayer.Tagged = false;
				taggedPlayer.Tagged = false;
				inOutPlayer.Model = taggedPlayer.Model;
				taggedPlayer = null;
			}
		}

		void HandleViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (!ViewModel.LineupMode) {
				if (e.PropertyName == "Tagged") {
					var player = sender as LMPlayerVM;
					if (player != null) {
						if (player.Tagged) {
							if (player == ViewModel.InPlayer) {
								ViewModel.OutPlayer.Tagged = false;
							} else if (player == ViewModel.OutPlayer) {
								ViewModel.InPlayer.Tagged = false;
							} else {
								taggedPlayer = player;
							}
							SwitchPlayers ();
						} else {
							if (player == taggedPlayer) {
								taggedPlayer = null;
							}
						}
					}
				}
			}
		}

		void HandleSaveSubstitutionEvent (UpdateEvent<SubstitutionEvent> e)
		{
			e.Object.In = ViewModel.InPlayer.Model;
			e.Object.Out = ViewModel.OutPlayer.Model;
		}

		void HandleSaveLineupEvent (UpdateEvent<LineupEvent> e)
		{
			e.Object.HomeStartingPlayers = ViewModel.TeamTagger.HomeTeam.StartingPlayersList.
				Select (p => p.Model).OfType<LMPlayer> ().ToList ();

			e.Object.HomeBenchPlayers = ViewModel.TeamTagger.HomeTeam.BenchPlayersList.
				Select (p => p.Model).OfType<LMPlayer> ().ToList ();

			e.Object.AwayStartingPlayers = ViewModel.TeamTagger.AwayTeam.StartingPlayersList.
				Select (p => p.Model).OfType<LMPlayer> ().ToList ();

			e.Object.AwayBenchPlayers = ViewModel.TeamTagger.AwayTeam.BenchPlayersList.
				Select (p => p.Model).OfType<LMPlayer> ().ToList ();
		}
	}
}
