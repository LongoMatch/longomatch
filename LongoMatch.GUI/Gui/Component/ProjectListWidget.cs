// ProjectListWidget.cs
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
using Gtk;
using LongoMatch.Core.Store;
using LongoMatch.Services.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.UI.Helpers.Bindings;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ProjectListWidget : Gtk.Bin, IView<SportsProjectsManagerVM>
	{
		const int PREVIEW_SIZE = 100;

		List<LMProject> projects;
		List<LMProject> selectedProjects;
		bool swallowSignals;
		LMProjectTreeView treeview;
		SportsProjectsManagerVM viewModel;
		BindingContext ctx;

		public ProjectListWidget ()
		{
			this.Build ();
			treeview = new LMProjectTreeView ();
			scrolledwindow1.Add (treeview);

			selectedProjects = new List<LMProject> ();

			sortcombobox.Active = (int)App.Current.Config.ProjectSortMethod;
			sortcombobox.Changed += (sender, e) => {
				App.Current.Config.ProjectSortMethod = (ProjectSortMethod)sortcombobox.Active;
				ViewModel.SortType = (ProjectSortType)sortcombobox.Active;
			};
			focusimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-search", 27);
			Bind ();
		}

		public SportsProjectsManagerVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				ctx.UpdateViewModel (viewModel);
				limitationWidget.SetViewModel (viewModel?.LimitationChart);
				treeview.SetViewModel (viewModel);
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (SportsProjectsManagerVM)viewModel;
		}

		public void ClearSearch ()
		{
			filterEntry.Text = "";
		}

		protected virtual void OnFilterentryChanged (object sender, System.EventArgs e)
		{
		}

		void Bind ()
		{
			ctx = this.GetBindingContext ();
			ctx.Add (filterEntry.Bind (vm => ((SportsProjectsManagerVM)vm).FilterText));
		}

	}
}
