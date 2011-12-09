// TreeWidgetPopup.cs
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
using Gdk;
using Gtk;
using Mono.Unix;
using LongoMatch.Handlers;
using LongoMatch.Store;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public class PlayerPropertiesTreeView : Gtk.TreeView
	{
		public event PlayerPropertiesHandler PlayerClicked;
		public event PlayersPropertiesHandler PlayersSelected;

		public PlayerPropertiesTreeView() {

			RowActivated += OnTreeviewRowActivated;
			Selection.Changed += OnSelectionChanged;
			Selection.Mode =  SelectionMode.Multiple;

			Gtk.TreeViewColumn photoColumn = new Gtk.TreeViewColumn();
			photoColumn.Title = Catalog.GetString("Photo");
			Gtk.CellRendererPixbuf photoCell = new Gtk.CellRendererPixbuf();
			photoColumn.PackStart(photoCell, true);

			Gtk.TreeViewColumn nameColumn = new Gtk.TreeViewColumn();
			nameColumn.Title = Catalog.GetString("Name");
			Gtk.CellRendererText nameCell = new Gtk.CellRendererText();
			nameColumn.PackStart(nameCell, true);

			Gtk.TreeViewColumn playsColumn = new Gtk.TreeViewColumn();
			playsColumn.Title = Catalog.GetString("Play this match");
			Gtk.CellRendererText playCell = new Gtk.CellRendererText();
			playsColumn.PackStart(playCell, true);

			Gtk.TreeViewColumn birthdayColumn = new Gtk.TreeViewColumn();
			birthdayColumn.Title = Catalog.GetString("Date of Birth");
			Gtk.CellRendererText birthdayCell = new Gtk.CellRendererText();
			birthdayColumn.PackStart(birthdayCell, true);

			Gtk.TreeViewColumn nationColumn = new Gtk.TreeViewColumn();
			nationColumn.Title = Catalog.GetString("Nationality");
			Gtk.CellRendererText nationCell = new Gtk.CellRendererText();
			nationColumn.PackStart(nationCell, true);

			Gtk.TreeViewColumn heightColumn = new Gtk.TreeViewColumn();
			heightColumn.Title = Catalog.GetString("Height");
			Gtk.CellRendererText heightCell = new Gtk.CellRendererText();
			heightColumn.PackStart(heightCell, true);

			Gtk.TreeViewColumn weightColumn = new Gtk.TreeViewColumn();
			weightColumn.Title = Catalog.GetString("Weight");
			Gtk.CellRendererText weightCell = new Gtk.CellRendererText();
			weightColumn.PackStart(weightCell, true);

			Gtk.TreeViewColumn positionColumn = new Gtk.TreeViewColumn();
			positionColumn.Title = Catalog.GetString("Position");
			Gtk.CellRendererText positionCell = new Gtk.CellRendererText();
			positionColumn.PackStart(positionCell, true);

			Gtk.TreeViewColumn numberColumn = new Gtk.TreeViewColumn();
			numberColumn.Title = Catalog.GetString("Number");
			Gtk.CellRendererText numberCell = new Gtk.CellRendererText();
			numberColumn.PackStart(numberCell, true);

			photoColumn.SetCellDataFunc(photoCell, new Gtk.TreeCellDataFunc(RenderPhoto));
			nameColumn.SetCellDataFunc(nameCell, new Gtk.TreeCellDataFunc(RenderName));
			playsColumn.SetCellDataFunc(playCell, new Gtk.TreeCellDataFunc(RenderPlay));
			nationColumn.SetCellDataFunc(nationCell, new Gtk.TreeCellDataFunc(RenderNationality));
			positionColumn.SetCellDataFunc(positionCell, new Gtk.TreeCellDataFunc(RenderPosition));
			numberColumn.SetCellDataFunc(numberCell, new Gtk.TreeCellDataFunc(RenderNumber));
			heightColumn.SetCellDataFunc(heightCell, new Gtk.TreeCellDataFunc(RenderHeight));
			weightColumn.SetCellDataFunc(weightCell, new Gtk.TreeCellDataFunc(RenderWeight));
			birthdayColumn.SetCellDataFunc(birthdayCell, new Gtk.TreeCellDataFunc(RenderBirthday));

			AppendColumn(photoColumn);
			AppendColumn(nameColumn);
			AppendColumn(playsColumn);
			AppendColumn(numberColumn);
			AppendColumn(positionColumn);
			AppendColumn(heightColumn);
			AppendColumn(weightColumn);
			AppendColumn(birthdayColumn);
			AppendColumn(nationColumn);
		}

		private void RenderPhoto(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Player player = (Player) model.GetValue(iter, 0);

			(cell as Gtk.CellRendererPixbuf).Pixbuf = player.Photo.Value;
		}

		private void RenderName(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Player player = (Player) model.GetValue(iter, 0);

			(cell as Gtk.CellRendererText).Text = player.Name;
		}

		private void RenderPlay(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Player player = (Player) model.GetValue(iter, 0);

			(cell as Gtk.CellRendererText).Text = player.Playing ? Catalog.GetString("Yes") : Catalog.GetString("No");
		}

		private void RenderNationality(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Player player = (Player) model.GetValue(iter, 0);

			(cell as Gtk.CellRendererText).Text = player.Nationality;
		}

		private void RenderPosition(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Player player = (Player) model.GetValue(iter, 0);

			(cell as Gtk.CellRendererText).Text = player.Position;
		}

		private void RenderNumber(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Player player = (Player) model.GetValue(iter, 0);

			(cell as Gtk.CellRendererText).Text = player.Number.ToString();
		}

		private void RenderHeight(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Player player = (Player) model.GetValue(iter, 0);

			(cell as Gtk.CellRendererText).Text = player.Height.ToString();
		}

		private void RenderWeight(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Player player = (Player) model.GetValue(iter, 0);

			(cell as Gtk.CellRendererText).Text = player.Weight.ToString();
		}

		private void RenderBirthday(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Player player = (Player) model.GetValue(iter, 0);

			(cell as Gtk.CellRendererText).Text = player.Birthday.ToShortDateString();
		}

		protected virtual void OnSelectionChanged(object o, System.EventArgs e) {
			TreeIter iter;
			List<Player> list;
			TreePath[] pathArray;

			list = new List<Player>();
			pathArray = Selection.GetSelectedRows();

			for(int i=0; i< pathArray.Length; i++) {
				Model.GetIterFromString(out iter, pathArray[i].ToString());
				list.Add((Player) Model.GetValue(iter, 0));
			}
			if(PlayersSelected != null)
				PlayersSelected(list);
		}

		protected virtual void OnTreeviewRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			Gtk.TreeIter iter;
			Model.GetIter(out iter, args.Path);
			Player player = (Player) Model.GetValue(iter, 0);
			if(PlayerClicked != null)
				PlayerClicked(player);
		}
	}
}