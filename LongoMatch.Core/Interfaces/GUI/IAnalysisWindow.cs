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
	public interface IAnalysisWindow
	{	
		/* Tags */
		event NewTagHandler NewTagEvent;
		event NewTagStartHandler NewTagStartEvent;
		event NewTagStopHandler NewTagStopEvent;
		event NewTagCancelHandler NewTagCancelEvent;
		event PlaySelectedHandler PlaySelectedEvent;
		event NewTagAtFrameHandler NewTagAtFrameEvent;
		event TagPlayHandler TagPlayEvent;
		event PlaysDeletedHandler PlaysDeletedEvent;
		event TimeNodeChangedHandler TimeNodeChanged;
		event PlayCategoryChangedHandler PlayCategoryChanged;
		event DuplicatePlayHandler DuplicatePlay;
		
		/* Playlist */
		event RenderPlaylistHandler RenderPlaylistEvent;
		event PlayListNodeAddedHandler PlayListNodeAddedEvent;
		event PlayListNodeSelectedHandler PlayListNodeSelectedEvent;
		event OpenPlaylistHandler OpenPlaylistEvent;
		event NewPlaylistHandler NewPlaylistEvent;
		event SavePlaylistHandler SavePlaylistEvent; 
		
		/* Snapshots */
		event SnapshotSeriesHandler SnapshotSeriesEvent;
		
		/* Game Units events */
		event GameUnitHandler GameUnitEvent;
		event UnitChangedHandler UnitChanged;
		event UnitSelectedHandler UnitSelected;
		event UnitsDeletedHandler UnitDeleted;
		event UnitAddedHandler UnitAdded;
		
		/* Error handling */
		event CloseOpenendProjectHandler CloseOpenedProjectEvent;
		
		event KeyHandler KeyPressed;
		
		void SetProject(Project project, ProjectType projectType, CaptureSettings props, PlaysFilter filter);
		void CloseOpenedProject ();
		void AddPlay(Play play);
		void UpdateSelectedPlay (Play play);
		void UpdateCategories (Categories categories);
		void DeletePlays (List<Play> plays);
		void UpdateGameUnits (GameUnitsList gameUnits);
		
		bool Fullscreen {set;}
		bool WidgetsVisible {set;}
		bool PlaylistVisible {set;}
		VideoAnalysisMode AnalysisMode {set;}
		
		IPlayerBin Player{get;}
		ICapturerBin Capturer{get;}
		IPlaylistWidget Playlist{get;}
		ITemplatesService TemplatesService{set;}
	}
}

