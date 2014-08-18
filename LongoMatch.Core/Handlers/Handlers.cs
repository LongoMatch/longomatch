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
using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Store;
using LongoMatch.Store.Playlists;
using LongoMatch.Store.Templates;

namespace LongoMatch.Handlers
{

	/*Tagging Events*/
	/* A Play was selected */
	public delegate void PlaySelectedHandler(Play play);
	/* A new play needs to be create for a specific category at the current play time */
	public delegate void NewTagHandler (TaggerButton tagger, List<Player> plays, List<Tag> tags, Time start, Time stop);
	//A play was edited
	public delegate void TimeNodeChangedHandler(TimeNode tNode, object val);
	public delegate void CategoryChangedHandler(Category cat);
	/* A list of plays needs to be deleted */
	public delegate void PlaysDeletedHandler(List<Play> plays);
	/* Tag a play */
	public delegate void TagPlayHandler(Play play);
	/* Change the Play's category */
	public delegate void PlayCategoryChangedHandler(Play play, Category cat);
	/* DUplicate play */
	public delegate void DuplicatePlaysHandler (List<Play> plays);
	/* Category Selected */
	public delegate void TaggersSelectedHandler (List<TaggerButton> taggerbuttons);
	public delegate void TaggerSelectedHandler (TaggerButton taggerbutton);
	public delegate void ShowButtonsTaggerMenuHandler (TaggerButton taggerbutton, Tag tag);
	
	/* Penalty Card */
	public delegate void PenaltyCardHandler (PenaltyCard card);
	/* Score */
	public delegate void ScoreHandler (Score score);
	
	public delegate void TeamsTagsChangedHandler ();
	
	/* Project Events */
	public delegate void SaveProjectHandler(Project project, ProjectType projectType);
	public delegate void OpenedProjectChangedHandler(Project project, ProjectType projectType, PlaysFilter filter,
	                                                 IAnalysisWindow analysisWindow);
	public delegate void OpenProjectIDHandler(Guid project_id);
	public delegate void OpenProjectHandler();
	public delegate void CloseOpenendProjectHandler();
	public delegate void NewProjectHandler(Project project);
	public delegate void OpenNewProjectHandler(Project project, ProjectType projectType, CaptureSettings captureSettings);
	public delegate void ImportProjectHandler ();
	public delegate void ExportProjectHandler (Project project);
	public delegate void QuitApplicationHandler ();
	public delegate void CreateThumbnailsHandler (Project project);
	
	/* GUI */
	public delegate void ManageJobsHandler();
	public delegate void ManageTeamsHandler();
	public delegate void ManageCategoriesHandler();
	public delegate void ManageProjects();
	public delegate void ManageDatabases();
	public delegate void EditPreferences();
	

	/*Playlist Events*/
	/* Create a new playlist */
	public delegate Playlist NewPlaylistHandler (Project project);
	/* Add a new rendering job */
	public delegate void RenderPlaylistHandler(Playlist playlist);
	/* A play list element is selected */
	public delegate void PlaylistElementSelectedHandler (Playlist playlist, IPlaylistElement element);
	/* Add a play to a playlist */
	public delegate void AddPlaylistElementHandler (Playlist playlist, List<IPlaylistElement> element);
	/* Play next playlist element */
	public delegate void NextPlaylistElementHandler (Playlist playlist);
	/* Play previous playlist element */
	public delegate void PreviousPlaylistElementHandler (Playlist playlist);
	/* Playlists have been edited */
	public delegate void PlaylistsChangedHandler (object sender);

	/* Create snapshots for a play */
	public delegate void SnapshotSeriesHandler(Play tNode);
	
	/* Convert a video file */
	public delegate void ConvertVideoFilesHandler (List<MediaFile> inputFiles, EncodingSettings encSettings);
	
	/* A date was selected */
	public delegate void DateSelectedHandler(DateTime selectedDate);
	
	/* A new version of the software exists */
	public delegate void NewVersionHandler(Version version, string URL);

	/* Edit Category */
	public delegate void CategoryHandler(Category category);
	public delegate void CategoriesHandler(List<Category> categoriesList);
	
	/* Edit player properties */
	public delegate void PlayerPropertiesHandler(Player player);
	public delegate void PlayersPropertiesHandler(List<Player> players);
	
	/* Players selection */
	public delegate void PlayersSubstitutionHandler (Player p1, Player p2, TeamTemplate team);
	public delegate void PlayersSelectionChangedHandler (List<Player> players);
	
	/* A list of projects have been selected */
	public delegate void ProjectsSelectedHandler(List<ProjectDescription> projects);
	public delegate void ProjectSelectedHandler(ProjectDescription project);
	
	public delegate void KeyHandler (object sender, int key, int modifier);

	/* The plays filter was updated */	
	public delegate void FilterUpdatedHandler ();
	
	public delegate void DetachPlayerHandler ();
	
	/* Show project stats */
	public delegate void ShowProjectStats(Project project);
	
	public delegate void ShowFullScreenHandler (bool fullscreen);
	public delegate void PlaylistVisibiltyHandler (bool visible);
	public delegate void AnalysisWidgetsVisibilityHandler (bool visible);
	public delegate void AnalysisModeChangedHandler (VideoAnalysisMode mode);
	public delegate void TagSubcategoriesChangedHandler (bool tagsubcategories);
	
	public delegate void ShowTimelineMenuHandler (List<Play> plays, Category cat, Time time);
	public delegate void ShowTaggerMenuHandler (List<Play> plays);
	public delegate void ShowDrawToolMenuHandler (IBlackboardObject drawable);
	public delegate void ConfigureDrawingObjectHandler (IBlackboardObject drawable);
	public delegate void DrawableChangedHandler (IBlackboardObject drawable);

	public delegate void BackEventHandle ();
}
