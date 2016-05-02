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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Services;
using Constants = LongoMatch.Core.Common.Constants;
using LMCommon = LongoMatch.Core.Common;

namespace LongoMatch.Services
{
	public class EventsManager: EventsManagerBase
	{
		/* Current play loaded. null if no play is loaded */
		//TimelineEventLongoMatch loadedPlay;
		/* current project in use */
		//ProjectLongoMatch openedProject;
		//ProjectType projectType;
		//EventsFilter filter;
		//IAnalysisWindow analysisWindow;
		//IPlayerController player;
		//ICapturerBin capturer;
		//IFramesCapturer framesCapturer;

		public EventsManager () : base ()
		{
		}

		protected override void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                                    VAS.Core.Filters.EventsFilter filter, IAnalysisWindowBase analysisWindow)
		{
			this.openedProject = project;
			this.projectType = projectType;
			this.filter = filter;

			if (project == null) {
				if (framesCapturer != null) {
					framesCapturer.Dispose ();
					framesCapturer = null;
				}
				return;
			}

			if (projectType == ProjectType.FileProject) {
				framesCapturer = Config.MultimediaToolkit.GetFramesCapturer ();
				framesCapturer.Open (((ProjectLongoMatch)openedProject).Description.FileSet.First ().FilePath);
			}
			this.analysisWindow = analysisWindow;
			player = analysisWindow.Player;
			capturer = analysisWindow.Capturer;
		}

		protected override void Save (Project project)
		{
			Save (project as ProjectLongoMatch);
		}

		void Save (ProjectLongoMatch project)
		{
			if (Config.AutoSave) {
				Config.DatabaseManager.ActiveDB.Store<ProjectLongoMatch> (project);
			}
		}

		protected void HandlePlayerSubstitutionEvent (Team team, PlayerLongoMatch p1, PlayerLongoMatch p2, SubstitutionReason reason, Time time)
		{
			if (openedProject != null) {
				TimelineEventLongoMatch evt;

				try {
					evt = ((ProjectLongoMatch)openedProject).SubsitutePlayer (team, p1, p2, reason, time);
					analysisWindow.AddPlay (evt);
					filter.Update ();
				} catch (SubstitutionException ex) {
					Config.GUIToolkit.ErrorMessage (ex.Message);
				}
			}
		}

		protected override void HandlePlaylistElementSelectedEvent (Playlist playlist, IPlaylistElement element, bool playing)
		{
			if (element is PlaylistPlayElement) {
				loadedPlay = (element as PlaylistPlayElement).Play as TimelineEventLongoMatch;
			} else {
				loadedPlay = null;
			}
		}

		protected override void RenderPlay (Project project, TimelineEvent play)
		{
			RenderPlay (project as ProjectLongoMatch, play);
		}

		void RenderPlay (ProjectLongoMatch project, TimelineEvent play)
		{
			Playlist playlist;
			EncodingSettings settings;
			EditionJob job;
			string outputDir, outputProjectDir, outputFile;

			if (Config.AutoRenderDir == null ||
			    !Directory.Exists (Config.AutoRenderDir)) {
				outputDir = Config.VideosDir;
			} else {
				outputDir = Config.AutoRenderDir;
			}

			outputProjectDir = Path.Combine (outputDir,
				Utils.SanitizePath (project.Description.DateTitle));
			outputFile = String.Format ("{0}-{1}.mp4", play.EventType.Name, play.Name);
			outputFile = Utils.SanitizePath (outputFile, ' ');
			outputFile = Path.Combine (outputProjectDir, outputFile);
			try {
				PlaylistPlayElement element;

				Directory.CreateDirectory (outputProjectDir);
				settings = EncodingSettings.DefaultRenderingSettings (outputFile);
				playlist = new Playlist ();
				element = new PlaylistPlayElement (play);
				playlist.Elements.Add (element);
				job = new EditionJob (playlist, settings);
				Config.RenderingJobsManger.AddJob (job);
			} catch (Exception ex) {
				Log.Exception (ex);
			}
		}


		public void OnNewTag (EventType evType, List<PlayerLongoMatch> players, ObservableCollection<Team> teams, List<Tag> tags,
		                      Time start, Time stop, Time eventTime, DashboardButton btn)
		{
			if (openedProject == null) {
				return;
			} else if (projectType == ProjectType.FileProject && player == null) {
				Log.Error ("Player not set, new event will not be created");
				return;
			} else if (projectType == ProjectType.CaptureProject ||
			           projectType == ProjectType.URICaptureProject ||
			           projectType == ProjectType.FakeCaptureProject) {
				if (!capturer.Capturing) {
					Config.GUIToolkit.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}
			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				start.ToMSecondsString (), stop.ToMSecondsString (),
				evType.Name));
			/* Add the new created play to the project and update the GUI */
			var play = openedProject.AddEvent (evType, start, stop, eventTime, null) as TimelineEventLongoMatch;
			play.Teams = teams;
			if (players != null) {
				play.Players = new ObservableCollection<PlayerLongoMatch> (players);
			}
			if (tags != null) {
				play.Tags = new ObservableCollection<Tag> (tags);
			}
			AddNewPlay (play);
		}


		protected void OnPlaysDeleted (List<TimelineEventLongoMatch> plays)
		{
			base.OnPlaysDeleted (plays.Cast<TimelineEvent> ().ToList ());
		}

		protected override void OnDuplicatePlays (List<TimelineEvent> plays)
		{
			OnDuplicatePlays (plays.Cast<TimelineEventLongoMatch> ().ToList ());
		}

		void OnDuplicatePlays (List<TimelineEventLongoMatch> plays)
		{
			foreach (var play in plays) {
				TimelineEventLongoMatch copy = Cloner.Clone (play);
				copy.ID = Guid.NewGuid ();
				/* The category is also serialized and desarialized */
				copy.EventType = play.EventType;
				copy.Players = new ObservableCollection<PlayerLongoMatch> (play.Players);
				openedProject.AddEvent (copy);
				analysisWindow.AddPlay (copy);
			}
			filter.Update ();
		}

		protected override void OnPlayCategoryChanged (TimelineEvent play, EventType evType)
		{
			OnPlayCategoryChanged (play as TimelineEventLongoMatch, evType);
		}

		protected void OnPlayCategoryChanged (TimelineEventLongoMatch play, EventType evType)
		{
			var newplay = Cloner.Clone (play);
			newplay.ID = Guid.NewGuid ();
			newplay.EventType = evType;
			newplay.Players = play.Players;
			DeletePlays (new List<TimelineEvent> { play }, false);
			openedProject.AddEvent (newplay);
			analysisWindow.AddPlay (newplay);
			Save (openedProject);
			filter.Update ();
		}

		protected override void HandleKeyPressed (object sender, HotKey key)
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
				DeletePlays (new List<TimelineEvent> { loadedPlay });
				break;
			}
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

		public override bool Start ()
		{
			((LMCommon.EventsBroker)Config.EventsBroker).NewEventEvent += OnNewTag;
			((LMCommon.EventsBroker)Config.EventsBroker).NewDashboardEventEvent += HandleNewDashboardEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).EventsDeletedEvent += OnPlaysDeleted;
			((LMCommon.EventsBroker)Config.EventsBroker).MoveToEventTypeEvent += OnPlayCategoryChanged;
			((LMCommon.EventsBroker)Config.EventsBroker).DuplicateEventsEvent += OnDuplicatePlays;
			((LMCommon.EventsBroker)Config.EventsBroker).SnapshotSeries += OnSnapshotSeries;
			((LMCommon.EventsBroker)Config.EventsBroker).EventLoadedEvent += HandlePlayLoaded;
			((LMCommon.EventsBroker)Config.EventsBroker).PlaylistElementSelectedEvent += HandlePlaylistElementSelectedEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).PlayerSubstitutionEvent += HandlePlayerSubstitutionEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).DashboardEditedEvent += HandleDashboardEditedEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).ShowProjectStatsEvent += HandleShowProjectStatsEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).TagSubcategoriesChangedEvent += HandleTagSubcategoriesChangedEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).OpenedProjectChanged += HandleOpenedProjectChanged;
			((LMCommon.EventsBroker)Config.EventsBroker).DrawFrame += HandleDrawFrame;
			((LMCommon.EventsBroker)Config.EventsBroker).Detach += HandleDetach;
			((LMCommon.EventsBroker)Config.EventsBroker).ShowFullScreenEvent += HandleShowFullScreenEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).KeyPressed += HandleKeyPressed;
			return true;
		}

		public override bool Stop ()
		{
			((LMCommon.EventsBroker)Config.EventsBroker).NewEventEvent -= OnNewTag;
			((LMCommon.EventsBroker)Config.EventsBroker).NewDashboardEventEvent -= HandleNewDashboardEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).EventsDeletedEvent -= OnPlaysDeleted;
			((LMCommon.EventsBroker)Config.EventsBroker).MoveToEventTypeEvent -= OnPlayCategoryChanged;
			((LMCommon.EventsBroker)Config.EventsBroker).DuplicateEventsEvent -= OnDuplicatePlays;
			((LMCommon.EventsBroker)Config.EventsBroker).SnapshotSeries -= OnSnapshotSeries;
			((LMCommon.EventsBroker)Config.EventsBroker).EventLoadedEvent -= HandlePlayLoaded;
			((LMCommon.EventsBroker)Config.EventsBroker).PlaylistElementSelectedEvent -= HandlePlaylistElementSelectedEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).PlayerSubstitutionEvent -= HandlePlayerSubstitutionEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).DashboardEditedEvent -= HandleDashboardEditedEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).ShowProjectStatsEvent -= HandleShowProjectStatsEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).TagSubcategoriesChangedEvent -= HandleTagSubcategoriesChangedEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).OpenedProjectChanged -= HandleOpenedProjectChanged;
			((LMCommon.EventsBroker)Config.EventsBroker).DrawFrame -= HandleDrawFrame;
			((LMCommon.EventsBroker)Config.EventsBroker).Detach -= HandleDetach;
			((LMCommon.EventsBroker)Config.EventsBroker).ShowFullScreenEvent -= HandleShowFullScreenEvent;
			((LMCommon.EventsBroker)Config.EventsBroker).KeyPressed -= HandleKeyPressed;
			return true;
		}

		#endregion
	}
}