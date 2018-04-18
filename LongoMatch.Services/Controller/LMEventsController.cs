// EventsManager.cs
//
//  Copyright (C2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch.Core.Common;
//using LongoMatch.Core.Common;
using LongoMatch.Core.Events;
using LongoMatch.Core.Hotkeys;
using LongoMatch.Core.Store;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Services.Controller;
using KeyAction = VAS.Core.Hotkeys.KeyAction;

namespace LongoMatch.Services
{
	[Controller (ProjectAnalysisState.NAME)]
	[Controller (FakeLiveProjectAnalysisState.NAME)]
	[Controller (LiveProjectAnalysisState.NAME)]
	[Controller (LightLiveProjectState.NAME)]
	public class LMEventsController : EventsController
	{
		LMProjectAnalysisVM viewModel;

		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.Subscribe<LoadTimelineEventEvent<TimelineEvent>> (HandleLoadTimelineEvent);
			App.Current.EventsBroker.Subscribe<PlayerSubstitutionEvent> (HandlePlayerSubstitutionEvent);
			App.Current.EventsBroker.Subscribe<ShowProjectStatsEvent> (HandleShowProjectStatsEvent);
		}

		public override async Task Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<LoadTimelineEventEvent<TimelineEvent>> (HandleLoadTimelineEvent);
			App.Current.EventsBroker.Unsubscribe<PlayerSubstitutionEvent> (HandlePlayerSubstitutionEvent);
			App.Current.EventsBroker.Unsubscribe<ShowProjectStatsEvent> (HandleShowProjectStatsEvent);
			await base.Stop ();
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			yield return new KeyAction (App.Current.HotkeysService.GetByName (GeneralUIHotkeys.DELETE),
										DeleteLoadedEvent);
			yield return new KeyAction (App.Current.HotkeysService.GetByName (LMGeneralUIHotkeys.EDIT_SELECTED_EVENT),
										EditLoadedEvent);
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			this.viewModel = (LMProjectAnalysisVM)viewModel;
			base.SetViewModel (viewModel);
		}

		public override async Task<bool> DeletePlays (IEnumerable<TimelineEvent> plays, bool askConfirmation = true)
		{
			var deleted = await base.DeletePlays (plays, askConfirmation);
			if (deleted) {
				(viewModel?.Project?.Model as LMProject)?.UpdateScore ();
			}
			return deleted;
		}

		// FIXME: remove this when the video capturer is ported to MVVM
		void HandleLoadTimelineEvent (LoadTimelineEventEvent<TimelineEvent> e)
		{
			VideoPlayer.LoadEvent (e.Object, e.Playing);
		}

		void HandlePlayerSubstitutionEvent (PlayerSubstitutionEvent e)
		{
			if (CheckTimelineEventsLimitation ()) {
				return;
			}
			LMTimelineEvent evt;

			try {
				evt = viewModel.Project.Model.SubsitutePlayer (e.Team, e.Player1, e.Player2, e.SubstitutionReason, e.Time);
				App.Current.EventsBroker.Publish (
					new EventCreatedEvent {
						TimelineEvent = evt
					}
				);
			} catch (SubstitutionException ex) {
				App.Current.Dialogs.ErrorMessage (ex.Message);
			}
		}

		void HandleShowProjectStatsEvent (ShowProjectStatsEvent e)
		{
			App.Current.GUIToolkit.ShowProjectStats (e.Project);
		}

		void DeleteLoadedEvent ()
		{
			if (LoadedPlay == null) {
				return;
			}
			App.Current.EventsBroker.Publish (
				new EventsDeletedEvent {
					TimelineEvents = new List<TimelineEvent> { LoadedPlay }
				}
			);
		}

		void EditLoadedEvent ()
		{
			if (LoadedPlay == null) {
				return;
			}
			bool playing = VideoPlayer.Playing;
			VideoPlayer.Pause (false);

			App.Current.EventsBroker.Publish (new EditEventEvent { TimelineEvent = LoadedPlay });

			if (playing) {
				VideoPlayer.Play ();
			}
		}
	}
}