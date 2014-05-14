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
using Cairo;
using LongoMatch.Interfaces;
using LongoMatch.Common;
using LColor = LongoMatch.Common.Color;
using Point = LongoMatch.Common.Point;
using Color = Cairo.Color;
using LFontSlant = LongoMatch.Common.FontSlant;
using LFontWeight = LongoMatch.Common.FontWeight;
using FontSlant = Cairo.FontSlant;
using FontWeight = Cairo.FontWeight;

namespace LongoMatch.Drawing.Cairo
{
	public class CairoBackend: IDrawingToolkit
	{
		Context context;
		Color strokeColor;
		Color fillColor;
		FontSlant fSlant;
		FontWeight fWeight;
		
		public CairoBackend ()
		{
			Translation = new Point (0, 0);
			StrokeColor = new LColor (0, 0, 0, 0);
			FillColor = new LColor (0, 0, 0, 0);
			LineWidth = 2;
			FontSize = 12;
			FontFamily = "Verdana";
			FontWeight = LFontWeight.Normal;
			FontSlant = LFontSlant.Normal;
		}
		
		public object Context {
			set {
				context = value as Context;
			}
		}
		
		public int LineWidth {
			set;
			protected get;
		}
		
		public Point Translation {
			set;
			protected get;
		}
		
		public LColor StrokeColor {
			set {
				strokeColor = ColorToCairoColor (value);
			}
		}
		
		public LColor FillColor {
			set {
				fillColor = ColorToCairoColor (value);
			}
		}
		
		public string FontFamily {
			set;
			protected get;
			
		}
		
		public int FontSize {
			set;
			protected get;
		}
		
		public LFontSlant FontSlant {
			set {
				switch (value) {
				case LFontSlant.Italic:
					fSlant = FontSlant.Italic;
					break;
				case LFontSlant.Normal:
					fSlant = FontSlant.Normal;
					break;
				case LFontSlant.Oblique:
					fSlant = FontSlant.Oblique;
					break;
				}
			}
		}
		
		public LFontWeight FontWeight {
			set {
				switch (value) {
				case LFontWeight.Bold:
					fWeight = FontWeight.Bold;
					break;
				case LFontWeight.Normal:
					fWeight = FontWeight.Normal;
					break;
				}
			}
		}
		
		public void Begin() {
			context.Save ();
			context.Translate (Translation.X, Translation.Y);
		}
		
		public void End() {
			context.Restore ();
		}
		
		public void DrawLine (Point start, Point stop) {
			context.Color = strokeColor;
			context.LineWidth = LineWidth;
			context.MoveTo (start.X, start.Y);
			context.LineTo (stop.X, stop.Y);
			context.Stroke();
		}
		
		public void DrawTriangle (Point corner, double width, double height) {
			double x, y;
			
			x = corner.X;
			y = corner.Y;
			context.Color = strokeColor;
			context.MoveTo (x, y);
			context.LineTo (x + width/2, y + height);
			context.LineTo (x - width/2, y - height);
			context.ClosePath();
			context.StrokePreserve ();
			context.Color = fillColor;
			context.Fill();
		}
		
		public void DrawArea (List<Point> vertices) {
			for (int i=0; i < vertices.Count - 1; i++) {
				double x1, y1, x2, y2;
				
				x1 = vertices[i].X;
				y1 = vertices[i].Y;
				x2 = vertices[i+1].X;
				y2 = vertices[i+1].Y;
				
				context.MoveTo (x1, y1);
				context.LineTo (x2, y2);
			}
			context.ClosePath();
			StrokeAndFill ();
		}
		
		public void DrawRectangle (Point start, double width, double height) {
			context.Rectangle (new PointD (start.X + LineWidth / 2, start.Y + LineWidth / 2),
			                   width - LineWidth, height - LineWidth);
			StrokeAndFill ();
		}
		
		public void DrawRoundedRectangle (Point start, double width, double height, double radius) {
			double x, y;
			
			x = start.X + LineWidth / 2;
			y = start.Y + LineWidth / 2;
			height -= LineWidth;
			width -= LineWidth;

			if((radius > height / 2) || (radius > width / 2))
				radius = Math.Min (height / 2, width / 2);

			context.MoveTo (x, y + radius);
			context.Arc (x + radius, y + radius, radius, Math.PI, -Math.PI / 2);
			context.LineTo (x + width - radius, y);
			context.Arc (x + width - radius, y + radius, radius, -Math.PI / 2, 0);
			context.LineTo (x + width, y + height - radius);
			context.Arc (x + width - radius, y + height - radius, radius, 0, Math.PI / 2);
			context.LineTo (x + radius, y + height);
			context.Arc (x + radius, y + height - radius, radius, Math.PI / 2, Math.PI);
			context.ClosePath();
			StrokeAndFill ();
		}

		public void DrawCircle (Point center, double radius) {
			context.MoveTo (center.X, center.Y);
			context.Arc (center.X, center.Y, radius, 0, 2 * Math.PI);
			StrokeAndFill ();
		}

		public void DrawPoint (Point point) {
			DrawCircle (point, LineWidth);
		}
		
		public void DrawText (Point point, double width, double height, string text) {
			TextExtents extents;
			FontExtents fextents;
			double x, y;
			
			context.Color = strokeColor;
			context.SelectFontFace (FontFamily, fSlant, fWeight);
			context.SetFontSize (FontSize);
			extents = context.TextExtents (text);
			fextents = context.FontExtents;
			x = point.X + width / 2 - (extents.Width / 2 + extents.XBearing);
			y = point.Y + height / 2 - (extents.Height / 2 + extents.YBearing);
			context.MoveTo (x, y);
			context.ShowText (text);
		}
		
		public void DrawImage (Point start, double width, double height, Image image) {
		}

		public void DrawEllipse (Point center, double axisX, double axisY) {
		}
		
		void StrokeAndFill () {
			context.LineCap = LineCap.Round;
			context.LineJoin = LineJoin.Round;
			context.LineWidth = LineWidth;
			context.Color = strokeColor;
			context.StrokePreserve();
			context.Color = fillColor;
			context.Fill();
		}
		
		Color ColorToCairoColor (LColor color) {
			return new Color ((double) color.R / ushort.MaxValue,
			                  (double) color.G / ushort.MaxValue,
			                  (double) color.B / ushort.MaxValue,
			                  (double) color.A / ushort.MaxValue);
		}
		
	}
}

