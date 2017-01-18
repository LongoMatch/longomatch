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
using System.ComponentModel;
using Gtk;
using LongoMatch.Core.ViewModel;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.ViewModel;
using VAS.UI.Component;

namespace LongoMatch.Gui.Component
{
	[Category ("LongoMatch")]
	[ToolboxItem (true)]
	public class EventsFilterTreeView : FilterTreeViewBase, IView
	{
		CompositePredicate<TimelineEventVM> predicate;

		public CompositePredicate<TimelineEventVM> Predicate {
			get {
				return predicate;
			}
			set {
				if (predicate != null) {
					predicate.PropertyChanged -= HandleFilterPropertyChanged;
				}
				predicate = value;
				if (predicate != null) {
					HandleFilterPropertyChanged (this, new PropertyChangedEventArgs ("Collection"));
					predicate.PropertyChanged += HandleFilterPropertyChanged;
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
		}

		protected override void UpdateSelection (Gtk.TreeIter iter, bool active)
		{
			bool oldActive = (bool)store.GetValue (iter, COL_ACTIVE);
			if (active == oldActive) {
				return;
			}

			IPredicate<TimelineEventVM> predicate = store.GetValue (iter, COL_VALUE) as IPredicate<TimelineEventVM>;
			predicate.Active = active;
			store.SetValue (iter, COL_ACTIVE, active);

			if (predicate is CompositePredicate<TimelineEventVM>) {
				TreeIter child;
				store.IterChildren (out child, iter);
				while (store.IterIsValid (child)) {
					UpdateSelection (child, active);
					store.IterNext (ref child);
				}
			} else if (predicate is Predicate<TimelineEventVM>) {
				TreeIter parent;
				store.IterParent (out parent, iter);
				while (store.IterIsValid (parent)) {
					store.SetValue (parent, COL_ACTIVE, (store.GetValue (parent, COL_VALUE) as CompositePredicate<TimelineEventVM>).Active);
					TreeIter newParent;
					store.IterParent (out newParent, parent);
					parent = newParent;
				}
			}
		}

		void FillFilters (TreeIter parentRow, CompositePredicate<TimelineEventVM> predicate)
		{
			foreach (var filter in predicate.Elements) {
				var newRow = TreeIter.Zero;
				if (parentRow.Equals (TreeIter.Zero)) {
					newRow = store.AppendValues (filter.Name, true, filter);
				} else {
					newRow = store.AppendValues (parentRow, filter.Name, true, filter);
				}
				var composite = filter as CompositePredicate<TimelineEventVM>;
				if (composite != null) {
					FillFilters (newRow, composite);
				}
			}
		}

		void HandleFilterPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Collection") {
				store.Clear ();
				FillFilters (TreeIter.Zero, Predicate);
			}
		}
	}
}

