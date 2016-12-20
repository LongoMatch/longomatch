//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using System.Collections.Generic;
using LongoMatch.Core.Common;
using LongoMatch.Core.Stats;
using LongoMatch.Core.Store;
using LongoMatch.Drawing.Widgets;
using VAS.Core.Common;
using VAS.Drawing.Cairo;
using Gtk;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlaysCoordinatesTagger : Gtk.Bin
	{
		PositionsViewerView field, hfield, goal;

		public PlaysCoordinatesTagger ()
		{
			this.Build ();
			HeightRequest = 300;
			WidthRequest = 500;

			field = new PositionsViewerView (new WidgetWrapper (fieldDrawingarea));
			hfield = new PositionsViewerView (new WidgetWrapper (hfieldDrawingarea));
			goal = new PositionsViewerView (new WidgetWrapper (goalDrawingarea));
		}

		protected override void OnDestroyed ()
		{
			field.Dispose ();
			hfield.Dispose ();
			goal.Dispose ();
			base.OnDestroyed ();
		}

		public void LoadBackgrounds (LMProject project)
		{
			field.Background = project.GetBackground (FieldPositionType.Field);
			hfield.Background = project.GetBackground (FieldPositionType.HalfField);
			goal.Background = project.GetBackground (FieldPositionType.Goal);
		}

		public void LoadStats (EventTypeStats stats, TeamType team)
		{
			Visible = false;

			UpdateTags (stats.GetFieldCoordinates (team, FieldPositionType.Field), field, fieldDrawingarea);
			UpdateTags (stats.GetFieldCoordinates (team, FieldPositionType.HalfField), hfield, hfieldDrawingarea);
			UpdateTags (stats.GetFieldCoordinates (team, FieldPositionType.Goal), goal, goalDrawingarea);
		}

		public void LoadStats (PlayerEventTypeStats stats)
		{
			Visible = false;

			UpdateTags (stats.GetFieldCoordinates (FieldPositionType.Field), field, fieldDrawingarea);
			UpdateTags (stats.GetFieldCoordinates (FieldPositionType.HalfField), hfield, hfieldDrawingarea);
			UpdateTags (stats.GetFieldCoordinates (FieldPositionType.Goal), goal, goalDrawingarea);
		}

		void UpdateTags (List<Coordinates> coords, PositionsViewerView tagger, Widget widget)
		{
			if (coords.Count > 0) {
				Visible = true;
			}
			tagger.Coordinates = coords;
			widget.Visible = coords.Count != 0;
		}
	}
}

