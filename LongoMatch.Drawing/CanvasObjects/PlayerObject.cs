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
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Drawing.Widgets;
using System.IO;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class PlayerObject: CanvasButtonObject, ICanvasSelectableObject
	{
		static ISurface Photo;
		static ISurface Background;
		static ISurface HomeNumber;
		static ISurface AwayNumber;
		static ISurface HomeOut;
		static ISurface AwayOut;
		static ISurface HomeIn;
		static ISurface AwayIn;
		static bool surfacesCached = false;

		public PlayerObject ()
		{
			Init ();
		}
		
		public PlayerObject (Player player, Point position = null)
		{
			Player = player;
			Init (position);
		}

		public bool SubstitutionMode {
			get;
			set;
		}
		
		public bool Playing {
			get;
			set;
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

		public Selection GetSelection (Point point, double precision, bool inMotion=false)
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
			Point zero, start, p;
			double numberWidth, numberHeight;
			double size, scale;
			ISurface number, sin, sout;

			if (Player == null)
				return;

			zero = new Point (0, 0);
			size = Background.Height - StyleConf.PlayerLineWidth;
			scale = (double) Width / Background.Height; 
			
			if (Team == Team.LOCAL) {
				number = HomeNumber;
				sin = HomeIn;
				sout = HomeOut;
			} else {
				number = AwayNumber;
				sin = AwayIn;
				sout = AwayOut;
			}

			tk.Begin ();
			start = new Point (Size / 2, Size / 2);
			tk.TranslateAndScale (Position - start, new Point (scale, scale));

			if (!UpdateDrawArea (tk, area, new Area (zero, Background.Height, Background.Height))) {
				tk.End();
				return;
			};

			/* Background */
			tk.DrawSurface (Background, zero);
			
			/* Image */
			if (Player.Photo != null) {
				tk.DrawImage (zero, size, size, Player.Photo, true);
			} else {
				tk.DrawSurface (Photo, zero);
			}
			numberHeight = StyleConf.PlayerNumberHeight;
			numberWidth = StyleConf.PlayerNumberWidth;
			p = new Point (StyleConf.PlayerNumberOffset, size - numberHeight);
			
			/* Draw background */
			tk.DrawSurface (number, zero);
			
			/* Draw Arrow */
			if (SubstitutionMode && (Highlighted || Active)) {
				ISurface arrow;
				
				if (Playing) {
					arrow = sout;
				} else {
					arrow = sin;
				}
				tk.DrawSurface (arrow, new Point (Background.Width / 2 - arrow.Width / 2,
				                                  Background.Height / 2 - arrow.Height / 2));
			}
			
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
				tk.LineWidth = StyleConf.PlayerLineWidth;
				tk.FillColor = null;
				tk.StrokeColor = Config.Style.PaletteActive;
				tk.DrawRoundedRectangle (zero, size + 1, size + 1, StyleConf.PlayerLineWidth);
			}
			
			tk.End ();
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
			LoadSurfaces ();
		}

		void LoadSurfaces ()
		{
			if (!surfacesCached) {
				Photo = CreateSurface (StyleConf.PlayerPhoto);
				Background = CreateSurface (StyleConf.PlayerBackground);
				HomeNumber = CreateSurface (StyleConf.PlayerHomeNumber);
				AwayNumber = CreateSurface (StyleConf.PlayerAwayNumber);
				HomeOut = CreateSurface (StyleConf.PlayerHomeOut);
				AwayOut = CreateSurface (StyleConf.PlayerAwayOut);
				HomeIn = CreateSurface (StyleConf.PlayerHomeIn);
				AwayIn = CreateSurface (StyleConf.PlayerAwayIn);
				surfacesCached = true;
			}
		}

		ISurface CreateSurface (string name)
		{
			return Config.DrawingToolkit.CreateSurface (Path.Combine (Config.ImagesDir, name), false);
		}

	}
}

