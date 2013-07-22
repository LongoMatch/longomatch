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

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlaysCoordinatesTagger : Gtk.Bin
	{
		
		CoordinatesTagger field, hfield, goal;
		Box box;
		
		public PlaysCoordinatesTagger ()
		{
			this.Build ();
			SetMode (true);
		}
		
		public void SetMode (bool horizontal) {
			if (box != null) {
				mainbox.Remove (box);
				box.Destroy();
			}
			if (horizontal) {
				box = new HBox ();
			} else {
				box = new VBox ();
			}
			field = new CoordinatesTagger ();
			hfield = new CoordinatesTagger ();
			goal = new CoordinatesTagger ();
			box.PackStart (field, true, true, 0);
			box.PackStart (hfield, true, true, 0);
			box.PackStart (goal, true, true, 0);
			mainbox.PackStart (box, true, true, 0);
			ShowAll ();
		}
		
		public void LoadPlays (List<Play> plays, Categories template, bool horizontal=true) {
			field.Visible = hfield.Visible = goal.Visible = false;
			SetBackgrounds (template);
			foreach (Play play in plays) {
				AddPlay (play, false);
			}
		}
		
		public void LoadPlay (Play play, Categories template, bool horizontal=true) {
			field.Visible = hfield.Visible = goal.Visible = false;
			
			SetBackgrounds (template);
			AddPlay (play, true);
			
		}

		void SetBackgrounds (Categories template) {
			if (template.FieldBackgroundImage != null) {
				field.Background = template.FieldBackgroundImage.Value;
			} else {
				field.Background = Gdk.Pixbuf.LoadFromResource (Constants.FIELD_BACKGROUND);
			}
			if (template.HalfFieldBackgroundImage != null) {
				hfield.Background = template.HalfFieldBackgroundImage.Value;
			} else {
				hfield.Background = Gdk.Pixbuf.LoadFromResource (Constants.HALF_FIELD_BACKGROUND);
			}
			if (template.GoalBackgroundImage != null) {
				goal.Background = template.GoalBackgroundImage.Value;
			} else {
				goal.Background = Gdk.Pixbuf.LoadFromResource (Constants.GOAL_BACKGROUND);
			}
		}
		
		void AddPlay (Play play, bool fill) {
			if (play.Category.TagFieldPosition) {
				AddFieldPosTagger (play, fill);				
			}
			if (play.Category.TagHalfFieldPosition) {
				AddHalfFieldPosTagger (play, fill);
			}
			if (play.Category.TagGoalPosition) {
				AddGoalPosTagger (play, fill);
			}
		}
		
		void AddFieldPosTagger (Play play, bool fill) {
			List<Coordinates> coords = new List<Coordinates>();
			
			if (play.FieldPosition != null) {
				coords.Add (play.FieldPosition);
			} else if (fill) {
				Coordinates c = new Coordinates ();
				c.Add (new Point(100, 100));
				if (play.Category.FieldPositionIsDistance) {
					c.Add (new Point (300, 300));
				}
				coords.Add (c);
				play.FieldPosition = c;
			} else {
				return;
			}
			field.Coordinates = coords;
			field.Visible = true;
		}
		
		void AddHalfFieldPosTagger (Play play, bool fill) {
			List<Coordinates> coords = new List<Coordinates>();
			
			if (play.HalfFieldPosition != null) {
				coords.Add (play.HalfFieldPosition);
			} else  if (fill) {
				Coordinates c = new Coordinates ();
				c.Add (new Point(100, 100));
				if (play.Category.HalfFieldPositionIsDistance) {
					c.Add (new Point (300, 300));
				}
				coords.Add (c);
				play.HalfFieldPosition = c;
			} else {
				return;
			}
			hfield.Coordinates = coords;
			hfield.Visible = true;
		}
		
		void AddGoalPosTagger (Play play, bool fill) {
			List<Coordinates> coords = new List<Coordinates>();
			
			if (play.GoalPosition != null) {
				coords.Add (play.GoalPosition);
			} else if (fill) {
				Coordinates c = new Coordinates ();
				c.Add (new Point(100, 100));
				coords.Add (c);
				play.GoalPosition = c;
			} else {
				return;
			}
			goal.Coordinates = coords; 
			goal.Visible = true;
		}
	}
}

