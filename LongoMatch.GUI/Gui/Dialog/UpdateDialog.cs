// UpdateDialog.cs
//
//  Copyright (C) 2009 Andoni Morales
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

namespace LongoMatch.Gui.Dialog
{

	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (false)]
	public partial class UpdateDialog : Gtk.Dialog
	{

		public UpdateDialog (Window parent)
		{
			TransientFor = parent;
			this.Build ();
		}

		public void Fill (Version version, string URL)
		{
			this.label5.Text = "The new version is: LongoMatch " + version.ToString ();
			this.label7.Text = URL;
		}


	}
}
