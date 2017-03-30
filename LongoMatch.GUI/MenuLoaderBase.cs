//
//  Copyright (C) 2017 FLUENDO S.A
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
using Gtk;

namespace LongoMatch.Gui
{
	/// <summary>
	/// Base class for the menu loaders
	/// </summary>
	public abstract class MenuLoaderBase
	{
		protected MenuLoaderBase ()
		{
			MenuItems = new List<MenuItem> ();
		}

		/// <summary>
		/// Registered menu items
		/// </summary>
		/// <value>The menu items registered.</value>
		public List<MenuItem> MenuItems { get; private set; }

		protected void RegisterMenuItem (MenuItem item, Menu menu, MenuExtensionEntry menuEntry)
		{
			MenuItems.Add (item);
			menu.Insert (item, menuEntry.LastPosition);
			menuEntry.UpdateLastPosition ();
		}

		protected void CleanMenu (MenuItem menu)
		{
			foreach (MenuItem item in MenuItems) {
				(menu.Submenu as Menu).Remove (item);
			}

			this.MenuItems.Clear ();
		}
	}
}
