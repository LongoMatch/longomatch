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
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;
using VAS.Drawing.CanvasObjects;
using VASDrawing = VAS.Drawing;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class PositionObject:  CanvasObject, ICanvasSelectableObject
	{

		public PositionObject (IList<Point> points, int width, int height)
		{
			Points = points;
			Width = width;
			Height = height;
		}

		public override string Description {
			get {
				if (Play != null) {
					return Play.Name;
				}
				return null;
			}
		}

		public int Width {
			get;
			set;
		}

		public int Height {
			get;
			set;
		}

		public ProjectLongoMatch Project {
			get;
			set;
		}

		public TimelineEventLongoMatch Play {
			get;
			set;
		}

		public IList<Point> Points {
			get;
			set;
		}

		Point Start {
			get {
				return Points [0].Denormalize (Width, Height);
			}
			set {
				Points [0] = value.Normalize (Width, Height);
			}
		}

		Point Stop {
			get {
				return Points [1].Denormalize (Width, Height);
			}
			set {
				Points [1] = value.Normalize (Width, Height);
			}
		}

		Area GetArea (double relSize)
		{
			if (Points != null) {
				if (Points.Count == 1) {
					return new Area (new Point (Start.X - relSize * 2, Start.Y - relSize * 2),
						relSize * 4, relSize * 4);
				} else {
					Area a = new Line { Start = Start, Stop = Stop }.Area;
					a.Start.X -= relSize * 3;
					a.Start.Y -= relSize * 3;
					a.Width += relSize * 6;
					a.Height += relSize * 6;
					return a;
				}
			}
			return new Area (new Point (0, 0), 0, 0);
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			if (point.Distance (Start) < precision) {
				return new Selection (this, SelectionPosition.LineStart);
			} else if (Points.Count == 2 && point.Distance (Stop) < precision) {
				return new Selection (this, SelectionPosition.LineStop);
			}
			return null;
		}

		public void Move (Selection sel, Point p, Point start)
		{
			switch (sel.Position) {
			case SelectionPosition.LineStart:
				Start = new Point (Math.Max (p.X, 0), Math.Max (p.Y, 0));
				break;
			case SelectionPosition.LineStop:
				Stop = new Point (Math.Max (p.X, 0), Math.Max (p.Y, 0));
				break;
			default:
				throw new Exception ("Unsupported move for circle:  " + sel.Position);
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Color fillColor, strokeColor;
			double relSize;

			relSize = Math.Max (1, (double)Width / 200);

			if (!UpdateDrawArea (tk, area, GetArea (relSize))) {
				return;
			}

			tk.Begin ();
			if (Play != null) {
				fillColor = Play.Color;
				strokeColor = fillColor;
				if (Play.Teams.Count == 1) {
					strokeColor = Play.Teams.First ().Color;
				}
			} else {
				fillColor = strokeColor = VASDrawing.Constants.TAGGER_POINT_COLOR;
			}
			
			if (Selected) {
				fillColor = VASDrawing.Constants.TAGGER_SELECTION_COLOR;
			} else if (Highlighted) {
				fillColor = App.Current.Style.PaletteActive;
			}

			tk.FillColor = fillColor;
			tk.StrokeColor = strokeColor;
			tk.LineWidth = (int)relSize;
			tk.DrawCircle (Start, relSize * 1.5);
			if (Points.Count == 2) {
				tk.StrokeColor = fillColor;
				tk.LineWidth = (int)relSize;
				tk.DrawLine (Start, Stop);
				tk.DrawArrow (Start, Stop, 10, 0.3, true);
			}
			tk.End ();
		}
	}
}

