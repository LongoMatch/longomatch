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
using Gdk;
using LongoMatch.Core.Stats;
using LongoMatch.Core.Store;

namespace LongoMatch.Plugins.Stats
{
	public partial class StatsDialog : Gtk.Dialog
	{
		ProjectStats stats;

		public StatsDialog ()
		{
			this.Build ();
			Screen screen = Display.Default.DefaultScreen;
			int monitor = screen.GetMonitorAtWindow (GdkWindow);
			Rectangle monitor_geometry = screen.GetMonitorGeometry (monitor);
			Resize (monitor_geometry.Width * 80 / 100, monitor_geometry.Height * 80 / 100);
#if OSTYPE_OS_X
			this.Move (monitor_geometry.Width * 10 / 100, monitor_geometry.Height * 10 / 100);
#endif
			
		}

		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			if (stats != null)
				stats.Dispose ();
		}

		public void LoadStats (ProjectLongoMatch project)
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

