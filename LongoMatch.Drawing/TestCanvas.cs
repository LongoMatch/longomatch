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
		string serializedImage = "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";


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
			TestImage = Image.Deserialize (Convert.FromBase64String(serializedImage));
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
			if (drawingToolkit != null) {
				drawingToolkit.Context = context;

				drawingToolkit.Begin ();
				drawingToolkit.Begin ();
				drawingToolkit.End ();
				drawingToolkit.End ();

				DrawGrid (area);
				DrawTexts ();
				if (TestImage != null) {
					DrawImages ();
					DrawSurface ();
				}
				DrawShapes ();
				DrawClipped ();
			}
		}



		void DrawImages ()
		{
			Point f5c1 = new Point (0, 400);
			Point f5c2 = new Point (100, 400);
			Point f5c3 = new Point (200, 400);
			Point f5c4 = new Point (300, 400);
			Point f5c5 = new Point (400, 400);
			Point f5c6 = new Point (500, 400);
			drawingToolkit.FillColor = new Color(255,0,0,255);
			drawingToolkit.DrawRectangle (f5c1, 500, 100);
			drawingToolkit.FillColor = new Color(0,0,255,255);
			drawingToolkit.DrawImage (f5c1, 100, 100, TestImage, false, false);
			drawingToolkit.DrawImage (f5c2, 100, 100, TestImage, true, false);
			drawingToolkit.DrawImage (f5c3, 100, 100, TestImage, false, true);
			drawingToolkit.DrawImage (f5c4, 100, 100, TestImage, true, true);
			drawingToolkit.FillColor = new Color(0,0,255,128);
			drawingToolkit.DrawImage (f5c5, 100, 100, TestImage, true, true);
			drawingToolkit.FillColor = new Color(0,0,255,255);
			drawingToolkit.Begin ();

			double scaleX, scaleY;
			LongoMatch.Core.Common.Point offset;
			TestImage.ScaleFactor(100,100,out scaleX, out scaleY, out offset);
			drawingToolkit.TranslateAndScale (f5c6 + offset, new Point (scaleX, scaleY));
			drawingToolkit.DrawImage (TestImage);

			drawingToolkit.End ();

		}

		void DrawSurface(){
			drawingToolkit.Begin ();

			drawingToolkit.StrokeColor = drawingToolkit.FillColor = Color.Blue1;
			drawingToolkit.DrawRectangle (new Point (400, 200), 300, 200);

			IContext oldContext = drawingToolkit.Context;
			ISurface surface = drawingToolkit.CreateSurface (200, 200, TestImage);
			drawingToolkit.Context = surface.Context;
			drawingToolkit.StrokeColor = drawingToolkit.FillColor = Color.Black;
			drawingToolkit.FontSize = 16;
			drawingToolkit.DrawText (new Point (10, 90), 180, 20, "This is a surface");
			drawingToolkit.FillColor = new Color (0, 0, 0, 0);
			drawingToolkit.StrokeColor = Color.Blue;
			drawingToolkit.DrawRectangle (new Point (0, 0), 198, 198);
			drawingToolkit.Context = oldContext;
			drawingToolkit.End ();
			drawingToolkit.DrawSurface (surface, new Point (500, 200));

			drawingToolkit.Begin ();
			drawingToolkit.TranslateAndScale (new Point (400, 200), new Point (0.5, 0.5));
			drawingToolkit.DrawSurface (surface);
			drawingToolkit.End ();
		}

		void DrawTexts(){
			string longText = "Ellipsis (plural ellipses; from the Ancient Greek: ἔλλειψις, élleipsis, \"omission\" or \"falling short\") is a series of dots that usually indicates an intentional omission of a word, sentence, or whole section from a text without altering its original meaning.";
			string longTextMulti = "Ellipsis (plural ellipses;\nfrom the Ancient Greek: ἔλλειψις, élleipsis,\n\"omission\" or \"falling short\")\nis a series of dots that usually\n indicates an intentional omission of a word,\nsentence, or whole section from a text\nwithout altering its original meaning.";
			string shortText = "This is a short text with arial bold in a measured rectangle";
			string shortTextMulti = "This is \n a short \n text";

			Point f1c1 = new Point (0, 0);
			Point f1c2 = new Point (100, 0);
			Point f1c3 = new Point (200, 0);
			Point f1c4 = new Point (300, 0);
			Point f1c5 = new Point (400, 0);
			Point f1c6 = new Point (500, 0);
			Point f2c1 = new Point (0, 100);
			Point f3c1 = new Point (0, 200);
			Point f4c1 = new Point (0, 300);

			drawingToolkit.StrokeColor = Color.Black;
			drawingToolkit.FillColor = Color.Blue1;
			drawingToolkit.DrawRectangle (f1c1, 100, 100);
			drawingToolkit.FillColor = Color.Green;
			drawingToolkit.DrawRectangle (f1c2, 100, 100);
			drawingToolkit.FillColor = Color.Red;
			drawingToolkit.DrawRectangle (f1c3, 100, 100);
			drawingToolkit.FillColor = Color.Yellow;
			drawingToolkit.DrawRectangle (f1c4, 100, 100);
			drawingToolkit.FillColor = Color.White;
			drawingToolkit.DrawRectangle (f1c5, 100, 100);
			drawingToolkit.DrawRectangle (f1c6, 100, 100);
			drawingToolkit.FillColor = Color.Yellow;
			drawingToolkit.DrawRectangle (f2c1, 400, 100);
			drawingToolkit.FillColor = Color.Green;
			drawingToolkit.DrawRectangle (f3c1, 400, 100);
			drawingToolkit.StrokeColor = Color.Black;

			drawingToolkit.StrokeColor = Color.Black;
			drawingToolkit.FontSize = 12;
			drawingToolkit.FontSlant = FontSlant.Normal;
			drawingToolkit.FontWeight = FontWeight.Normal;
			drawingToolkit.FontAlignment = FontAlignment.Center;

			drawingToolkit.FontAlignment = FontAlignment.Left;
			drawingToolkit.DrawText (f1c1, 100, 100, shortText, false, false);
			drawingToolkit.FontAlignment = FontAlignment.Center;
			drawingToolkit.DrawText (f1c2, 100, 100, shortText, false, false);
			drawingToolkit.FontAlignment = FontAlignment.Right;
			drawingToolkit.DrawText (f1c3, 100, 100, shortText, false, false);
			drawingToolkit.FontAlignment = FontAlignment.Center;
			drawingToolkit.DrawText (f1c5, 100, 100, longText, false, false);
			drawingToolkit.DrawText (f1c6, 100, 100, longText, false, true);
			drawingToolkit.FontAlignment = FontAlignment.Center;
			drawingToolkit.StrokeColor = Color.Black;
			drawingToolkit.FontSize = 10;
			drawingToolkit.FontSlant = FontSlant.Italic;
			drawingToolkit.FontWeight = FontWeight.Bold;
			drawingToolkit.DrawText (f1c4, 100, 100, shortTextMulti, false, false);
			drawingToolkit.FontAlignment = FontAlignment.Left;
			drawingToolkit.FontSlant = FontSlant.Italic;
			drawingToolkit.FillColor = Color.White;
			drawingToolkit.FontSize = 14;
			drawingToolkit.DrawText (f2c1, 400, 100, longText, false, true);
			drawingToolkit.FontSlant = FontSlant.Normal;
			drawingToolkit.FillColor = Color.Blue;
			drawingToolkit.DrawText (f3c1, 400, 100, longTextMulti, false, true);



			int width, height;
			drawingToolkit.MeasureText (shortText, out width, out height, "Arial", 14, FontWeight.Bold);
			drawingToolkit.StrokeColor = drawingToolkit.FillColor = Color.White;
			f4c1.X += (400 - width) / 2;
			drawingToolkit.DrawRectangle (f4c1, width, height);

			drawingToolkit.StrokeColor = drawingToolkit.FillColor = Color.Blue;
			drawingToolkit.FontFamily = "Arial";
			drawingToolkit.FontSize = 14;
			drawingToolkit.FontWeight = FontWeight.Bold;
			f4c1.X += 1;
			f4c1.Y += 1;
			drawingToolkit.DrawText (f4c1, width, height, shortText);

		}

		void DrawShapes(){
			Point newOrigin = new Point (0, 500);
			drawingToolkit.Begin ();
			drawingToolkit.TranslateAndScale(newOrigin, new Point(1,1));

			drawingToolkit.StrokeColor = new Color (0, 0, 0, 255);
			drawingToolkit.FillColor = new Color (255, 200, 255, 255);

			drawingToolkit.DrawLine (new Point (0, 0), new Point (760, 760));
			drawingToolkit.DrawLine (new Point (760, 380), new Point (760, 760));
			drawingToolkit.DrawLine (new Point (380, 760), new Point (760, 760));
			drawingToolkit.DrawLine (new Point (380, 0), new Point (760, 380));
			drawingToolkit.DrawLine (new Point (0, 380), new Point (380, 760));
			drawingToolkit.FillColor = new Color (255, 255, 255, 255);
			drawingToolkit.DrawRectangle (new Point (0, 0), 500, 500);
			drawingToolkit.FillColor = new Color (0, 255, 0, 255);    
			drawingToolkit.DrawRoundedRectangle (new Point (0, 0), 500, 500, 100);

			drawingToolkit.FillColor = new Color (255, 0, 0, 255);
			drawingToolkit.StrokeColor = Color.Black;    
			Point[] points = {
				new Point (0, 0),
				new Point (100, 0),
				new Point (100, 100),
				new Point (200, 100),
				new Point (200, 300),
				new Point (54, 186),
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

			drawingToolkit.FillColor = Color.Blue1;    
			drawingToolkit.StrokeColor = Color.White;    
			drawingToolkit.DrawTriangle (new Point (300,0), 100, 100, SelectionPosition.Top);

			drawingToolkit.StrokeColor = drawingToolkit.FillColor = Color.Red;
			// < 2 won't appear in android O_O
			drawingToolkit.LineWidth = 2;
			drawingToolkit.DrawPoint (new Point (250,50));
		

			drawingToolkit.End();
		}

		void DrawClipped(){
			drawingToolkit.Begin ();
			Area clipArea = new Area (new Point (600, 0), 168, 200);
			drawingToolkit.Clip (clipArea);
			drawingToolkit.Clear (Color.Red1);

			drawingToolkit.FillColor = Color.Green1;
			Point[] points = {
				new Point (550, 100),
				new Point (700, -100),
				new Point (800, 100),
				new Point (700, 300),
			};
			drawingToolkit.DrawArea (points);

			drawingToolkit.End ();

		}

		void DrawGrid(Area area){
			drawingToolkit.LineWidth = 1;
			drawingToolkit.StrokeColor = Color.Green;
			drawingToolkit.FillColor = Color.Grey1;
			drawingToolkit.Clear(Color.Grey1);

			for (double i = area.Left; i < area.Right; i+=10) {
				drawingToolkit.DrawLine (new Point (i, area.Top), new Point (i, area.Bottom));
			}

			for (double i = area.Top; i < area.Bottom; i+=10) {
				drawingToolkit.DrawLine (new Point (area.Left, i), new Point (area.Right, i));
			}
		}
	}
}

