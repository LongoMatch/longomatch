//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DatePicker : Gtk.Bin
	{
		public event EventHandler ValueChanged;

		DateTime date;

		public DatePicker ()
		{
			this.Build ();

			datebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-calendar", Gtk.IconSize.Button, 0);

			datebutton.Clicked += HandleClicked;
			dateentry.Changed += HandleChanged;
			Date = DateTime.UtcNow;
		}

		public DateTime Date {
			set {
				date = value;
				dateentry.Text = value.ToShortDateString ();
			}
			get {
				return date;
			}
		}

		void HandleChanged (object sender, EventArgs e)
		{
			// Proxy event to potential listeners
			if (this.ValueChanged != null) {
				this.ValueChanged (this, EventArgs.Empty);
			}
		}

		void HandleClicked (object sender, EventArgs e)
		{
			Date = Config.GUIToolkit.SelectDate (Date, this).Result;
		}
	}
}

