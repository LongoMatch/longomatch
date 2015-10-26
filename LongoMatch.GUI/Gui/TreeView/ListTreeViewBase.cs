//
//  Copyright (C) 2010 Andoni Morales Alastruey
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
using System.Linq;
using Gdk;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;
using LongoMatch.Drawing;
using LongoMatch.Drawing.Cairo;
using LongoMatch.Gui.Menus;
using Color = Gdk.Color;
using Image = LongoMatch.Core.Common.Image;
using Point = LongoMatch.Core.Common.Point;

namespace LongoMatch.Gui.Component
{
	public abstract class ListTreeViewBase:TreeView
	{
		protected bool editing;
		protected bool enableCategoryMove = false;
		protected PlaysMenu playsMenu;
		protected TreeModelFilter modelFilter;
		protected TreeModelSort modelSort;
		protected TreeStore childModel;
		EventsFilter filter;

		public event EventHandler NewRenderingJob;

		public ListTreeViewBase ()
		{
			Selection.Mode = SelectionMode.Multiple;
			Selection.SelectFunction = SelectFunction;
			RowActivated += new RowActivatedHandler (OnTreeviewRowActivated);
			HeadersVisible = false;
			ShowExpanders = false;
			
			TreeViewColumn custColumn = new TreeViewColumn ();
			CellRenderer cr = new PlaysCellRenderer ();
			custColumn.PackStart (cr, true);
			custColumn.SetCellDataFunc (cr, RenderElement); 

			playsMenu = new PlaysMenu ();
			playsMenu.EditPlayEvent += HandleEditPlayEvent;
			AppendColumn (custColumn);
		}

		public bool Colors {
			get;
			set;
		}

		public EventsFilter Filter {
			set {
				filter = value;
				filter.FilterUpdated += OnFilterUpdated;
				Refilter ();
			}
			get {
				return filter;
			}
		}

		public void Refilter ()
		{
			if (modelFilter != null)
				modelFilter.Refilter ();
		}

		public Project Project {
			set;
			protected get;
		}

		new public TreeStore Model {
			set {
				childModel = value;
				if (value != null) {
					modelFilter = new TreeModelFilter (value, null);
					modelFilter.VisibleFunc = new TreeModelFilterVisibleFunc (FilterFunction);
					modelSort = new TreeModelSort (modelFilter);
					modelSort.SetSortFunc (0, SortFunction);
					modelSort.SetSortColumnId (0, SortType.Ascending);
					// Assign the filter as our tree's model
					base.Model = modelSort;
				} else {
					base.Model = null;
				}
			}
			get {
				return childModel;
			}
		}

		protected TimelineEvent SelectedPlay {
			get {
				return GetValueFromPath (Selection.GetSelectedRows () [0]) as TimelineEvent;
			}
		}

		protected List<TimelineEvent> SelectedPlays {
			get {
				return Selection.GetSelectedRows ().Select (
					p => GetValueFromPath (p) as TimelineEvent).ToList ();
			}
		}

		protected void ShowMenu ()
		{
			playsMenu.ShowListMenu (Project, SelectedPlays);
		}

		protected object GetValueFromPath (TreePath path)
		{
			Gtk.TreeIter iter;
			modelSort.GetIter (out iter, path);
			return modelSort.GetValue (iter, 0);
		}

		protected bool FilterFunction (TreeModel model, TreeIter iter)
		{
			if (Filter == null)
				return true;
			object o = model.GetValue (iter, 0);
			return Filter.IsVisible (o);
		}

		protected virtual void OnTreeviewRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			object item = GetValueFromPath (args.Path);
			if (!(item is TimelineEvent))
				return;

			Config.EventsBroker.EmitLoadEvent (item as TimelineEvent);
		}

		void HandleEditPlayEvent (object sender, EventArgs e)
		{
			List<Player> players = SelectedPlay.Players.ToList ();

			Config.GUIToolkit.EditPlay (SelectedPlay, Project, true, true, true, true);

			if (!Enumerable.SequenceEqual (players, SelectedPlay.Players)) {
				Config.EventsBroker.EmitTeamTagsChanged ();
			}
			Config.EventsBroker.EmitEventEdited (SelectedPlay);
			modelSort.SetSortFunc (0, SortFunction);
			modelSort.SetSortColumnId (0, SortType.Ascending);
		}

		protected void OnFilterUpdated ()
		{
			Refilter ();
		}

		protected void RenderElement (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			var item = model.GetValue (iter, 0);
			PlaysCellRenderer c = cell as PlaysCellRenderer;
			c.Item = item;
			c.Count = model.IterNChildren (iter);
			c.Project = Project;
		}

		protected abstract bool SelectFunction (TreeSelection selection, TreeModel model, TreePath path, bool selected);

		protected abstract int SortFunction (TreeModel model, TreeIter a, TreeIter b);
	}

	public class PlaysCellRenderer: CellRenderer
	{

		public object Item {
			get;
			set;
		}

		public int Count {
			get;
			set;
		}

		public Project Project {
			get;
			set;
		}

		public override void GetSize (Widget widget, ref Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			x_offset = 0;
			y_offset = 0;
			width = StyleConf.ListSelectedWidth + StyleConf.ListRowSeparator + StyleConf.ListTextWidth;
			height = StyleConf.ListCategoryHeight;
			if (Item is TimelineEvent) {
				TimelineEvent evt = Item as TimelineEvent;
				if (evt.Miniature != null) {
					width += StyleConf.ListImageWidth + StyleConf.ListRowSeparator;
				}
				width += (StyleConf.ListImageWidth + StyleConf.ListRowSeparator) * evt.Players.Count;
				if (evt.Team != TeamType.LOCAL || evt.Team != TeamType.VISITOR) {
					width += (StyleConf.ListImageWidth + StyleConf.ListRowSeparator);
				} else if (evt.Team != TeamType.BOTH) {
					width += (StyleConf.ListImageWidth + StyleConf.ListRowSeparator) * 2;
				}
			}
		}

		protected override void Render (Drawable window, Widget widget, Rectangle backgroundArea,
		                                Rectangle cellArea, Rectangle exposeArea, CellRendererState flags)
		{
			CellState state = (CellState)flags;
			
			using (IContext context = new CairoContext (window)) {
				Area bkg = new Area (new Point (backgroundArea.X, backgroundArea.Y),
					           backgroundArea.Width, backgroundArea.Height);
				Area cell = new Area (new Point (cellArea.X, cellArea.Y),
					            cellArea.Width, cellArea.Height);
				PlayslistCellRenderer.Render (Item, Project, Count, IsExpanded, Config.DrawingToolkit,
					context, bkg, cell, state);
			}
		}
	}
}
