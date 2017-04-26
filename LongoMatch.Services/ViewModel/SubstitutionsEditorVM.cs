//
//  Copyright (C) 2017 FLUENDO S.A.
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
using System.Threading.Tasks;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.Interfaces;
using VAS.Core.Events;
using VAS.Core.MVVMC;

namespace LongoMatch.Services.ViewModel
{
	public class SubstitutionsEditorVM : LMProjectVM, ILMTeamTaggerVM, ILMProjectVM
	{
		LMTimelineEvent play;

		public SubstitutionsEditorVM ()
		{
			SaveCommand = new Command (Save);
			TeamTagger = new LMTeamTaggerVM ();
			TeamTagger.ShowSubstitutionButtons = false;
			InPlayer = new LMPlayerVM ();
			OutPlayer = new LMPlayerVM ();
		}

		/// <summary>
		/// Gets or sets the timeline event.
		/// </summary>
		/// <value>The timeline event.</value>
		public LMTimelineEvent Play {
			get {
				return play;
			}
			set {
				play = value;
				UpdateViewModels ();
			}
		}

		/// <summary>
		/// Gets the team tagger.
		/// </summary>
		/// <value>The team tagger.</value>
		public LMTeamTaggerVM TeamTagger {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:LongoMatch.Services.ViewModel.SubstitutionsEditorVM"/>
		/// lineup mode.
		/// </summary>
		/// <value><c>true</c> if lineup mode; otherwise, <c>false</c>.</value>
		public bool LineupMode { get; set; }

		/// <summary>
		/// Gets or sets the in player.
		/// </summary>
		/// <value>The in player.</value>
		public LMPlayerVM InPlayer { get; set; }

		/// <summary>
		/// Gets or sets the out player.
		/// </summary>
		/// <value>The out player.</value>
		public LMPlayerVM OutPlayer { get; set; }

		/// <summary>
		/// Command to save a template.
		/// </summary>
		/// <value>The save command.</value>
		[PropertyChanged.DoNotNotify]
		public Command SaveCommand {
			get;
			protected set;
		}

		public LMProjectVM Project {
			get {
				return this;
			}
		}

		protected override void SyncLoadedModel ()
		{
			base.SyncLoadedModel ();
			UpdateViewModels ();
		}

		Task Save ()
		{
			if (LineupMode) {
				return App.Current.EventsBroker.Publish (new UpdateEvent<LineupEvent> {
					Object = play as LineupEvent
				});
			} else {
				return App.Current.EventsBroker.Publish (new UpdateEvent<SubstitutionEvent> {
					Object = play as SubstitutionEvent
				});
			}
		}

		void UpdateViewModels ()
		{
			if (play != null && Model != null) {
				var substitutionEvent = play as SubstitutionEvent;
				if (substitutionEvent != null) {
					InPlayer.Model = substitutionEvent.In;
					OutPlayer.Model = substitutionEvent.Out;
					TeamTagger.CurrentTime = substitutionEvent.EventTime;
					if (substitutionEvent.Teams.Contains (Model.LocalTeamTemplate)) {
						TeamTagger.HomeTeam = HomeTeam;
						TeamTagger.AwayTeam = null;
					} else {
						TeamTagger.AwayTeam = AwayTeam;
						TeamTagger.HomeTeam = null;

					}
				}
				var lineupEvent = play as LineupEvent;
				if (lineupEvent != null) {
					LineupMode = true;
					TeamTagger.HomeTeam = HomeTeam;
					TeamTagger.AwayTeam = AwayTeam;
					TeamTagger.SubstitutionMode = true;
				}
				TeamTagger.Background = Model.Dashboard?.FieldBackground;
			}
		}
	}
}
