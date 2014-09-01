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
using System;
using System.Collections.Generic;
using Gtk;
using Gdk;

using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Common;

using Point = LongoMatch.Common.Point;
using Image = LongoMatch.Common.Image;
using LongoMatch.Stats;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
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

		public void LoadBackgrounds (Project project) {
			field.Tagger.Background = project.GetBackground (FieldPositionType.Field);
			hfield.Tagger.Background = project.GetBackground (FieldPositionType.HalfField);
			goal.Tagger.Background = project.GetBackground (FieldPositionType.Goal);
		}
		
		public void LoadStats (CategoryStats stats) {
		}
		
		public void LoadPlay (Play play) {
			field.Visible = play.Category.TagFieldPosition;
			hfield.Visible = play.Category.TagHalfFieldPosition;
			goal.Visible = play.Category.TagGoalPosition;
			vbox2.Visible = hfield.Visible || goal.Visible;
			
			play.AddDefaultPositions ();

			if (play.FieldPosition != null) {
				field.Tagger.Points = play.FieldPosition.Points;
			}
			if (play.HalfFieldPosition != null) {
				hfield.Tagger.Points = play.HalfFieldPosition.Points;
			}
			if (play.GoalPosition != null ) {
				goal.Tagger.Points = play.GoalPosition.Points;
			}
		}
		
		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
		}
	}
}

