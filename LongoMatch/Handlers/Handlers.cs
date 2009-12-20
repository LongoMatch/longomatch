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
using LongoMatch.TimeNodes;

namespace LongoMatch.Handlers
{

	/*Tagging Events*/
	//A Play was selected
	public delegate void TimeNodeSelectedHandler(MediaTimeNode tNode);
	//A new play needs to be create for a specific category at the current play time
	public delegate void NewMarkEventHandler(int i);
	//Several plays needs to be created for a several categories
	public delegate void NewMarksEventHandler(List<int> sections);
	//A need play needs to be created at precise frame
	public delegate void NewMarkAtFrameEventHandler(int i,int frame);
	//A play was edited
	public delegate void TimeNodeChangedHandler(TimeNode tNode, object val);
	//A play was deleted
	public delegate void TimeNodeDeletedHandler(MediaTimeNode tNode,int section);
	//Players needs to be tagged
	public delegate void PlayersTaggedHandler(MediaTimeNode tNode, Team team);
	//Tag a play
	public delegate void TagPlayHandler(MediaTimeNode tNode);

	/*Playlist Events*/
	//Add the a play to the opened playlist
	public delegate void PlayListNodeAddedHandler(MediaTimeNode tNode);
	//A play list element is selected
	public delegate void PlayListNodeSelectedHandler(PlayListTimeNode plNode, bool hasNext);
	//Save current playrate to a play list element
	public delegate void ApplyCurrentRateHandler(PlayListTimeNode plNode);

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
	public delegate void SnapshotSeriesHandler(MediaTimeNode tNode);
	//A new version of the software exists
	public delegate void NewVersionHandler(Version version, string URL);
	
}
