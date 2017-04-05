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
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Stats;
using LongoMatch.Core.Store;
using Image = VAS.Core.Common.Image;

namespace LongoMatch.Plugins.Stats
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CategoryViewer : Gtk.Bin
	{
		List<SubCategoryViewer> subcatViewers;

		public CategoryViewer ()
		{
			this.Build ();
			HomeName = "Home";
			AwayName = "Away";
		}

		public string HomeName { get; set; }

		public string AwayName { get; set; }

		public void LoadBackgrounds (LMProject project)
		{
			alltagger.LoadBackgrounds (project);
			hometagger.LoadBackgrounds (project);
			awaytagger.LoadBackgrounds (project);
		}

		public void LoadStats (EventTypeStats stats)
		{
			homeLabel.Text = HomeName;
			awayLabel.Text = AwayName;
			
			alltagger.LoadStats (stats, TeamType.BOTH);
			
			hometagger.LoadStats (stats, TeamType.LOCAL);
			    
			awaytagger.LoadStats (stats, TeamType.VISITOR);
			
			foreach (Widget child in vbox1.AllChildren) {
				if (child is SubCategoryViewer || child is HSeparator)
					vbox1.Remove (child);
			}
			subcatViewers = new List<SubCategoryViewer> ();
			nodatalabel.Visible = stats.SubcategoriesStats.Count == 0;
			foreach (SubCategoryStat st in stats.SubcategoriesStats) {
				SubCategoryViewer subcatviewer = new SubCategoryViewer ();
				subcatviewer.LoadStats (st, HomeName, AwayName);
				subcatViewers.Add (subcatviewer);
				vbox1.PackStart (subcatviewer);
				vbox1.PackStart (new HSeparator ());
				subcatviewer.Show ();
			}
		}
	}
}

