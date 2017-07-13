//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
		SubstitutionsEditorVM substitutionEditor;
		LMPlayerVM taggedPlayer;

		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.Subscribe<UpdateEvent<SubstitutionEvent>> (HandleSaveSubstitutionEvent);
			App.Current.EventsBroker.Subscribe<UpdateEvent<LineupEvent>> (HandleSaveLineupEvent);
			if (substitutionEditor != null) {
				substitutionEditor.PropertyChanged += HandleViewModelPropertyChanged;
			}
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			App.Current.EventsBroker.Unsubscribe<UpdateEvent<SubstitutionEvent>> (HandleSaveSubstitutionEvent);
			App.Current.EventsBroker.Unsubscribe<UpdateEvent<LineupEvent>> (HandleSaveLineupEvent);
			if (substitutionEditor != null) {
				substitutionEditor.PropertyChanged -= HandleViewModelPropertyChanged;
			}
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			substitutionEditor = (SubstitutionsEditorVM)viewModel;
		}

		void SwitchPlayers ()
		{
			LMPlayerVM inOutPlayer = null;
			if (substitutionEditor.InPlayer.Tagged) {
				inOutPlayer = substitutionEditor.InPlayer;
			} else if (substitutionEditor.OutPlayer.Tagged) {
				inOutPlayer = substitutionEditor.OutPlayer;
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
			if (!substitutionEditor.LineupMode) {
				if (e.PropertyName == "Tagged") {
					var player = sender as LMPlayerVM;
					if (player != null) {
						if (player.Tagged) {
							if (player == substitutionEditor.InPlayer) {
								substitutionEditor.OutPlayer.Tagged = false;
							} else if (player == substitutionEditor.OutPlayer) {
								substitutionEditor.InPlayer.Tagged = false;
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
			e.Object.In = substitutionEditor.InPlayer.Model;
			e.Object.Out = substitutionEditor.OutPlayer.Model;
		}

		void HandleSaveLineupEvent (UpdateEvent<LineupEvent> e)
		{
			e.Object.HomeStartingPlayers = substitutionEditor.TeamTagger.HomeTeam.FieldPlayersList.
				Select (p => p.Model).OfType<LMPlayer> ().ToList ();

			e.Object.HomeBenchPlayers = substitutionEditor.TeamTagger.HomeTeam.BenchPlayersList.
				Select (p => p.Model).OfType<LMPlayer> ().ToList ();

			e.Object.AwayStartingPlayers = substitutionEditor.TeamTagger.AwayTeam.FieldPlayersList.
				Select (p => p.Model).OfType<LMPlayer> ().ToList ();

			e.Object.AwayBenchPlayers = substitutionEditor.TeamTagger.AwayTeam.BenchPlayersList.
				Select (p => p.Model).OfType<LMPlayer> ().ToList ();
		}
	}
}
