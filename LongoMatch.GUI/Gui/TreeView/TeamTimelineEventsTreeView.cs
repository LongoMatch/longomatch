//
//  Copyright (C) 2017 Fluendo S.A.
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
using System.Linq;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.ViewModel;
using LongoMatch.Gui.Menus;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.UI.Component;

namespace LongoMatch.Gui.Component
{
	public class TeamTimelineEventsTreeView : TimelineEventsTreeView<PlayerTimelineVM, Player>
	{
		SportsPlaysMenu menu;
		PlayerMenu playerMenu;
		TeamType teamType;

		public TeamTimelineEventsTreeView (TeamType teamType)
		{
			this.teamType = teamType;
			playerMenu = new PlayerMenu ();
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

		protected override NestedViewModel<PlayerTimelineVM> GetSubTimeline (TimelineVM viewModel)
		{
			if (teamType == TeamType.LOCAL) {
				return ((LMTimelineVM)viewModel).HomeTeamTimelineVM;
			} else {
				return ((LMTimelineVM)viewModel).AwayTeamTimelineVM;
			}
		}

		protected override void SetCellViewModel (CellRenderer cell, TreeIter iter, IViewModel viewModel)
		{
			PlaysCellRenderer renderer = (cell as PlaysCellRenderer);
			renderer.Item = viewModel;
			renderer.Project = Project.Model;
			renderer.Count = Model.IterNChildren (iter);
		}

		protected override void ShowMenu ()
		{
			IEnumerable<IViewModel> viewModels = GetSelectedViewModels ();
			IEnumerable<TimelineEventVM> eventVMs = viewModels.OfType<TimelineEventVM> ();
			PlayerTimelineVM playerVM = viewModels.OfType<PlayerTimelineVM> ().FirstOrDefault ();

			if (!eventVMs.Any () && playerVM != null) {
				eventVMs = playerVM.ViewModels.Where (vm => vm.Visible);
				playerMenu.ShowMenu (Project.Model, eventVMs);
			} else {
				menu.ShowMenu (Project.Model, eventVMs.ToList ());
			}
		}
	}
}
