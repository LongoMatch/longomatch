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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch.Core.Common;
using LongoMatch.Core.Events;
using LongoMatch.Core.Hotkeys;
using LongoMatch.Core.Interfaces.Services;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Interfaces.Services;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.Controller;
using KeyAction = VAS.Core.Hotkeys.KeyAction;

namespace LongoMatch.Services
{
	[Controller (ProjectAnalysisState.NAME)]
	[Controller (FakeLiveProjectAnalysisState.NAME)]
	[Controller (LiveProjectAnalysisState.NAME)]
	[Controller (LightLiveProjectState.NAME)]
	public class LMEventsController : EventsController, ILMEventsService
	{
		IEventEditorService editorService;
		LMProjectAnalysisVM viewModel;

		public void Init (IEventEditorService evEditorService)
		{
			editorService = evEditorService;
		}

		public override async Task Start ()
		{
			await base.Start ();
		}

		public override async Task Stop ()
		{
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

		public void SetDefaultCallbacks (LMProjectAnalysisVM projectAnalysisVM)
		{
			projectAnalysisVM.ShowStatsCommand.SetCallback (() => ShowProjectStats (projectAnalysisVM.Project));
			// TODO: This should be in the base class
			projectAnalysisVM.Project.Timeline.LoadEventCommand.SetCallback (args => LoadTimelineEvent ((LoadTimelineEventEvent<TimelineEventVM>)args));
		}

		public void CreatePlayerSubstitutionEvent (LMTeamVM team, LMPlayerVM player1, LMPlayerVM player2, SubstitutionReason substitutionReason, Time time)
		{
			if (CheckTimelineEventsLimitation ()) {
				return;
			}
			LMTimelineEvent evt;

			try {
				evt = viewModel.Project.Model.SubsitutePlayer (team.TypedModel, player1.TypedModel, player2.TypedModel, substitutionReason, time);

				var timelineEventVM = viewModel.Project.Timeline.FullTimeline.Where (x => x.Model == evt).FirstOrDefault ();

				// FIXME: Move to a service call, but keep the event for the LicenseLimitationService
				App.Current.EventsBroker.Publish (
					new EventCreatedEvent {
						TimelineEvent = timelineEventVM
					}
				);
			} catch (SubstitutionException ex) {
				App.Current.Dialogs.ErrorMessage (ex.Message);
			}
		}

		public void ShowProjectStats (LMProjectVM project)
		{
			// FIXME: WTF is that doing there? It should be moved to a state, without all the addins things
			App.Current.GUIToolkit.ShowProjectStats (project.Model);
		}

		void DeleteLoadedEvent ()
		{
			if (LoadedPlay?.Model == null) {
				return;
			}
			// FIXME: Move to a service call, but keep the event for the LicenseLimitationService
			App.Current.EventsBroker.Publish (
				new EventsDeletedEvent {
					TimelineEvents = new List<TimelineEventVM> { LoadedPlay }
				}
			);
		}

		void EditLoadedEvent ()
		{
			if (LoadedPlay?.Model == null) {
				return;
			}
			bool playing = VideoPlayer.Playing;
			VideoPlayer.PauseCommand.Execute (false);

			// FIXME: Not awaited!
			editorService.EditEvent (LoadedPlay);

			if (playing) {
				VideoPlayer.PlayCommand.Execute ();
			}
		}
	}
}