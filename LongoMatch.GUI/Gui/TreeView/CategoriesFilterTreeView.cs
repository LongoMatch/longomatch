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
using System;
using System.Linq;
using Gtk;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.Core;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public class CategoriesFilterTreeView: FilterTreeViewBase
	{

		public CategoriesFilterTreeView () : base ()
		{
			firstColumnName = Catalog.GetString ("Category");
			HeadersVisible = false;
		}

		public override void SetFilter (EventsFilter filter, Project project)
		{
			this.project = project;
			base.SetFilter (filter, project);
		}

		protected override void FillTree ()
		{
			TreeIter catIter;
			store = new TreeStore (typeof(object), typeof(bool));
			
			filter.IgnoreUpdates = true;
			/* Periods */
			catIter = store.AppendValues (new StringObject (Catalog.GetString ("Periods")), false);
			foreach (Period p in project.Periods) {
				store.AppendValues (catIter, p, false);
			}
			
			catIter = store.AppendValues (new StringObject (Catalog.GetString ("Timers")), false);
			foreach (Timer t in project.Timers) {
				store.AppendValues (catIter, t, false);
			}
			
			foreach (EventType evType in project.EventTypes) {
				catIter = store.AppendValues (evType, true);
				filter.FilterEventType (evType, true);

				if (evType is AnalysisEventType) {
					foreach (Tag tag in (evType as AnalysisEventType).Tags) {
						store.AppendValues (catIter, tag, false);
					}
				}
			}

			var tagsByGroup = project.Dashboard.CommonTagsByGroup.ToDictionary (x => x.Key, x => x.Value);
			foreach (string grp in tagsByGroup.Keys) {
				TreeIter grpIter = store.AppendValues (new StringObject (grp), false);
				foreach (Tag tag in tagsByGroup[grp]) {
					store.AppendValues (grpIter, tag, false);
				}
			}

			filter.IgnoreUpdates = false;
			filter.Update ();
			Model = store;
		}

		void UpdateSelectionPriv (TreeIter iter, bool active, bool checkParents = true, bool recurse = true)
		{
			TreeIter child, parent;
			
			object o = store.GetValue (iter, 0);
			store.IterParent (out parent, iter);
			
			if (o is Tag) {
				EventType evType = store.GetValue (parent, 0) as EventType;
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
			store.SetValue (iter, 1, active);
			
			/* Check its parents */
			if (active && checkParents && store.IterIsValid (parent)) {
				UpdateSelectionPriv (parent, active, true, false);
			}
			
			/* Check/Uncheck all children */
			if (recurse) {
				filter.IgnoreUpdates = true;
				store.IterChildren (out child, iter);
				while (store.IterIsValid (child)) {
					UpdateSelectionPriv (child, active, false, false);
					store.IterNext (ref child);
				}
				filter.IgnoreUpdates = false;
			}
			
			if (recurse && checkParents)
				filter.Update ();
		}

		protected override void UpdateSelection (TreeIter iter, bool active)
		{
			UpdateSelectionPriv (iter, active, true, true);
		}

		protected override void RenderColumn (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			object obj = store.GetValue (iter, 0);
			string text = "";
			
			if (obj is EventType) {
				EventType evType = obj as EventType;
				text = evType.Name;
			} else if (obj is Tag) {
				text = (obj as Tag).Value;
			} else if (obj is Timer) {
				text = (obj as Timer).Name;
			} else if (obj is StringObject) {
				text = (obj as StringObject).Text;
			}
			
			(cell as CellRendererText).Text = text;
		}

		protected override void Select (bool select_all)
		{
			TreeIter iter;
			
			filter.Silent = true;
			store.GetIterFirst (out iter);
			while (store.IterIsValid (iter)) {
				UpdateSelection (iter, select_all);
				store.IterNext (ref iter);
			}
			filter.Silent = false;
			filter.Update ();
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			return false;
		}

	
	}
}

