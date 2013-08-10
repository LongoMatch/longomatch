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
using LongoMatch.Stats;
using LongoMatch.Store;

namespace LongoMatch.Gui.Dialog
{
	public partial class StatsViewer : Gtk.Dialog
	{
		ProjectStats stats;
		
		public StatsViewer ()
		{
			this.Build ();
		}
		
		public override void Destroy ()
		{
			base.Destroy ();
			if (stats != null)
				stats.Dispose();
		}
		
		public void LoadStats (Project project) {
			if (stats != null)
				stats.Dispose();
			stats = new ProjectStats (project);
			categoriesviewer.LoadStats (stats);
			gameviewer.Project = project;
		}
	}
}

