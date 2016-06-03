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
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using VAS.Core.Common;
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

		void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                 EventsFilter filter, IAnalysisWindowBase analysisWindow)
		{
			this.openedProject = project;
			this.projectType = projectType;
			this.filter = filter;

			this.analysisWindow = analysisWindow;
			player = analysisWindow.Player;
			capturer = analysisWindow.Capturer;
		}

		void HandlePlayerSubstitutionEvent (SportsTeam team, PlayerLongoMatch p1, PlayerLongoMatch p2, SubstitutionReason reason, Time time)
		{
			if (openedProject != null) {
				TimelineEventLongoMatch evt;

				try {
					evt = ((ProjectLongoMatch)openedProject).SubsitutePlayer (team, p1, p2, reason, time);
					Config.EventsBroker.EmitEventCreated (evt);
					filter.Update ();
				} catch (SubstitutionException ex) {
					Config.GUIToolkit.ErrorMessage (ex.Message);
				}
			}
		}

		void HandleKeyPressed (object sender, HotKey key)
		{
			KeyAction action;

			try {
				action = Config.Hotkeys.ActionsHotkeys.GetKeyByValue (key);
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
				Config.GUIToolkit.EditPlay (loadedPlay, openedProject, true, true, true, true);
				if (playing) {
					player.Play ();
				}
				break;
			case KeyAction.DeleteEvent:
				Config.EventsBroker.EmitEventsDeleted (new List<TimelineEvent> { loadedPlay });
				break;
			}
		}

		void HandlePlayLoaded (TimelineEvent play)
		{
			loadedPlay = play;
		}

		void HandleShowProjectStatsEvent (Project project)
		{
			Config.GUIToolkit.ShowProjectStats (project);
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
			Config.EventsBroker.EventLoadedEvent += HandlePlayLoaded;
			Config.EventsBroker.KeyPressed += HandleKeyPressed;
			((LMCommon.EventsBroker)Config.EventsBroker).PlayerSubstitutionEvent += HandlePlayerSubstitutionEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).ShowProjectStatsEvent += HandleShowProjectStatsEvent;
			Config.EventsBroker.OpenedProjectChanged += HandleOpenedProjectChanged;
			return true;
		}

		public bool Stop ()
		{
			Config.EventsBroker.EventLoadedEvent -= HandlePlayLoaded;
			Config.EventsBroker.KeyPressed -= HandleKeyPressed;
			((LMCommon.EventsBroker)Config.EventsBroker).PlayerSubstitutionEvent -= HandlePlayerSubstitutionEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).ShowProjectStatsEvent -= HandleShowProjectStatsEvent;
			Config.EventsBroker.OpenedProjectChanged -= HandleOpenedProjectChanged;
			return true;
		}

		#endregion
	}
}