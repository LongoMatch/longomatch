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
using Gtk;
using LongoMatch.Stats;
using System.Collections.Generic;
using LongoMatch.Common;

namespace LongoMatch.Gui.Component.Stats
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CategoryViewer : Gtk.Bin
	{
		List<SubCategoryViewer> subcatViewers;
		public CategoryViewer ()
		{
			this.Build ();
		}
		
		public void LoadStats (CategoryStats stats) {
			PlaysCoordinatesTagger tagger;
			
			tagger = new PlaysCoordinatesTagger();
			vbox1.PackStart (tagger);
			subcatViewers = new List<SubCategoryViewer>();
						
			foreach (Widget child in vbox1.AllChildren) {
				vbox1.Remove (child);
			}
			
			foreach (SubCategoryStat st in stats.SubcategoriesStats) {
				SubCategoryViewer subcatviewer = new SubCategoryViewer();
				subcatviewer.LoadStats (st);
				subcatViewers.Add (subcatviewer);
				vbox1.PackStart (subcatviewer);
				vbox1.PackStart (new HSeparator());
			}
			
			tagger = new PlaysCoordinatesTagger ();
			vbox1.ShowAll ();
		}
	}
}

