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
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Store.Drawables;
using LongoMatch.Common;
using LongoMatch.Interfaces;

namespace LongoMatch.Drawing.CanvasObject
{
	public class PointObject: BaseCanvasDrawableObject<Circle>, ICanvasSelectableObject
	{

		public PointObject (Point center, double radius, Color color)
		{
			Drawable = new Circle (center, radius);
			Drawable.FillColor = color;
			Drawable.StrokeColor = color;
			Drawable.LineWidth = 1;
		}
		
		public override void Draw (IDrawingToolkit tk, Area area) {
			tk.FillColor = Drawable.FillColor;
			tk.StrokeColor = Drawable.StrokeColor;
			tk.LineWidth = Drawable.LineWidth;
			tk.DrawCircle (Drawable.Center, Drawable.Radius);
		}
	}
}

