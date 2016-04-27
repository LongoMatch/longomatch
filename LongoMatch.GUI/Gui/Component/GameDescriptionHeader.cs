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
using Gtk;
using LongoMatch.Core.Store;
using Misc = LongoMatch.Gui.Helpers.Misc;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class GameDescriptionHeader : Gtk.Bin
	{
		SizeGroup namesGroup, scoreGroup;

		public GameDescriptionHeader ()
		{
			this.Build ();
			SetStyle ();
			namesGroup = new SizeGroup (SizeGroupMode.Horizontal);
			namesGroup.AddWidget (homenamelabel);
			namesGroup.AddWidget (awaynamelabel);
			scoreGroup = new SizeGroup (SizeGroupMode.Horizontal);
			scoreGroup.AddWidget (homescorelabel);
			scoreGroup.AddWidget (awayscorelabel);
		}

		public ProjectDescription ProjectDescription {
			set {
				if (value.LocalShield != null) {
					homeimage.Pixbuf = value.LocalShield.Scale (100, 50).Value;
				} else {
					homeimage.Pixbuf = Misc.LoadIcon ("longomatch-default-shield", 50);
				}
				homenamelabel.Text = value.LocalName;
				homescorelabel.Text = value.LocalGoals.ToString ();
				
				if (value.VisitorShield != null) {
					awayimage.Pixbuf = value.VisitorShield.Scale (100, 50).Value;
				} else {
					awayimage.Pixbuf = Misc.LoadIcon ("longomatch-default-shield", 50);
				}
				awaynamelabel.Text = value.VisitorName;
				awayscorelabel.Text = value.VisitorGoals.ToString ();
			}
		}

		void SetStyle ()
		{
			Pango.FontDescription numDesc = Pango.FontDescription.FromString (Config.Style.Font + " 48px");
			Pango.FontDescription nameDesc = Pango.FontDescription.FromString (Config.Style.Font + " 30px");

			homescoreeventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundDark));
			homescorelabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			homescorelabel.ModifyFont (numDesc);
			awayscoreeventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundDark));
			awayscorelabel.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			awayscorelabel.ModifyFont (numDesc);
			homenamelabel.ModifyFont (nameDesc);
			homenamelabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			awaynamelabel.ModifyFont (nameDesc);
			awaynamelabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
		}
	}
}
