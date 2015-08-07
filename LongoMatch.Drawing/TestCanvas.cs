//
//  Copyright (C) 2015 vguzman
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
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Common;

namespace LongoMatch.Drawing
{
	public class TestCanvas: ICanvas
	{

		protected IDrawingToolkit drawingToolkit;
		protected IWidget widget;
		bool disposed;

		/// <summary>
		/// TestCanvas constructor. Will Draw as a handler of the DrawEvent of the widget.
		/// </summary>
		/// <param name="widget">Widget that will send the DrawEvent</param>
		public TestCanvas (IWidget widget)
		{
			Init ();
			this.widget = widget;
			widget.DrawEvent += Draw;
		}

		/// <summary>
		/// TestCanvas Constructor. Needs to call Draw manually.
		/// </summary>
		public TestCanvas(){
			Init ();
		}

		private void Init(){
			drawingToolkit = Config.DrawingToolkit;
			ScaleX = 1;
			ScaleY = 1;
			Translation = new Point (0, 0);
			TestImage = null;
		}

		~ TestCanvas ()
		{
			if (!disposed) {
				Log.Error (String.Format ("Canvas {0} was not disposed correctly", this));
				Dispose (true);
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				disposed = true;
			}
		}

		/// <summary>
		/// Applied scale on the X axis
		/// </summary>
		protected double ScaleX {
			get;
			set;
		}

		/// <summary>
		/// Applied scale on the Y axis.
		/// </summary>
		protected double ScaleY {
			get;
			set;
		}

		/// <summary>
		/// Applied XY translation.
		/// </summary>
		protected Point Translation {
			get;
			set;
		}

		/// <summary>
		/// Image to draw.
		/// </summary>
		public Image TestImage {
			get;
			set;
		}

		public void Draw (IContext context, Area area)
		{
			drawingToolkit.Context = context;
			DrawGrid (area);
			DrawTexts ();
			if (TestImage != null) {
				DrawImages ();
			}
			DrawShapes ();


		}

		void DrawImages ()
		{
			Point f4c1 = new Point (0, 300);
			Point f4c2 = new Point (100, 300);
			Point f4c3 = new Point (200, 300);
			Point f4c4 = new Point (300, 300);
			Point f4c5 = new Point (400, 300);
			drawingToolkit.FillColor = new Color(255,0,0,255);
			drawingToolkit.DrawRectangle (f4c1, 500, 100);
			drawingToolkit.FillColor = new Color(0,0,255,255);
			drawingToolkit.DrawImage (f4c1, 100, 100, TestImage, false, false);
			drawingToolkit.DrawImage (f4c2, 100, 100, TestImage, true, false);
			drawingToolkit.DrawImage (f4c3, 100, 100, TestImage, false, true);
			drawingToolkit.DrawImage (f4c4, 100, 100, TestImage, true, true);
			drawingToolkit.FillColor = new Color(0,0,255,128);
			drawingToolkit.DrawImage (f4c5, 100, 100, TestImage, true, true);
		}

		void DrawTexts(){
			Point f1c1 = new Point (0, 0);
			Point f1c2 = new Point (100, 0);
			Point f1c3 = new Point (200, 0);
			Point f1c4 = new Point (300, 0);
			Point f2c1 = new Point (0, 100);
			Point f3c1 = new Point (0, 200);

			drawingToolkit.StrokeColor = Color.Black;
			drawingToolkit.FillColor = Color.Black;
			drawingToolkit.DrawRectangle (f1c1, 100, 100);
			drawingToolkit.FillColor = Color.Green;
			drawingToolkit.DrawRectangle (f1c2, 100, 100);
			drawingToolkit.FillColor = Color.Red;
			drawingToolkit.DrawRectangle (f1c3, 100, 100);
			drawingToolkit.FillColor = Color.Yellow;
			drawingToolkit.DrawRectangle (f1c4, 100, 100);
			drawingToolkit.FillColor = Color.Yellow;
			drawingToolkit.DrawRectangle (f2c1, 500, 100);
			drawingToolkit.FillColor = Color.Green;
			drawingToolkit.DrawRectangle (f3c1, 500, 100);
			drawingToolkit.StrokeColor = Color.Black;

			drawingToolkit.StrokeColor = Color.Black;
			drawingToolkit.FontSize = 12;
			drawingToolkit.FontSlant = FontSlant.Normal;
			drawingToolkit.FontWeight = FontWeight.Normal;
			drawingToolkit.FontAlignment = FontAlignment.Center;

			drawingToolkit.DrawText (f1c2, 100, 100, "Esto es un texto jodidamente largo que seguro que no cabe y tendrá que hacer elipsis o lo que quiera que esto haga cuando se pasa de tamaño y no está la elipsis puesta", false, true);
			drawingToolkit.DrawText (f1c3, 100, 100, "Esto \n es \n multilínea con elipsis \n y otra elipsis por este lado", false, true);
			drawingToolkit.DrawText (f2c1, 500, 100, "Esto es un texto jodidamente largo que seguro que no cabe y tendrá que hacer elipsis o lo que quiera que esto haga cuando se pasa de tamaño y no está la elipsis puesta", false, true);
			drawingToolkit.DrawText (f3c1, 500, 100, "Texto", false, true);
			drawingToolkit.StrokeColor = Color.Blue;
			drawingToolkit.DrawText (f3c1, 500, 100, "inicio                    Texto                    fin", false, false);
			drawingToolkit.StrokeColor = Color.Green;
			drawingToolkit.DrawText (f1c1, 100, 100, "Esto es un texto jodidamente largo que seguro que no cabe y tendrá que hacer elipsis o lo que quiera que esto haga cuando se pasa de tamaño y no está la elipsis puesta", false, false);

			drawingToolkit.StrokeColor = Color.Black;
			drawingToolkit.FontSize = 10;
			drawingToolkit.FontSlant = FontSlant.Italic;
			drawingToolkit.FontWeight = FontWeight.Bold;
			drawingToolkit.FontAlignment = FontAlignment.Right;
			drawingToolkit.DrawText (f1c4, 100, 100, "BoldItalic \n right \n and font 10", false, true);
		}

		void DrawShapes(){
			Point newOrigin = new Point (0, 400);
			drawingToolkit.Begin ();
			drawingToolkit.Clip (new Area (newOrigin, 1000, 1000));
			drawingToolkit.TranslateAndScale(newOrigin, new Point(1,1));
			//drawingToolkit.Clear (Color.Black);

			drawingToolkit.StrokeColor = new Color (0, 0, 0, 255);
			drawingToolkit.FillColor = new Color (255, 200, 255, 255);

			drawingToolkit.DrawRectangle (new Point (500, 0), 380, 380);
			drawingToolkit.DrawLine (new Point (0, 0), new Point (760, 760));
			drawingToolkit.DrawLine (new Point (760, 380), new Point (760, 760));
			drawingToolkit.DrawLine (new Point (380, 760), new Point (760, 760));
			drawingToolkit.DrawLine (new Point (380, 0), new Point (760, 380));
			drawingToolkit.DrawLine (new Point (0, 380), new Point (380, 760));
			drawingToolkit.FillColor = new Color (255, 255, 255, 255);
			drawingToolkit.DrawRectangle (new Point (0, 0), 500, 500);
			drawingToolkit.FillColor = new Color (0, 255, 0, 255);    
			drawingToolkit.DrawRoundedRectangle (new Point (0, 0), 500, 500, 100);
			drawingToolkit.FillColor = Color.Black;    
			drawingToolkit.DrawTriangle (new Point (500,760), 100, 100, SelectionPosition.BottomLeft);
			drawingToolkit.FillColor = new Color (255, 0, 0, 255);
			Point[] points = {
				new Point (0, 0),
				new Point (100, 0),
				new Point (100, 100),
				new Point (200, 100),
				new Point (200, 300),
				new Point (54, 186),
				//new Point (0, -100)
			};
			drawingToolkit.DrawArea (points);
			drawingToolkit.FillColor = new Color (0, 0, 255, 255);
			drawingToolkit.DrawCircle (new Point (400, 400), 50);
			drawingToolkit.DrawEllipse (new Point (200, 200), 50, 100);
			drawingToolkit.FillColor = new Color (255, 0, 255, 255);
			drawingToolkit.DrawArrow (new Point (0, 0), 
				new Point (200, 200), 100, 0.3, true);

			drawingToolkit.FillColor = new Color (0, 128, 128, 255);

			drawingToolkit.ClearOperation = true;
			drawingToolkit.DrawRectangle (new Point (400, 0), 100, 100);
			drawingToolkit.ClearOperation = false;
			drawingToolkit.DrawRectangle (new Point (450, 50), 50, 50);



			drawingToolkit.End();
		}

		void DrawGrid(Area area){
			drawingToolkit.LineWidth = 1;
			drawingToolkit.StrokeColor = Color.Green;
			drawingToolkit.FillColor = Color.Grey1;
			drawingToolkit.DrawRectangle (new Point(area.Left,area.Top), area.Width, area.Height);

			for (double i = area.Left; i < area.Right; i+=10) {
				drawingToolkit.DrawLine (new Point (i, area.Top), new Point (i, area.Bottom));
			}

			for (double i = area.Top; i < area.Bottom; i+=10) {
				drawingToolkit.DrawLine (new Point (area.Left, i), new Point (area.Right, i));
			}
		}
	}
}

