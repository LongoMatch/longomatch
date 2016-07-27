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
using VAS.Core.Common;
using Image = VAS.Core.Common.Image;
using Point = VAS.Core.Common.Point;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlaysCoordinatesTagger : Gtk.Bin
	{
		public PlaysCoordinatesTagger ()
		{
			this.Build ();
			HeightRequest = 300;
			WidthRequest = 500;
			field.Tagger.EmitSignals = false;
			hfield.Tagger.EmitSignals = false;
			goal.Tagger.EmitSignals = false;
			field.Tagger.Accuracy = 20;
			hfield.Tagger.Accuracy = 20;
			goal.Tagger.Accuracy = 20;
		}

		public void LoadBackgrounds (ProjectLongoMatch project)
		{
			field.Tagger.Background = project.GetBackground (FieldPositionType.Field);
			hfield.Tagger.Background = project.GetBackground (FieldPositionType.HalfField);
			goal.Tagger.Background = project.GetBackground (FieldPositionType.Goal);
		}

		public void LoadStats (EventTypeStats stats, TeamType team)
		{
			Visible = false;
			
			UpdateTags (stats.GetFieldCoordinates (team, FieldPositionType.Field), field);
			UpdateTags (stats.GetFieldCoordinates (team, FieldPositionType.HalfField), hfield);
			UpdateTags (stats.GetFieldCoordinates (team, FieldPositionType.Goal), goal);
			field.Tagger.ObjectsCanMove = false;
			hfield.Tagger.ObjectsCanMove = false;
			goal.Tagger.ObjectsCanMove = false;
		}

		public void LoadStats (PlayerEventTypeStats stats, TeamType team)
		{
			Visible = false;
			
			UpdateTags (stats.GetFieldCoordinates (FieldPositionType.Field), field);
			UpdateTags (stats.GetFieldCoordinates (FieldPositionType.HalfField), hfield);
			UpdateTags (stats.GetFieldCoordinates (FieldPositionType.Goal), goal);
			field.Tagger.ObjectsCanMove = false;
			hfield.Tagger.ObjectsCanMove = false;
			goal.Tagger.ObjectsCanMove = false;
		}

		public void LoadPlay (TimelineEventLongoMatch play)
		{
			field.Visible = play.EventType.TagFieldPosition;
			hfield.Visible = play.EventType.TagHalfFieldPosition;
			goal.Visible = play.EventType.TagGoalPosition;
			
			play.AddDefaultPositions ();

			if (play.FieldPosition != null) {
				field.Tagger.Points = play.FieldPosition.Points;
			}
			if (play.HalfFieldPosition != null) {
				hfield.Tagger.Points = play.HalfFieldPosition.Points;
			}
			if (play.GoalPosition != null) {
				goal.Tagger.Points = play.GoalPosition.Points;
			}
		}

		void UpdateTags (List<Coordinates> coords, CoordinatesTagger tagger)
		{
			if (coords.Count > 0) {
				Visible = true;
			}
			tagger.Tagger.Coordinates = coords;
			tagger.Visible = coords.Count != 0;
		}

		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
		}
	}
}

