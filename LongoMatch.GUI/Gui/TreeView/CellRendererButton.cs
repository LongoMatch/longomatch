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
using Gdk;
using GLib;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Drawing.Cairo;
using Point = VAS.Core.Common.Point;
using VAS.Core.Common;

namespace LongoMatch.Gui.Component
{
	public delegate void ClickedHandler (object sender,ClickedArgs args);

	public class CellRendererButton: CellRendererToggle
	{
		public event ClickedHandler Clicked;

		public CellRendererButton (string text)
		{
			Text = text;
		}

		public String Text {
			get;
			set;
		}

		protected override void OnToggled (string path)
		{
			if (Clicked != null) {
				Clicked (this, new ClickedArgs (path));
			}
		}

		public override void GetSize (Widget widget, ref Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			x_offset = 0;
			y_offset = 0;

			Config.DrawingToolkit.MeasureText (Text, out width, out height, Config.Style.Font, 12, FontWeight.Normal);

			width += StyleConf.FilterTreeViewOnlyRightOffset * 2;
			height += 10;
		}

		protected override void Render (Drawable window, Widget widget, Rectangle backgroundArea,
		                                Rectangle cellArea, Rectangle exposeArea, CellRendererState flags)
		{
			IDrawingToolkit tk = Config.DrawingToolkit;

			using (IContext context = new CairoContext (window)) {
				int width = cellArea.Width - StyleConf.FilterTreeViewOnlyRightOffset;
				int height = cellArea.Height - StyleConf.FilterTreeViewOnlyTopOffset * 2;
				Point pos = new Point (cellArea.X + backgroundArea.Width - cellArea.Width,
					            cellArea.Y + StyleConf.FilterTreeViewOnlyTopOffset);
				tk.Context = context;
				tk.Begin ();
				tk.FontSize = 12;
				tk.FillColor = null;
				tk.LineWidth = 1;
				tk.StrokeColor = Config.Style.PaletteBackgroundDark;
				tk.DrawRoundedRectangle (pos, width, height, 3);
				tk.StrokeColor = Config.Style.PaletteText;
				tk.FontAlignment = FontAlignment.Center;
				tk.DrawText (pos, width, height, Text);
				tk.End ();
				tk.Context = null;
			}
		}
	}

	public class ClickedArgs : SignalArgs
	{
		public ClickedArgs (string path)
		{
			this.Path = path;
		}

		public string Path {
			get;
		}
	}
}
