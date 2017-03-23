//
//  Copyright (C) 2017 FLUENDO S.A.
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
namespace LongoMatch.Gui
{
	/// <summary>
	/// Defines an extension point for a given menu
	/// </summary>
	public class MenuExtensionEntry
	{
		int positionEntry;

		public MenuExtensionEntry (string menuName, int positionEntry)
		{
			this.MenuName = menuName;
			this.LastPosition = positionEntry;
			this.positionEntry = positionEntry;
		}

		/// <summary>
		/// Position to add a new menu entry
		/// </summary>
		/// <value>The last menu item position.</value>
		public int LastPosition { get; protected set; }

		/// <summary>
		/// The name of the menu
		/// </summary>
		/// <value>The name of the menu.</value>
		public string MenuName { get; }

		/// <summary>
		/// After a new menu item has been added update has to be called to refresh the position
		/// </summary>
		public void UpdateLastPosition ()
		{
			this.LastPosition++;
		}

		/// <summary>
		/// Resets the menu entry to the original state
		/// </summary>
		public void ResetMenuEntry ()
		{
			this.LastPosition = this.positionEntry;
		}
	}
}
