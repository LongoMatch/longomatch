//  Copyright (C) 2016 Andoni Morales Alastruey
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
using Gtk;
using LongoMatch.Core.Store;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.UI.Common;

namespace LongoMatch.Gui.Component
{

	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public class ProjectsTreeView :
	TreeViewBase<CollectionViewModel<ProjectLongoMatch, SportsProjectVM>, ProjectLongoMatch, SportsProjectVM>
	{
		ProjectSortType sortType;
		string textFilter;

		public ProjectsTreeView ()
		{
			HasFocus = false;
			HeadersVisible = false;
			Selection.Mode = SelectionMode.Multiple;
			EnableGridLines = TreeViewGridLines.None;
			CreateViews ();
		}

		/// <summary>
		/// Gets or sets how projects are sorted.
		/// </summary>
		/// <value>The sort type.</value>
		public ProjectSortType SortType {
			get {
				return sortType;
			}

			set {
				sortType = value;
				sort.SetSortFunc (COL_DATA, HandleSort);
			}
		}

		public string TextFilter {
			get {
				return textFilter;
			}

			set {
				textFilter = value;
				filter.Refilter ();
			}
		}

		public override void SetViewModel (object viewModel)
		{
			base.SetViewModel (viewModel);
			CreateFilterAndSort ();
		}

		void CreateViews ()
		{
			CellRenderer descCell = new CellRendererText ();
			AppendColumn (null, descCell, RenderProject);
		}

		void RenderProject (TreeViewColumn tree_column, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			(cell as CellRendererText).Text = (Model.GetValue (iter, COL_DATA) as SportsProjectVM).Description;
		}

		protected override int HandleSort (TreeModel model, TreeIter a, TreeIter b)
		{
			SportsProjectVM p1, p2;

			p1 = model.GetValue (a, COL_DATA) as SportsProjectVM;
			p2 = model.GetValue (b, COL_DATA) as SportsProjectVM;

			if (p1 == null && p2 == null) {
				return 0;
			} else if (p1 == null) {
				return -1;
			} else if (p2 == null) {
				return 1;
			}

			return ProjectDescription.Sort (p1.Model.Description, p2.Model.Description, SortType);
		}

		protected override bool HandleFilter (TreeModel model, TreeIter iter)
		{
			SportsProjectVM projectVM = model.GetValue (iter, COL_DATA) as SportsProjectVM;

			if (projectVM == null)
				return true;

			return projectVM.Model.Description.Search (TextFilter);
		}
	}
}
