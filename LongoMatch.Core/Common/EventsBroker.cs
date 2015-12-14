//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.Core.Common
{
	public class EventsBroker
	{
	
		public event NewEventHandler NewEventEvent;
		public event NewDashboardEventHandler NewDashboardEventEvent;
		public event EventCreatedHandler EventCreatedEvent;
		public event DeleteEventsHandler EventsDeletedEvent;
		public event LoadEventHandler LoadEventEvent;
		public event EventLoadedHandler EventLoadedEvent;
		public event EventEditedHandler EventEditedEvent;
		public event MoveEventHandler MoveToEventTypeEvent;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event TimeNodeStartedHandler TimeNodeStartedEvent;
		public event TimeNodeStoppedHandler TimeNodeStoppedEvent;
		public event SnapshotSeriesHandler SnapshotSeries;
		public event DuplicateEventsHandler DuplicateEventsEvent;
		public event TeamsTagsChangedHandler TeamTagsChanged;
		public event PlayersSubstitutionHandler PlayerSubstitutionEvent;
		public event DashboardEditedHandler DashboardEditedEvent;
		
		/* Playlist */
		public event RenderPlaylistHandler RenderPlaylist;
		public event AddPlaylistElementHandler AddPlaylistElementEvent;
		public event PlaylistElementSelectedHandler PlaylistElementSelectedEvent;
		public event NewPlaylistHandler NewPlaylistEvent;
		public event NextPlaylistElementHandler NextPlaylistElementEvent;
		public event PreviousPlaylistElementHandler PreviousPlaylistElementEvent;

		public event KeyHandler KeyPressed;
		
		/* Project options */
		public event SaveProjectHandler SaveProjectEvent;
		public event CloseOpenendProjectHandler CloseOpenedProjectEvent;
		public event ShowFullScreenHandler ShowFullScreenEvent;
		public event ShowProjectStats ShowProjectStatsEvent;
		public event TagSubcategoriesChangedHandler TagSubcategoriesChangedEvent;
		
		/* IMainController */
		public event NewProjectHandler NewProjectEvent;
		public event OpenNewProjectHandler OpenNewProjectEvent;
		public event OpenProjectHandler OpenProjectEvent;
		public event OpenProjectIDHandler OpenProjectIDEvent;
		public event ImportProjectHandler ImportProjectEvent;
		public event ExportProjectHandler ExportProjectEvent;
		public event QuitApplicationHandler QuitApplicationEvent;
		public event ManageJobsHandler ManageJobsEvent;
		public event ManageTeamsHandler ManageTeamsEvent;
		public event ManageDashboardsHandler ManageCategoriesEvent;
		public event ManageProjects ManageProjectsEvent;
		public event ManageDatabases ManageDatabasesEvent;
		public event EditPreferences EditPreferencesEvent;
		public event ConvertVideoFilesHandler ConvertVideoFilesEvent;
		public event MigrateDBHandler MigrateDB;
		
		public event OpenedProjectChangedHandler OpenedProjectChanged;
		public event OpenedPresentationChangedHandler OpenedPresentationChanged;
		
		/* Player and Capturer */
		public event TickHandler PlayerTick;
		public event TickHandler CapturerTick;
		public event ErrorHandler MultimediaError;
		public event ErrorHandler CaptureError;
		public event CaptureFinishedHandler CaptureFinished;
		public event DrawFrameHandler DrawFrame;
		public event DetachPlayerHandler Detach;
		public event PlaybackRateChangedHandler PlaybackRateChanged;
		public event SeekEventHandler SeekEvent;
		public event TogglePlayEventHandler TogglePlayEvent;

		/* Query handlers */
		public event QueryToolsHandler QueryTools;

		public void EmitNewEvent (EventType eventType, List<Player> players = null, ObservableCollection<Team> teams = null,
		                          List<Tag> tags = null, Time start = null, Time stop = null, Time eventTime = null)
		{
			if (NewEventEvent != null)
				NewEventEvent (eventType, players, teams, tags, start, stop, eventTime, null);
		}

		public void EmitNewDashboardEvent (TimelineEvent evt, DashboardButton btn, bool edit, List<DashboardButton> from)
		{
			if (NewDashboardEventEvent != null) {
				if (from == null)
					from = new List<DashboardButton> ();
				NewDashboardEventEvent (evt, btn, edit, from);
			}
		}

		public void EmitEventsDeleted (List<TimelineEvent> events)
		{
			if (EventsDeletedEvent != null)
				EventsDeletedEvent (events);
		}

		public void EmitLoadEvent (TimelineEvent evt)
		{
			if (LoadEventEvent != null)
				LoadEventEvent (evt);
		}

		public void EmitEventLoaded (TimelineEvent play)
		{
			if (EventLoadedEvent != null)
				EventLoadedEvent (play);
		}

		public void EmitEventEdited (TimelineEvent play)
		{
			if (EventEditedEvent != null) {
				EventEditedEvent (play);
			}
		}

		public void EmitSnapshotSeries (TimelineEvent play)
		{
			if (SnapshotSeries != null)
				SnapshotSeries (play);
		}

		public void EmitRenderPlaylist (Playlist playlist)
		{
			if (RenderPlaylist != null)
				RenderPlaylist (playlist);
		}

		public void EmitNewPlaylist (Project project)
		{
			if (NewPlaylistEvent != null) {
				NewPlaylistEvent (project);
			}
		}

		public void EmitAddPlaylistElement (Playlist playlist, List<IPlaylistElement> plays)
		{
			if (AddPlaylistElementEvent != null)
				AddPlaylistElementEvent (playlist, plays);
		}

		public void EmitPlaylistElementSelected (Playlist playlist, IPlaylistElement element, bool playing)
		{
			if (PlaylistElementSelectedEvent != null)
				PlaylistElementSelectedEvent (playlist, element, playing);
		}

		public void EmitTimeNodeChanged (TimeNode tn, Time time)
		{
			if (TimeNodeChanged != null)
				TimeNodeChanged (tn, time);
		}

		public virtual void EmitMoveToEventType (TimelineEvent evnt, EventType eventType)
		{
			if (MoveToEventTypeEvent != null)
				MoveToEventTypeEvent (evnt, eventType);
		}

		public void EmitDuplicateEvent (List<TimelineEvent> events)
		{
			if (DuplicateEventsEvent != null)
				DuplicateEventsEvent (events);
		}

		public void EmitKeyPressed (object sender, HotKey key)
		{
			if (KeyPressed != null)
				KeyPressed (sender, key);
		}

		public bool EmitCloseOpenedProject ()
		{
			if (CloseOpenedProjectEvent != null)
				return CloseOpenedProjectEvent ();
			return false;
		}

		public void EmitShowProjectStats (Project project)
		{
			if (ShowProjectStatsEvent != null)
				ShowProjectStatsEvent (project);
		}

		public void EmitTagSubcategories (bool active)
		{
			if (TagSubcategoriesChangedEvent != null)
				TagSubcategoriesChangedEvent (active);
		}

		public void EmitShowFullScreen (bool active)
		{
			if (ShowFullScreenEvent != null) {
				ShowFullScreenEvent (active);
			}
		}

		public void EmitSaveProject (Project project, ProjectType projectType)
		{
			if (SaveProjectEvent != null)
				SaveProjectEvent (project, projectType);
		}

		public void EmitNewProject (Project project)
		{
			if (NewProjectEvent != null)
				NewProjectEvent (project);
		}

		public void EmitOpenProject ()
		{
			if (OpenProjectEvent != null)
				OpenProjectEvent ();
		}

		public void EmitEditPreferences ()
		{
			if (EditPreferencesEvent != null)
				EditPreferencesEvent ();
		}

		public void EmitManageJobs ()
		{
			if (ManageJobsEvent != null)
				ManageJobsEvent ();
		}

		public void EmitManageTeams ()
		{
			if (ManageTeamsEvent != null)
				ManageTeamsEvent ();
		}

		public void EmitManageProjects ()
		{
			if (ManageProjectsEvent != null)
				ManageProjectsEvent ();
		}

		public void EmitManageDatabases ()
		{
			if (ManageDatabasesEvent != null)
				ManageDatabasesEvent ();
		}

		public void EmitManageCategories ()
		{
			if (ManageCategoriesEvent != null)
				ManageCategoriesEvent ();
		}

		public void EmitImportProject ()
		{
			if (ImportProjectEvent != null)
				ImportProjectEvent ();
		}

		public void EmitExportProject (Project project)
		{
			if (ExportProjectEvent != null)
				ExportProjectEvent (project);
		}

		public void EmitOpenProjectID (Guid projectID, Project project)
		{
			if (OpenProjectIDEvent != null) {
				OpenProjectIDEvent (projectID, project);
			}
		}

		public void EmitOpenNewProject (Project project, ProjectType projectType, CaptureSettings captureSettings)
		{
			if (OpenNewProjectEvent != null) {
				OpenNewProjectEvent (project, projectType, captureSettings);
			}
		}

		public void EmitConvertVideoFiles (List<MediaFile> files, EncodingSettings settings)
		{
			if (ConvertVideoFilesEvent != null)
				ConvertVideoFilesEvent (files, settings);
		}

		public void EmitQuitApplication ()
		{
			if (QuitApplicationEvent != null) {
				QuitApplicationEvent ();
			}
		}

		public  void EmitOpenedProjectChanged (Project project, ProjectType projectType,
		                                       EventsFilter filter, IAnalysisWindow analysisWindow)
		{
			if (OpenedProjectChanged != null) {
				OpenedProjectChanged (project, projectType, filter, analysisWindow);
			}
		}

		public  void EmitOpenedPresentationChanged (Playlist presentation, IPlayerController player)
		{
			if (OpenedPresentationChanged != null) {
				OpenedPresentationChanged (presentation, player);
			}
		}

		public void EmitCapturerTick (Time currentTime)
		{
			if (CapturerTick != null) {
				CapturerTick (currentTime);
			}
		}

		public void EmitPlayerTick (Time currentTime)
		{
			if (PlayerTick != null) {
				PlayerTick (currentTime);
			}
		}

		public void EmitTeamTagsChanged ()
		{
			if (TeamTagsChanged != null) {
				TeamTagsChanged ();
			}
		}

		/// <summary>
		/// Emits the capture finished event.
		/// </summary>
		/// <param name="cancel">If set to <c>true</c> the capture was cancelled.</param>
		public void EmitCaptureFinished (bool cancel)
		{
			if (CaptureFinished != null) {
				CaptureFinished (cancel);
			}
		}

		public void EmitCaptureError (object sender, string message)
		{
			if (CaptureError != null) {
				CaptureError (sender, message);
			}
		}

		public void EmitMultimediaError (object sender, string message)
		{
			if (MultimediaError != null) {
				MultimediaError (sender, message);
			}
		}

		public void EmitDetach ()
		{
			if (Detach != null) {
				Detach ();
			}
		}

		public void EmitNextPlaylistElement (Playlist playlist)
		{
			if (NextPlaylistElementEvent != null) {
				NextPlaylistElementEvent (playlist);
			}
		}

		public void EmitPreviousPlaylistElement (Playlist playlist)
		{
			if (PreviousPlaylistElementEvent != null) {
				PreviousPlaylistElementEvent (playlist);
			}
		}

		public void EmitDrawFrame (TimelineEvent play, int drawingIndex, CameraConfig camConfig, bool current)
		{
			if (DrawFrame != null) {
				DrawFrame (play, drawingIndex, camConfig, current);
			}
		}

		public void EmitPlaybackRateChanged (float val)
		{
			if (PlaybackRateChanged != null) {
				PlaybackRateChanged (val);
			}
		}

		public void EmitPressButton (DashboardButton button)
		{
		}

		public void EmitSubstitutionEvent (Team team, Player p1, Player p2,
		                                   SubstitutionReason reason, Time time)
		{
			if (PlayerSubstitutionEvent != null) {
				PlayerSubstitutionEvent (team, p1, p2, reason, time);
			}
		}

		public void EmitDashboardEdited ()
		{
			if (DashboardEditedEvent != null) {
				DashboardEditedEvent ();
			}
		}

		public void EmitSeekEvent (Time time, bool accurate)
		{
			if (SeekEvent != null) {
				SeekEvent (time, accurate);
			}
		}

		public void EmitTogglePlayEvent (bool playing)
		{
			if (TogglePlayEvent != null) {
				TogglePlayEvent (playing);
			}
		}

		public void EmitMigrateDB ()
		{
			if (MigrateDB != null) {
				MigrateDB ();
			}
		}

		public void EmitTimeNodeStartedEvent (TimeNode node, TimerButton btn, List<DashboardButton> from)
		{
			if (TimeNodeStartedEvent != null) {
				if (from == null)
					from = new List<DashboardButton> ();
				TimeNodeStartedEvent (node, btn, from);
			}
		}

		public void EmitTimeNodeStoppedEvent (TimeNode node, TimerButton btn, List<DashboardButton> from)
		{
			if (TimeNodeStoppedEvent != null) {
				if (from == null)
					from = new List<DashboardButton> ();
				TimeNodeStoppedEvent (node, btn, from);
			}
		}

		public void EmitEventCreated (TimelineEvent evt)
		{
			if (EventCreatedEvent != null) {
				EventCreatedEvent (evt);
			}
		}

		#region Queries

		/// <summary>
		/// Emit the QueryTools event so that listeners can provide a list of available tools.
		/// </summary>
		/// <param name="tools">an empty list of Tools that will get popuplated.</param>
		public void EmitQueryTools (List<ITool> tools)
		{
			if (QueryTools != null) {
				QueryTools (tools);
			}
		}

		#endregion
	}
}
