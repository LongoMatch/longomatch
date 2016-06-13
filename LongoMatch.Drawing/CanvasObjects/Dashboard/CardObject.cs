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
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Drawing.CanvasObjects.Dashboard;

namespace LongoMatch.Drawing.CanvasObjects.Dashboard
{
	public class CardObject: TimedTaggerObject
	{
		public CardObject (PenaltyCardButton card) : base (card)
		{
			Button = card;
		}

		public PenaltyCardButton Button {
			get;
			set;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Color front, back;
			int width;

			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}
			tk.Begin ();

			if (Active) {
				tk.LineWidth = StyleConf.ButtonLineWidth;
				tk.StrokeColor = BackgroundColor;
				tk.FillColor = TextColor;
			} else {
				tk.LineWidth = 0;
				tk.StrokeColor = TextColor;
				tk.FillColor = BackgroundColor;
			}

			/* Draw Shape */
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
			tk.FontSize = StyleConf.ButtonNameFontSize;
			tk.FontWeight = FontWeight.Light;
			tk.FontAlignment = FontAlignment.Center;
			if (Recording) {
				tk.DrawText (Position, Button.Width, Button.Height, (CurrentTime - Start).ToSecondsString ());
			} else {
				tk.DrawText (Position, Button.Width, Button.Height, Button.PenaltyCard.Name);
			}
			DrawSelectionArea (tk);
			if (ShowLinks) {
				GetAnchor (null).Draw (tk, area);
			}
			tk.End ();
		}
	}
}

