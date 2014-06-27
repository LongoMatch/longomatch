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

namespace LongoMatch.Drawing.CanvasObject
{
	public class CategoryLabel: CanvasObject, ICanvasObject
	{
		Category category;
		double width, height;

		public CategoryLabel (Category category, double width, double height,
		                      double offsetY)
		{
			this.category = category;
			this.height = height;
			this.width = width;
			OffsetY = offsetY;
		}
		
		public double Scroll {
			get;
			set;
		}
		
		public double OffsetY {
			set;
			protected get;
		}
		
		public override void Draw (IDrawingToolkit tk, Area area) {
			double y;
			
			y = OffsetY - Scroll;
			tk.Begin();
			tk.FillColor = category.Color;
			tk.StrokeColor = category.Color;
			tk.FontSlant = FontSlant.Normal;
			tk.FontSize = 12;
			tk.DrawRoundedRectangle (new Point(0, y + 1), width, height - 1, 3);  
			tk.FillColor = Common.TEXT_COLOR;
			tk.StrokeColor = Common.TEXT_COLOR;
			tk.DrawText (new Point (0, y), width, height,
			             category.Name);
			tk.End();
		}
	}
}

