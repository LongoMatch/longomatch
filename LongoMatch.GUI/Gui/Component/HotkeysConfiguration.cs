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
using Gtk;
using LongoMatch.Core.Common;
using VAS.Core;
using VAS.Core.Store;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class HotkeysConfiguration : Gtk.Bin
	{
		SizeGroup sgroup;

		public HotkeysConfiguration ()
		{
			int i = 0;
			this.Build ();

			sgroup = new SizeGroup (SizeGroupMode.Horizontal);
			foreach (KeyAction action in App.Current.Config.Hotkeys.ActionsDescriptions.Keys) {
				AddWidget (action, App.Current.Config.Hotkeys.ActionsDescriptions [action],
					App.Current.Config.Hotkeys.ActionsHotkeys [action], i);
				i++;
			}
		}

		public void AddWidget (KeyAction action, string desc, HotKey key, int position)
		{
			uint row_top, row_bottom, col_left, col_right;
			HBox box;
			Label descLabel, keyLabel;
			Button edit;
			Gtk.Image editImage;

			box = new HBox ();
			box.Spacing = 5;
			descLabel = new Label ();
			descLabel.Markup = String.Format ("<b>{0}</b>", desc); 
			keyLabel = new Label (); 
			keyLabel.Markup = GLib.Markup.EscapeText (key.ToString ());
			edit = new Button ();
			editImage = new Gtk.Image (Helpers.Misc.LoadIcon ("vas-pencil", 24));
			edit.Add (editImage);
			box.PackStart (descLabel, true, true, 0);
			box.PackStart (keyLabel, false, true, 0);
			box.PackStart (edit, false, true, 0);
			box.ShowAll ();

			sgroup.AddWidget (keyLabel);
			descLabel.Justify = Justification.Left;
			descLabel.SetAlignment (0f, 0.5f);
			edit.Clicked += (sender, e) => {
				HotKey hotkey = App.Current.GUIToolkit.SelectHotkey (key);
				if (hotkey != null) {
					if (App.Current.Config.Hotkeys.ActionsHotkeys.ContainsValue (hotkey)) {
						App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Hotkey already in use: ") +
						GLib.Markup.EscapeText (hotkey.ToString ()), this);
					} else {
						App.Current.Config.Hotkeys.ActionsHotkeys [action] = hotkey;
						App.Current.Config.Save ();
						keyLabel.Markup = GLib.Markup.EscapeText (hotkey.ToString ());
					}
				}
			};

			row_top = (uint)(position / table.NColumns);
			row_bottom = (uint)row_top + 1;
			col_left = (uint)position % table.NColumns;
			col_right = (uint)col_left + 1;
			table.Attach (box, col_left, col_right, row_top, row_bottom);
		}
	}
}
