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
using LongoMatch.Core.Common;
using VAS.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Store.Drawables;

namespace LongoMatch.Drawing.CanvasObjects.Teams
{
	public class PlayerObject: CanvasButtonObject, ICanvasSelectableObject
	{
		static ISurface Photo;
		static ISurface ArrowOut;
		static ISurface ArrowIn;
		static bool surfacesCached = false;

		public PlayerObject ()
		{
			Init ();
		}

		public PlayerObject (PlayerLongoMatch player, Point position = null)
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

		public PlayerLongoMatch Player {
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

		Color Color {
			get {
				return Player.Color;
			}
		}

		public override double Width {
			get {
				return Size;
			}
		}

		public override double Height {
			get {
				return Size;
			}
		}

		public TeamType Team {
			get;
			set;
		}

		static public void LoadSurfaces ()
		{
			if (!surfacesCached) {
				Photo = CreateSurface (StyleConf.PlayerPhoto);
				ArrowOut = CreateSurface (StyleConf.PlayerArrowOut);
				ArrowIn = CreateSurface (StyleConf.PlayerArrowIn);
				surfacesCached = true;
			}
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
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
			double size, scale;
			ISurface arrowin, arrowout;

			if (Player == null)
				return;

			zero = new Point (0, 0);
			size = StyleConf.PlayerSize;
			scale = (double)Width / size;
			
			if (Team == TeamType.LOCAL) {
				arrowin = ArrowIn;
				arrowout = ArrowOut;
			} else {
				arrowin = ArrowOut;
				arrowout = ArrowIn;
			}

			tk.Begin ();
			start = new Point (Size / 2, Size / 2);
			tk.TranslateAndScale (Position - start, new Point (scale, scale));

			if (!UpdateDrawArea (tk, area, new Area (zero, size, size))) {
				tk.End ();
				return;
			}

			/* Background */
			tk.FillColor = Config.Style.PaletteBackgroundDark;
			tk.LineWidth = 0;
			tk.DrawRectangle (zero, StyleConf.PlayerSize, StyleConf.PlayerSize);
			
			/* Image */
			if (Player.Photo != null) {
				tk.DrawImage (zero, size, size, Player.Photo, ScaleMode.AspectFit);
			} else {
				tk.DrawSurface (zero, StyleConf.PlayerSize, StyleConf.PlayerSize, Photo, ScaleMode.AspectFit);
			}

			/* Bottom line */
			p = new Point (0, size - StyleConf.PlayerLineWidth);
			tk.FillColor = Color;
			tk.DrawRectangle (p, size, 3);
			
			/* Draw Arrow */
			if (SubstitutionMode && (Highlighted || Active)) {
				ISurface arrow;
				Point ap;

				if (Playing) {
					arrow = arrowout;
				} else {
					arrow = arrowin;
				}
				ap = new Point (StyleConf.PlayerArrowX, StyleConf.PlayerArrowY);
				tk.DrawRectangle (ap, StyleConf.PlayerArrowSize, StyleConf.PlayerArrowSize);
				tk.DrawSurface (arrow, ap);
			}

			/* Draw number */
			p = new Point (StyleConf.PlayerNumberX, StyleConf.PlayerNumberY);
			tk.FillColor = Color;
			tk.DrawRectangle (p, StyleConf.PlayerNumberSize, StyleConf.PlayerNumberSize);
			
			tk.FillColor = Color.White;
			tk.StrokeColor = Color.White;
			tk.FontWeight = FontWeight.Normal;
			if (Player.Number >= 100) {
				tk.FontSize = 12;
			} else {
				tk.FontSize = 16;
			}
			tk.DrawText (p, StyleConf.PlayerNumberSize, StyleConf.PlayerNumberSize,
				Player.Number.ToString ());
			
			if (Active) {
				Color c = Color.Copy ();
				c.A = (byte)(c.A * 60 / 100);
				tk.FillColor = c;
				tk.DrawRectangle (zero, size, size);
			}
			
			tk.End ();
		}

		void Init (Point pos = null)
		{
			if (pos == null) {
				pos = new Point (0, 0);
			}
			Position = pos;
			DrawPhoto = true;
			Size = (int)PlayersIconSize.Medium;
			Toggle = true;
			LoadSurfaces ();
		}

		static ISurface CreateSurface (string name)
		{
			Image img = Resources.LoadImage (name);
			return Config.DrawingToolkit.CreateSurface (img.Width, img.Height, img, false);
		}
	}
}

