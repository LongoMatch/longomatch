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
using LongoMatch.Core.Handlers;
using System.Collections.Generic;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.Core.Common
{
	public class EventsBroker
	{
	
		public event NewEventHandler NewTagEvent;
		public event NewTimelineEventHandler NewTimelineEventEvent;
		public event DeleteEventsHandler EventsDeletedEvent;
		public event LoadEventHandler LoadEventEvent;
		public event EventLoadedHandler EventLoadedEvent;
		public event TimerNodeAddedHandler TimerNodeAddedEvent;
		public event MoveEventHandler MoveToEventTypeEvent;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event SnapshotSeriesHandler SnapshotSeries;
		public event TagEventHandler TagEventEvent;
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
		public event PlaylistsChangedHandler PlaylistsChangedEvent;
		
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

		public void EmitNewTag (EventType eventType, List<Player> players = null, Team team = Team.NONE,
		                        List<Tag> tags = null, Time start = null, Time stop = null,
		                        Time eventTime = null, Score score = null, PenaltyCard card = null) {
			if (NewTagEvent != null)
				NewTagEvent (eventType, players, team, tags, start, stop, eventTime, score, card);
		}
		
		public void EmitNewEvent (TimelineEvent evt) {
			if (NewTimelineEventEvent != null)
				NewTimelineEventEvent (evt);
		}

		public void EmitEventsDeleted(List<TimelineEvent> events)
		{
			if (EventsDeletedEvent != null)
				EventsDeletedEvent(events);
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
		
		public void EmitSnapshotSeries(TimelineEvent play)
		{
			if (SnapshotSeries != null)
				SnapshotSeries(play);
		}
		
		public void EmitRenderPlaylist(Playlist playlist) {
			if (RenderPlaylist != null)
				RenderPlaylist(playlist);
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
		
		public void EmitPlaylistElementSelected (Playlist playlist, IPlaylistElement element)
		{
			if (PlaylistElementSelectedEvent != null)
				PlaylistElementSelectedEvent (playlist, element);
		}
		
		public void EmitTimeNodeChanged (TimeNode tn, object val)
		{
			if (TimeNodeChanged != null)
				TimeNodeChanged(tn, val);
		}
		
		public virtual void EmitMoveToEventType(TimelineEvent evnt, EventType eventType)
		{
			if(MoveToEventTypeEvent != null)
				MoveToEventTypeEvent(evnt, eventType);
		}
		
		public void EmitTagEvent(TimelineEvent evt) {
			if (TagEventEvent != null)
				TagEventEvent (evt);
		}

		public void EmitDuplicateEvent (List<TimelineEvent> events)
		{
			if (DuplicateEventsEvent != null)
				DuplicateEventsEvent (events);
		}
		
		public void EmitKeyPressed(object sender, int key, int modifier) {
			if (KeyPressed != null)
				KeyPressed(sender, key, modifier);
		}
		
		public bool EmitCloseOpenedProject () {
			if (CloseOpenedProjectEvent != null)
				return CloseOpenedProjectEvent ();
			return false;
		}
		
		public void EmitShowProjectStats (Project project) {
			if (ShowProjectStatsEvent != null)
				ShowProjectStatsEvent (project);
		}
		
		public void EmitTagSubcategories (bool active) {
			if (TagSubcategoriesChangedEvent != null)
				TagSubcategoriesChangedEvent (active);
		}

		public void EmitShowFullScreen (bool active)
		{
			if (ShowFullScreenEvent != null) {
				ShowFullScreenEvent (active);
			}
		}
		
		public void EmitSaveProject (Project project, ProjectType projectType) {
			if (SaveProjectEvent != null)
				SaveProjectEvent (project, projectType);
		}
		
		public void EmitNewProject (Project project) {
			if (NewProjectEvent != null)
				NewProjectEvent(project);
		}
		
		public void EmitOpenProject () {
			if(OpenProjectEvent != null)
				OpenProjectEvent();
		}
				
		public void EmitEditPreferences ()
		{
			if (EditPreferencesEvent != null)
				EditPreferencesEvent();
		}
		
		public void EmitManageJobs() {
			if(ManageJobsEvent != null)
				ManageJobsEvent();
		}
		
		public void EmitManageTeams() {
			if(ManageTeamsEvent != null)
				ManageTeamsEvent();
		}
		
		public void EmitManageProjects()
		{
			if (ManageProjectsEvent != null)
				ManageProjectsEvent();
		}
		
		public void EmitManageDatabases()
		{
			if (ManageDatabasesEvent != null)
				ManageDatabasesEvent();
		}
		
		public void EmitManageCategories() {
			if(ManageCategoriesEvent != null)
				ManageCategoriesEvent();
		}
		
		public void EmitImportProject () {
			if (ImportProjectEvent != null)
				ImportProjectEvent ();
		}
		
		public void EmitExportProject (Project project) {
			if(ExportProjectEvent != null)
				ExportProjectEvent (project);
		}
		
		public void EmitOpenProjectID (Guid projectID ) {
			if (OpenProjectIDEvent != null) {
				OpenProjectIDEvent (projectID);
			}
		}
		
		public void EmitOpenNewProject (Project project, ProjectType projectType, CaptureSettings captureSettings)
		{
			if (OpenNewProjectEvent != null) {
				OpenNewProjectEvent (project, projectType, captureSettings);
			}
		}
		
		public void EmitConvertVideoFiles (List<MediaFile> files, EncodingSettings settings) {
			if (ConvertVideoFilesEvent != null)
				ConvertVideoFilesEvent (files, settings);
		}
		
		public void EmitQuitApplication () {
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
		
		public void EmitCaptureFinished (bool close)
		{
			if (CaptureFinished != null) {
				CaptureFinished (close);
			}
		}
		
		public void EmitCaptureError (string message)
		{
			if (CaptureError != null) {
				CaptureError (message);
			}
		}

		public void EmitMultimediaError (string message)
		{
			if (MultimediaError != null) {
				MultimediaError (message);
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

		public void EmitDrawFrame (TimelineEvent play, int drawingIndex, MediaFileAngle angle, bool current)
		{
			if (DrawFrame != null) {
				DrawFrame (play, drawingIndex, angle, current);
			}
		}

		public void EmitPlaybackRateChanged (float val)
		{
			if (PlaybackRateChanged != null) {
				PlaybackRateChanged (val);
			}
		}
		
		public void EmitPlaylistsChanged (object sender) {
			if (PlaylistsChangedEvent != null)
				PlaylistsChangedEvent (sender);
		}
		
		public void EmitPressButton (DashboardButton button) {
		}
		
		public void EmitSubstitutionEvent (TeamTemplate team, Player p1, Player p2,
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
		
		public void EmitTimerNodeAddedEvent (Timer timer, TimeNode node)
		{
			if (TimerNodeAddedEvent != null) {
				TimerNodeAddedEvent (timer, node);
			}
		}
	}
}

