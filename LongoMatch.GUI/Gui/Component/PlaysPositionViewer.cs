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
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Menus;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlaysPositionViewer : Gtk.Bin, IView<ProjectVM>
	{
		SportsPlaysMenu menu;
		ProjectVM viewModel;

		public PlaysPositionViewer ()
		{
			this.Build ();
			field.FieldPosition = FieldPositionType.Field;
			hfield.FieldPosition = FieldPositionType.HalfField;
			goal.FieldPosition = FieldPositionType.Goal;
			field.ShowMenuEvent += HandleShowMenuEvent;
			hfield.ShowMenuEvent += HandleShowMenuEvent;
			goal.ShowMenuEvent += HandleShowMenuEvent;
			menu = new SportsPlaysMenu ();
		}

		public ProjectVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				field.ViewModel = viewModel;
				hfield.ViewModel = viewModel;
				goal.ViewModel = viewModel;
			}
		}

		void HandleShowMenuEvent (IEnumerable<TimelineEvent> plays)
		{
			if (plays == null || !plays.Any ()) {
				return;
			}
			menu.ShowMenu (ViewModel.Model, plays.ToList ());
		}

		protected override void OnDestroyed ()
		{
			field.Destroy ();
			hfield.Destroy ();
			goal.Destroy ();
			base.OnDestroyed ();
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (ProjectVM)viewModel;
		}
	}
}
