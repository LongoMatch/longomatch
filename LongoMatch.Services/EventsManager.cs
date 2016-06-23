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
using LongoMatch.Core.Common;
using LongoMatch.Core.Events;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using Constants = LongoMatch.Core.Common.Constants;
using LMCommon = LongoMatch.Core.Common;

namespace LongoMatch.Services
{
	public class EventsManager: IService
	{
		TimelineEvent loadedPlay;
		Project openedProject;
		ProjectType projectType;
		EventsFilter filter;
		IAnalysisWindowBase analysisWindow;
		IPlayerController player;
		ICapturerBin capturer;

		void HandleOpenedProjectChanged (OpenedProjectEvent e)
		{
			this.openedProject = e.Project;
			this.projectType = e.ProjectType;
			this.filter = e.Filter;

			this.analysisWindow = e.AnalysisWindow;
			player = e.AnalysisWindow.Player;
			capturer = e.AnalysisWindow.Capturer;
		}

		void HandlePlayerSubstitutionEvent (PlayerSubstitutionEvent e)
		{
			if (openedProject != null) {
				TimelineEventLongoMatch evt;

				try {
					evt = ((ProjectLongoMatch)openedProject).SubsitutePlayer (e.Team, e.Player1, e.Player2, e.SubstitutionReason, e.Time);
					App.Current.EventsBroker.Publish<EventCreatedEvent> (
						new EventCreatedEvent {
							TimelineEvent = evt
						}
					);
					filter.Update ();
				} catch (SubstitutionException ex) {
					App.Current.GUIToolkit.ErrorMessage (ex.Message);
				}
			}
		}

		void HandleKeyPressed (KeyPressedEvent e)
		{
			KeyAction action;

			try {
				action = App.Current.Config.Hotkeys.ActionsHotkeys.GetKeyByValue (e.Key);
			} catch (Exception ex) {
				/* The dictionary contains 2 equal values for different keys */
				Log.Exception (ex);
				return;
			}

			if (action == KeyAction.None || loadedPlay == null) {
				return;
			}

			switch (action) {
			case KeyAction.EditEvent:
				bool playing = player.Playing;
				player.Pause ();
				App.Current.GUIToolkit.EditPlay (loadedPlay, openedProject, true, true, true, true);
				if (playing) {
					player.Play ();
				}
				break;
			case KeyAction.DeleteEvent:
				App.Current.EventsBroker.Publish <EventsDeletedEvent> (
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

		#region IService

		public int Level {
			get {
				return 60;
			}
		}

		public string Name {
			get {
				return "Events";
			}
		}

		public bool Start ()
		{
			App.Current.EventsBroker.Subscribe<EventLoadedEvent> (HandlePlayLoaded);
			App.Current.EventsBroker.Subscribe<KeyPressedEvent> (HandleKeyPressed);
			App.Current.EventsBroker.Subscribe<PlayerSubstitutionEvent> (HandlePlayerSubstitutionEvent);
			App.Current.EventsBroker.Subscribe<ShowProjectStatsEvent> (HandleShowProjectStatsEvent);
			App.Current.EventsBroker.Subscribe<OpenedProjectEvent> (HandleOpenedProjectChanged);
			return true;
		}

		public bool Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<EventLoadedEvent> (HandlePlayLoaded);
			App.Current.EventsBroker.Unsubscribe<KeyPressedEvent> (HandleKeyPressed);
			App.Current.EventsBroker.Unsubscribe<PlayerSubstitutionEvent> (HandlePlayerSubstitutionEvent);
			App.Current.EventsBroker.Unsubscribe<ShowProjectStatsEvent> (HandleShowProjectStatsEvent);
			App.Current.EventsBroker.Unsubscribe<OpenedProjectEvent> (HandleOpenedProjectChanged);
			return true;
		}

		#endregion
	}
}