// EntryDialog.cs
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
using System.Collections.Generic;
using Gtk;
using Misc = VAS.UI.Helpers.Misc;

namespace LongoMatch.Gui.Dialog
{


	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (false)]
	public partial class EntryDialog : Gtk.Dialog
	{

		bool showCount;

		public EntryDialog (Window parent)
		{
			TransientFor = parent;
			this.Build ();
			// Configure window icon
			Icon = Misc.LoadIcon (Config.SoftwareIconName, IconSize.Dialog);
			ShowCount = false;
			setAvailableTemplatesVisible (false);
		}

		public string Text {
			get {
				return this.entry1.Text;
			}
			set {
				this.entry1.Text = value;
			}
		}

		public void SelectText ()
		{
			this.entry1.SelectRegion (0, -1);
		}

		public int Count {
			get {
				return (int)playersspinbutton.Value;
			}
			set {
				playersspinbutton.Value = value;
			}
		}

		public string CountText {
			set {
				playerslabel.Text = value;
			}
		}

		public bool ShowCount {
			set {
				showCount = value;
				playerslabel.Visible = value;
				playersspinbutton.Visible = value;
			}
			get {
				return showCount;
			}
		}

		public List<string> AvailableTemplates {
			set {
				if (value.Count > 0) {
					foreach (String text in value)
						combobox.AppendText (text);
					setAvailableTemplatesVisible (true);
					combobox.Active = 0;
				} else
					setAvailableTemplatesVisible (false);
			}
		}

		public string SelectedTemplate {
			get {
				if (checkbutton.Active)
					return combobox.ActiveText;
				else
					return null;
			}
		}

		private void setAvailableTemplatesVisible (bool visible)
		{
			combobox.Visible = visible;
			existentemplatelabel.Visible = visible;
			checkbutton.Visible = visible;
		}

		protected virtual void OnCheckbuttonToggled (object sender, System.EventArgs e)
		{
			bool active = checkbutton.Active;
			if (ShowCount) {
				playerslabel.Sensitive = !active;
				playersspinbutton.Sensitive = !active;
			}
			combobox.Sensitive = active;
		}
	}
}
