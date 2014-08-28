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
using LongoMatch.Handlers;
using System.Collections.Generic;
using LongoMatch.Store;
using LongoMatch.Store.Playlists;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;

namespace LongoMatch.Common
{
	public class EventsBroker
	{
	
		public event NewTagHandler NewTagEvent;
		public event PlaysDeletedHandler PlaysDeleted;
		public event LoadPlayHandler LoadPlayEvent;
		public event PlayLoadedHandler PlayLoadedEvent;
		public event PlayCategoryChangedHandler PlayCategoryChanged;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event SnapshotSeriesHandler SnapshotSeries;
		public event TagPlayHandler TagPlay;
		public event DuplicatePlaysHandler DuplicatePlays;
		public event TeamsTagsChangedHandler TeamTagsChanged;
		
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
		public event ManageCategoriesHandler ManageCategoriesEvent;
		public event ManageProjects ManageProjectsEvent;
		public event ManageDatabases ManageDatabasesEvent;
		public event EditPreferences EditPreferencesEvent;
		public event ConvertVideoFilesHandler ConvertVideoFilesEvent;
		
		public event OpenedProjectChangedHandler OpenedProjectChanged;
		public event CreateThumbnailsHandler CreateThumbnailsEvent;
		
		/* Player and Capturer */
		public event TickHandler PlayerTick;
		public event TickHandler CapturerTick;
		public event ErrorHandler MultimediaError;
		public event ErrorHandler CaptureError;
		public event CaptureFinishedHandler CaptureFinished;
		public event DrawFrameHandler DrawFrame;
		public event DetachPlayerHandler Detach;
		public event PlaybackRateChangedHandler PlaybackRateChanged;

		public void EmitNewTag (TaggerButton tagger, List<Player> players = null,
		                        List<Tag> tags = null, Time start = null, Time stop = null) {
			if (NewTagEvent != null)
				NewTagEvent (tagger, players, tags, start, stop);
		}

		public void EmitPlaysDeleted(List<Play> plays)
		{
			if (PlaysDeleted != null)
				PlaysDeleted(plays);
		}
		
		public void EmitLoadPlay (Play play)
		{
			if (LoadPlayEvent != null)
				LoadPlayEvent (play);
		}
		
		public void EmitPlayLoaded (Play play)
		{
			if (PlayLoadedEvent != null)
				PlayLoadedEvent (play);
		}
		
		public void EmitSnapshotSeries(Play play)
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
		
		public virtual void EmitPlayCategoryChanged(Play play, Category cat)
		{
			if(PlayCategoryChanged != null)
				PlayCategoryChanged(play, cat);
		}
		
		public void EmitTagPlay(Play play) {
			if (TagPlay != null)
				TagPlay (play);
		}

		public void EmitDuplicatePlay (List<Play> plays)
		{
			if (DuplicatePlays != null)
				DuplicatePlays (plays);
		}
		
		public void EmitKeyPressed(object sender, int key, int modifier) {
			if (KeyPressed != null)
				KeyPressed(sender, key, modifier);
		}
		
		public void EmitCloseOpenedProject () {
			if (CloseOpenedProjectEvent != null)
				CloseOpenedProjectEvent ();
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
		                                       PlaysFilter filter, IAnalysisWindow analysisWindow)
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

		public void EmitDrawFrame (Play play, int drawingIndex)
		{
			if (DrawFrame != null) {
				DrawFrame (play, drawingIndex);
			}
		}

		public void EmitPlaybackRateChanged (float val)
		{
			if (PlaybackRateChanged != null) {
				PlaybackRateChanged (val);
			}
		}
		
		public void EmitCreateThumbnails (Project project) {
			if (CreateThumbnailsEvent != null) {
				CreateThumbnailsEvent (project);
			}
		}
		
		public void EmitPlaylistsChanged (object sender) {
			if (PlaylistsChangedEvent != null)
				PlaylistsChangedEvent (sender);
		}
	}
}

