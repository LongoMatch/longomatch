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
using LongoMatch.Common;
using LongoMatch.Store.Drawables;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Interfaces;

namespace LongoMatch.Drawing.CanvasObject
{
	public class LineObject: BaseCanvasDrawableObject<Line>, ICanvasSelectableObject
	{
		public LineObject (Point start, Point stop, LineType type, LineStyle stile,
		                   Color color, int width)
		{
			Drawable = new Line (start, stop, type, stile);
			Drawable.FillColor = color;
			Drawable.StrokeColor = color;
			Drawable.LineWidth = width;
		}
		
		public override void Draw (IDrawingToolkit tk, Area area) {
			tk.Begin ();
			tk.FillColor = Drawable.FillColor;
			tk.StrokeColor = Drawable.StrokeColor;
			tk.LineWidth = Drawable.LineWidth;
			tk.DrawLine (Drawable.Start, Drawable.Stop);
			tk.End ();
		}

	}
}

