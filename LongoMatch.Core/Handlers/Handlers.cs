// Handlers.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;

namespace LongoMatch.Core.Handlers
{

	/* An event was loaded */
	//public delegate void EventLoadedHandler (TimelineEventLongoMatch evt);

	/* A new play needs to be created for a specific category at the current play time */
	public delegate void NewEventHandler (EventType eventType,List<PlayerLongoMatch> players,ObservableCollection<Team> team,
		List<Tag> tags,Time start,Time stop,Time EventTime,DashboardButton btn);
	/* Add a new play to the current project from Dashboard */
	public delegate void NewDashboardEventHandler (TimelineEventLongoMatch evt,DashboardButton btn,bool edit,
		List<DashboardButton> from);
	/* A list of plays needs to be deleted */
	public delegate void DeleteEventsHandler (List<TimelineEventLongoMatch> events);
	/* Change the Play's category */
	public delegate void MoveEventHandler (TimelineEventLongoMatch play,EventType eventType);
	/* An event was edited */
	public delegate void EventEditedHandler (TimelineEventLongoMatch play);
	/* Duplicate play */
	public delegate void DuplicateEventsHandler (List<TimelineEventLongoMatch> events);

	/* The players tagged in an event have changed */
	public delegate void TeamsTagsChangedHandler ();
	/* Project Events */
	public delegate void SaveProjectHandler (ProjectLongoMatch project,ProjectType projectType);
	//public delegate void OpenedProjectChangedHandler (ProjectLongoMatch project,ProjectType projectType,EventsFilter filter,
	//	IAnalysisWindow analysisWindow);
	//public delegate void OpenedPresentationChangedHandler (Playlist presentation,IPlayerController player);
	public delegate void OpenProjectIDHandler (Guid project_id,ProjectLongoMatch project);
	public delegate void OpenProjectHandler ();
	public delegate bool CloseOpenendProjectHandler ();
	public delegate void NewProjectHandler (ProjectLongoMatch project);
	public delegate void OpenNewProjectHandler (ProjectLongoMatch project,ProjectType projectType,CaptureSettings captureSettings);
	public delegate void ImportProjectHandler ();
	public delegate void ExportProjectHandler (ProjectLongoMatch project);
	public delegate void CreateThumbnailsHandler (ProjectLongoMatch project);
	/*Playlist Events*/
	/* Create a new playlist */
	//public delegate Playlist NewPlaylistHandler (ProjectLongoMatch project);
	/* Add a new rendering job */
	//public delegate void RenderPlaylistHandler (Playlist playlist);
	/* Add a play to a playlist */
	//public delegate void AddPlaylistElementHandler (Playlist playlist,List<IPlaylistElement> element);
	/* Play next playlist element */
	//public delegate void NextPlaylistElementHandler (Playlist playlist);
	/* Play previous playlist element */
	//public delegate void PreviousPlaylistElementHandler (Playlist playlist);
	/* Playlists have been edited */
	public delegate void PlaylistsChangedHandler (object sender);
	/* Create snapshots for a play */
	public delegate void SnapshotSeriesHandler (TimelineEventLongoMatch tNode);

	/* Edit player properties */
	public delegate void PlayerPropertiesHandler (PlayerLongoMatch player);
	public delegate void PlayersPropertiesHandler (List<PlayerLongoMatch> players);
	/* Players selection */
	public delegate void PlayersSubstitutionHandler (Team team,PlayerLongoMatch p1,PlayerLongoMatch p2,
		SubstitutionReason reason,Time time);
	public delegate void PlayersSelectionChangedHandler (List<PlayerLongoMatch> players);
	public delegate void TeamSelectionChangedHandler (ObservableCollection<Team> teams);
	/* A list of projects have been selected */
	public delegate void ProjectsSelectedHandler (List<ProjectLongoMatch> projects);
	public delegate void ProjectSelectedHandler (ProjectLongoMatch project);
	/* Show project stats */
	public delegate void ShowProjectStats (ProjectLongoMatch project);
	public delegate void PlaylistVisibiltyHandler (bool visible);
	public delegate void AnalysisWidgetsVisibilityHandler (bool visible);
	public delegate void AnalysisModeChangedHandler (VideoAnalysisMode mode);
	public delegate void TagSubcategoriesChangedHandler (bool tagsubcategories);
	public delegate void ShowTimelineMenuHandler (List<TimelineEventLongoMatch> plays,EventType cat,Time time);
	public delegate void ShowTaggerMenuHandler (List<TimelineEventLongoMatch> plays);
}
