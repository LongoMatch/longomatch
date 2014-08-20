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
using Cairo;
using LongoMatch.Common;
using LongoMatch.Interfaces.Drawing;
using Color = LongoMatch.Common.Color;
using FontSlant = Cairo.FontSlant;
using FontWeight = Cairo.FontWeight;
using Image = LongoMatch.Common.Image;
using LFontSlant = LongoMatch.Common.FontSlant;
using LFontWeight = LongoMatch.Common.FontWeight;
using LineStyle = LongoMatch.Common.LineStyle;
using Point = LongoMatch.Common.Point;
using Gdk;

namespace LongoMatch.Drawing.Cairo
{
	public class CairoBackend: IDrawingToolkit
	{
		IContext context;
		Color savedStrokeColor, savedFillColor;
		FontSlant fSlant, savedFSlant;
		FontWeight fWeight, savedFWeight;
		int savedLineWidth, savedFontSize;
		bool savedClear;
		LineStyle savedLineStyle;
		string savedFontFamily;
		bool disableScalling;

		public CairoBackend ()
		{
			StrokeColor = Color.Black;
			FillColor = Color.Black;
			LineWidth = 2;
			FontSize = 12;
			FontFamily = "Verdana";
			FontWeight = LFontWeight.Normal;
			FontSlant = LFontSlant.Normal;
			LineStyle = LineStyle.Normal;
			ClearOperation = false;
		}

		public IContext Context {
			set {
				context = value;
			}
		}

		public int LineWidth {
			set;
			protected get;
		}

		public Color StrokeColor {
			set;
			protected get;
		}

		public Color FillColor {
			set;
			protected get;
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

		public LineStyle LineStyle {
			get;
			set;
		}

		public bool ClearOperation {
			get;
			set;
		}

		public ISurface CreateSurface (string filename)
		{
			return new Surface (filename);
		}

		public ISurface CreateSurface (int width, int height, Image image=null)
		{
			return new Surface (width, height, image);
		}

		public void Clear (Color color)
		{
			SetColor (color);
			CContext.Operator = Operator.Source;
			CContext.Paint ();
			CContext.Operator = Operator.Over;
		}

		public void Begin ()
		{
			savedStrokeColor = StrokeColor;
			savedFillColor = FillColor;
			savedFSlant = fSlant;
			savedFWeight = fWeight;
			savedLineWidth = LineWidth;
			savedFontSize = FontSize;
			savedFontFamily = FontFamily;
			savedLineStyle = LineStyle;
			savedClear = ClearOperation;
			CContext.Save ();
		}

		public void TranslateAndScale (Point translation, Point scale)
		{
			if (!disableScalling) {
				CContext.Translate (translation.X, translation.Y);
				CContext.Scale (scale.X, scale.Y);
			}
		}

		public void End ()
		{
			CContext.Restore ();
			ClearOperation = savedClear;
			StrokeColor = savedStrokeColor;
			FillColor = savedFillColor;
			fSlant = savedFSlant;
			fWeight = savedFWeight;
			LineWidth = savedLineWidth;
			FontSize = savedFontSize;
			FontFamily = savedFontFamily;
			LineStyle = savedLineStyle;
		}

		public void DrawLine (Point start, Point stop)
		{
			CContext.LineWidth = LineWidth;
			CContext.MoveTo (start.X, start.Y);
			CContext.LineTo (stop.X, stop.Y);
			StrokeAndFill ();
		}

		public void DrawTriangle (Point corner, double width, double height,
		                          SelectionPosition position)
		{
			double x1, y1, x2, y2, x3, y3;

			x1 = corner.X;
			y1 = corner.Y;
			
			switch (position) {
			case SelectionPosition.Top:
				x2 = x1 + width / 2;
				y2 = y1 + height;
				x3 = x1 - width / 2;
				y3 = y1 + height;
				break;
			case SelectionPosition.Bottom:
			default:
				x2 = x1 + width / 2;
				y2 = y1 - height;
				x3 = x1 - width / 2;
				y3 = y1 - height;
				break;
			}
			
			SetColor (StrokeColor);
			CContext.MoveTo (x1, y1);
			CContext.LineTo (x2, y2);
			CContext.LineTo (x3, y3);
			CContext.ClosePath ();
			StrokeAndFill ();
		}

		public void DrawArea (params Point[] vertices)
		{
			for (int i=0; i < vertices.Length - 1; i++) {
				double x1, y1, x2, y2;
				
				x1 = vertices [i].X;
				y1 = vertices [i].Y;
				x2 = vertices [i + 1].X;
				y2 = vertices [i + 1].Y;
				
				CContext.MoveTo (x1, y1);
				CContext.LineTo (x2, y2);
			}
			CContext.ClosePath ();
			StrokeAndFill ();
		}

		public void DrawRectangle (Point start, double width, double height)
		{
			DrawRoundedRectangle (start, width, height, 0);
		}

		static public double ByteToDouble (byte val)
		{
			return (double)(val) / byte.MaxValue;
		}

		public static global::Cairo.Color RGBToCairoColor (Color c)
		{
			return new global::Cairo.Color (ByteToDouble (c.R),
			                                ByteToDouble (c.G),
			                                ByteToDouble (c.B));
		}

		public void DrawButton (Point start, double width, double height, double radius, Color startColor, Color stopColor)
		{
			LinearGradient p;
			DrawRoundedRectangle (start, width, height, radius, false);
			p = new LinearGradient (start.X, start.Y, start.X, start.Y + height);
			p.AddColorStop (0, RGBToCairoColor (startColor));
			p.AddColorStop (1, RGBToCairoColor (stopColor));
			CContext.Pattern = p;
			CContext.LineCap = LineCap.Round;
			CContext.LineJoin = LineJoin.Round;
			CContext.LineWidth = LineWidth;
			CContext.FillPreserve ();
			SetColor (StrokeColor);
			CContext.StrokePreserve ();
			CContext.Stroke ();
			p.Dispose ();
		}

		public void DrawRoundedRectangle (Point start, double width, double height, double radius)
		{
			DrawRoundedRectangle (start, width, height, radius, true);
		}

		public void DrawRoundedRectangle (Point start, double width, double height, double radius, bool strokeAndFill)
		{
			double x, y;
			
			x = start.X + LineWidth / 2;
			y = start.Y + LineWidth / 2;
			height -= LineWidth;
			width -= LineWidth;

			if ((radius > height / 2) || (radius > width / 2))
				radius = Math.Min (height / 2, width / 2);

			CContext.MoveTo (x, y + radius);
			CContext.Arc (x + radius, y + radius, radius, Math.PI, -Math.PI / 2);
			CContext.LineTo (x + width - radius, y);
			CContext.Arc (x + width - radius, y + radius, radius, -Math.PI / 2, 0);
			CContext.LineTo (x + width, y + height - radius);
			CContext.Arc (x + width - radius, y + height - radius, radius, 0, Math.PI / 2);
			CContext.LineTo (x + radius, y + height);
			CContext.Arc (x + radius, y + height - radius, radius, Math.PI / 2, Math.PI);
			CContext.ClosePath ();
			if (strokeAndFill) {
				StrokeAndFill ();
			}
		}

		public void DrawCircle (Point center, double radius)
		{
			CContext.Arc (center.X, center.Y, radius, 0, 2 * Math.PI);
			StrokeAndFill ();
		}

		public void DrawPoint (Point point)
		{
			DrawCircle (point, LineWidth);
		}

		public void DrawText (Point point, double width, double height, string text)
		{
			TextExtents extents;
			FontExtents fextents;
			double x, y;
			
			if (text == null) {
				return;
			}
			SetColor (StrokeColor);
			CContext.SelectFontFace (FontFamily, fSlant, fWeight);
			CContext.SetFontSize (FontSize);
			extents = CContext.TextExtents (text);
			fextents = CContext.FontExtents;
			x = point.X + width / 2 - (extents.Width / 2 + extents.XBearing);
			y = point.Y + height / 2 - (extents.Height / 2 + extents.YBearing);
			CContext.MoveTo (x, y);
			CContext.ShowText (text);
			StrokeAndFill ();
		}

		public void DrawImage (Image image)
		{
			CairoHelper.SetSourcePixbuf (CContext, image.Value, 0, 0);
			CContext.Paint ();
		}

		public void DrawImage (Point start, double width, double height, Image image, bool scale)
		{
			double scaleX, scaleY;
			Point offset;
			
			if (scale) {
				image.ScaleFactor ((int)width, (int)height, out scaleX, out scaleY, out offset);
			} else {
				offset = new Point (0, 0);
				scaleX = width / image.Width;
				scaleY = height / image.Height;
			}
			CContext.Save ();
			CContext.Translate (start.X + offset.X, start.Y + offset.Y);
			CContext.Scale (scaleX, scaleY);
			CairoHelper.SetSourcePixbuf (CContext, image.Value, 0, 0);
			CContext.Paint ();
			CContext.Restore ();
		}

		public void DrawEllipse (Point center, double axisX, double axisY)
		{
			double max = Math.Max (axisX, axisY);
			CContext.Save ();
			CContext.Translate (center.X, center.Y);
			CContext.Scale (axisX / max, axisY / max);
			CContext.Arc (0, 0, max, 0, 2 * Math.PI);
			StrokeAndFill ();
			CContext.Restore ();
		}

		public void DrawArrow (Point start, Point stop, int lenght, double radians, bool closed)
		{
			double vx1, vy1, vx2, vy2;
			double angle = Math.Atan2 (stop.Y - start.Y, stop.X - start.X) + Math.PI;

			vx1 = stop.X + (lenght + LineWidth) * Math.Cos (angle - radians);
			vy1 = stop.Y + (lenght + LineWidth) * Math.Sin (angle - radians);
			vx2 = stop.X + (lenght + LineWidth) * Math.Cos (angle + radians);
			vy2 = stop.Y + (lenght + LineWidth) * Math.Sin (angle + radians);

			CContext.MoveTo (stop.X, stop.Y);
			CContext.LineTo (vx1, vy1);
			if (!closed) {
				CContext.MoveTo (stop.X, stop.Y);
				CContext.LineTo (vx2, vy2);
			} else {
				CContext.LineTo (vx2, vy2);
				CContext.ClosePath ();
			}
			StrokeAndFill ();
		}

		public void DrawSurface (ISurface surface, Point p = null)
		{
			ImageSurface image;

			image = surface.Value as ImageSurface;
			if (p == null) {
				CContext.SetSourceSurface (image, 0, 0);
				CContext.Paint ();
			} else {
				CContext.SetSourceSurface (image, (int)p.X, (int)p.Y);
				CContext.Rectangle (p.X, p.Y, image.Width, image.Height);
				CContext.Fill ();
			}
		}

		public Image Copy (ICanvas canvas, double width, double height)
		{
			Image img;
			Pixmap pm;
			
			pm = new Pixmap (null, (int)width, (int)height, 24);
			disableScalling = true;
			using (CairoContext c = new CairoContext (CairoHelper.Create (pm))) {
				canvas.Draw (c, new Area (new Point (0, 0), width, height));
			}
			img = new Image (Gdk.Pixbuf.FromDrawable (pm, Colormap.System, 0, 0, 0, 0,
			                                          (int)width, (int)height));
			disableScalling = false;
			Context = null;
			return img;
		}

		public void Save (ICanvas canvas, double width, double height, string filename)
		{
			ImageSurface pngSurface = new ImageSurface (Format.ARGB32, (int)width, (int)height);
			disableScalling = true;
			using (CairoContext c = new CairoContext (new Context(pngSurface))) {
				canvas.Draw (c, new Area (new Point (0, 0), width, height));
			}
			pngSurface.WriteToPng (filename);
			disableScalling = false;
			pngSurface.Dispose ();
		}

		Context CContext {
			get {
				return context.Value as Context;
			}
		}

		void SetDash ()
		{
			switch (LineStyle) {
			case LineStyle.Normal:
				CContext.SetDash (new double[] { }, 0);
				break;	
			default:
				CContext.SetDash (new double[] { 10, 10 }, 10);
				break;
			}
		}

		void StrokeAndFill ()
		{
			SetDash ();
			if (ClearOperation) {
				CContext.Operator = Operator.Clear;
			} else {
				CContext.Operator = Operator.Over;
			}
			CContext.LineCap = LineCap.Round;
			CContext.LineJoin = LineJoin.Round;
			CContext.LineWidth = LineWidth;
			SetColor (StrokeColor);
			CContext.StrokePreserve ();
			SetColor (FillColor);
			CContext.Fill ();
		}

		void SetColor (Color color)
		{
			if (color != null) {
				CContext.SetSourceRGBA ((double)color.R / byte.MaxValue,
				                        (double)color.G / byte.MaxValue,
				                        (double)color.B / byte.MaxValue,
				                        (double)color.A / byte.MaxValue);
			} else {
				CContext.SetSourceRGBA (0, 0, 0, 0);
			}
		}
	}
}

