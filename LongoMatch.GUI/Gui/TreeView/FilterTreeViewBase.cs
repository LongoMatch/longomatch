//
//  Copyright (C) 2015  Fluendo S.A.
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
using Gtk;
using LongoMatch.Core.Filters;
using LongoMatch.Gui.Component;
using Pango;
using VAS.Core;
using VAS.Core.Common;
using EventType = VAS.Core.Store.EventType;
using Misc = VAS.UI.Helpers.Misc;

namespace LongoMatch.Gui.Component
{
	public abstract class FilterTreeViewBase: TreeView
	{
		protected const int COL_DESCRIPTION = 0;
		protected const int COL_ACTIVE = 1;
		protected const int COL_VALUE = 2;
		protected TreeStore store;

		public FilterTreeViewBase ()
		{
			Selection.Mode = SelectionMode.None;
			HeadersVisible = false;
			EnableGridLines = TreeViewGridLines.Horizontal;
			SearchColumn = COL_DESCRIPTION;

			Model = store = new TreeStore (typeof(string), typeof(bool), typeof(object));

			TreeViewColumn filterColumn = new TreeViewColumn ();
			CellRendererToggle filterCell = new CellRendererToggle ();
			filterCell.Width = StyleConf.FilterTreeViewToogleWidth;
			filterCell.Toggled += HandleFilterCellToggled;
			filterColumn.PackStart (filterCell, false);
			filterColumn.AddAttribute (filterCell, "active", COL_ACTIVE);

			CellRendererText nameCell = new CellRendererText ();
			nameCell.FontDesc = FontDescription.FromString (
				String.Format ("{0} {1} {2}px", App.Current.Style.Font, "semibold", StyleConf.ListTextFontSize));
			nameCell.Height = 32;
			filterColumn.PackStart (nameCell, true);
			filterColumn.AddAttribute (nameCell, "text", COL_DESCRIPTION);

			TreeViewColumn onlyColumn = new TreeViewColumn ();
			CellRendererButton onlyCell = new CellRendererButton (Catalog.GetString ("Only"));
			onlyColumn.PackStart (onlyCell, false);
			onlyCell.Clicked += HandleOnlyClicked;

			AppendColumn (filterColumn);
			AppendColumn (onlyColumn);

			ModifyFg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.PaletteBackgroundLight));
		}

		public virtual void ToggleAll (bool active)
		{
			TreeIter current;
			store.GetIterFirst (out current);
			ToggleAll (current, active);
		}

		protected void AddNullOrEmptyValue (string text, bool check)
		{
			store.AppendValues (text, check, new StringObject (ProjectsFilter.EMPTY_OR_NULL));
		}

		protected abstract void UpdateSelection (TreeIter iter, bool active);

		protected void ToggleAll (TreeIter current, bool active, bool recurse = true)
		{
			while (store.IterIsValid (current)) {
				UpdateSelection (current, active);
				if (recurse && store.IterHasChild (current)) {
					TreeIter child;
					store.IterChildren (out child, current);
					ToggleAll (child, active);
				}
				store.IterNext (ref current);
			}
		}

		void HandleOnlyClicked (object o, ClickedArgs args)
		{
			TreeIter iter;

			ToggleAll (false);
			if (store.GetIterFromString (out iter, args.Path)) {
				UpdateSelection (iter, true);
			}
		}

		protected void HandleFilterCellToggled (object o, ToggledArgs args)
		{
			TreeIter iter;

			if (store.GetIterFromString (out iter, args.Path)) {
				bool active = !((bool)store.GetValue (iter, COL_ACTIVE));
				UpdateSelection (iter, active);
			}
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			// We only want to handle searches, and ignore selection through space bar press
			if (evnt.Key != Gdk.Key.space) {
				base.OnKeyPressEvent (evnt);
			}
			return false;
		}
	}
}
