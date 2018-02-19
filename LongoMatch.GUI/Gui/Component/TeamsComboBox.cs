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
using System.Collections.Generic;
using Gtk;
using LongoMatch.Core.Store.Templates;
using VAS.Core.Resources;
using VAS.Core.Resources.Styles;
using VAS.UI.Component;
using Color = VAS.Core.Common.Color;
using Image = VAS.Core.Common.Image;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public class TeamsComboBox : Gtk.ComboBox
	{
		ListStore store;
		CellRendererImage imageRenderer;
		CellRendererText texrender;

		public void Load (List<LMTeam> teams)
		{
			Clear ();
			imageRenderer = new CellRendererImage ();
			imageRenderer.Width = Sizes.NewTeamsIconSize;
			imageRenderer.Height = Sizes.NewTeamsIconSize;
			texrender = new CellRendererText ();
			texrender.Font = App.Current.Style.Font + " " + Sizes.NewTeamsFontSize;
			texrender.Alignment = Pango.Alignment.Center;

			if (Direction == TextDirection.Ltr) {
				PackStart (imageRenderer, false);
				PackEnd (texrender, true);
			} else {
				PackEnd (imageRenderer, false);
				PackStart (texrender, true);
			}

			store = new ListStore (typeof (Image), typeof (string), typeof (LMTeam), typeof (Color));
			foreach (LMTeam t in teams) {
				Image shield;
				Color maskColor;

				if (t.Shield == null) {
					maskColor = Colors.DefaultShield;
					shield = App.Current.ResourcesLocator.LoadIcon (Icons.DefaultShield, Sizes.NewTeamsIconSize);
				} else {
					maskColor = null;
					shield = t.Shield;
				}
				store.AppendValues (shield, t.Name, t, maskColor);
			}

			SetAttributes (texrender, "text", 1);
			SetAttributes (imageRenderer, "Image", 0, "MaskColor", 3);
			Model = store;
		}

		public LMTeam ActiveTeam {
			get {
				TreeIter iter;

				GetActiveIter (out iter);
				return store.GetValue (iter, 2) as LMTeam;
			}
		}
	}

	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public class HomeTeamsComboBox : TeamsComboBox
	{
		public HomeTeamsComboBox ()
		{
			Direction = TextDirection.Rtl;
		}
	}

	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public class AwayTeamsComboBox : TeamsComboBox
	{
		public AwayTeamsComboBox ()
		{
			Direction = TextDirection.Ltr;
		}
	}
}

