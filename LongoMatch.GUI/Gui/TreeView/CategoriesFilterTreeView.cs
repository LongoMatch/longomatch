// 
//  Copyright (C) 2012 Andoni Morales Alastruey
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
using System.Linq;
using Gtk;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using VAS.Core;
using VAS.Core.Store;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public class CategoriesFilterTreeView: FilterTreeViewBase
	{
		ProjectLongoMatch project;
		EventsFilter filter;

		public void SetFilter (EventsFilter filter, ProjectLongoMatch project)
		{
			this.project = project;
			this.filter = filter;
			FillTree ();
		}

		protected override void UpdateSelection (TreeIter iter, bool active)
		{
			UpdateSelectionPriv (iter, active, true, true);
		}

		public override void ToggleAll (bool active)
		{
			TreeIter current;
			store.GetIterFirst (out current);
			filter.IgnoreUpdates = true;
			ToggleAll (current, active, false);
			filter.IgnoreUpdates = false;
			filter.Update ();
		}

		void FillTree ()
		{
			TreeIter catIter;

			store = Model as TreeStore;
			store.Clear ();
			filter.IgnoreUpdates = true;

			/* Periods */
			catIter = store.AppendValues (Catalog.GetString ("Periods"), false,
				new StringObject (Catalog.GetString ("Periods")));
			foreach (Period p in project.Periods) {
				store.AppendValues (catIter, p.Name, false, p);
			}
			
			catIter = store.AppendValues (Catalog.GetString ("Timers"), false,
				new StringObject (Catalog.GetString ("Timers")));
			foreach (Timer t in project.Timers) {
				store.AppendValues (catIter, t.Name, false, t);
			}
			
			foreach (EventType evType in project.EventTypes) {
				catIter = store.AppendValues (evType.Name, false, evType);

				if (evType is AnalysisEventType) {
					foreach (Tag tag in (evType as AnalysisEventType).Tags) {
						store.AppendValues (catIter, tag.Value, false, tag);
					}
				}
			}

			var tagsByGroup = project.Dashboard.CommonTagsByGroup.ToDictionary (x => x.Key, x => x.Value);
			foreach (string grp in tagsByGroup.Keys) {
				TreeIter grpIter = store.AppendValues (grp, false, new StringObject (grp));
				foreach (Tag tag in tagsByGroup[grp]) {
					store.AppendValues (grpIter, tag.Value, false, tag);
				}
			}

			filter.IgnoreUpdates = false;
			filter.Update ();
		}

		void UpdateSelectionPriv (TreeIter iter, bool active, bool checkParents = true, bool recurse = true)
		{
			TreeIter child, parent;
			
			object o = store.GetValue (iter, COL_VALUE);
			store.IterParent (out parent, iter);
			
			if (o is Tag) {
				EventType evType = store.GetValue (parent, COL_VALUE) as EventType;
				if (evType != null) {
					filter.FilterEventTag (evType, o as Tag, active);
				} else {
					filter.FilterTag (o as Tag, active);
				}
			} else if (o is EventType) {
				filter.FilterEventType (o as EventType, active);
			} else if (o is Period) {
				filter.FilterPeriod (o as Period, active);
			} else if (o is Timer) {
				filter.FilterTimer (o as Timer, active);
			}
			store.SetValue (iter, COL_ACTIVE, active);
			
			/* Check its parents */
			if (active && checkParents && store.IterIsValid (parent)) {
				UpdateSelectionPriv (parent, active, true, false);
			}
			
			/* Check/Uncheck all children */
			if (recurse) {
				bool state = filter.IgnoreUpdates;
				filter.IgnoreUpdates = true;
				store.IterChildren (out child, iter);
				while (store.IterIsValid (child)) {
					UpdateSelectionPriv (child, active, false, false);
					store.IterNext (ref child);
				}
				filter.IgnoreUpdates = state;
			}
			
			if (recurse && checkParents)
				filter.Update ();
		}
	}
}
