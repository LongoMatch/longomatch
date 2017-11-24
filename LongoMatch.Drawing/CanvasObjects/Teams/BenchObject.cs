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
using System.Collections.Generic;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Resources.Styles;
using VAS.Core.Store.Drawables;
using VAS.Drawing.CanvasObjects;
using VASDrawing = VAS.Drawing;

namespace LongoMatch.Drawing.CanvasObjects.Teams
{
	public class BenchObject : CanvasObject, ICanvasSelectableObject
	{
		public BenchObject ()
		{
			BenchPlayers = new List<LMPlayerView> ();
		}

		public List<LMPlayerView> BenchPlayers {
			get;
			set;
		}

		public bool SubstitutionMode {
			get;
			set;
		}

		public Point Position {
			get;
			set;
		}

		public double Width {
			get;
			set;
		}

		public double Height {
			get;
			set;
		}

		public int PlayersPerRow {
			get;
			set;
		}

		public int PlayersSize {
			get;
			set;
		}

		public void Update ()
		{
			if (BenchPlayers == null) {
				return;
			}
			for (int i = 0; i < BenchPlayers.Count; i++) {
				LMPlayerView po;
				double x, y;
				double s = Width / PlayersPerRow;

				x = s * (i % PlayersPerRow) + s / 2;
				y = s * (i / PlayersPerRow) + s / 2;

				po = BenchPlayers [i];
				po.Size = PlayersSize;
				po.Center = new Point (x, y);
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			if (BenchPlayers == null || Position == null) {
				return;
			}
			tk.Begin ();
			tk.TranslateAndScale (Position, new Point (1, 1));
			tk.LineStyle = LineStyle.Dashed;
			tk.LineWidth = Sizes.BenchLineWidth;
			tk.StrokeColor = App.Current.Style.ThemeContrastDisabled;
			tk.FillColor = null;
			tk.DrawRectangle (new Point (0, 0), Width, Height);
			tk.LineStyle = LineStyle.Normal;

			foreach (LMPlayerView po in BenchPlayers) {
				po.SubstitutionMode = SubstitutionMode;
				po.Size = PlayersSize;
				po.Draw (tk, area);
			}

			tk.End ();
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			Selection selection = null;

			if (BenchPlayers == null || Position == null) {
				return selection;
			}

			point = VASDrawing.Utils.ToUserCoords (point, Position, 1, 1);

			foreach (LMPlayerView po in BenchPlayers) {
				selection = po.GetSelection (point, precision);
				if (selection != null)
					break;
			}
			return selection;
		}

		public void Move (Selection s, Point p, Point start)
		{
		}
	}
}

