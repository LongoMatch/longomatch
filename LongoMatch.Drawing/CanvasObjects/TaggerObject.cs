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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Core.Store;
using LongoMatch.Core.Handlers;

namespace LongoMatch.Drawing.CanvasObjects
{
	public abstract class TaggerObject: CanvasButtonObject, ICanvasSelectableObject 
	{

		public TaggerObject (DashboardButton tagger)
		{
			Tagger = tagger;
		}

		public DashboardButton Tagger {
			get;
			set;
		}

		public Point Position {
			get {
				if (!Active) {
					return Tagger.Position;
				} else {
					return new Point (Tagger.Position.X + 1, Tagger.Position.Y + 1);
				}
			}
		}

		public Color Color {
			get {
				if (!Active) {
					return Tagger.BackgroundColor;
				} else {
					return Tagger.DarkColor;
				}
			}
		}

		public TagMode Mode {
			get;
			set;
		}

		public virtual int NRows {
			get {
				return 1;
			}
		}

		public Time Start {
			get;
			set;
		}

		public Selection GetSelection (Point p, double precision, bool inMotion=false)
		{
			Selection s;

			Rectangle r = new Rectangle (Tagger.Position, Tagger.Width,
			                             Tagger.Height);
			s = r.GetSelection (p, precision);
			if (s != null) {
				s.Drawable = this;
				if (s.Position != SelectionPosition.BottomRight &&
					s.Position != SelectionPosition.Right &&
					s.Position != SelectionPosition.Bottom) {
					s.Position = SelectionPosition.All;
				}
			}
			return s;
		}

		public void Move (Selection s, Point p, Point start)
		{
			switch (s.Position) {
			case SelectionPosition.Right:
				Tagger.Width = (int)(p.X - Tagger.Position.X);
				Tagger.Width = (int)Math.Max (10, Tagger.Width);
				break;
			case SelectionPosition.Bottom:
				Tagger.Height = (int)(p.Y - Tagger.Position.Y);
				Tagger.Height = (int)Math.Max (10, Tagger.Height);
				break;
			case SelectionPosition.BottomRight:
				Tagger.Width = (int)(p.X - Tagger.Position.X);
				Tagger.Height = (int)(p.Y - Tagger.Position.Y);
				Tagger.Width = Math.Max (10, Tagger.Width);
				Tagger.Height = Math.Max (10, Tagger.Height);
				break;
			case SelectionPosition.All:
				{
					Tagger.Position.X += p.X - start.X;
					Tagger.Position.Y += p.Y - start.Y;
					Tagger.Position.X = Math.Max (Tagger.Position.X, 0);
					Tagger.Position.Y = Math.Max (Tagger.Position.Y, 0);
					break;
				}
			default:
				throw new Exception ("Unsupported move for tagger object:  " + s.Position);
			}
		}

		protected void DrawSelectionArea (IDrawingToolkit tk)
		{
			if (!Selected || Mode != TagMode.Edit) {
				return;
			}
			tk.StrokeColor = Constants.SELECTION_INDICATOR_COLOR;
			tk.StrokeColor = Constants.SELECTION_AREA_COLOR;
			tk.FillColor = null;
			tk.LineStyle = LineStyle.Dashed;
			tk.LineWidth = 1;
			tk.DrawRectangle (Tagger.Position, Tagger.Width, Tagger.Height);

			tk.StrokeColor = tk.FillColor = Constants.SELECTION_INDICATOR_COLOR;
			tk.LineStyle = LineStyle.Normal;
			tk.DrawRectangle (new Point (Tagger.Position.X + Tagger.Width - 3,
			                             Tagger.Position.Y + Tagger.Height - 3),
			                  6, 6);
		}

		protected void DrawButton (IDrawingToolkit tk, bool ignoreActive=false)
		{
			tk.LineWidth = 0;
			if (Active && !ignoreActive) {
				tk.DrawButton (Tagger.Position, Tagger.Width, Tagger.Height, 3, Tagger.BackgroundColor, Tagger.DarkColor);
			} else {
				tk.DrawButton (Tagger.Position, Tagger.Width, Tagger.Height, 3, Tagger.BackgroundColor, Tagger.BackgroundColor);
			}
		}

		public abstract override void Draw (IDrawingToolkit tk, Area area);
	}
}

