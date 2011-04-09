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
using LongoMatch;
using LongoMatch.DB;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Common;

namespace LongoMatch.Handlers
{

	/*Tagging Events*/
	//A Play was selected
	public delegate void TimeNodeSelectedHandler(Play play);
	//A new play needs to be create for a specific category at the current play time
	public delegate void NewMarkEventHandler(Category category);
	//The start time of a new play has been signaled
	public delegate void NewMarkStartHandler();
	//The stop of a nes play has been signaled
	public delegate void NewMarkStopHandler(Category category);
	//Several plays needs to be created for a several categories
	public delegate void NewMarksEventHandler(List<int> sections);
	//A need play needs to be created at precise frame
	public delegate void NewMarkAtFrameEventHandler(Category category,int frame);
	//A play was edited
	public delegate void TimeNodeChangedHandler(TimeNode tNode, object val);
	//A list of plays was deleted
	public delegate void TimeNodeDeletedHandler(List<Play> plays);
	//Players needs to be tagged
	public delegate void PlayersTaggedHandler(Play play, Team team);
	//Tag a play
	public delegate void TagPlayHandler(Play play);

	/*Playlist Events*/
	//Add the a play to the opened playlist
	public delegate void PlayListNodeAddedHandler(Play play);
	//A play list element is selected
	public delegate void PlayListNodeSelectedHandler(PlayListPlay play, bool hasNext);
	//Save current playrate to a play list element
	public delegate void ApplyCurrentRateHandler(PlayListPlay play);

	//Drawing events
	//Draw tool changed
	public delegate void DrawToolChangedHandler(LongoMatch.Gui.Component.DrawTool drawTool);
	//Paint color changed
	public delegate void ColorChangedHandler(Gdk.Color color);
	//Paint line width changed
	public delegate void LineWidthChangedHandler(int width);
	//Toggle widget visibility
	public delegate void VisibilityChangedHandler(bool visible);
	//Clear drawings
	public delegate void ClearDrawingHandler();
	//Transparency value changed
	public delegate void TransparencyChangedHandler(double transparency);


	//The position of the stream has changed
	public delegate void PositionChangedHandler(Time pos);
	//A date was selected
	public delegate void DateSelectedHandler(DateTime selectedDate);
	//Create snapshots for a play
	public delegate void SnapshotSeriesHandler(Play tNode);
	//A new version of the software exists
	public delegate void NewVersionHandler(Version version, string URL);


	public delegate void CategoryHandler(Category category);
	public delegate void CategoriesHandler(List<Category> categoriesList);
	public delegate void SubCategoriesHandler(List<ISubCategory> subcat);

	public delegate void ProjectsSelectedHandler(List<ProjectDescription> projects);
}
