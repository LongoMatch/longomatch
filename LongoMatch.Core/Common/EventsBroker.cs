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
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;

namespace LongoMatch.Core.Common
{
	public class EventsBroker : VAS.Core.Common.EventsBroker
	{
		public event NewDashboardEventHandler NewDashboardEventEvent;
		//public event EventCreatedHandler EventCreatedEvent;
		public event DeleteEventsHandler EventsDeletedEvent;
		//public event LoadEventHandler LoadEventEvent;
		//public event EventLoadedHandler EventLoadedEvent;
		public event EventEditedHandler EventEditedEvent;
		public event MoveEventHandler MoveToEventTypeEvent;
		//public event TimeNodeChangedHandler TimeNodeChanged;
		//		public event TimeNodeStartedHandler TimeNodeStartedEvent;
		//		public event TimeNodeStoppedHandler TimeNodeStoppedEvent;
		public event SnapshotSeriesHandler SnapshotSeries;
		public event DuplicateEventsHandler DuplicateEventsEvent;
		public event DashboardEditedHandler DashboardEditedEvent;

		/* Playlist 
		public event RenderPlaylistHandler RenderPlaylist;
		public event AddPlaylistElementHandler AddPlaylistElementEvent;
		//public event PlaylistElementSelectedHandler PlaylistElementSelectedEvent;
		public event NewPlaylistHandler NewPlaylistEvent;
		public event NextPlaylistElementHandler NextPlaylistElementEvent;
		public event PreviousPlaylistElementHandler PreviousPlaylistElementEvent;
		*/

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
		//public event ConvertVideoFilesHandler ConvertVideoFilesEvent;
		public event MigrateDBHandler MigrateDB;


		/* Player and Capturer */
		//public event TickHandler PlayerTick;
		public event TickHandler CapturerTick;
		//public event ErrorHandler MultimediaError;
		public event ErrorHandler CaptureError;
		public event CaptureFinishedHandler CaptureFinished;
		//public event DrawFrameHandler DrawFrame;
		public event DetachPlayerHandler Detach;
		//public event PlaybackRateChangedHandler PlaybackRateChanged;
		//public event SeekEventHandler SeekEvent;
		//public event TogglePlayEventHandler TogglePlayEvent;
		//public event StateChangeHandler PlaybackStateChangedEvent;


		public event PlayersSubstitutionHandler PlayerSubstitutionEvent;

		public event TeamsTagsChangedHandler TeamTagsChanged;

		public event NewEventHandler NewEventEvent;

		//public event OpenedPresentationChangedHandler OpenedPresentationChanged;

		/* Query handlers */
		public event QueryToolsHandler QueryTools;

		public void EmitNewEvent (EventType eventType, List<PlayerLongoMatch> players = null, ObservableCollection<Team> teams = null,
		                          List<Tag> tags = null, Time start = null, Time stop = null, Time eventTime = null)
		{
			if (NewEventEvent != null)
				NewEventEvent (eventType, players, teams, tags, start, stop, eventTime, null);
		}

		public void EmitSubstitutionEvent (Team team, PlayerLongoMatch p1, PlayerLongoMatch p2,
		                                   SubstitutionReason reason, Time time)
		{
			if (PlayerSubstitutionEvent != null) {
				PlayerSubstitutionEvent (team, p1, p2, reason, time);
			}
		}

		public void EmitTeamTagsChanged ()
		{
			if (TeamTagsChanged != null) {
				TeamTagsChanged ();
			}
		}

		public void EmitNewDashboardEvent (TimelineEventLongoMatch evt, DashboardButton btn, bool edit, List<DashboardButton> from)
		{
			if (NewDashboardEventEvent != null) {
				if (from == null)
					from = new List<DashboardButton> ();
				NewDashboardEventEvent (evt, btn, edit, from);
			}
		}

		public void EmitEventsDeleted (List<TimelineEventLongoMatch> events)
		{
			if (EventsDeletedEvent != null)
				EventsDeletedEvent (events);
		}

		public void EmitEventEdited (TimelineEventLongoMatch play)
		{
			if (EventEditedEvent != null) {
				EventEditedEvent (play);
			}
		}

		public void EmitSnapshotSeries (TimelineEventLongoMatch play)
		{
			if (SnapshotSeries != null)
				SnapshotSeries (play);
		}

		public virtual void EmitMoveToEventType (TimelineEventLongoMatch evnt, EventType eventType)
		{
			if (MoveToEventTypeEvent != null)
				MoveToEventTypeEvent (evnt, eventType);
		}

		public void EmitDuplicateEvent (List<TimelineEventLongoMatch> events)
		{
			if (DuplicateEventsEvent != null)
				DuplicateEventsEvent (events);
		}

		public bool EmitCloseOpenedProject ()
		{
			if (CloseOpenedProjectEvent != null)
				return CloseOpenedProjectEvent ();
			return false;
		}

		public void EmitShowProjectStats (ProjectLongoMatch project)
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

		public void EmitSaveProject (ProjectLongoMatch project, ProjectType projectType)
		{
			if (SaveProjectEvent != null)
				SaveProjectEvent (project, projectType);
		}

		public void EmitNewProject (ProjectLongoMatch project)
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

		public void EmitExportProject (ProjectLongoMatch project)
		{
			if (ExportProjectEvent != null)
				ExportProjectEvent (project);
		}

		public void EmitOpenProjectID (Guid projectID, ProjectLongoMatch project)
		{
			if (OpenProjectIDEvent != null) {
				OpenProjectIDEvent (projectID, project);
			}
		}

		public void EmitOpenNewProject (ProjectLongoMatch project, ProjectType projectType, CaptureSettings captureSettings)
		{
			if (OpenNewProjectEvent != null) {
				OpenNewProjectEvent (project, projectType, captureSettings);
			}
		}

		public void EmitQuitApplication ()
		{
			if (QuitApplicationEvent != null) {
				QuitApplicationEvent ();
			}
		}

		public void EmitCapturerTick (Time currentTime)
		{
			if (CapturerTick != null) {
				CapturerTick (currentTime);
			}
		}

		/// <summary>
		/// Signals the current capture has finished.
		/// </summary>
		/// <param name="cancel">If set to <c>true</c> the capture was cancelled.</param>
		/// <param name="reopn">If set to <c>true</c> the finished project is reopened.</param>
		public void EmitCaptureFinished (bool cancel, bool reopen)
		{
			if (CaptureFinished != null) {
				CaptureFinished (cancel, reopen);
			}
		}

		public void EmitCaptureError (object sender, string message)
		{
			if (CaptureError != null) {
				CaptureError (sender, message);
			}
		}

		public void EmitDetach ()
		{
			if (Detach != null) {
				Detach ();
			}
		}

		public void EmitDrawFrame (TimelineEventLongoMatch play, int drawingIndex, CameraConfig camConfig, bool current)
		{
			base.EmitDrawFrame (play, drawingIndex, camConfig, current);
			/*
			if (DrawFrame != null) {
				DrawFrame (play, drawingIndex, camConfig, current);
			}
			*/
		}

		public void EmitPressButton (DashboardButton button)
		{
		}

		public void EmitDashboardEdited ()
		{
			if (DashboardEditedEvent != null) {
				DashboardEditedEvent ();
			}
		}

		//		public void EmitDatabaseCreated (string name)
		//		{
		//			if (DatabaseCreatedEvent != null) {
		//				DatabaseCreatedEvent (name);
		//			}
		//		}

		public void EmitMigrateDB ()
		{
			if (MigrateDB != null) {
				MigrateDB ();
			}
		}

		//		public void EmitTimeNodeStartedEvent (TimeNode node, TimerButton btn, List<DashboardButton> from)
		//		{
		//			if (TimeNodeStartedEvent != null) {
		//				if (from == null)
		//					from = new List<DashboardButton> ();
		//				TimeNodeStartedEvent (node, btn, from);
		//			}
		//		}
		//
		//		public void EmitTimeNodeStoppedEvent (TimeNode node, TimerButton btn, List<DashboardButton> from)
		//		{
		//			if (TimeNodeStoppedEvent != null) {
		//				if (from == null)
		//					from = new List<DashboardButton> ();
		//				TimeNodeStoppedEvent (node, btn, from);
		//			}
		//		}

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
