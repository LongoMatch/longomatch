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
using Gtk;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Gui.Component;
using Pango;
using EventType = LongoMatch.Core.Store.EventType;
using Misc = LongoMatch.Gui.Helpers.Misc;

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

			Model = store = new TreeStore (typeof(string), typeof(bool), typeof(object));

			TreeViewColumn filterColumn = new TreeViewColumn ();
			CellRendererToggle filterCell = new CellRendererToggle ();
			filterCell.Width = StyleConf.FilterTreeViewToogleWidth;
			filterCell.Toggled += HandleFilterCellToggled;
			filterColumn.PackStart (filterCell, false);
			filterColumn.AddAttribute (filterCell, "active", COL_ACTIVE);

			CellRendererText nameCell = new CellRendererText ();
			nameCell.FontDesc = FontDescription.FromString (Config.Style.Font + " bold 13");
			nameCell.Height = 32;
			filterColumn.PackStart (nameCell, true);
			filterColumn.AddAttribute (nameCell, "text", COL_DESCRIPTION);

			TreeViewColumn onlyColumn = new TreeViewColumn ();
			CellRendererButton onlyCell = new CellRendererButton (Catalog.GetString ("Only"));
			onlyColumn.PackStart (onlyCell, false);
			onlyCell.Clicked += HandleOnlyClicked;

			AppendColumn (filterColumn);
			AppendColumn (onlyColumn);

			ModifyFg (StateType.Normal, LongoMatch.Gui.Helpers.Misc.ToGdkColor (Config.Style.PaletteBackgroundLight));
		}

		public virtual void ToggleAll (bool active)
		{
			TreeIter current;
			store.GetIterFirst (out current);
			ToggleAll (current, active);
		}

		protected void AddNullOrEmptyValue ()
		{
			store.AppendValues (Catalog.GetString ("None specified"), false,
				new StringObject (ProjectsFilter.EMPTY_OR_NULL));
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
			return false;
		}
	}
}

