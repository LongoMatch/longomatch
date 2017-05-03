//
//  Copyright (C) 2017 Andoni Morales Alastruey
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
using System.Collections.Generic;
using System.Linq;
using Gtk;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Menus;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.UI.Component;

namespace LongoMatch.Gui.Component
{
	// FIXME: Change the view to not use the model, use the VM provided
	public class LMTimelineEventsTreeView : TimelineEventsTreeView<EventTypeTimelineVM, EventType>
	{
		SportsPlaysMenu menu;
		EventTypeMenu eventTypeMenu;

		public LMTimelineEventsTreeView ()
		{
			menu = new SportsPlaysMenu ();
			eventTypeMenu = new EventTypeMenu ();

			// FIXME: Fix the behaviour in the tree view 
			menu.EditPlayEvent += (sender, e) =>
				ViewModel.EditionCommand.Execute (ViewModel.FullTimeline.Selection.First ().Model);
			eventTypeMenu.EditProperties += (cat) => OnEditProperties (cat);
			ShowExpanders = false;
			eventTypeMenu.SortEvent += (sender, e) => sort.SetSortFunc (0, HandleSort);
		}

		public LMProjectVM Project {
			get;
			set;
		}

		protected override CellRenderer CreateCellRenderer ()
		{
			return new PlaysCellRenderer ();
		}

		protected override Area GetCellRedrawArea (int cellX, int cellY, double x, double y, int width, IViewModel viewModel)
		{
			return null;
		}

		protected override string GetCellTooltip (int cellX, int cellY, IViewModel vm)
		{
			if (vm is TimelineEventVM) {
				return (vm as TimelineEventVM).Name;
			} else if (vm is EventTypeTimelineVM) {
				return (vm as EventTypeTimelineVM).EventTypeVM.Name;
			}
			return null;
		}

		protected override NestedViewModel<EventTypeTimelineVM> GetSubTimeline (TimelineVM viewModel)
		{
			return ViewModel.EventTypesTimeline;
		}

		protected override void SetCellViewModel (CellRenderer cell, TreeIter iter, IViewModel vm)
		{
			PlaysCellRenderer renderer = (cell as PlaysCellRenderer);
			renderer.Item = vm;
			renderer.Project = Project.Model;
			renderer.Count = Model.IterNChildren (iter);
		}

		protected override void ShowMenu ()
		{
			IEnumerable<IViewModel> viewModels = GetSelectedViewModels ();
			IEnumerable<TimelineEventVM> events = viewModels.OfType<TimelineEventVM> ();

			EventTypeTimelineVM categoryVM = viewModels.OfType<EventTypeTimelineVM> ().FirstOrDefault ();
			if (!events.Any () && categoryVM != null) {
				events = categoryVM.ViewModels.Where (vm => vm.Visible);
				eventTypeMenu.ShowMenu (Project.Model, categoryVM.Model, events.Select (vm => vm.Model as LMTimelineEvent).ToList ());
			} else {
				menu.ShowMenu (Project.Model, events.Select (vm => vm.Model).ToList ());
			}
		}

		protected override int HandleSort (TreeModel model, TreeIter a, TreeIter b)
		{
			object objecta, objectb;
			LMTimelineEventVM tna, tnb;

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
			if (objecta is EventTypeTimelineVM && objectb is EventTypeTimelineVM) {
				return int.Parse (model.GetPath (a).ToString ())
				- int.Parse (model.GetPath (b).ToString ());
			} else if (objecta is LMTimelineEventVM && objectb is LMTimelineEventVM) {
				tna = objecta as LMTimelineEventVM;
				tnb = objectb as LMTimelineEventVM;
				switch (tna.Model.EventType.SortMethod) {
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

		// FIXME: Edit and sort functionality should be moved to commands in a wrapper view model
		// of the EventTypeTimelineVM since the logic could be different depending on the view
		void OnEditProperties (EventType eventType)
		{
			EditCategoryDialog dialog = new EditCategoryDialog (Project.Model, eventType, this.Toplevel as Window);
			dialog.Run ();
			dialog.Destroy ();
		}
	}
}
