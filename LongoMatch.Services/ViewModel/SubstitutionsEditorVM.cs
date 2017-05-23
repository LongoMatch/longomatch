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
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace LongoMatch.Services.ViewModel
{
	/// <summary>
	/// A ViewModel used in SubstitutionEditor View, it needs a TimelineEvent of type SubstitutionEvent
	/// or LineupEvent, it has a TeamTaggerVM that auto initializes based on the event type
	/// </summary>
	public class SubstitutionsEditorVM : ViewModelBase, ILMTeamTaggerDealer, IProjectDealer
	{
		LMTimelineEvent play;
		LMProjectVM project;

		public SubstitutionsEditorVM ()
		{
			SaveCommand = new AsyncCommand (Save);
			TeamTagger = new LMTeamTaggerVM ();
			TeamTagger.ShowSubstitutionButtons = false;
			InPlayer = new LMPlayerVM ();
			OutPlayer = new LMPlayerVM ();
		}

		public LMProjectVM Project {
			get {
				return project;
			}
			set {
				project = value;
				UpdateViewModels ();
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
				UpdateViewModels ();
			}
		}

		/// <summary>
		/// Gets the team tagger.
		/// </summary>
		/// <value>The team tagger.</value>
		public LMTeamTaggerVM TeamTagger {
			get;
			protected set;
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
		/// Command to save a Event.
		/// </summary>
		/// <value>The save command.</value>
		[PropertyChanged.DoNotNotify]
		public Command SaveCommand {
			get;
			protected set;
		}

		ProjectVM IProjectDealer.Project {
			get {
				return Project;
			}
		}

		async Task Save ()
		{
			if (LineupMode) {
				await App.Current.EventsBroker.Publish (new UpdateEvent<LineupEvent> {
					Object = Play as LineupEvent
				});
			} else {
				await App.Current.EventsBroker.Publish (new UpdateEvent<SubstitutionEvent> {
					Object = Play as SubstitutionEvent
				});
			}
		}

		void UpdateViewModels ()
		{
			if (Play != null && Project.Model != null) {
				var substitutionEvent = Play as SubstitutionEvent;
				if (substitutionEvent != null) {
					InPlayer.Model = substitutionEvent.In;
					OutPlayer.Model = substitutionEvent.Out;
					TeamTagger.CurrentTime = substitutionEvent.EventTime;
					if (substitutionEvent.Teams.Contains (Project.Model.LocalTeamTemplate)) {
						TeamTagger.HomeTeam = Project.HomeTeam;
						TeamTagger.AwayTeam = null;
					} else {
						TeamTagger.AwayTeam = Project.AwayTeam;
						TeamTagger.HomeTeam = null;

					}
				}
				var lineupEvent = Play as LineupEvent;
				if (lineupEvent != null) {
					LineupMode = true;
					TeamTagger.HomeTeam = Project.HomeTeam;
					TeamTagger.AwayTeam = Project.AwayTeam;
					TeamTagger.SubstitutionMode = true;
				}
				TeamTagger.Background = Project.Model.Dashboard?.FieldBackground;
			}
		}
	}
}
