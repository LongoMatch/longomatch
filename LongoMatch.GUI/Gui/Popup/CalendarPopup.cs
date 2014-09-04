// CalendarPopup.cs
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
using LongoMatch.Core.Handlers;

namespace LongoMatch.Gui.Popup
{

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class CalendarPopup : Gtk.Window
	{

		public event DateSelectedHandler DateSelectedEvent;
		private DateTime selectedDate;


		public CalendarPopup() :
		base(Gtk.WindowType.Toplevel)
		{
			this.Build();

		}

		public DateTime getSelectedDate() {
			return this.selectedDate;
		}

		protected virtual void OnFocusOutEvent(object o, Gtk.FocusOutEventArgs args)
		{
			this.Hide();
		}

		protected virtual void OnCalendar1DaySelectedDoubleClick(object sender, System.EventArgs e)
		{
			this.selectedDate = calendar1.Date;
			this.DateSelectedEvent(this.selectedDate);
			this.Hide();
		}
	}
}
