//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using Gdk;
using Gtk;
using Cairo;
using Point = LongoMatch.Common.Point;

using LongoMatch.Common;
using LongoMatch.Store;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CoordinatesTagger : Gtk.Bin
	{
		Surface source;
		List<Coordinates> coordinatesList;
		Coordinates selectedCoords;
		Point selectedPoint;
		int sourceWidth, sourceHeight;
		double sourceDAR;
		double xScale, yScale;
		int yOffset, xOffset;
		const double ARROW_DEGREES = 0.5;
		const int ARROW_LENGHT = 3, LINE_WIDTH = 3;
						
		public CoordinatesTagger ()
		{
			Coordinates = new List<Coordinates>();
			this.Build ();
			drawingarea.ExposeEvent += OnDrawingareaExposeEvent;
			drawingarea.ButtonPressEvent += OnDrawingareaButtonPressEvent;
			drawingarea.ButtonReleaseEvent += OnDrawingareaButtonReleaseEvent;
			drawingarea.MotionNotifyEvent += OnDrawingareaMotionNotifyEvent;
			HeightRequest = 100;
			WidthRequest = 100;
			Sensitive = true;
		}
		
		public new bool Sensitive {
			get;
			set;
		}

		public Pixbuf Background {
			set {
				if (value == null || value.Width == -1 || value.Height == -1)
					return;
				sourceWidth = value.Width;
				sourceHeight = value.Height;
				sourceDAR = (double) sourceWidth / sourceHeight;
				source = new ImageSurface(Format.ARGB32,sourceWidth,sourceHeight);
				using(Context sourceCR = new Context(source)) {
					CairoHelper.SetSourcePixbuf(sourceCR,value,0,0);
					sourceCR.Paint();
				}
				QueueDraw();
			}
		}
		
		public List<Coordinates> Coordinates {
			set {
				coordinatesList = value;
				QueueDraw ();
			}
			get {
				return coordinatesList;
			}
		}
		
		double Distance (Point p1, Point p2) {
			double xd = Math.Abs (p1.X - p2.X);
			double yd = Math.Abs (p1.Y - p2.Y);
			return Math.Sqrt (Math.Pow (xd, 2) + Math.Pow (yd, 2));
		}
		
		void TranslateToOriginCoords (Point point) {
			point.X = Math.Max (0, point.X - xOffset);
			point.Y = Math.Max (0, point.Y - yOffset);
			point.X = Math.Min (sourceWidth, (int) (point.X / xScale));
			point.Y = Math.Min (sourceHeight, (int) (point.Y / yScale));
		}
		
		Point TranslateToDestCoords (Point point) {
			return new Point ((int) (point.X * xScale),
			                  (int) (point.Y * yScale));
		}
		
		void FindNearestPoint (Point cursor, out Coordinates coords, out Point point) {
			double minDistance = Int32.MaxValue;
			coords = null;
			point = null;
			
			TranslateToOriginCoords (cursor);
			foreach (Coordinates c in Coordinates) {
				foreach (Point p in c.Points) {
					double dist = Distance (cursor, p);
					if (dist < minDistance) {
						minDistance = dist;
						coords = c;
						point = p;
					}
				}
			}
		}
		
		void SetContextProperties(Context c, bool selected) {
			c.LineCap = LineCap.Round;
			c.LineJoin = LineJoin.Round;
			if (selected) {
				c.Color = new Cairo.Color (255, 0, 0, 1);
			} else {
				c.Color = new Cairo.Color (0, 0, 0, 1);
			}
			c.LineWidth = LINE_WIDTH;
			c.Operator = Operator.Source;
		}
		
		void DrawLine(Context c, Point src, Point dest) {
			c.MoveTo(src.X, src.Y);
			c.LineTo(dest.X, dest.Y);
			c.Stroke();
		}
		
		void DrawPoint (Context c, Point location) {
			c.Arc (location.X, location.Y, LINE_WIDTH, 0, 2 * Math.PI);
			c.StrokePreserve();
			c.Fill();
		}
		
		void DrawArrow(Context c, Point src, Point dest) {
			double vx1,vy1,vx2,vy2;
			double angle = Math.Atan2(dest.Y - src.Y, dest.X - src.X) + Math.PI;

			vx1 = dest.X + (ARROW_LENGHT + LINE_WIDTH) * Math.Cos(angle - ARROW_DEGREES);
			vy1 = dest.Y + (ARROW_LENGHT + LINE_WIDTH) * Math.Sin(angle - ARROW_DEGREES);
			vx2 = dest.X + (ARROW_LENGHT + LINE_WIDTH) * Math.Cos(angle + ARROW_DEGREES);
			vy2 = dest.Y + (ARROW_LENGHT + LINE_WIDTH) * Math.Sin(angle + ARROW_DEGREES);

			c.MoveTo(dest.X, dest.Y);
			c.LineTo(vx1, vy1);
			c.MoveTo(dest.X, dest.Y);
			c.LineTo(vx2, vy2);
			c.Stroke();
			c.Fill();
		}

		
		void DrawCoordinates (Context context, Coordinates coords) {
			SetContextProperties(context, coords == selectedCoords);
			for (int i=0; i < coords.Points.Count; i++) {
				if (i != 0 && i == coords.Points.Count - 1) {
					DrawArrow (context, TranslateToDestCoords(coords.Points[i -1]),
					           TranslateToDestCoords (coords.Points [i]));
				} else {
					DrawPoint (context, TranslateToDestCoords (coords.Points[i]));
				}
				if (i>0) {
					DrawLine (context, TranslateToDestCoords(coords.Points[i-1]),
					          TranslateToDestCoords (coords.Points[i]));
				}
			} 
		}
		
		void RedrawAllCoordinates (Context ctx) {
			foreach (Coordinates c in Coordinates) {
				if (c == selectedCoords)
					continue;
				DrawCoordinates (ctx, c);
			}
		}
		
		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			if (source != null)
				source.Destroy();
		}
		
		protected virtual void OnDrawingareaButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
		{
			if (!Sensitive)
				return;
				
			FindNearestPoint (new Point((int) args.Event.X, (int) args.Event.Y),
			                  out selectedCoords, out selectedPoint);

			QueueDraw ();
		}

		protected virtual void OnDrawingareaButtonReleaseEvent(object o, Gtk.ButtonReleaseEventArgs args)
		{
			if (!Sensitive)
				return;
				
			selectedCoords = null;
			QueueDraw ();
		}
		
		protected virtual void OnDrawingareaMotionNotifyEvent(object o, Gtk.MotionNotifyEventArgs args)
		{
			Point point;
			
			if (!Sensitive)
				return;
				
			if (selectedCoords == null) {
				return;
			}
			point = new Point ((int) args.Event.X, (int) args.Event.Y);
			TranslateToOriginCoords (point);
			selectedPoint.Y = point.Y;
			selectedPoint.X = point.X;
			QueueDraw();
		}
		
		protected virtual void OnDrawingareaExposeEvent(object o, Gtk.ExposeEventArgs args)
		{
			double dar;
			int w,h, destH, destW;
			
			if (source == null) {
				return;
			}
			drawingarea.GdkWindow.Clear();

			using(Context c = CairoHelper.Create(drawingarea.GdkWindow)) {
				w = drawingarea.Allocation.Width;
				h = drawingarea.Allocation.Height;
				dar = (double) w / h;
				
				if (sourceDAR > dar) {
					destW = w;
					destH = (int) (w / sourceDAR);
					xOffset = 0;
					yOffset = (h - destH) / 2;
				} else {
					destH = h;
					destW = (int) (h * sourceDAR);
					xOffset = (w - destW) / 2;
					yOffset = 0;
				}
				c.Translate (xOffset, yOffset);
				xScale = (double) destW / sourceWidth;
				yScale = (double) destH / sourceHeight;
				
				c.Save ();
				c.Scale(xScale, yScale);
				c.SetSourceSurface(source, 0, 0);
				c.Paint();
				c.Restore();
				RedrawAllCoordinates (c);
				if (selectedCoords != null) {
					DrawCoordinates (c, selectedCoords);
				}
			}
		}
	}
}

