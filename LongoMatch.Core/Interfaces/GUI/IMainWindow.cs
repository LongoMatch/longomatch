// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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

using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Interfaces.GUI
{
	public interface IMainWindow
	{	
		/* Tags */
		event NewTagHandler NewTagEvent;
		event NewTagStartHandler NewTagStartEvent;
		event NewTagStopHandler NewTagStopEvent;
		event PlaySelectedHandler PlaySelectedEvent;
		event NewTagAtFrameHandler NewTagAtFrameEvent;
		event TagPlayHandler TagPlayEvent;
		event PlaysDeletedHandler PlaysDeletedEvent;
		event TimeNodeChangedHandler TimeNodeChanged;
		
		/* Playlist */
		event RenderPlaylistHandler RenderPlaylistEvent;
		event PlayListNodeAddedHandler PlayListNodeAddedEvent;
		event PlayListNodeSelectedHandler PlayListNodeSelectedEvent;
		event OpenPlaylistHandler OpenPlaylistEvent;
		event NewPlaylistHandler NewPlaylistEvent;
		event SavePlaylistHandler SavePlaylistEvent; 
		
		/* Snapshots */
		event SnapshotSeriesHandler SnapshotSeriesEvent;
		
		/* Projects */
		event SaveProjectHandler SaveProjectEvent;
		event NewProjectHandler NewProjectEvent;
		event OpenProjectHandler OpenProjectEvent;
		event CloseOpenendProjectHandler CloseOpenedProjectEvent;
		event ImportProjectHandler ImportProjectEvent;
		event ExportProjectHandler ExportProjectEvent;
		
		/* Managers */
		event ManageJobsHandler ManageJobsEvent; 
		event ManageTeamsHandler ManageTeamsEvent;
		event ManageCategoriesHandler ManageCategoriesEvent;
		event ManageProjects ManageProjectsEvent;
		event ApplyCurrentRateHandler ApplyRateEvent;
		
		/* Game Units events */
		event GameUnitHandler GameUnitEvent;
		event UnitChangedHandler UnitChanged;
		event UnitSelectedHandler UnitSelected;
		event UnitsDeletedHandler UnitDeleted;
		event UnitAddedHandler UnitAdded;
		
		void SetProject(Project project, ProjectType projectType, CaptureSettings props);
		void AddPlay(Play play);
		void UpdateSelectedPlay (Play play);
		void UpdateCategories (Categories categories);
		void DeletePlays (List<Play> plays);
		void UpdateGameUnits (GameUnitsList gameUnits);
		
		IRenderingStateBar RenderingStateBar{get;}
		IPlayer Player{get;}
		ICapturer Capturer{get;}
		IPlaylistWidget Playlist{get;}
		
		void AddExportEntry (string name, string shortName, Action<Project, string> exportAction);
	}
}

