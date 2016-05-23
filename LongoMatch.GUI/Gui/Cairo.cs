//
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using VAS.UI.Helpers;


namespace LongoMatch.Core.Common
{
	public class CairoUtils
	{
		public static void DrawRoundedRectangle (Cairo.Context gr, double x, double y,
		                                         double width, double height, double radius,
		                                         Cairo.Color color, Cairo.Color borderColor)
		{
			gr.Save ();

			if ((radius > height / 2) || (radius > width / 2))
				radius = Math.Min (height / 2, width / 2);

			gr.MoveTo (x, y + radius);
			gr.Arc (x + radius, y + radius, radius, Math.PI, -Math.PI / 2);
			gr.LineTo (x + width - radius, y);
			gr.Arc (x + width - radius, y + radius, radius, -Math.PI / 2, 0);
			gr.LineTo (x + width, y + height - radius);
			gr.Arc (x + width - radius, y + height - radius, radius, 0, Math.PI / 2);
			gr.LineTo (x + radius, y + height);
			gr.Arc (x + radius, y + height - radius, radius, Math.PI / 2, Math.PI);
			gr.ClosePath ();
			gr.Restore ();

			gr.LineJoin = LineJoin.Round;
			gr.Color = borderColor;
			gr.StrokePreserve ();
			gr.Color = color;
			gr.Fill ();
		}

		public static void DrawLine (Cairo.Context g, double x1, double y1,
		                             double x2, double y2,
		                             int width, Cairo.Color color)
		{
			g.Color = color;
			g.Operator = Operator.Over;
			g.LineWidth = width;
			g.MoveTo (x1, y1);
			g.LineTo (x2, y2);
			g.Stroke ();
		}

		public static void DrawTriangle (Cairo.Context g, double x, double y,
		                                 int width, int height, Cairo.Color color)
		{
			g.Color = color;
			g.MoveTo (x, y);
			g.LineTo (x + width / 2, y - height);
			g.LineTo (x - width / 2, y - height);
			g.ClosePath ();
			g.Fill ();
			g.Stroke ();
		}

		public static Cairo.Color RGBToCairoColor (Gdk.Color gdkColor)
		{
			return new Cairo.Color (Misc.ShortToDouble (gdkColor.Red),
				Misc.ShortToDouble (gdkColor.Green),
				Misc.ShortToDouble (gdkColor.Blue));
		}

		public static Cairo.Color ColorFromRGB (byte r, byte g, byte b)
		{
			return new Cairo.Color (Misc.ByteToDouble (r),
				Misc.ByteToDouble (g),
				Misc.ByteToDouble (b));
		}
	}
}

