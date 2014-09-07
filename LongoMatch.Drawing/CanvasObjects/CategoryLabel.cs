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
using LongoMatch.Core.Store;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Common;
using System;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class CategoryLabel: CanvasObject, ICanvasObject
	{
		EventType eventType;
		double width;

		public CategoryLabel (EventType eventType, double width, double height, double offsetY)
		{
			this.eventType = eventType;
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

		public Color BackgroundColor {
			get;
			set;
		}

		public double OffsetY {
			set;
			get;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double hs, vs, to, rectSize;
			double y;
			
			hs = StyleConf.TimelineLabelHSpacing;
			vs = StyleConf.TimelineLabelVSpacing;
			rectSize = Height - vs * 2;
			to = hs + rectSize + hs;
			
			y = OffsetY - Math.Floor (Scroll);
			tk.Begin ();
			tk.FillColor = BackgroundColor;
			tk.StrokeColor = BackgroundColor;
			tk.LineWidth = 0;
			tk.DrawRectangle (new Point (0, y), width, Height);
			
			/* Draw a rectangle with the category color */
			tk.FillColor = eventType.Color;
			tk.StrokeColor = eventType.Color;
			tk.DrawRectangle (new Point (hs, y + vs), rectSize, rectSize); 
			
			/* Draw category name */
			tk.FontSlant = FontSlant.Normal;
			tk.FontWeight = FontWeight.Bold;
			tk.FontSize = 12;
			tk.FillColor = Config.Style.PaletteWidgets;
			tk.FontAlignment = FontAlignment.Left;
			tk.StrokeColor = Config.Style.PaletteWidgets;
			tk.DrawText (new Point (to, y), width - to, Height, eventType.Name);
			tk.End ();
		}
	}
}

