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
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.MVVMC;

namespace LongoMatch.Services.ViewModel
{
	/// <summary>
	/// Play Editor view model, used in PlayEditor View. It needs a TimelineEvent.
	/// It has a LMTeamTaggerVM for the teamtagger component that auto initializes.
	/// </summary>
	public class PlayEditorVM : LMProjectVM, ILMTeamTaggerVM, ILMProjectVM
	{
		LMTimelineEvent play;

		public PlayEditorVM ()
		{
			TeamTagger = new LMTeamTaggerVM ();
			TeamTagger.ShowSubstitutionButtons = false;
			TeamTagger.Compact = true;
			TeamTagger.SelectionMode = MultiSelectionMode.Multiple;
			TeamTagger.ShowTeamsButtons = true;
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
				UpdateTeamTagger ();
			}
		}

		/// <summary>
		/// Gets or sets the edition settings.
		/// </summary>
		/// <value>The edition settings.</value>
		public PlayEventEditionSettings EditionSettings { get; set; }

		/// <summary>
		/// Gets the team tagger.
		/// </summary>
		/// <value>The team tagger.</value>
		public LMTeamTaggerVM TeamTagger {
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
			ResetTeamTagger ();
		}

		void ResetTeamTagger ()
		{
			TeamTagger.AwayTeam = AwayTeam;
			TeamTagger.HomeTeam = HomeTeam;
			TeamTagger.Background = Model.Dashboard?.FieldBackground;
			UpdateTeamTagger ();
		}

		void UpdateTeamTagger ()
		{
			if (Play == null) {
				return;
			}
			foreach (var player in TeamTagger.HomeTeam) {
				if (Play.Players.Contains (player.Model)) {
					player.Tagged = true;
					TeamTagger.HomeTeam.Selection.Add (player);
				}
			}
			foreach (var player in TeamTagger.AwayTeam) {
				if (Play.Players.Contains (player.Model)) {
					player.Tagged = true;
					TeamTagger.AwayTeam.Selection.Add (player);
				}
			}
			foreach (var team in Play.Teams) {
				if (team == TeamTagger.HomeTeam.Model) {
					TeamTagger.HomeTeam.Tagged = true;
				} else if (team == TeamTagger.AwayTeam.Model) {
					TeamTagger.AwayTeam.Tagged = true;
				}
			}
			TeamTagger.CurrentTime = Play.EventTime;
		}
	}
}
