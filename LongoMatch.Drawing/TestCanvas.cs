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
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Drawing;

namespace LongoMatch.Drawing
{
	public class TestCanvas: Canvas
	{
		const int CANVAS_WIDTH = 1000;
		const int CANVAS_HEIGHT = 1200;

		string serializedImage = "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";

		/// <summary>
		/// TestCanvas constructor. Will Draw as a handler of the DrawEvent of the widget.
		/// </summary>
		/// <param name="widget">Widget that will send the DrawEvent</param>
		public TestCanvas (IWidget widget) : base (widget)
		{
			TestImage = Image.Deserialize (Convert.FromBase64String (serializedImage));
		}

		/// <summary>
		/// Image to draw.
		/// </summary>
		public Image TestImage {
			get;
			set;
		}

		public override void Draw (IContext context, Area area)
		{
			if (tk != null) {
				Scale ();
				Begin (context);
				DrawGrid (new Area (new Point (0, 0), CANVAS_WIDTH, CANVAS_HEIGHT));
				DrawTexts ();
				if (TestImage != null) {
					DrawImages ();
					DrawSurface ();
				}
				DrawShapes ();
				DrawClipped ();
				End ();
			}
		}

		void Scale ()
		{
			double scaleX, scaleY;
			Point translation;

			/* Scale the canvas to fit in the widget kepping DAR */
			Image.ScaleFactor (CANVAS_WIDTH, CANVAS_HEIGHT, (int)widget.Width, (int)widget.Height,
				ScaleMode.AspectFit, out scaleX, out scaleY, out translation);
			ClipRegion = new Area (new Point (translation.X, translation.Y),
				CANVAS_WIDTH * scaleX, CANVAS_HEIGHT * scaleY);
			ScaleX = scaleX;
			ScaleY = scaleY;
			Translation = translation;
		}

		void DrawImages ()
		{
			Point f5c1 = new Point (0, 400);
			Point f5c2 = new Point (100, 400);
			Point f5c3 = new Point (200, 400);
			Point f5c4 = new Point (300, 400);
			Point f5c5 = new Point (400, 400);
			Point f5c6 = new Point (500, 400);
			tk.FillColor = new Color (255, 0, 0, 255);
			tk.DrawRectangle (f5c1, 500, 100);
			tk.FillColor = new Color (0, 0, 255, 255);
			tk.DrawImage (f5c1, 100, 100, TestImage, ScaleMode.Fill, false);
			tk.DrawImage (f5c2, 100, 100, TestImage, ScaleMode.AspectFit, false);
			tk.DrawImage (f5c3, 100, 100, TestImage, ScaleMode.Fill, true);
			tk.DrawImage (f5c4, 100, 100, TestImage, ScaleMode.AspectFit, true);
			tk.FillColor = new Color (0, 0, 255, 128);
			tk.DrawImage (f5c5, 100, 100, TestImage, ScaleMode.AspectFit, true);
			tk.FillColor = new Color (0, 0, 255, 255);
			tk.Begin ();

			double scaleX, scaleY;
			Point offset;
			TestImage.ScaleFactor (100, 100, ScaleMode.AspectFit, out scaleX, out scaleY, out offset);
			tk.TranslateAndScale (f5c6 + offset, new Point (scaleX, scaleY));
			tk.DrawImage (TestImage);

			tk.End ();

		}

		void DrawSurface ()
		{
			tk.Begin ();

			tk.StrokeColor = tk.FillColor = Color.Blue1;
			tk.DrawRectangle (new Point (400, 200), 300, 200);

			IContext oldContext = tk.Context;
			ISurface surface = tk.CreateSurface (200, 200, TestImage);
			using (IContext surfaceContext = surface.Context) {
				tk.Context = surfaceContext;
				tk.StrokeColor = tk.FillColor = Color.Black;
				tk.FontSize = 16;
				tk.DrawText (new Point (10, 90), 180, 20, "This is a surface");
				tk.FillColor = new Color (0, 0, 0, 0);
				tk.StrokeColor = Color.Blue;
				tk.DrawRectangle (new Point (0, 0), 198, 198);
				tk.Context = oldContext;
				tk.End ();
				tk.DrawSurface (surface, new Point (500, 200));
			}


			tk.Begin ();
			tk.TranslateAndScale (new Point (400, 200), new Point (0.5, 0.5));
			tk.DrawSurface (surface);
			tk.End ();
			surface.Dispose ();
		}

		void DrawTexts ()
		{
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

			tk.StrokeColor = Color.Black;
			tk.FillColor = Color.Blue1;
			tk.DrawRectangle (f1c1, 100, 100);
			tk.FillColor = Color.Green;
			tk.DrawRectangle (f1c2, 100, 100);
			tk.FillColor = Color.Red;
			tk.DrawRectangle (f1c3, 100, 100);
			tk.FillColor = Color.Yellow;
			tk.DrawRectangle (f1c4, 100, 100);
			tk.FillColor = Color.White;
			tk.DrawRectangle (f1c5, 100, 100);
			tk.DrawRectangle (f1c6, 100, 100);
			tk.FillColor = Color.Yellow;
			tk.DrawRectangle (f2c1, 400, 100);
			tk.FillColor = Color.Green;
			tk.DrawRectangle (f3c1, 400, 100);
			tk.StrokeColor = Color.Black;

			tk.StrokeColor = Color.Black;
			tk.FontSize = 12;
			tk.FontSlant = FontSlant.Normal;
			tk.FontWeight = FontWeight.Normal;
			tk.FontAlignment = FontAlignment.Center;

			tk.FontAlignment = FontAlignment.Left;
			tk.DrawText (f1c1, 100, 100, shortText, false, false);
			tk.FontAlignment = FontAlignment.Center;
			tk.DrawText (f1c2, 100, 100, shortText, false, false);
			tk.FontAlignment = FontAlignment.Right;
			tk.DrawText (f1c3, 100, 100, shortText, false, false);
			tk.FontAlignment = FontAlignment.Center;
			tk.DrawText (f1c5, 100, 100, longText, false, false);
			tk.DrawText (f1c6, 100, 100, longText, false, true);
			tk.FontAlignment = FontAlignment.Center;
			tk.StrokeColor = Color.Black;
			tk.FontSize = 10;
			tk.FontSlant = FontSlant.Italic;
			tk.FontWeight = FontWeight.Bold;
			tk.DrawText (f1c4, 100, 100, shortTextMulti, false, false);
			tk.FontAlignment = FontAlignment.Left;
			tk.FontSlant = FontSlant.Italic;
			tk.FillColor = Color.White;
			tk.FontSize = 14;
			tk.DrawText (f2c1, 400, 100, longText, false, true);
			tk.FontSlant = FontSlant.Normal;
			tk.FillColor = Color.Blue;
			tk.DrawText (f3c1, 400, 100, longTextMulti, false, true);



			int width, height;
			tk.MeasureText (shortText, out width, out height, "Arial", 14, FontWeight.Bold);
			tk.StrokeColor = tk.FillColor = Color.White;
			f4c1.X += (400 - width) / 2;
			tk.DrawRectangle (f4c1, width, height);

			tk.StrokeColor = tk.FillColor = Color.Blue;
			tk.FontFamily = "Arial";
			tk.FontSize = 14;
			tk.FontWeight = FontWeight.Bold;
			f4c1.X += 1;
			f4c1.Y += 1;
			tk.DrawText (f4c1, width, height, shortText);

		}

		void DrawShapes ()
		{
			Point newOrigin = new Point (0, 500);
			tk.Begin ();
			tk.TranslateAndScale (newOrigin, new Point (1, 1));

			tk.StrokeColor = new Color (0, 0, 0, 255);
			tk.FillColor = new Color (255, 200, 255, 255);

			tk.DrawLine (new Point (0, 0), new Point (760, 760));
			tk.DrawLine (new Point (760, 380), new Point (760, 760));
			tk.DrawLine (new Point (380, 760), new Point (760, 760));
			tk.DrawLine (new Point (380, 0), new Point (760, 380));
			tk.DrawLine (new Point (0, 380), new Point (380, 760));
			tk.FillColor = new Color (255, 255, 255, 255);
			tk.DrawRectangle (new Point (0, 0), 500, 500);
			tk.FillColor = new Color (0, 255, 0, 255);    
			tk.DrawRoundedRectangle (new Point (0, 0), 500, 500, 100);


			tk.FillColor = new Color (255, 0, 0, 255);
			tk.StrokeColor = Color.Black;    
			Point[] points = {
				new Point (0, 0),
				new Point (100, 0),
				new Point (100, 100),
				new Point (200, 100),
				new Point (200, 300),
				new Point (54, 186),
			};
			tk.DrawArea (points);
			tk.FillColor = new Color (0, 0, 255, 255);
			tk.DrawCircle (new Point (400, 400), 50);
			tk.DrawEllipse (new Point (200, 200), 50, 100);
			tk.FillColor = new Color (255, 0, 255, 255);
			tk.DrawArrow (new Point (0, 0), 
				new Point (200, 200), 100, 0.3, true);

			tk.FillColor = new Color (0, 128, 128, 255);

			tk.ClearOperation = true;
			tk.DrawRectangle (new Point (400, 0), 100, 100);
			tk.ClearOperation = false;
			tk.DrawRectangle (new Point (450, 50), 50, 50);

			tk.FillColor = Color.Blue1;    
			tk.StrokeColor = Color.White;    
			tk.DrawTriangle (new Point (300, 0), 100, 100, SelectionPosition.Top);

			tk.StrokeColor = tk.FillColor = Color.Red;
			// < 2 won't appear in android O_O
			tk.LineWidth = 2;
			tk.DrawPoint (new Point (250, 50));
		
			tk.StrokeColor = tk.FillColor = Color.Black;
			Area transformed = tk.UserToDevice (new Area (new Point (0, 0), 500, 500));
			tk.DrawText (new Point (0, 150), 500, 350, "This rectangle is drawn at (0,0) but the real coords are " + transformed.TopLeft);

			tk.End ();
		}

		void DrawClipped ()
		{
			tk.Begin ();
			Area clipArea = new Area (new Point (600, 0), 168, 200);
			tk.Clip (clipArea);
			tk.Clear (Color.Red1);

			tk.FillColor = Color.Green1;
			Point[] points = {
				new Point (550, 100),
				new Point (700, -100),
				new Point (800, 100),
				new Point (700, 300),
			};
			tk.DrawArea (points);

			tk.End ();

		}

		void Save ()
		{
			Area toSave = new Area (new Point (0, 0), 200, 200);
			var otherCanvas = new DummyCanvas (widget);
			tk.Save (otherCanvas, toSave, "testCanvas.png");
		}

		void Copy ()
		{
			Area toSave = new Area (new Point (0, 0), 200, 200);
			var otherCanvas = new DummyCanvas (widget);
			Image copied = tk.Copy (otherCanvas, toSave);
			tk.DrawImage (new Point (600, 400), 200, 200, copied, ScaleMode.AspectFill);

		}

		void DrawGrid (Area area)
		{
			tk.LineWidth = 1;
			tk.StrokeColor = Color.Green;
			tk.FillColor = Color.Grey1;
			tk.Clear (Color.Grey1);

			for (double i = area.Left; i < area.Right; i += 10) {
				tk.DrawLine (new Point (i, area.Top), new Point (i, area.Bottom));
			}

			for (double i = area.Top; i < area.Bottom; i += 10) {
				tk.DrawLine (new Point (area.Left, i), new Point (area.Right, i));
			}
		}

		class DummyCanvas:Canvas
		{
			public DummyCanvas (IWidget widget) : base (widget)
			{
			}

			#region ICanvas implementation

			public override void Draw (IContext context, Area area)
			{
				IDrawingToolkit dt = App.Current.DrawingToolkit;
				IContext oldcontext = dt.Context;
				dt.Context = context;
				dt.DrawCircle (new Point (0, 0), 50);

				dt.Context = oldcontext;

			}

			#endregion
			
		}
	}
}

