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
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Store;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class CardObject: TaggerObject
	{

		public CardObject (PenaltyCard card): base (card)
		{
			Card = card;
		}

		public PenaltyCard Card {
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
			switch (Card.Shape) {
			case CardShape.Rectangle:
				tk.DrawRoundedRectangle (Card.Position, Card.Width, Card.Height, 3);
				break;
			case CardShape.Circle:
				tk.DrawCircle (new Point (Card.Position.X + Card.Width / 2,
				                          Card.Position.Y + Card.Height / 2),
				               Math.Min (Card.Width, Card.Height) / 2);
				break;
			case CardShape.Triangle:
				tk.DrawTriangle (new Point (Card.Position.X + Card.Width / 2, Card.Position.Y),
				                 Card.Width, Card.Height, SelectionPosition.Top);
				break;
			}

			/* Draw header */
			tk.LineWidth = 2;
			tk.StrokeColor = Color.Grey2;
			tk.FillColor = Color.Grey2;
			tk.DrawText (Position, Card.Width, Card.Height, Card.Name);
			DrawSelectionArea (tk);
			tk.End ();
		}
	}
}

