//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using Gtk;
using LongoMatch.Core;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.UI.Common;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("LongoMatch")]
	public class LMProjectTreeView : TreeViewBase<SportsProjectsManagerVM, LMProject, LMProjectVM>
	{
		CellRenderer cellRenderer;

		public LMProjectTreeView ()
		{
			HasFocus = false;
			HeadersVisible = false;
			Selection.Mode = SelectionMode.Multiple;
			EnableGridLines = TreeViewGridLines.None;
			ShowExpanders = false;
			CreateViews ();
			CreateFilterAndSort ();
		}

		protected override void OnDestroyed ()
		{
			if (ViewModel != null) {
				ViewModel.PropertyChanged -= HandleViewModelPropertyChanged;
				ViewModel = null;
			}
			if (cellRenderer != null) {
				cellRenderer.Dispose ();
				cellRenderer = null;
			}
			base.OnDestroyed ();
		}

		public override void SetViewModel (object viewModel)
		{
			if (ViewModel != null) {
				ViewModel.PropertyChanged -= HandleViewModelPropertyChanged;
			}
			base.SetViewModel (viewModel);
			if (ViewModel != null) {
				ViewModel.PropertyChanged += HandleViewModelPropertyChanged;
			}
		}

		void CreateViews ()
		{
			cellRenderer = new LMProjectCellRenderer ();
			AppendColumn (null, cellRenderer, SetCellRendererViewModel);
		}

		void SetCellRendererViewModel (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			LMProjectVM projectVM = (LMProjectVM)model.GetValue (iter, COL_DATA);
			if (projectVM != null) {
				((LMProjectCellRenderer)cell).SetViewModel (projectVM);
			}
		}

		protected override int HandleSort (TreeModel model, TreeIter a, TreeIter b)
		{
			LMProjectVM p1, p2;

			p1 = (LMProjectVM)model.GetValue (a, COL_DATA);
			p2 = (LMProjectVM)model.GetValue (b, COL_DATA);

			if (p1 == null && p2 == null) {
				return 0;
			} else if (p1 == null) {
				return -1;
			} else if (p2 == null) {
				return 1;
			}

			return ProjectDescription.Sort (p1.Model.Description, p2.Model.Description, ViewModel.SortType);
		}

		protected override void HandleTreeviewRowActivated (object o, RowActivatedArgs args)
		{
			base.HandleTreeviewRowActivated (o, args);
			ViewModel.OpenCommand.Execute (activatedViewModel);
		}

		protected override bool HandleFilter (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			LMProjectVM item = (LMProjectVM)model.GetValue (iter, COL_DATA);
			if (item == null) {
				return false;
			}
			return item.Model.Description.Search (ViewModel.FilterText);
		}

		protected override void RemoveSubViewModel (IViewModel subViewModel)
		{
			// Since the Model and store has different iter because RAProjectTreeView needs
			// a filter and a sort TreeModels we need to First Unselect the iter from the Selection
			// With a iter conversion
			foreach (TreeIter element in dictionaryStore [subViewModel]) {
				TreeIter iterToDelete = element;
				iterToDelete = filter.ConvertChildIterToIter (iterToDelete);
				iterToDelete = sort.ConvertChildIterToIter (iterToDelete);
				Selection.UnselectIter (iterToDelete);
				base.RemoveSubViewModel (subViewModel);
			}
		}

		protected override void HandleViewModelPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Collection_" + nameof (ViewModel.Selection) && Selection.CountSelectedRows () > 0) {
				Selection.UnselectAll ();
			}
			base.HandleViewModelPropertyChanged (sender, e);
		}
	}
}
