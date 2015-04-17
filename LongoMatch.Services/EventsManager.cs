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
using System.Linq;
using System.Collections.Generic;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using Mono.Unix;
using System.IO;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.Services
{
	public class EventsManager: IService
	{
		/* Current play loaded. null if no play is loaded */
		TimelineEvent loadedPlay;
		/* current project in use */
		Project openedProject;
		ProjectType projectType;
		EventsFilter filter;
		IGUIToolkit guiToolkit;
		IAnalysisWindow analysisWindow;
		IPlayerController player;
		ICapturerBin capturer;
		IFramesCapturer framesCapturer;
		IRenderingJobsManager renderer;

		public EventsManager (IGUIToolkit guiToolkit, IRenderingJobsManager renderer)
		{
			this.guiToolkit = guiToolkit;
			this.renderer = renderer;
		}

		void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                 EventsFilter filter, IAnalysisWindow analysisWindow)
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
				framesCapturer.Open (openedProject.Description.FileSet.First ().FilePath);
			}
			this.analysisWindow = analysisWindow;
			player = analysisWindow.Player;
			capturer = analysisWindow.Capturer;
		}

		void Save (Project project)
		{
			if (Config.AutoSave) {
				Config.DatabaseManager.ActiveDB.UpdateProject (project);
			}
		}

		void DeletePlays (List<TimelineEvent> plays, bool update = true)
		{
			Log.Debug (plays.Count + " plays deleted");
			analysisWindow.DeletePlays (plays);
			openedProject.RemoveEvents (plays);
			if (projectType == ProjectType.FileProject) {
				Save (openedProject);
			}
			if (loadedPlay != null && plays.Contains (loadedPlay)) {
				Config.EventsBroker.EmitLoadEvent (null);
			}
			filter.Update ();
		}

		void HandlePlayerSubstitutionEvent (Team team, Player p1, Player p2, SubstitutionReason reason, Time time)
		{
			if (openedProject != null) {
				TimelineEvent evt;

				try {
					evt = openedProject.SubsitutePlayer (team, p1, p2, reason, time);
					analysisWindow.AddPlay (evt);
					filter.Update ();
				} catch (SubstitutionException ex) {
					guiToolkit.ErrorMessage (ex.Message);
				}
			}
		}

		void HandleShowFullScreenEvent (bool fullscreen)
		{
			guiToolkit.FullScreen = fullscreen;
		}

		void HandlePlayLoaded (TimelineEvent play)
		{
			loadedPlay = play;
		}

		void HandlePlaylistElementSelectedEvent (Playlist playlist, IPlaylistElement element)
		{
			if (element is PlaylistPlayElement) {
				loadedPlay = (element as PlaylistPlayElement).Play;
			} else {
				loadedPlay = null;
			}
		}

		void HandleDetach ()
		{
			analysisWindow.DetachPlayer ();
		}

		void HandleTagSubcategoriesChangedEvent (bool tagsubcategories)
		{
			Config.FastTagging = !tagsubcategories;
		}

		void HandleShowProjectStatsEvent (Project project)
		{
			guiToolkit.ShowProjectStats (project);
		}

		void HandleDrawFrame (TimelineEvent play, int drawingIndex, int cameraIndex, bool current)
		{
			Image pixbuf;
			FrameDrawing drawing = null;
			Time pos;

			player.Pause ();
			if (play == null) {
				play = loadedPlay as TimelineEvent;
			}
			if (play != null) {
				if (drawingIndex == -1) {
					drawing = new FrameDrawing ();
					drawing.Render = player.CurrentTime;
					drawing.CameraIndex = cameraIndex;
				} else {
					drawing = play.Drawings [drawingIndex];
				}
				pos = drawing.Render;
			} else {
				pos = player.CurrentTime;
			}

			if (framesCapturer != null && !current) {
				// FIXME
				Time offset = openedProject.Description.FileSet.First ().Offset;
				pixbuf = framesCapturer.GetFrame (pos + offset, true, -1, -1);
			} else {
				pixbuf = player.CurrentFrame;
			}
			if (pixbuf == null) {
				guiToolkit.ErrorMessage (Catalog.GetString ("Error capturing video frame"));
			} else {
				guiToolkit.DrawingTool (pixbuf, play, drawing, openedProject);
			}
		}

		void RenderPlay (Project project, TimelineEvent play)
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
				element = new PlaylistPlayElement (play, project.Description.FileSet);
				playlist.Elements.Add (element);
				job = new EditionJob (playlist, settings);
				renderer.AddJob (job);
			} catch (Exception ex) {
				Log.Exception (ex);
			}
			
		}

		private Image CaptureFrame (Time tagtime)
		{
			Image frame = null;

			/* Get the current frame and get a thumbnail from it */
			if (projectType == ProjectType.CaptureProject ||
			    projectType == ProjectType.URICaptureProject) {
				frame = capturer.CurrentCaptureFrame;
			} else if (projectType == ProjectType.FileProject) {
				frame = framesCapturer.GetFrame (tagtime, true, Constants.MAX_THUMBNAIL_SIZE,
					Constants.MAX_THUMBNAIL_SIZE);
			}
			return frame;
		}

		private void AddNewPlay (TimelineEvent play)
		{
			/* Clip play boundaries */
			play.Start.MSeconds = Math.Max (0, play.Start.MSeconds);
			if (projectType == ProjectType.FileProject) {
				play.Stop.MSeconds = Math.Min (player.StreamLength.MSeconds, play.Stop.MSeconds);
				play.CamerasLayout = player.CamerasLayout;
				play.CamerasVisible = player.CamerasVisible;
			} else {
				play.CamerasLayout = null;
				play.CamerasVisible = new List<int> { 0 };
			}

			filter.Update ();
			analysisWindow.AddPlay (play);
			Config.EventsBroker.EmitEventCreated (play);
			if (projectType == ProjectType.FileProject) {
				player.Play ();
			}
			Save (openedProject);
			
			if (projectType == ProjectType.CaptureProject ||
			    projectType == ProjectType.URICaptureProject) {
				if (Config.AutoRenderPlaysInLive) {
					RenderPlay (openedProject, play);
				}
			}
		}

		public void OnNewTag (EventType evType, List<Player> players, TeamType team, List<Tag> tags,
		                      Time start, Time stop, Time eventTime, Score score, PenaltyCard card, DashboardButton btn)
		{
			if (player == null || openedProject == null)
				return;

			if (projectType == ProjectType.CaptureProject ||
			    projectType == ProjectType.URICaptureProject) {
				if (!capturer.Capturing) {
					guiToolkit.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}
			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				start.ToMSecondsString (), stop.ToMSecondsString (),
				evType.Name));
			/* Add the new created play to the project and update the GUI*/
			var play = openedProject.AddEvent (evType, start, stop, eventTime, null, score, card);
			play.Team = team;
			if (players != null) {
				play.Players = players;
			}
			if (tags != null) {
				play.Tags = tags;
			}
			AddNewPlay (play);
		}

		public void HandleNewDashboardEvent (TimelineEvent play, DashboardButton btn, bool edit)
		{
			if (openedProject == null)
				return;
			
			if (projectType == ProjectType.CaptureProject ||
			    projectType == ProjectType.URICaptureProject ||
			    projectType == ProjectType.FakeCaptureProject) {
				if (!capturer.Capturing) {
					guiToolkit.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}

			if (!openedProject.Dashboard.DisablePopupWindow && edit) {
				if (projectType == ProjectType.FileProject) {
					bool playing = player.Playing;
					player.Pause ();
					Config.GUIToolkit.EditPlay (play, openedProject, true, true, true, true);
					if (playing) {
						player.Play ();
					}
				} else {
					Config.GUIToolkit.EditPlay (play, openedProject, true, true, true, true);
				}
			}

			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
				play.Start.ToMSecondsString (), play.Stop.ToMSecondsString (),
				play.EventType.Name));
			openedProject.AddEvent (play);
			AddNewPlay (play);
		}

		protected virtual void OnPlaysDeleted (List<TimelineEvent> plays)
		{
			DeletePlays (plays);
		}

		void OnDuplicatePlays (List<TimelineEvent> plays)
		{
			foreach (TimelineEvent play in plays) {
				TimelineEvent copy = Cloner.Clone (play);
				copy.ID = Guid.NewGuid ();
				/* The category is also serialized and desarialized */
				copy.EventType = play.EventType;
				copy.Players = play.Players.ToList ();
				openedProject.AddEvent (copy);
				analysisWindow.AddPlay (copy);
			}
			filter.Update ();
		}

		protected virtual void OnSnapshotSeries (TimelineEvent play)
		{
			player.Pause ();
			guiToolkit.ExportFrameSeries (openedProject, play, Config.SnapshotsDir);
		}

		protected virtual void OnPlayCategoryChanged (TimelineEvent play, EventType evType)
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

		void HandleDashboardEditedEvent ()
		{
			openedProject.UpdateEventTypesAndTimers ();
			analysisWindow.ReloadProject ();
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

		public bool Start ()
		{
			Config.EventsBroker.NewEventEvent += OnNewTag;
			Config.EventsBroker.NewDashboardEventEvent += HandleNewDashboardEvent;
			Config.EventsBroker.EventsDeletedEvent += OnPlaysDeleted;
			Config.EventsBroker.MoveToEventTypeEvent += OnPlayCategoryChanged;
			Config.EventsBroker.DuplicateEventsEvent += OnDuplicatePlays;
			Config.EventsBroker.SnapshotSeries += OnSnapshotSeries;
			Config.EventsBroker.EventLoadedEvent += HandlePlayLoaded;
			Config.EventsBroker.PlaylistElementSelectedEvent += HandlePlaylistElementSelectedEvent;
			Config.EventsBroker.PlayerSubstitutionEvent += HandlePlayerSubstitutionEvent;
			Config.EventsBroker.DashboardEditedEvent += HandleDashboardEditedEvent;

			Config.EventsBroker.ShowProjectStatsEvent += HandleShowProjectStatsEvent;
			Config.EventsBroker.TagSubcategoriesChangedEvent += HandleTagSubcategoriesChangedEvent;

			Config.EventsBroker.OpenedProjectChanged += HandleOpenedProjectChanged;

			Config.EventsBroker.DrawFrame += HandleDrawFrame;
			Config.EventsBroker.Detach += HandleDetach;

			Config.EventsBroker.ShowFullScreenEvent += HandleShowFullScreenEvent;

			Config.EventsBroker.KeyPressed += HandleKeyPressed;

			return true;
		}

		public bool Stop ()
		{
			Config.EventsBroker.NewEventEvent -= OnNewTag;
			Config.EventsBroker.NewDashboardEventEvent -= HandleNewDashboardEvent;
			Config.EventsBroker.EventsDeletedEvent -= OnPlaysDeleted;
			Config.EventsBroker.MoveToEventTypeEvent -= OnPlayCategoryChanged;
			Config.EventsBroker.DuplicateEventsEvent -= OnDuplicatePlays;
			Config.EventsBroker.SnapshotSeries -= OnSnapshotSeries;
			Config.EventsBroker.EventLoadedEvent -= HandlePlayLoaded;
			Config.EventsBroker.PlaylistElementSelectedEvent -= HandlePlaylistElementSelectedEvent;
			Config.EventsBroker.PlayerSubstitutionEvent -= HandlePlayerSubstitutionEvent;
			Config.EventsBroker.DashboardEditedEvent -= HandleDashboardEditedEvent;

			Config.EventsBroker.ShowProjectStatsEvent -= HandleShowProjectStatsEvent;
			Config.EventsBroker.TagSubcategoriesChangedEvent -= HandleTagSubcategoriesChangedEvent;

			Config.EventsBroker.OpenedProjectChanged -= HandleOpenedProjectChanged;

			Config.EventsBroker.DrawFrame -= HandleDrawFrame;
			Config.EventsBroker.Detach -= HandleDetach;

			Config.EventsBroker.ShowFullScreenEvent -= HandleShowFullScreenEvent;

			Config.EventsBroker.KeyPressed -= HandleKeyPressed;

			return true;
		}

		#endregion
	}
}
