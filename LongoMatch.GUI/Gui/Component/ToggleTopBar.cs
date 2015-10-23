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
using System.Linq;
using Gdk;
using Gtk;
using LongoMatch.Core.Common;
using Misc = LongoMatch.Gui.Helpers.Misc;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ToggleTopBar : Gtk.Bin
	{
		public event ChangeCurrentPageHandler SwitchPageEvent;

		int currentPage = 0;

		public ToggleTopBar ()
		{
			this.Build ();
		}

		public int CurrentPage {
			get { return currentPage; }

			private set {
				currentPage = value;
				if (SwitchPageEvent != null) {
					SwitchPageEvent (this, new ChangeCurrentPageArgs ());
				}
			}
		}

		/// <summary>
		/// Switches the topbar togglebuttons to the page sent
		/// If page is not in range nothing happens
		/// </summary>
		/// <param name="page">Index of the page to switch</param>
		public void SwitchPage (int page)
		{
			if (page >= 0 && page < buttoncontainer.Children.Length) {
				var button = buttoncontainer.Children [page] as RadioButton;
				if (button != null) {
					button.Toggle ();
				}
			}
		}

		/// <summary>
		/// Adds a button to the end of the button list.
		/// Each button has an icon and a text, and changes
		/// 	CurrentPage when clicked.
		/// </summary>
		/// <param name="icon">Icon to show at the left of the button</param>
		/// <param name="text">Text to show at the right of the button</param>
		public void AddButton (Pixbuf icon, string text)
		{
			// If there are other buttons, add the new button to their buttonGroup
			RadioButton otherbutton = null;
			string styleName = "topbarbutton_left";
			if (buttoncontainer.Children.Any ()) {
				otherbutton = buttoncontainer.Children.First () as RadioButton;
				styleName = "topbarbutton_right";
			}
			var button = new RadioButton (otherbutton);


			button.Name = styleName;
			button.DrawIndicator = false;

			var box = new HBox ();
			box.Spacing = 20;
			box.Add (new Gtk.Image (icon));
			box.Add (new Label (text));
			button.Add (box);

			// Restyle the widget that was the last (if any)
			int pos = buttoncontainer.Children.Length - 1;
			if (pos >= 0) {
				Widget previousLast = buttoncontainer.Children.Last ();
				previousLast.Name = pos == 0 ? "topbarbutton_left" : "topbarbutton_center";
				previousLast.ResetRcStyles ();
			}

			buttoncontainer.Add (button);
			pos++;

			button.Toggled += (object sender, EventArgs e) => CurrentPage = pos;
			button.ShowAll ();
		}


	}
}

