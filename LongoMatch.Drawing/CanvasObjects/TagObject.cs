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
using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Store;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class TagObject: TaggerObject
	{
		public TagObject (TagButton tagger): base (tagger)
		{
			TagButton = tagger;
			Toggle = true;
		}

		public TagButton TagButton {
			get;
			set;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			tk.Begin ();
			
			/* Draw Rectangle */
			DrawButton (tk);
			
			/* Draw header */
			tk.LineWidth = 2;
			tk.StrokeColor = TagButton.TextColor;
			tk.FillColor = TagButton.TextColor;
			tk.DrawText (Position, TagButton.Width, TagButton.Height, TagButton.Name);
			DrawSelectionArea (tk);
			tk.End ();
		}
	}
}
