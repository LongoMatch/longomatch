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
using System;
using LongoMatch.Store;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Interfaces;
using LongoMatch.Common;
using LongoMatch.Store.Drawables;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class PlayObject: TimeNodeObject
	{
		public PlayObject (Play play):base (play)
		{
		}

		public override string Description {
			get {
				return Play.Name;
			}
		}

		public Play Play {
			get {
				return TimeNode as Play;
			}
		}

		void DrawLine (IDrawingToolkit tk, double start, double stop, int lineWidth)
		{
			double y;
			
			y = OffsetY + Height / 2;
			tk.LineWidth = lineWidth;
			tk.FillColor = Play.Category.Color;
			tk.StrokeColor = Play.Category.Color;
			if (stop - start <= lineWidth) {
				tk.LineWidth = 0;
				tk.DrawCircle (new Point (start + (stop - start) / 2, y), 3);
			} else {
				tk.DrawLine (new Point (start + lineWidth / 2, y),
				             new Point (stop - lineWidth / 2, y));
			}
		}

		void DrawBorders (IDrawingToolkit tk, double start, double stop, int lineWidth)
		{
			double y1, y2;

			tk.LineWidth = lineWidth;
			tk.FillColor = Config.Style.PaletteWidgets;
			tk.StrokeColor = Config.Style.PaletteWidgets;
			y1 = OffsetY + 6;
			y2 = OffsetY + Height - 6;
				tk.DrawLine (new Point (start, y1), new Point (start, y2));
				tk.DrawLine (new Point (stop, y1), new Point (stop, y2));
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double start, stop;
			int lineWidth = StyleConf.TimelineLineSize;

			tk.Begin ();
			
			start = StartX;
			stop = StopX;
			
			if (stop - start <= lineWidth) {
				DrawBorders (tk, start, stop, lineWidth);
				DrawLine (tk, start, stop, lineWidth);
			} else {
				DrawLine (tk, start, stop, lineWidth);
				DrawBorders (tk, start, stop, lineWidth);
			}
			tk.End ();
		}
	}
}

