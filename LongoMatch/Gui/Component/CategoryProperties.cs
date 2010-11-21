// CategoryProperties.cs
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
using Gdk;
using Gtk;
using Mono.Unix;
using LongoMatch.TimeNodes;
using LongoMatch.Gui.Dialog;

namespace LongoMatch.Gui.Component
{

	public delegate void HotKeyChangeHandler(HotKey prevHotKey, Category newSection);

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial  class CategoryProperties : Gtk.Bin
	{

		public event HotKeyChangeHandler HotKeyChanged;

		private Category stn;

		public CategoryProperties()
		{
			this.Build();
		}

		public Category Category{
			set {
				stn = value;
				UpdateGui();
			}
			get {
				return stn;
			}
		}

		private void  UpdateGui() {
			if (stn != null) {
				nameentry.Text = stn.Name;
				timeadjustwidget1.SetTimeNode(stn);
				colorbutton1.Color = stn.Color;
				sortmethodcombobox.Active = (int)stn.SortMethod;

				if (stn.HotKey.Defined) {
					hotKeyLabel.Text = stn.HotKey.ToString();
				}
				else hotKeyLabel.Text = Catalog.GetString("none");
			}
		}

		protected virtual void OnChangebutonClicked(object sender, System.EventArgs e)
		{
			HotKeySelectorDialog dialog = new HotKeySelectorDialog();
			dialog.TransientFor=(Gtk.Window)this.Toplevel;
			HotKey prevHotKey =  stn.HotKey;
			if (dialog.Run() == (int)ResponseType.Ok) {
				stn.HotKey=dialog.HotKey;
				UpdateGui();
			}
			dialog.Destroy();
			if (HotKeyChanged != null)
				HotKeyChanged(prevHotKey,stn);
		}

		protected virtual void OnColorbutton1ColorSet(object sender, System.EventArgs e)
		{
			if (stn != null)
				stn.Color=colorbutton1.Color;
		}

		protected virtual void OnTimeadjustwidget1LeadTimeChanged(object sender, System.EventArgs e)
		{
			stn.Start = timeadjustwidget1.GetStartTime();
		}

		protected virtual void OnTimeadjustwidget1LagTimeChanged(object sender, System.EventArgs e)
		{
			stn.Stop= timeadjustwidget1.GetStopTime();
		}

		protected virtual void OnNameentryChanged(object sender, System.EventArgs e)
		{
			stn.Name = nameentry.Text;
		}

		protected virtual void OnSortmethodcomboboxChanged (object sender, System.EventArgs e)
		{
			stn.SortMethodString = sortmethodcombobox.ActiveText;
		}
	}
}
