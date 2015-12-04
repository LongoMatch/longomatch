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
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using Pango;

namespace LongoMatch.Gui.Dialog
{
	public partial class EditPlaylistElementProperties : Gtk.Dialog
	{
		SizeGroup sizegroupLeft, sizegroupRight;
		IPlaylistElement plElement;

		public EditPlaylistElementProperties (Window parent, IPlaylistElement element)
		{
			TransientFor = parent;
			this.Build ();

			sizegroupLeft = new SizeGroup (SizeGroupMode.Horizontal);
			sizegroupLeft.IgnoreHidden = false;
			foreach (Widget w in vbox2.Children) {
				foreach (Widget t in (w as Table).Children) {
					if ((t is Label)) {
						t.ModifyFont (FontDescription.FromString (Config.Style.Font + " 10"));
						sizegroupLeft.AddWidget (t);
					}
				}
			}

			sizegroupRight = new SizeGroup (SizeGroupMode.Horizontal);
			sizegroupRight.IgnoreHidden = false;
			foreach (Widget w in vbox2.Children) {
				foreach (Widget t in (w as Table).Children) {
					if (!(t is Label)) {
						sizegroupRight.AddWidget (t);
					}
				}
			}

			PlaylistElement = element;
			durationspinbutton.ValueChanged += HandleDurationChanged;
			nameentry.Changed += HandleNameChanged;
		}

		IPlaylistElement PlaylistElement {
			set {
				if (plElement != null) {
					durationspinbutton.ValueChanged -= HandleDurationChanged;
				}

				plElement = value;
				if (plElement is PlaylistImage || plElement is PlaylistDrawing) {
					slidetable.Visible = true;
					durationspinbutton.Value = plElement.Duration.Seconds;
				} else {
					slidetable.Visible = false;
				}

				if (plElement is PlaylistPlayElement) {
					nameentry.Text = (plElement as PlaylistPlayElement).Title;
					nametable.Visible = true;
				} else {
					nametable.Visible = false;
				}
			}
		}

		void HandleDurationChanged (object sender, EventArgs e)
		{
			plElement.Duration.TotalSeconds = durationspinbutton.ValueAsInt;
		}

		void HandleNameChanged (object sender, EventArgs e)
		{
			(plElement as PlaylistPlayElement).Title = nameentry.Text;
		}
	}
}

