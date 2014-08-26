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
using LongoMatch.Store;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Common;
using System;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class CategoryLabel: CanvasObject, ICanvasObject
	{
		TaggerButton category;
		double width;

		public CategoryLabel (TaggerButton category, double width, double height,
		                            double offsetY)
		{
			this.category = category;
			this.Height = height;
			this.width = width;
			OffsetY = offsetY;
		}

		public double Height {
			get;
			set;
		}

		public double Scroll {
			get;
			set;
		}

		public bool Even {
			get;
			set;
		}

		public double OffsetY {
			set;
			get;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Color color;
			double hs, vs, to, rectSize;
			double y;
			
			if (Even) {
				color = Config.Style.PaletteBackground;
			} else {
				color = Config.Style.PaletteBackgroundLight;
			}
			
			hs = StyleConf.TimelineLabelHSpacing;
			vs = StyleConf.TimelineLabelVSpacing;
			rectSize = Height - vs * 2;
			to = hs + rectSize + hs;
			
			y = OffsetY - Math.Floor (Scroll);
			tk.Begin ();
			tk.FillColor = color;
			tk.StrokeColor = color;
			tk.LineWidth = 0;
			tk.DrawRectangle (new Point (0, y), width, Height);
			
			/* Draw a rectangle with the category color */
			tk.FillColor = category.Color;
			tk.StrokeColor = category.Color;
			tk.DrawRectangle (new Point (hs, y + vs), rectSize, rectSize); 
			
			/* Draw category name */
			tk.FontSlant = FontSlant.Normal;
			tk.FontWeight = FontWeight.Bold;
			tk.FontSize = 12;
			tk.FillColor = Config.Style.PaletteWidgets;
			tk.FontAlignment = FontAlignment.Left;
			tk.StrokeColor = Config.Style.PaletteWidgets;
			tk.DrawText (new Point (to, y), width - to, Height, category.Name);
			tk.End ();
		}
	}
}

