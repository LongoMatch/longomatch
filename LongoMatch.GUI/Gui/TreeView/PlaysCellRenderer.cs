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
using Gdk;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;
using LongoMatch.Drawing;
using LongoMatch.Drawing.Cairo;
using VAS.Core.Common;
using Point = VAS.Core.Common.Point;

namespace LongoMatch.Gui.Component
{
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

		public ProjectLongoMatch Project {
			get;
			set;
		}

		public override void GetSize (Widget widget, ref Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			x_offset = 0;
			y_offset = 0;
			width = StyleConf.ListSelectedWidth + StyleConf.ListRowSeparator + StyleConf.ListTextWidth;
			height = StyleConf.ListCategoryHeight;
			if (Item is TimelineEventLongoMatch) {
				TimelineEventLongoMatch evt = Item as TimelineEventLongoMatch;
				if (evt.Miniature != null) {
					width += StyleConf.ListImageWidth + StyleConf.ListRowSeparator;
				}
				width += (StyleConf.ListImageWidth + StyleConf.ListRowSeparator) * (evt.Players.Count + evt.Teams.Count);
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
