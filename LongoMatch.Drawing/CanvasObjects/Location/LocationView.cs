//
//  Copyright (C) 2016 Andoni Morales Alastruey
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
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;
using VAS.Drawing.CanvasObjects;
using VASDrawing = VAS.Drawing;

namespace LongoMatch.Drawing.CanvasObjects.Location
{
	/// <summary>
	/// A base class for views displaying locations in a map.
	/// </summary>
	public abstract class LocationView : CanvasObject, ICanvasSelectableObject
	{
		IList<Point> points;
		int backgroundWidth;
		double pointRelativeSize;

		/// <summary>
		/// Gets or sets the color used to draw the point.
		/// </summary>
		/// <value>The color.</value>
		abstract protected Color Color {
			get;
			set;
		}

		/// <summary>
		/// Outline color of the point, can used to change it based in the tagged team.
		/// </summary>
		/// <value>The color of the outline.</value>
		abstract protected Color OutlineColor {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the width of the background, used to denormalize/normalize points.
		/// </summary>
		/// <value>The width of the background.</value>
		public int BackgroundWidth {
			get {
				return backgroundWidth;
			}

			set {
				backgroundWidth = value;
				PointRelativeSize = Math.Max (1, (double)backgroundWidth / 200);
			}
		}

		/// <summary>
		/// Gets or sets the height of the background, used to denormalize/normalize points.
		/// </summary>
		/// <value>The height of the background.</value>
		public int BackgroundHeight {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the field position.
		/// </summary>
		/// <value>The field position.</value>
		public FieldPositionType FieldPosition {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the list of points for the location.
		/// </summary>
		/// <value>The points.</value>
		public IList<Point> Points {
			get {
				return points;
			}

			set {
				points = value;
				UpdateArea ();
			}
		}

		Area Area {
			get;
			set;
		}

		Point Start {
			get {
				return Points [0].Denormalize (BackgroundWidth, BackgroundHeight);
			}
			set {
				Points [0] = value.Normalize (BackgroundWidth, BackgroundHeight);
			}
		}

		Point Stop {
			get {
				return Points [1].Denormalize (BackgroundWidth, BackgroundHeight);
			}
			set {
				Points [1] = value.Normalize (BackgroundWidth, BackgroundHeight);
			}
		}

		double PointRelativeSize {
			get {
				return pointRelativeSize;
			}
			set {
				pointRelativeSize = value;
				UpdateArea ();
			}
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			if (Points == null) {
				return null;
			}

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

			if (Points == null) {
				return;
			}

			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			tk.Begin ();
			fillColor = Color;
			strokeColor = OutlineColor;

			if (Selected) {
				fillColor = VASDrawing.Constants.TAGGER_SELECTION_COLOR;
			} else if (Highlighted) {
				fillColor = App.Current.Style.PaletteActive;
			}

			tk.FillColor = fillColor;
			tk.StrokeColor = strokeColor;
			tk.LineWidth = (int)PointRelativeSize;
			tk.DrawCircle (Start, PointRelativeSize * 1.5);
			if (Points.Count == 2) {
				tk.StrokeColor = fillColor;
				tk.LineWidth = (int)PointRelativeSize;
				tk.DrawLine (Start, Stop);
				tk.DrawArrow (Start, Stop, 10, 0.3, true);
			}
			tk.End ();
		}

		void UpdateArea ()
		{
			if (Points != null) {
				if (Points.Count == 1) {
					Area = new Area (new Point (Start.X - PointRelativeSize * 2, Start.Y - PointRelativeSize * 2),
						PointRelativeSize * 4, PointRelativeSize * 4);
				} else {
					Area a = new Line { Start = Start, Stop = Stop }.Area;
					a.Start.X -= PointRelativeSize * 3;
					a.Start.Y -= PointRelativeSize * 3;
					a.Width += PointRelativeSize * 6;
					a.Height += PointRelativeSize * 6;
					Area = a;
				}
			}
			Area = new Area (new Point (0, 0), 0, 0);
		}
	}
}

