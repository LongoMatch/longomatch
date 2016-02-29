//
//  Copyright (C) 2015 Fluendo S.A.
//
//
using System;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Drawing.CanvasObjects;
using LongoMatch.Core.Common;

namespace LongoMatch.Drawing
{
	/// <summary>
	/// A canvas that renders a single object using all the allocated space by the widget.
	/// </summary>
	public class ObjectCanvas: Canvas
	{
		FixedSizeCanvasObject canvasObject;

		public ObjectCanvas (FixedSizeCanvasObject obj, IWidget widget) : base (widget)
		{
			canvasObject = obj;
			AddObject (canvasObject);
		}

		public ObjectCanvas (FixedSizeCanvasObject obj) : this (obj, null)
		{
		}

		public override void Draw (IContext context, Area area)
		{
			Begin (context);
			DrawBackground ();
			canvasObject.Position = new Point (0, 0);
			canvasObject.Width = widget.Width;
			canvasObject.Height = widget.Height;
			canvasObject.Draw (tk, area);
			End ();
		}
	}
}

