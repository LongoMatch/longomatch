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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class CardObject: TaggerObject
	{

		public CardObject (PenaltyCardButton card): base (card)
		{
			Button = card;
		}

		public PenaltyCardButton Button {
			get;
			set;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			tk.Begin ();

			/* Draw Rectangle */
			tk.FillColor = Color;
			tk.StrokeColor = Color;
			tk.LineWidth = 0;
			switch (Button.PenaltyCard.Shape) {
			case CardShape.Rectangle:
				tk.DrawRoundedRectangle (Button.Position, Button.Width, Button.Height, 3);
				break;
			case CardShape.Circle:
				tk.DrawCircle (new Point (Button.Position.X + Button.Width / 2,
				                          Button.Position.Y + Button.Height / 2),
				               Math.Min (Button.Width, Button.Height) / 2);
				break;
			case CardShape.Triangle:
				tk.DrawTriangle (new Point (Button.Position.X + Button.Width / 2, Button.Position.Y),
				                 Button.Width, Button.Height, SelectionPosition.Top);
				break;
			}

			/* Draw header */
			tk.LineWidth = 2;
			tk.StrokeColor = Color.Grey2;
			tk.FillColor = Color.Grey2;
			tk.DrawText (Position, Button.Width, Button.Height, Button.PenaltyCard.Name);
			DrawSelectionArea (tk);
			tk.End ();
		}
	}
}

