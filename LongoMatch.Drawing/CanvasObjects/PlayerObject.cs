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

namespace LongoMatch.Drawing.CanvasObjects
{
	public class PlayerObject: CanvasButtonObject, ICanvasSelectableObject
	{
		public PlayerObject ()
		{
			Init ();
		}
		
		public PlayerObject (Player player, Point position = null)
		{
			Player = player;
			Init (position);
		}
		
		void Init (Point pos = null) {
			if (pos == null) {
				pos = new Point (0, 0);
			}
			Position = pos;
			DrawPhoto = true;
			Color = Constants.PLAYER_SELECTED_COLOR;
			Size = (int)PlayersIconSize.Medium;
			Toggle = true;
		}

		public Player Player {
			get;
			set;
		}

		public Point Position {
			get;
			set;
		}

		public int Size {
			set;
			get;
		}

		public bool DrawPhoto {
			get;
			set;
		}

		public Color Color {
			get;
			set;
		}

		int Width {
			get {
				return Size;
			}
		}

		int Height {
			get {
				return Size;
			}
		}
		
		public Team Team {
			get;
			set;
		}

		public Selection GetSelection (Point point, double precision)
		{
			Point position = new Point (Position.X - Width / 2, Position.Y - Height / 2);

			if (point.X >= position.X && point.X <= position.X + Width) {
				if (point.Y >= position.Y && point.Y <= position.Y + Height) {
					return new Selection (this, SelectionPosition.All, 0);
				}
			}
			return null;
		}

		public void Move (Selection sel, Point p, Point start)
		{
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Point zero, p;
			double numberWidth, numberHeight;
			double size, scale;

			zero = new Point (0, 0);
			size = Config.Style.PlayerSize;
			scale = Width / size; 
			
			tk.Begin ();
			tk.TranslateAndScale (Position - new Point (Size / 2, Size / 2),
			                      new Point (scale, scale));

			/* Background */
			tk.LineStyle = LineStyle.Normal;
			tk.LineWidth = Config.Style.PlayerBorder;
			tk.FillColor = Config.Style.PaletteBackgroundDark;
			tk.StrokeColor = Config.Style.PaletteBackgroundDark;
			tk.DrawRoundedRectangle (zero, size, size, Config.Style.PlayerRadius);
			
			if (!DrawPhoto || Player.Photo == null) {
				numberHeight = size;
				numberWidth = Config.Style.PlayerNumberWidth;
				p = new Point (Config.Style.PlayerNumberOffset, 0);
			} else {
				/* Image */
				tk.DrawImage (zero, size, size, Player.Photo, true);
				numberHeight = Config.Style.PlayerNumberHeight;
				numberWidth = Config.Style.PlayerNumberWidth;
				p = new Point (Config.Style.PlayerNumberOffset, size - numberHeight);
			}
			
			/* Draw background */
			tk.FillColor = Color;
			tk.StrokeColor = Color;
			tk.LineWidth = 0;
			tk.DrawRoundedRectangle (p, numberWidth, numberHeight, Config.Style.PlayerRadius);
				
			/* Draw bottom Line */
			tk.StrokeColor = Color;
			tk.FillColor = Color;
			tk.LineWidth = Config.Style.PlayerBorder;
			tk.DrawRoundedRectangle (new Point (0, size - Config.Style.PlayerTeamLineWidth),
			                         size, Config.Style.PlayerTeamLineWidth + 1, 2);
			
			/* Draw number */
			tk.FillColor = Color.White;
			tk.StrokeColor = Color.White;
			tk.FontWeight = FontWeight.Normal;
			if (Player.Number >= 100) {
				tk.FontSize = (int)(size / 4);
			} else {
				tk.FontSize = (int)(size / 3);
			}
			tk.DrawText (p, numberWidth, numberHeight, Player.Number.ToString ());
			
			/* Selection line */
			if (Active) {
				tk.LineStyle = LineStyle.Normal;
				tk.LineWidth = Config.Style.PlayerBorder;
				tk.FillColor = null;
				tk.StrokeColor = Config.Style.PaletteActive;
				tk.DrawRoundedRectangle (zero, size + 1, size + 1, Config.Style.PlayerRadius);
			}
			
			tk.End ();
		}
	}
}

