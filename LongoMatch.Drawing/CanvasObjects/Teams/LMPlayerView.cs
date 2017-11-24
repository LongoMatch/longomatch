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
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Resources;
using VAS.Core.Resources.Styles;
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
			ArrowOut = App.Current.DrawingToolkit.CreateSurfaceFromResource (Images.PlayerArrowOut);
			ArrowIn = App.Current.DrawingToolkit.CreateSurfaceFromResource (Images.PlayerArrowIn);
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

		public bool SubstitutionMode {
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
			size = Sizes.PlayerSize;
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
			tk.FillColor = App.Current.Style.ThemeBase;
			tk.LineWidth = 0;
			tk.DrawRectangle (zero, Sizes.PlayerSize, Sizes.PlayerSize);

			/* Image */
			if (Player.Photo != null) {
				tk.DrawImage (zero, size, size, Player.Photo, ScaleMode.AspectFit);
			} else {
				tk.DrawSurface (zero, Sizes.PlayerSize, Sizes.PlayerSize, DefaultPhoto, ScaleMode.AspectFit);
			}

			/* Bottom line */
			p = new Point (0, size - Sizes.PlayerLineWidth);
			tk.FillColor = Color;
			tk.DrawRectangle (p, size, 3);

			/* Draw Arrow */
			if (SubstitutionMode && (Highlighted || Player.Tagged)) {
				ISurface arrow;
				Point ap;

				if (ViewModel.Playing) {
					arrow = arrowout;
				} else {
					arrow = arrowin;
				}
				ap = new Point (Sizes.PlayerArrowX, Sizes.PlayerArrowY);
				tk.DrawRectangle (ap, Sizes.PlayerArrowSize, Sizes.PlayerArrowSize);
				tk.DrawSurface (arrow, ap);
			}

			/* Draw number */
			p = new Point (Sizes.PlayerNumberX, Sizes.PlayerNumberY);
			tk.FillColor = Color;
			tk.DrawRectangle (p, Sizes.PlayerNumberSize, Sizes.PlayerNumberSize);

			tk.FillColor = Color.White;
			tk.StrokeColor = Color.White;
			tk.FontAlignment = FontAlignment.Center;
			tk.FontWeight = FontWeight.Normal;
			if (ViewModel.Number >= 100) {
				tk.FontSize = 12;
			} else {
				tk.FontSize = 16;
			}
			tk.DrawText (p, Sizes.PlayerNumberSize, Sizes.PlayerNumberSize,
						 ViewModel.Number.ToString ());

			if (Player.Tagged && !SubstitutionMode) {
				Color c = Color.Copy ();
				c.A = (byte)(c.A * 60 / 100);
				tk.FillColor = c;
				tk.DrawRectangle (zero, size, size);
			}

			tk.End ();
		}
	}
}
