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
using LongoMatch.Store;
using LongoMatch.Interfaces;
using LongoMatch.Common;
using LongoMatch.Store.Drawables;
using LongoMatch.Drawing.Widgets;

namespace LongoMatch.Drawing.CanvasObject
{
	public class PlayerObject: CanvasObject, ICanvasSelectableObject
	{
		public PlayerObject (Player player, Point position)
		{
			Player = player;
			Position = position;
			DrawPhoto = true;
			SelectedColor = Common.PLAYER_SELECTED_COLOR;
			UnSelectedColor = Common.PLAYER_UNSELECTED_COLOR;
			IconSize = PlayersIconSize.Medium;
		}
		
		public Player Player  {
			get;
			protected set;
		}
		
		public Point Position {
			get;
			set;
		}
		
		public PlayersIconSize IconSize {
			set;
			get;
		}
		
		public bool DrawPhoto {
			get;
			set;
		}
		
		public Color SelectedColor {
			get;
			set;
		}
		
		public Color UnSelectedColor {
			get;
			set;
		}
		
		int Width {
			get {
				return (int)IconSize;
			}
		}

		int Height {
			get {
				return (int)IconSize;
			}
		}
		
		public Selection GetSelection (Point point, double precision) {
			Point position = new Point (Position.X - Width / 2, Position.Y - Height / 2);

			if (point.X >= position.X && point.X <= position.X + Width) {
				if (point.Y >= position.Y && point.Y <= position.Y + Height) {
					return new Selection (this, SelectionPosition.All, 0);
				}
			}
			return null;
		}
		
		public void Move (Selection sel, Point p, Point start) {
		}

		public override void Draw (IDrawingToolkit tk, Area area) {
			Color background, line;
			Point position = new Point (Position.X - Width / 2, Position.Y - Height / 2);
			
			tk.Begin();
			
			/* Background */
			if (Selected) {
				background = SelectedColor;
				line = SelectedColor;
			} else {
				background = UnSelectedColor;
				line = UnSelectedColor;
			}
			tk.StrokeColor = line;
			tk.FillColor = background;
			tk.LineWidth = 5;
			tk.DrawRoundedRectangle (position, Width, Height, 5);
			
			if (!DrawPhoto || Player.Photo == null || IconSize < PlayersIconSize.Medium) {
				tk.FillColor = Color.White;
				tk.StrokeColor = Color.White;
				tk.FontSize = Width / 2;
				tk.FontWeight = FontWeight.Bold;
				/* Only draw player number for the smaller size */
				if (IconSize > PlayersIconSize.Small) {
					tk.DrawText (position, Width, Height - 20, Player.Number.ToString());
					tk.FontSize = 8;
					tk.DrawText (new Point (position.X, position.Y + Height - 20), Width, 20, Player.Name);
				} else {
					tk.DrawText (position, Width, Height, Player.Number.ToString());
				}
			} else {
				tk.FillColor = Color.Black;
				tk.StrokeColor = Color.Black;
				tk.DrawImage (position, Width, Height, Player.Photo, true);
				tk.FontSize = 16;
				tk.FontWeight = FontWeight.Bold;
				tk.DrawText (new Point (position.X, position.Y + Height - 20), Width, 20, Player.Number.ToString());
			}

			tk.End();
		}

	}
}

