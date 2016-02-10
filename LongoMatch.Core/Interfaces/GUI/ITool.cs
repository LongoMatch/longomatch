//
//  Copyright (C) 2015 FLUENDO S.A.
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

using LongoMatch.Core.Common;

namespace LongoMatch.Core.Interfaces.GUI
{
	public interface ITool
	{
		/// <summary>
		/// Gets the name of the tool
		/// </summary>
		/// <value>The name of the tool.</value>
		string Name { get; }

		/// <summary>
		/// Gets the welcome panel icon. If null, the tool is not accessible from the welcome panel.
		/// </summary>
		/// <value>The welcome panel icon.</value>
		Image WelcomePanelIcon { get; }

		/// <summary>
		/// Gets the menubar label. If null, the tool is not accessible from the menu bar "tools" section.
		/// </summary>
		/// <value>The menubar label.</value>
		string MenubarLabel { get; }

		/// <summary>
		/// Gets the menubar accelerator. If null, the menubar entry will not have an accelerator.
		/// </summary>
		/// <value>The menubar label.</value>
		string MenubarAccelerator { get; }

		/// <summary>
		/// Bring up the Tool user interface using provided toolkit.
		/// </summary>
		void Load (IGUIToolkit toolkit);
	}
}

