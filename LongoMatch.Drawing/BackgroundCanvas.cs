//
//  Copyright (C) 2015 Fluendo S.A.
//
//

using System;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Common;

namespace LongoMatch.Drawing
{

	public abstract class BackgroundCanvas: SelectionCanvas
	{
		public event EventHandler RegionOfInterestChanged;

		Image background;
		Area regionOfInterest;

		public BackgroundCanvas (IWidget widget) : base (widget)
		{
		}

		public BackgroundCanvas () : this (null)
		{
		}

		/// <summary>
		/// Sets the background image of the canvas.
		/// This property is not optional
		/// </summary>
		public Image Background {
			set {
				background = value;
				HandleSizeChangedEvent ();
			}
			get {
				return background;
			}
		}

		/// <summary>
		/// Defines an area with the region of interest, which can
		/// be used to zoom into the canvas.
		/// </summary>
		/// <value>The region of interest.</value>
		public Area RegionOfInterest {
			set {
				regionOfInterest = value;
				HandleSizeChangedEvent ();
				widget?.ReDraw ();
				if (RegionOfInterestChanged != null) {
					RegionOfInterestChanged (this, null);
				}
			}
			get {
				return regionOfInterest;
			}
		}

		protected override void HandleSizeChangedEvent ()
		{
			if (background != null) {
				double scaleX, scaleY;
				Point translation;

				/* Add black borders to the canvas to keep the DAR of the background image */
				background.ScaleFactor ((int)widget.Width, (int)widget.Height, ScaleMode.AspectFit,
					out scaleX, out scaleY, out translation);
				ClipRegion = new Area (new Point (translation.X, translation.Y),
					background.Width * scaleX, background.Height * scaleY);
				ScaleX = scaleX;
				ScaleY = scaleY;
				Translation = translation;

				/* If there is a region of interest set, combine the transformation */
				if (RegionOfInterest != null && !RegionOfInterest.Empty) {
					ScaleX *= background.Width / RegionOfInterest.Width;
					ScaleY *= background.Height / RegionOfInterest.Height;
					Translation -= new Point (RegionOfInterest.Start.X * ScaleX,
						RegionOfInterest.Start.Y * ScaleY);
				}
			}
			base.HandleSizeChangedEvent ();
		}

		public override void Draw (IContext context, Area area)
		{
			// Draw the background before any translation or scalling is applied to the canvas.
			tk.Context = context;
			DrawBackground ();

			Begin (context);
			if (Background != null) {
				tk.DrawImage (Background);
			}
			DrawObjects (area);
			End ();
		}
	}
}
