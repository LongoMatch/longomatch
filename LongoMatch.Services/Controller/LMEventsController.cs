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
using LongoMatch.Core.Common;
using LongoMatch.Core.Events;
using LongoMatch.Core.Store;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Services.Controller;
using KeyAction = VAS.Core.Hotkeys.KeyAction;
using LMKeyAction = LongoMatch.Core.Common.KeyAction;

namespace LongoMatch.Services
{
	[Controller (ProjectAnalysisState.NAME)]
	public class LMEventsController : EventsController
	{
		TimelineEvent loadedPlay;
		IVideoPlayerController player;
		LMProjectAnalysisVM viewModel;
		bool started;

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (started) {
				Log.Error ("Controller disposed wihtout being stopped: " + this);
				Stop ();
			}
		}

		public override void Start ()
		{
			if (started) {
				throw new InvalidOperationException ("Controller is already started");
			}
			base.Start ();
			App.Current.EventsBroker.Subscribe<EventLoadedEvent> (HandlePlayLoaded);
			App.Current.EventsBroker.Subscribe<KeyPressedEvent> (HandleKeyPressed);
			App.Current.EventsBroker.Subscribe<PlayerSubstitutionEvent> (HandlePlayerSubstitutionEvent);
			App.Current.EventsBroker.Subscribe<ShowProjectStatsEvent> (HandleShowProjectStatsEvent);
			started = true;
		}

		public override void Stop ()
		{
			if (!started) {
				throw new InvalidOperationException ("Controller is already stoped");
			}
			base.Stop ();
			App.Current.EventsBroker.Unsubscribe<EventLoadedEvent> (HandlePlayLoaded);
			App.Current.EventsBroker.Unsubscribe<KeyPressedEvent> (HandleKeyPressed);
			App.Current.EventsBroker.Unsubscribe<PlayerSubstitutionEvent> (HandlePlayerSubstitutionEvent);
			App.Current.EventsBroker.Unsubscribe<ShowProjectStatsEvent> (HandleShowProjectStatsEvent);
			started = false;
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			this.viewModel = (LMProjectAnalysisVM)(viewModel as dynamic);
			base.SetViewModel (viewModel);
		}

		public IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		void HandlePlayerSubstitutionEvent (PlayerSubstitutionEvent e)
		{
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

		void HandleKeyPressed (KeyPressedEvent e)
		{
			LMKeyAction action;

			try {
				action = App.Current.Config.Hotkeys.ActionsHotkeys.GetKeyByValue (e.Key);
			} catch (Exception ex) {
				/* The dictionary contains 2 equal values for different keys */
				Log.Exception (ex);
				return;
			}

			if (action == LMKeyAction.None || loadedPlay == null) {
				return;
			}

			switch (action) {
			case LMKeyAction.EditEvent:
				bool playing = player.Playing;
				player.Pause ();
				App.Current.GUIToolkit.EditPlay (loadedPlay, viewModel.Project.Model, true, true, true, true);
				if (playing) {
					player.Play ();
				}
				break;
			case LMKeyAction.DeleteEvent:
				App.Current.EventsBroker.Publish (
					new EventsDeletedEvent {
						TimelineEvents = new List<TimelineEvent> { loadedPlay }
					}
				);
				break;
			}
		}

		void HandlePlayLoaded (EventLoadedEvent e)
		{
			loadedPlay = e.TimelineEvent;
		}

		void HandleShowProjectStatsEvent (ShowProjectStatsEvent e)
		{
			App.Current.GUIToolkit.ShowProjectStats (e.Project);
		}

	}
}