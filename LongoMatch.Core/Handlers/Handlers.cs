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
using VAS.Core.Store;

namespace LongoMatch.Core.Handlers
{
	/* The players tagged in an event have changed */
	public delegate void TeamsTagsChangedHandler ();

	public delegate void OpenProjectIDHandler (Guid project_id,ProjectLongoMatch project);
	public delegate void OpenProjectHandler ();
	public delegate void NewProjectHandler (ProjectLongoMatch project);
	public delegate void OpenNewProjectHandler (ProjectLongoMatch project,ProjectType projectType,CaptureSettings captureSettings);
	public delegate void ImportProjectHandler ();
	public delegate void ExportProjectHandler (ProjectLongoMatch project);
	public delegate void CreateThumbnailsHandler (ProjectLongoMatch project);

	/* Playlists have been edited */
	public delegate void PlaylistsChangedHandler (object sender);

	/* Edit player properties */
	public delegate void PlayerPropertiesHandler (PlayerLongoMatch player);
	public delegate void PlayersPropertiesHandler (List<PlayerLongoMatch> players);
	/* Players selection */
	public delegate void PlayersSubstitutionHandler (SportsTeam team,PlayerLongoMatch p1,PlayerLongoMatch p2,
		SubstitutionReason reason,Time time);
	public delegate void PlayersSelectionChangedHandler (List<PlayerLongoMatch> players);
	public delegate void TeamSelectionChangedHandler (ObservableCollection<SportsTeam> teams);
	/* A list of projects have been selected */
	public delegate void ProjectsSelectedHandler (List<ProjectLongoMatch> projects);
	public delegate void ProjectSelectedHandler (ProjectLongoMatch project);
	public delegate void PlaylistVisibiltyHandler (bool visible);
	public delegate void AnalysisWidgetsVisibilityHandler (bool visible);
	public delegate void AnalysisModeChangedHandler (VideoAnalysisMode mode);
	public delegate void ShowTimelineMenuHandler (List<TimelineEvent> plays,EventType cat,Time time);
	public delegate void ShowTaggerMenuHandler (List<TimelineEvent> plays);
}
