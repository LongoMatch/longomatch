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
using LongoMatch.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;
using VAS.Core.Common;

namespace LongoMatch.Drawing.CanvasObjects.Blackboard
{
	public class TextObject: CanvasDrawableObject<Text>
	{
		public TextObject ()
		{
		}

		public TextObject (Text text)
		{
			Drawable = text;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			if (!UpdateDrawArea (tk, area, Drawable.Area)) {
				return;
			}
			;

			tk.Begin ();
			tk.FillColor = Drawable.FillColor;
			tk.StrokeColor = Drawable.StrokeColor;
			tk.LineWidth = 1;
			tk.DrawRectangle (Drawable.TopLeft, Drawable.Width, Drawable.Height);
			tk.StrokeColor = Drawable.TextColor;
			tk.FontSize = Drawable.TextSize;
			tk.FontAlignment = FontAlignment.Center;
			tk.DrawText (Drawable.TopLeft, Drawable.Width, Drawable.Height,
				Drawable.Value);
			DrawSelectionArea (tk);
			tk.End ();
		}
	}
}

