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
using System.Linq;
using LongoMatch.Core.Store;
using LongoMatch.Services.Interfaces;
using VAS.Core.Common;
using VAS.Core.MVVMC;

namespace LongoMatch.Services.ViewModel
{
	/// <summary>
	/// Play Editor view model
	/// </summary>
	public class PlayEditorVM : ViewModelBase<LMProject>, ILMTeamTaggerVM
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

		public override LMProject Model {
			get {
				return base.Model;
			}
			set {
				base.Model = value;
				if (value != null) {
					ResetTeamTagger (value);
				}
			}
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
		}

		void ResetTeamTagger (LMProject project)
		{
			TeamTagger.AwayTeam.Model = project.VisitorTeamTemplate;
			TeamTagger.HomeTeam.Model = project.LocalTeamTemplate;
			TeamTagger.Background = project.Dashboard?.FieldBackground;
			UpdateTeamTagger ();
		}

		void UpdateTeamTagger ()
		{
			if (play != null) {
				foreach (var player in TeamTagger.HomeTeam) {
					if (play.Players.Contains (player.Model)) {
						player.Tagged = true;
					}
				}
				foreach (var player in TeamTagger.AwayTeam) {
					if (play.Players.Contains (player.Model)) {
						player.Tagged = true;
					}
				}
			}
		}
	}
}
