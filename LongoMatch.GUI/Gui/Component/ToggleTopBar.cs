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
using System.Linq;
using Gdk;
using Gtk;
using Misc = LongoMatch.Gui.Helpers.Misc;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ToggleTopBar : Gtk.Bin
	{
		public event SwitchPageHandler PageSwitchedEvent;

		int currentPage = -1;

		public ToggleTopBar ()
		{
			this.Build ();
		}

		public int CurrentPage {
			get { return currentPage; }

			set {
				if (value != currentPage && value >= 0 && value < buttoncontainer.Children.Length) {
					var button = buttoncontainer.Children [value] as RadioButton;
					if (button != null) {
						button.Click ();
					}
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
		/// <param name = "tooltiptext">Text to add to the button as tooltip</param>
		public void AddButton (Pixbuf icon, string text, string tooltiptext)
		{
			// If there are other buttons, add the new button to their buttonGroup
			RadioButton otherbutton = null;
			string styleName = "toggletabbutton_only";
			if (buttoncontainer.Children.Any ()) {
				otherbutton = buttoncontainer.Children.First () as RadioButton;
				styleName = "toggletabbutton_right";
			}
			var button = new RadioButton (otherbutton);


			button.Name = styleName;
			button.DrawIndicator = false;

			var bin = new HBox ();
			var box = new HBox ();
			box.Spacing = 12;
			box.Add (new Gtk.Image (icon));
			box.Add (new Label (text));
			bin.PackStart (box, false, false, 10);
			button.Add (bin);

			// Restyle the widget that was the last (if any)
			int pos = buttoncontainer.Children.Length - 1;
			if (pos >= 0) {
				Widget previousLast = buttoncontainer.Children.Last ();
				previousLast.Name = pos == 0 ? "toggletabbutton_left" : "toggletabbutton_center";
				previousLast.ResetRcStyles ();
			}

			if (tooltiptext != null) {
				button.TooltipText = tooltiptext;
			}

			buttoncontainer.Add (button);
			pos++;

			button.Toggled += (sender, e) => {
				if (button.Active) {
					SwitchPage (pos);
				}
			};
			button.ShowAll ();
		}

		void SwitchPage (int page)
		{
			currentPage = page;

			if (PageSwitchedEvent != null) {
				PageSwitchedEvent (this, new SwitchPageArgs ());
			}
		}


	}
}

