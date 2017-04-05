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
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Drawing.CanvasObjects.Teams;

namespace LongoMatch.Drawing.CanvasObjects.Teams
{
	[View ("PlayerView")]
	public class LMPlayerView : PlayerView, ICanvasObjectView<LMPlayerVM>
	{
		static ISurface ArrowOut;
		static ISurface ArrowIn;

		static LMPlayerView ()
		{
			ArrowOut = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.PlayerArrowOut);
			ArrowIn = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.PlayerArrowIn);
		}

		public LMPlayerView ()
		{
			Position = new Point (0, 0);
			DrawPhoto = true;
			Size = (int)PlayersIconSize.Medium;
			Toggle = true;
		}

		public LMPlayerVM ViewModel {
			get {
				return Player as LMPlayerVM;
			}
			set {
				Player = value;
			}
		}

		// FIXME: We keep this to be compatible with View that are not using MVVM yet
		public LMPlayer Model {
			get {
				return ViewModel.Model as LMPlayer;
			}
			set {
				ViewModel = new LMPlayerVM { Model = value };
			}
		}

		public bool SubstitutionMode {
			get;
			set;
		}

		public bool Playing {
			get;
			set;
		}

		public TeamType Team {
			get;
			set;
		}

		Color Color {
			get {
				return Player.Color;
			}
		}

		public void SetViewModel (object viewModel)
		{
			Player = viewModel as LMPlayerVM;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Point zero, p;
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
			tk.TranslateAndScale (Position, new Point (scale, scale));

			if (!UpdateDrawArea (tk, area, new Area (zero, size, size))) {
				tk.End ();
				return;
			}

			/* Background */
			tk.FillColor = App.Current.Style.PaletteBackgroundDark;
			tk.LineWidth = 0;
			tk.DrawRectangle (zero, StyleConf.PlayerSize, StyleConf.PlayerSize);

			/* Image */
			if (Player.Photo != null) {
				tk.DrawImage (zero, size, size, Player.Photo, ScaleMode.AspectFit);
			} else {
				tk.DrawSurface (zero, StyleConf.PlayerSize, StyleConf.PlayerSize, DefaultPhoto, ScaleMode.AspectFit);
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
			tk.FontAlignment = FontAlignment.Center;
			tk.FontWeight = FontWeight.Normal;
			if (ViewModel.Number >= 100) {
				tk.FontSize = 12;
			} else {
				tk.FontSize = 16;
			}
			tk.DrawText (p, StyleConf.PlayerNumberSize, StyleConf.PlayerNumberSize,
						 ViewModel.Number.ToString ());

			if (Active) {
				Color c = Color.Copy ();
				c.A = (byte)(c.A * 60 / 100);
				tk.FillColor = c;
				tk.DrawRectangle (zero, size, size);
			}

			tk.End ();
		}
	}
}
