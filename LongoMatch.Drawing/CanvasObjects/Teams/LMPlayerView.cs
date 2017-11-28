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
using VAS.Drawing.CanvasObjects.Teams;
using static LongoMatch.Core.Resources.Styles.Sizes;
using static LongoMatch.Core.Resources.Styles.Colors;

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
			Circular = true;
		}

		public LMPlayerVM ViewModel {
			get {
				return Player as LMPlayerVM;
			}
			set {
				Player = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:LongoMatch.Drawing.CanvasObjects.Teams.LMPlayerView"/>
		/// is rendered in a circular shape.
		/// </summary>
		/// <value><c>true</c> if circular; otherwise, <c>false</c>.</value>
		public bool Circular {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:LongoMatch.Drawing.CanvasObjects.Teams.LMPlayerView"/>
		/// is in substitution mode to render the substitution arrow in prelight
		/// </summary>
		/// <value><c>true</c> if substitution mode; otherwise, <c>false</c>.</value>
		public bool SubstitutionMode {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the team of the player to know it's rendered in the right or left side.
		/// </summary>
		/// <value>The team.</value>
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
			Point arrowLocation;
			ISurface arrow;

			if (Player == null)
				return;

			var zero = new Point (0, 0);
			GetArrowAndLocation (out arrow, out arrowLocation);

			tk.Begin ();
			tk.TranslateAndScale (Position, new Point (1, 1));

			if (!UpdateDrawArea (tk, area, new Area (zero, Size, Size))) {
				tk.End ();
				return;
			}

			// Draw shadow and clip
			if (Circular) {
				tk.FillColor = App.Current.Style.ColorShadow;
				tk.LineWidth = 0;
				tk.DrawCircle (new Point (Size / 2, Size / 2 + 1), Size / 2);
				tk.ClipCircle (new Point (Size / 2, Size / 2), Size / 2);
			} else {
				tk.Clip (new Area (new Point (0, 0), Size, Size));
			}

			/* Background */
			tk.FillColor = PlayerBackground;
			tk.LineWidth = 0;
			tk.DrawRectangle (zero, Size, Size);

			/* Image */
			if (Player.Photo != null) {
				tk.DrawImage (zero, Size, Size, Player.Photo, ScaleMode.AspectFit);
			} else {
				tk.DrawSurface (zero, Size, Size, DefaultPhoto, ScaleMode.AspectFit);
			}

			var numberY = Size * PlayerNumberRelativePosition;

			/* Draw number rectangle */
			var numberStart = new Point (0, numberY);
			var color = Color.Copy ();
			color.SetAlpha (PlayerNumberAlpha);
			tk.FillColor = color;
			tk.DrawRectangle (numberStart, Size, Size / 2);

			/* Draw number */
			tk.FillColor = Color.White;
			tk.StrokeColor = Color.White;
			tk.FontAlignment = FontAlignment.Center;
			tk.FontWeight = FontWeight.Bold;
			if (ViewModel.Number >= 100) {
				tk.FontSize = PlayerSmallFontSize;
			} else {
				tk.FontSize = PlayerFontSize;
			}
			tk.DrawText (numberStart, Size, Size - numberY, ViewModel.Number.ToString ());

			if (Player.Tagged) {
				Color selColor = Color.Copy ();
				selColor.SetAlpha (PlayerSelectionAlpha);
				tk.FillColor = selColor;
				tk.DrawRectangle (zero, Size, Size);
			}

			/* Draw Arrow */
			if (SubstitutionMode && (Highlighted || ViewModel.Tagged)) {
				var arrowSize = Size * PlayerArrowRelativeSize;
				/* Arrow shadow */
				tk.FillColor = Color.White;
				tk.DrawSurface (new Point (arrowLocation.X + 1, arrowLocation.Y + 1), arrowSize, arrowSize, arrow, ScaleMode.AspectFit, true);
				tk.FillColor = Color;
				tk.DrawSurface (arrowLocation, arrowSize, arrowSize, arrow, ScaleMode.AspectFit, true);
			}

			tk.End ();
		}

		void GetArrowAndLocation (out ISurface arrow, out Point arrowLocation)
		{
			arrowLocation = new Point (0, 0);

			if (ViewModel.Playing) {
				arrowLocation.Y = Size * PlayerNumberRelativePosition - Size * PlayerArrowRelativeSize;
			} else {
				arrowLocation.Y = 0;
			}

			if (Team == TeamType.LOCAL && ViewModel.Playing || Team == TeamType.VISITOR && !ViewModel.Playing) {
				arrow = ArrowOut;
				arrowLocation.X = 0;
			} else {
				arrow = ArrowIn;
				arrowLocation.X = Size - Size * PlayerArrowRelativeSize;
			}
		}
	}
}
