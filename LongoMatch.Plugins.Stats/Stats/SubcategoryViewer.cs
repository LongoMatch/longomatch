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
	public partial class SubCategoryViewer : Gtk.Bin
	{
		ListStore store;

		public SubCategoryViewer ()
		{
			this.Build ();
			treeview.AppendColumn (Catalog.GetString ("Count"), new Gtk.CellRendererText (), "text", 0);
			treeview.AppendColumn (Catalog.GetString ("All"), new Gtk.CellRendererText (), "text", 1);
			treeview.AppendColumn (Catalog.GetString ("Home"), new Gtk.CellRendererText (), "text", 2);
			treeview.AppendColumn (Catalog.GetString ("Away"), new Gtk.CellRendererText (), "text", 3);
		}

		public void LoadStats (SubCategoryStat stats, string homeName, string awayName)
		{
			store = new ListStore (typeof(string), typeof(string), typeof(string), typeof(string));
			treeview.Model = store;
			
			treeview.Columns [2].Title = homeName;
			treeview.Columns [3].Title = awayName;
			plotter.HomeName = homeName;
			plotter.AwayName = awayName;
			
			gtkframe.Markup = String.Format ("<b> {0} </b>", stats.Name);
			plotter.LoadHistogram (stats);
			
			foreach (PercentualStat st in stats.OptionStats) {
				store.AppendValues (st.Name, st.TotalCount.ToString (),
					st.LocalTeamCount.ToString (), st.VisitorTeamCount.ToString ());
			}
		}
	}
}
