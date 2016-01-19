// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.Collections.Generic;
using Gtk;
using Stetic;
using LongoMatch.Core;
using LongoMatch.Core.Common;


namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class RenderingJobsTreeView : Gtk.TreeView
	{
		public RenderingJobsTreeView ()
		{
			TreeViewColumn nameColumn = new TreeViewColumn ();
			nameColumn.Title = Catalog.GetString ("Job name");
			CellRendererText nameCell = new CellRendererText ();
			nameColumn.PackStart (nameCell, true);

			TreeViewColumn stateColumn = new TreeViewColumn ();
			stateColumn.Title = Catalog.GetString ("State");
			CellRendererPixbuf stateCell = new CellRendererPixbuf ();
			stateColumn.PackStart (stateCell, true);

			nameColumn.SetCellDataFunc (nameCell, new Gtk.TreeCellDataFunc (RenderName));
			stateColumn.SetCellDataFunc (stateCell, new Gtk.TreeCellDataFunc (RenderState));

			AppendColumn (nameColumn);
			AppendColumn (stateColumn);
		}

		public List<Job> SelectedJobs ()
		{
			/* FIXME: Only single selection is supported for now */
			TreeIter iter;
			List<Job> list;
			TreePath[] pathArray;

			list = new List<Job> ();
			pathArray = Selection.GetSelectedRows ();

			for (int i = 0; i < pathArray.Length; i++) {
				Model.GetIterFromString (out iter, pathArray [i].ToString ());
				list.Add ((Job)Model.GetValue (iter, 0));
			}
			
			return list;
		}

		private void RenderName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Job job = (Job)model.GetValue (iter, 0);

			(cell as Gtk.CellRendererText).Text = job.Name;
		}


		private void RenderState (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Job job = (Job)model.GetValue (iter, 0);

			(cell as Gtk.CellRendererPixbuf).Pixbuf = IconLoader.LoadIcon (this, job.StateIconName, IconSize.Button);
		}
	}
}

