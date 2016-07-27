//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using LongoMatch.Core.Stats;
using VAS.Core;

namespace LongoMatch.Plugins.Stats
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlayerSubcategoryViewer : Gtk.Bin
	{
		ListStore store;

		public PlayerSubcategoryViewer ()
		{
			this.Build ();
			treeview.AppendColumn (Catalog.GetString ("Name"), new Gtk.CellRendererText (), "text", 0);
			treeview.AppendColumn (Catalog.GetString ("Count"), new Gtk.CellRendererText (), "text", 1);
			plotter1.ShowTeams = false;
			plotter1.WidthRequest = 500;
		}

		public void LoadStats (SubCategoryStat stats)
		{
			store = new ListStore (typeof(string), typeof(string));
			treeview.Model = store;
			
			gtkframe.Markup = String.Format ("<b> {0} </b>", stats.Name);
			plotter1.LoadHistogram (stats);
			
			foreach (PercentualStat st in stats.OptionStats) {
				store.AppendValues (st.Name, st.TotalCount.ToString ());
			}
		}
	}
}

