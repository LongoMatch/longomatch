//
//  Copyright (C) 2017 ${CopyrightHolder}
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
namespace LongoMatch
{
	/// <summary>
	/// Defines an entry point in a specified menu by a window
	/// </summary>
	public interface IMenuExtensionEntry
	{
		/// <summary>
		/// Position available to add a new menu entry
		/// </summary>
		int LastPosition { get; }

		/// <summary>
		/// Name fo the menu to be extended
		/// </summary>
		string MenuName { get; }

		/// <summary>
		/// Called when a menu item is added to the extension
		/// </summary>
		void UpdateLastPosition ();

		/// <summary>
		/// Resets the menu entry to the original state
		/// </summary>
		void ResetMenuEntry ();
	}
}
