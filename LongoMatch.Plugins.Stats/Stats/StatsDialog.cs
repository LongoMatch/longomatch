//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using LongoMatch.Core.Store;
using LongoMatch.Core.Stats;

namespace LongoMatch.Plugins.Stats
{
	public partial class StatsDialog : Gtk.Dialog
	{
		ProjectStats stats;

		public StatsDialog ()
		{
			this.Build ();
		}

		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			if (stats != null)
				stats.Dispose ();
		}

		public void LoadStats (Project project)
		{
			if (stats != null)
				stats.Dispose ();
			stats = new ProjectStats (project);
			categoriesviewer.LoadStats (stats, project);
			gameviewer.LoadProject (project, stats);
			/* Player stats are filtered */
			playersviewer.LoadProject (project, new ProjectStats (project));
		}
	}
}

