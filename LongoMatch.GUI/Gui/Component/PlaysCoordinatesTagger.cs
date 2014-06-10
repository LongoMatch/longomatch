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

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlaysCoordinatesTagger : Gtk.Bin
	{
		
		CoordinatesTagger field, hfield, goal;
		Pixbuf fieldPixbuf, hFieldPixbuf, goalPixbuf;
		Box box;
		
		public PlaysCoordinatesTagger ()
		{
			this.Build ();
			SetMode (true);
			Config.EventsBroker.PlaySelected += (play) => LoadPlay (play, false);
		}
		
		public bool CoordinatesSensitive {
			set {
				field.Sensitive = value;
				hfield.Sensitive = value;
				goal.Sensitive = value;
			}
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
			box.Show ();
		}
		
		public void LoadPlay (Play play, bool horizontal=true) {
			field.Visible = hfield.Visible = goal.Visible = false;
			AddPlay (play, true);
		}
		
		public void LoadFieldCoordinates (List<Coordinates> coords) {
			field.Coordinates = coords;
			field.Visible = coords.Count != 0;
		}

		public void LoadHalfFieldCoordinates (List<Coordinates> coords) {
			hfield.Coordinates = coords;
			hfield.Visible = coords.Count != 0;
		}
		
		public void LoadGoalCoordinates (List<Coordinates> coords) {
			goal.Coordinates = coords;
			goal.Visible = coords.Count != 0;
		}
		
		public void LoadBackgrounds (Image fieldImage, Image halfFieldImage, Image goalImage) {
			if (fieldImage != null) {
				fieldPixbuf = fieldImage.Value;
			} else {
				fieldPixbuf = Config.FieldBackground.Value;
			}
			field.Background = fieldPixbuf;
			if (halfFieldImage != null) {
				hFieldPixbuf = halfFieldImage.Value;
			} else {
				hFieldPixbuf = Config.HalfFieldBackground.Value;
			}
			hfield.Background = hFieldPixbuf;
			if (goalImage != null) {
				goalPixbuf = goalImage.Value;
			} else {
				goalPixbuf = Config.GoalBackground.Value;
			}
			goal.Background = goalPixbuf;
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
				c.Points.Add (new Point((int) (fieldPixbuf.Width * 0.25),
				                 (int) (fieldPixbuf.Height * 0.25)));
				if (play.Category.FieldPositionIsDistance) {
					c.Points.Add (new Point((int) (fieldPixbuf.Width * 0.75),
					                 (int) (fieldPixbuf.Height * 0.75)));
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
				c.Points.Add (new Point((int) (hFieldPixbuf.Width * 0.25),
				                 (int) (hFieldPixbuf.Height * 0.25)));
				if (play.Category.HalfFieldPositionIsDistance) {
					c.Points.Add (new Point((int) (hFieldPixbuf.Width * 0.75),
					                 (int) (hFieldPixbuf.Height * 0.75)));
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
				c.Points.Add (new Point((int) (goalPixbuf.Width * 0.25),
				                 (int) (goalPixbuf.Height * 0.25)));
				coords.Add (c);
				play.GoalPosition = c;
			} else {
				return;
			}
			goal.Coordinates = coords; 
			goal.Visible = true;
		}
		
		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			if (fieldPixbuf != null)
				fieldPixbuf.Dispose();
			if (hFieldPixbuf != null)
				hFieldPixbuf.Dispose();
			if (goalPixbuf != null)
				goalPixbuf.Dispose();
		}
	}
}

