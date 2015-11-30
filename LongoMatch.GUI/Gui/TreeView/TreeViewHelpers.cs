//
//  Copyright (C) 2015 Fluendo S.A.
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
using LongoMatch.Core.Store;

namespace LongoMatch.Gui.Component
{
	public static class TreeViewHelpers
	{
		/// <summary>
		/// Returns the value in the path for the given model
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="model">Model.</param>
		/// <param name="path">Path.</param>
		public static object GetValue (this TreeModel model, TreePath path, int col = 0)
		{
			TreeIter iter;
			model.GetIter (out iter, path);
			return model.GetValue (iter, col);
		}

		/// <summary>
		/// Fill a list of events from a list of paths, if the first and unique path is an EventType the list
		/// is filled with al the child events in this EventType category.
		/// </summary>
		/// <param name = "model">Model.</param>
		/// <param name="events">Events.</param>
		/// <param name="paths">Paths.</param>
		public static List<TimelineEvent> EventsListFromPaths (TreeModel model, TreePath[] paths)
		{
			List<TimelineEvent> events = new List<TimelineEvent> ();

			// If it's an EventType or a Player, traverse all children to fill the list
			if (paths.Length == 1 && !(model.GetValue (paths [0]) is TimelineEvent)) {
				TreeIter parentIter;
				TreeIter child;
				bool hasChild;

				model.GetIter (out parentIter, paths [0]);
				hasChild = model.IterHasChild (parentIter);
				model.IterChildren (out child, parentIter);
				while (hasChild) {
					TimelineEvent evt = model.GetValue (child, 0) as TimelineEvent;
					if (evt != null) {
						events.Add (evt);
					}
					hasChild = model.IterNext (ref child);
				}
			} else {
				foreach (var path in paths) {
					TimelineEvent evt = model.GetValue (path) as TimelineEvent;
					if (evt != null) {
						events.Add (evt);
					}
				}
			}
			return events;
		}
	}
}

