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
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Common;

namespace LongoMatch.Handlers
{

	/*Tagging Events*/
	/* A Play was selected */
	public delegate void PlaySelectedHandler(Play play);
	/* A new play needs to be create for a specific category at the current play time */
	public delegate void NewTagHandler(Category category);
	/* Signal the start time to tag a new play */
	public delegate void NewTagStartHandler();
	/* Signal the stop time to tag a new play */
	public delegate void NewTagStopHandler(Category category);
	/* A new play needs to be created at a defined frame */
	public delegate void NewTagAtFrameHandler(Category category,int frame);
	//A play was edited
	public delegate void TimeNodeChangedHandler(TimeNode tNode, object val);
	/* A list of plays needs to be deleted */
	public delegate void PlaysDeletedHandler(List<Play> plays);
	/* Tag a play */
	public delegate void TagPlayHandler(Play play);
	
	/* Project Events */
	public delegate void SaveProjectHandler(Project project, ProjectType projectType);
	public delegate void OpenedProjectChangedHandler(Project project, ProjectType projectType);
	public delegate void OpenProjectHandler();
	public delegate void CloseOpenendProjectHandler(bool save);
	public delegate void NewProjectHandler();
	public delegate void ImportProjectHandler();
	public delegate void ExportProjectHandler();
	
	/* GUI */
	public delegate void ManageJobsHandler();
	public delegate void ManageTeamsHandler();
	public delegate void ManageCategoriesHandler();
	public delegate void ManageProjects();
	

	/*Playlist Events*/
	/* Add the a play to the opened playlist */
	public delegate void PlayListNodeAddedHandler(Play play);
	/* A play list element is selected */
	public delegate void PlayListNodeSelectedHandler(PlayListPlay play);
	/* Save current playrate for a play list element */
	public delegate void ApplyCurrentRateHandler(PlayListPlay play);
	/* Open a playlist */
	public delegate void OpenPlaylistHandler();
	/* New a playlist */
	public delegate void NewPlaylistHandler();
	/* Save a playlist */
	public delegate void SavePlaylistHandler();

	/* Drawing events */
	/* Draw tool changed */
	public delegate void DrawToolChangedHandler(DrawTool drawTool);
	/* Paint color changed */
	public delegate void ColorChangedHandler(System.Drawing.Color color);
	/* Paint line width changed */
	public delegate void LineWidthChangedHandler(int width);
	/* Toggle widget visibility */
	public delegate void VisibilityChangedHandler(bool visible);
	/* Clear drawings */
	public delegate void ClearDrawingHandler();
	/* Transparency value changed */
	public delegate void TransparencyChangedHandler(double transparency);
	
	/* The position of the stream has changed */
	public delegate void PositionChangedHandler(Time pos);
	
	/* Create snapshots for a play */
	public delegate void SnapshotSeriesHandler(Play tNode);
	
	/* Add a new rendering job */
	public delegate void RenderPlaylistHandler(IPlayList playlist);
	 
	/* A date was selected */
	public delegate void DateSelectedHandler(DateTime selectedDate);
	
	/* A new version of the software exists */
	public delegate void NewVersionHandler(Version version, string URL);

	/* Edit Category */
	public delegate void CategoryHandler(Category category);
	public delegate void CategoriesHandler(List<Category> categoriesList);
	
	/* Edit Subcategory properties */
	public delegate void SubCategoryHandler(ISubCategory subcat);
	public delegate void SubCategoriesHandler(List<ISubCategory> subcat);

	/* Edit player properties */
	public delegate void PlayerPropertiesHandler(Player player);
	public delegate void PlayersPropertiesHandler(List<Player> players);
	
	/* A list of projects have been selected */
	public delegate void ProjectsSelectedHandler(List<ProjectDescription> projects);
	
	/* Start/Stop/Cancel game units */
	public delegate void GameUnitHandler(GameUnit gameUnit, GameUnitEventType eType);
	
	public delegate void UnitChangedHandler (GameUnit gameUnit, TimelineNode unit, Time time);
	public delegate void UnitSelectedHandler (GameUnit gameUnit, TimelineNode unit);
	public delegate void UnitAddedHandler (GameUnit gameUnit, int frame);
	public delegate void UnitsDeletedHandler (GameUnit gameUnit, List<TimelineNode> unit);

}
