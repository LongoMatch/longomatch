// TreeWidgetPopup.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using Gdk;
using Gtk;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Menus;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Store;
using EventType = VAS.Core.Store.EventType;
using Misc = VAS.UI.Helpers.Misc;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public class PlaysTreeView : ListTreeViewBase
	{
		public event EditEventTypeHandler EditProperties;
		TreeIter srcIter;
		EventType draggedEventType;
		int startX, startY;
		TargetList targetList;
		TargetEntry [] targetEntry;
		bool dragging, dragStarted, catClicked;
		TreeViewDropPosition dropPos;
		EventTypeMenu eventTypeMenu;

		public PlaysTreeView ()
		{
			enableCategoryMove = true;
			targetEntry = new TargetEntry [] { new TargetEntry ("event-type-dnd", TargetFlags.Widget, 0) };
			targetList = new TargetList (targetEntry);
			this.EnableModelDragDest (targetEntry, DragAction.Move);
			eventTypeMenu = new EventTypeMenu ();
			eventTypeMenu.EditProperties += (cat) => EditProperties (cat);
			eventTypeMenu.SortEvent += (sender, e) => modelSort.SetSortFunc (0, SortFunction);
		}

		new public TreeStore Model {
			set {
				base.Model = value;
			}
			get {
				return base.Model as TreeStore;
			}
		}

		public TreePath AddEvent (LMTimelineEvent evt, TreeIter evtTter)
		{
			TreeIter childIter = childModel.AppendValues (evtTter, evt);
			TreePath childPath = childModel.GetPath (childIter);
			TreePath path = modelSort.ConvertChildPathToPath (
								modelFilter.ConvertChildPathToPath (childPath));
			return path;
		}

		protected override int SortFunction (TreeModel model, TreeIter a, TreeIter b)
		{
			object objecta, objectb;
			LMTimelineEvent tna, tnb;

			if (model == null)
				return 0;

			objecta = model.GetValue (a, 0);
			objectb = model.GetValue (b, 0);

			if (objecta == null && objectb == null) {
				return 0;
			} else if (objecta == null) {
				return -1;
			} else if (objectb == null) {
				return 1;
			}

			// Dont't store categories
			if (objecta is EventType && objectb is EventType) {
				return int.Parse (model.GetPath (a).ToString ())
				- int.Parse (model.GetPath (b).ToString ());
			} else if (objecta is LMTimelineEvent && objectb is LMTimelineEvent) {
				tna = objecta as LMTimelineEvent;
				tnb = objectb as LMTimelineEvent;
				switch (tna.EventType.SortMethod) {
				case (SortMethodType.SortByName):
					return String.Compare (tna.Name, tnb.Name);
				case (SortMethodType.SortByStartTime):
					return (tna.Start - tnb.Start).MSeconds;
				case (SortMethodType.SortByStopTime):
					return (tna.Stop - tnb.Stop).MSeconds;
				case (SortMethodType.SortByDuration):
					return (tna.Duration - tnb.Duration).MSeconds;
				default:
					return 0;
				}
			} else {
				return 0;
			}
		}

		override protected bool SelectFunction (TreeSelection selection, TreeModel model, TreePath path, bool selected)
		{
			TreePath [] selectedRows;

			selectedRows = selection.GetSelectedRows ();
			if (!selected && selectedRows.Length > 0) {
				object currentSelected;
				object firstSelected;

				firstSelected = GetValueFromPath (selectedRows [0]);
				// No multiple selection for event types and substitution events
				if (selectedRows.Length == 1) {
					if (firstSelected is EventType) {
						return false;
					} else if (firstSelected is StatEvent) {
						return false;
					}
				}

				currentSelected = GetValueFromPath (path);
				if (currentSelected is EventType || currentSelected is StatEvent) {
					return false;
				}
				return true;
			}
			// Always unselect
			return true;
		}

		protected override bool OnMotionNotifyEvent (EventMotion evnt)
		{
			if (dragging && !dragStarted) {
				if (Math.Sqrt (Math.Pow (startX - evnt.X, 2) + Math.Pow (startY - evnt.Y, 2)) > 5) {
					Gtk.Drag.Begin (this, targetList, DragAction.Move, 1, evnt);
					dragStarted = true;
				}
			}
			return base.OnMotionNotifyEvent (evnt);
		}

		protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
		{
			if (catClicked && !dragStarted) {
				TreePath path;
				GetPathAtPos ((int)evnt.X, (int)evnt.Y, out path);
				if (GetRowExpanded (path)) {
					CollapseRow (path);
				} else {
					ExpandRow (path, true);
				}
			}
			dragging = dragStarted = catClicked = false;
			return base.OnButtonReleaseEvent (evnt);
		}

		protected override bool OnExpandCollapseCursorRow (bool logical, bool expand, bool open_all)
		{
			Console.WriteLine (logical + " " + expand + " " + open_all);
			return base.OnExpandCollapseCursorRow (logical, expand, open_all);
		}

		override protected bool OnButtonPressEvent (Gdk.EventButton evnt)
		{
			TreePath [] paths = Selection.GetSelectedRows ();

			if (Misc.RightButtonClicked (evnt)) {
				// We don't want to unselect the play when several
				// plays are selected and we clik the right button
				// For multiedition
				if (paths.Length <= 1) {
					base.OnButtonPressEvent (evnt);
					paths = Selection.GetSelectedRows ();
				}

				if (paths.Length == 1) {
					TimeNode selectedTimeNode = GetValueFromPath (paths [0]) as TimeNode;

					if (selectedTimeNode != null) {
						ShowMenu ();
					} else {
						ShowEventTypeMenu (GetValueFromPath (paths [0]) as EventType, paths);
					}
				} else if (paths.Length > 1) {
					ShowMenu ();
				}
				return true;
			} else if ((evnt.Type == Gdk.EventType.ButtonPress) && (evnt.Button == 1)) {
				base.OnButtonPressEvent (evnt);
				paths = Selection.GetSelectedRows ();
				if (paths.Length == 1 && GetValueFromPath (paths [0]) is EventType) {
					dragging = true;
					catClicked = true;
					dragStarted = false;
					startX = (int)evnt.X;
					startY = (int)evnt.Y;
				}
				return true;
			} else {
				return base.OnButtonPressEvent (evnt);
			}
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			return false;
		}

		void DisableDragInto (TreePath path, Gdk.DragContext context, uint time, TreeViewDropPosition pos)
		{
			if (pos == TreeViewDropPosition.IntoOrAfter) {
				pos = TreeViewDropPosition.After;
			} else if (pos == TreeViewDropPosition.IntoOrBefore) {
				pos = TreeViewDropPosition.Before;
			}
			SetDragDestRow (path, pos);
			dropPos = pos;
			Gdk.Drag.Status (context, context.SuggestedAction, time);
		}

		protected override bool OnDragDrop (Gdk.DragContext context, int x, int y, uint time)
		{
			TreePath path;
			TreeViewDropPosition pos;
			if (GetDestRowAtPos (x, y, out path, out pos)) {
				TreeIter destIter;

				Project.EventTypes.Remove (draggedEventType);
				Project.EventTypes.Insert (path.Indices [0], draggedEventType);

				Model.GetIter (out destIter, path);
				if (dropPos == TreeViewDropPosition.After) {
					Model.MoveAfter (srcIter, destIter);
				} else {
					Model.MoveBefore (srcIter, destIter);
				}
				Refilter ();
			}
			Gtk.Drag.Finish (context, true, false, time);
			return true;
		}

		protected override bool OnDragMotion (Gdk.DragContext context, int x, int y, uint time)
		{
			TreePath path;
			TreeViewDropPosition pos;

			if (GetDestRowAtPos (x, y, out path, out pos)) {
				EventType ev = GetValueFromPath (path) as EventType;
				if (ev != null) {
					DisableDragInto (path, context, time, pos);
					return true;
				} else {
					return false;
				}
			}
			return false;
		}

		protected override void OnDragBegin (Gdk.DragContext context)
		{
			TreePath path;
			TreeViewColumn col;
			int cellX, cellY;

			GetPathAtPos (startX, startY, out path, out col, out cellX, out cellY);
			draggedEventType = GetValueFromPath (path) as EventType;


			if (draggedEventType != null) {
				GetPathAtPos (startX, startY, out path, out col, out cellX, out cellY);
				Model.GetIter (out srcIter, path);
				Pixmap rowPix = CreateRowDragIcon (path);
				Gtk.Drag.SetIconPixmap (context, rowPix.Colormap, rowPix, null, startX + 1, cellY + 1);
			} else {
				Gtk.Drag.Finish (context, false, false, context.StartTime);
			}
		}

		void ShowEventTypeMenu (EventType eventType, TreePath [] paths)
		{
			List<LMTimelineEvent> events = TreeViewHelpers.EventsListFromPaths (modelSort, paths);
			eventTypeMenu.ShowMenu (Project, eventType, events);
		}

	}
}
