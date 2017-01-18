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
using LongoMatch.Core.ViewModel;
using LongoMatch.Gui.Menus;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.UI.Component;

namespace LongoMatch.Gui.Component
{
	public class LMTimelineEventsTreeView : TimelineEventsTreeView<EventTypeTimelineVM, EventType>
	{
		SportsPlaysMenu menu;

		public LMTimelineEventsTreeView ()
		{
			menu = new SportsPlaysMenu ();
			ShowExpanders = false;
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

		protected override void ShowMenu (IEnumerable<TimelineEventVM> events)
		{
			menu.ShowMenu (Project.Model, events.Select (vm => vm.Model).ToList ());
		}
	}
}
