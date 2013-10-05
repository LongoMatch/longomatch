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

using LongoMatch.Common;
using LongoMatch.Stats;
using Image = LongoMatch.Common.Image;

namespace LongoMatch.Gui.Component.Stats
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayerCategoryViewer : Gtk.Bin
	{
		public PlayerCategoryViewer ()
		{
			this.Build ();
		}

		public void LoadBackgrounds (Image field, Image halfField, Image goal) {
			tagger.LoadBackgrounds (field, halfField, goal);
		}

		public void LoadStats (CategoryStats stats) {
			tagger.LoadFieldCoordinates (stats.FieldCoordinates);
			tagger.LoadHalfFieldCoordinates (stats.HalfFieldCoordinates);
			tagger.LoadGoalCoordinates (stats.GoalCoordinates);
			tagger.CoordinatesSensitive = false;
			
			foreach (Widget child in vbox1.AllChildren) {
				if (!(child is PlaysCoordinatesTagger))
					vbox1.Remove (child);
			}
			foreach (SubCategoryStat st in stats.SubcategoriesStats) {
				PlayerSubcategoryViewer subcatviewer = new PlayerSubcategoryViewer();
				subcatviewer.LoadStats (st);
				vbox1.PackStart (subcatviewer);
				vbox1.PackStart (new HSeparator());
				subcatviewer.Show ();
			}
		}	
	}
}

