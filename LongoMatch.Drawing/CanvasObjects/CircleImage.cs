//
//  Copyright (C) 2015 Fluendo S.A.
//
//
using System;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Common;

namespace LongoMatch.Drawing.CanvasObjects
{
	/// <summary>
	/// An object that draws circular image image.
	/// </summary>
	public class CircleImage: FixedSizeCanvasObject
	{
		/// <summary>
		/// Gets or sets the image to display.
		/// </summary>
		public Image Image {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the color of the background for the circle. This can be usefull when the image has
		/// transparencies, to draw a circle with this color and achieve the same circular effect.
		/// </summary>
		public Color BackgroundColor {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the backup text used when the image can't be loaded or is empty/null.
		/// </summary>
		public string BackupText {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the color of the text used when the backup character is in use.
		/// </summary>
		/// <value>The color of the text.</value>
		public Color TextColor {
			get;
			set;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			tk.Begin ();
			var center = new Point (Position.X + Width / 2, Position.Y + Height / 2);
			var radius = Math.Min (Width, Height) / 2;

			tk.FillColor = BackgroundColor;
			tk.StrokeColor = BackgroundColor;
			tk.LineWidth = 0;

			if (Image != null) {
				tk.DrawCircleImage (center, radius, Image);
			} else {
				tk.DrawCircle (center, radius);
				if (!String.IsNullOrEmpty (BackupText)) {
					tk.FontSize = (int)(radius * 1.3);
					tk.FontWeight = FontWeight.Bold;
					tk.FontAlignment = FontAlignment.Center;
					tk.StrokeColor = TextColor;
					tk.DrawText (Position, Width, Height, BackupText [0].ToString ());
				}
			}
			tk.End ();
		}
	}
}

