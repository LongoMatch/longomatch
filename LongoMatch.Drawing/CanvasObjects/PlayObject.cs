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

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Color c = Play.Category.Color;
			tk.Begin ();
			tk.FillColor = new Color (c.R, c.G, c.B, (byte)(0.8 * byte.MaxValue));
			if (Selected) {
				tk.StrokeColor = Constants.PLAY_OBJECT_SELECTED_COLOR;
			} else {
				tk.StrokeColor = Play.Category.Color;
			}
			tk.LineWidth = 2;
			tk.DrawRoundedRectangle (new Point (StartX, OffsetY),
			                         Utils.TimeToPos (Play.Duration, SecondsPerPixel),
			                         Constants.CATEGORY_HEIGHT, 2);
			tk.End ();
		}
	}
}

