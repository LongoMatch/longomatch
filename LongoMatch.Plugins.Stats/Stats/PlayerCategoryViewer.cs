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
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Stats;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Component;
using VAS.Core.Store;
using Image = VAS.Core.Common.Image;

namespace LongoMatch.Plugins.Stats
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlayerCategoryViewer : Gtk.Bin
	{
		public PlayerCategoryViewer ()
		{
			this.Build ();
		}

		public void LoadBackgrounds (Project project)
		{
			tagger.LoadBackgrounds (project as LMProject);
		}

		public void LoadStats (PlayerEventTypeStats stats)
		{
			tagger.LoadStats (stats);

			foreach (Widget child in vbox1.AllChildren) {
				if (!(child is PlaysCoordinatesTagger))
					vbox1.Remove (child);
			}
			foreach (SubCategoryStat st in stats.SubcategoriesStats) {
				PlayerSubcategoryViewer subcatviewer = new PlayerSubcategoryViewer ();
				subcatviewer.LoadStats (st);
				vbox1.PackStart (subcatviewer);
				vbox1.PackStart (new HSeparator ());
				subcatviewer.Show ();
			}
		}
	}
}

