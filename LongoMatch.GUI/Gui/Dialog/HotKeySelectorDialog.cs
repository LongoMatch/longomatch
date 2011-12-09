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
using Gtk;
using Gdk;
using LongoMatch.Store;

namespace LongoMatch.Gui.Dialog
{


	public partial class HotKeySelectorDialog : Gtk.Dialog
	{
		HotKey hotKey;

		#region Constructors

		public HotKeySelectorDialog()
		{
			hotKey = new HotKey();
			this.Build();
		}
		#endregion

		#region Properties

		public HotKey HotKey {
			get {
				return this.hotKey;
			}
		}
		#endregion

		#region Overrides

		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			Gdk.Key key = evnt.Key;
			ModifierType modifier = evnt.State;

			// Only react to {Shift|Alt|Ctrl}+key
			// Ctrl is a modifier to select single keys
			// Combination are allowed with Alt and Shift (Ctrl is not allowed to avoid
			// conflicts with menus shortcuts)
			if((modifier & (ModifierType.Mod1Mask | ModifierType.ShiftMask | ModifierType.ControlMask)) != 0
			                && key != Gdk.Key.Shift_L
			                && key != Gdk.Key.Shift_R
			                && key != Gdk.Key.Alt_L
			                && key != Gdk.Key.Control_L
			                && key != Gdk.Key.Control_R)
			{
				hotKey.Key = (int)key;
				hotKey.Modifier = (int) (modifier & (ModifierType.Mod1Mask | ModifierType.ShiftMask));
				this.Respond(ResponseType.Ok);
			}

			return base.OnKeyPressEvent(evnt);
		}
		#endregion

	}
}
