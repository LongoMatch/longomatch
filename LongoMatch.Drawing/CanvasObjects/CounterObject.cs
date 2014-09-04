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
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Common;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class CounterObject: CanvasDrawableObject<Counter>, ICanvasSelectableObject
	{

		public CounterObject ()
		{
		}

		public CounterObject (Counter counter)
		{
			Drawable = counter;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Area darea;
			
			darea = Drawable.Area;
			tk.Begin ();
			tk.FillColor = Drawable.FillColor;
			tk.StrokeColor = Drawable.StrokeColor;
			tk.LineWidth = Drawable.LineWidth;
			tk.DrawEllipse (Drawable.Center, Drawable.AxisX, Drawable.AxisY);
			tk.StrokeColor = Drawable.TextColor;
			tk.FontAlignment = FontAlignment.Center;
			tk.DrawText (darea.Start, darea.Width, darea.Height,
			             Drawable.Count.ToString ());
			DrawSelectionArea (tk);
			tk.End ();
		}
	}
}

