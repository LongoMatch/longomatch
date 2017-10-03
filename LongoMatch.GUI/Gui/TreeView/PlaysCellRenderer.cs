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
using LongoMatch.Core.Store;
using LongoMatch.Drawing;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.MVVMC;
using VAS.Drawing.Cairo;
using Point = VAS.Core.Common.Point;

namespace LongoMatch.Gui.Component
{
	public class PlaysCellRenderer : CellRenderer
	{

		public object Item {
			get;
			set;
		}

		public int Count {
			get;
			set;
		}

		public LMProject Project {
			get;
			set;
		}

		/// <summary>
		/// Shoulds the redraw, just a proxy to the PlayslistCellRenderer
		/// </summary>
		/// <returns>The redraw.</returns>
		/// <param name="cellX">Cell x.</param>
		/// <param name="cellY">Cell y.</param>
		/// <param name="TotalY">Total y.</param>
		/// <param name="width">Width.</param>
		/// <param name="viewModel">View model.</param>
		public static Area ShouldRedraw (double cellX, double cellY, double TotalY, int width, IViewModel viewModel)
		{
			return PlayslistCellRenderer.ShouldRedraw (cellX, cellY, TotalY, width, viewModel);
		}

		/// <summary>
		/// Clickeds the play button, just a proxy to the PlayslistCellRenderer
		/// </summary>
		/// <returns><c>true</c>, if play button was clickeded, <c>false</c> otherwise.</returns>
		/// <param name="cellX">Cell x.</param>
		/// <param name="cellY">Cell y.</param>
		/// <param name="width">Width.</param>
		public static bool ClickedPlayButton (double cellX, double cellY, int width)
		{
			return PlayslistCellRenderer.ClickedPlayButton (cellX, cellY, width);
		}

		public override void GetSize (Widget widget, ref Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			x_offset = 0;
			y_offset = 0;
			width = StyleConf.ListSelectedWidth + StyleConf.ListRowSeparator + StyleConf.ListTextWidth;
			height = StyleConf.ListCategoryHeight;
			if (Item is LMTimelineEvent) {
				LMTimelineEvent evt = Item as LMTimelineEvent;
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
				PlayslistCellRenderer.Render (Item, Project, Count, IsExpanded, App.Current.DrawingToolkit,
					context, bkg, cell, state);
			}
		}
	}
}
