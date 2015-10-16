//
//  Copyright (C) 2015 Fluendo S.A.
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
using LongoMatch.Core;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ToggleTopBar : Gtk.Bin
	{
		public event ChangeCurrentPageHandler SwitchPage;

		int currentPage = 0;

		public ToggleTopBar ()
		{
			this.Build ();

			topbarbutton_left.Toggled += (object sender, EventArgs e) => CurrentPage = 0;
			topbarbutton_right.Toggled += (object sender, EventArgs e) => CurrentPage = 1;

			projectsToggleButtonImage.Pixbuf = Resources.LoadImage ("topptab_img_proj_icon.png").Value;
			athletesToggleButtonImage.Pixbuf = Resources.LoadImage ("topptab_img_athl_icon.png").Value;
		}

		public int CurrentPage {
			get { return currentPage; }
			set {
				currentPage = value;
				if (SwitchPage != null) {
					SwitchPage (this, new ChangeCurrentPageArgs ());
				}
			}
		}

	}
}

