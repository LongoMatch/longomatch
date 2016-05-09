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
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Drawing.CanvasObjects;

namespace LongoMatch.Drawing.CanvasObjects.Timeline
{
	public class LabelObject: CanvasObject, ICanvasObject
	{
		const int DEFAULT_FONT_SIZE = 12;
		const FontWeight DEFAULT_FONT_WEIGHT = FontWeight.Bold;

		public LabelObject (double width, double height, double offsetY)
		{
			Height = height;
			Width = width;
			OffsetY = offsetY;
			Color = Color.Red1;
		}

		public virtual string Name {
			get;
			set;
		}

		public virtual Color Color {
			get;
			set;
		}

		public double Width {
			get;
			set;
		}

		public double Height {
			get;
			set;
		}

		public double RequiredWidth {
			get {
				int width, height;
				Config.DrawingToolkit.MeasureText (
					Name, out width, out height, Config.Style.Font,
					DEFAULT_FONT_SIZE, DEFAULT_FONT_WEIGHT);
				return TextOffset + width + StyleConf.TimelineLabelHSpacing;
			}
		}

		public double Scroll {
			get;
			set;
		}

		public Color BackgroundColor {
			get;
			set;
		}

		public double OffsetY {
			set;
			get;
		}

		double RectSize {
			get {
				return Height - StyleConf.TimelineLabelVSpacing * 2;
			}
		}

		double TextOffset {
			get {
				return StyleConf.TimelineLabelHSpacing * 2 + RectSize;
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double hs, vs;
			double y;
			
			hs = StyleConf.TimelineLabelHSpacing;
			vs = StyleConf.TimelineLabelVSpacing;
			y = OffsetY - Math.Floor (Scroll);
			tk.Begin ();
			tk.FillColor = BackgroundColor;
			tk.StrokeColor = BackgroundColor;
			tk.LineWidth = 0;
			tk.DrawRectangle (new Point (0, y), Width, Height);
			
			/* Draw a rectangle with the category color */
			tk.FillColor = Color;
			tk.StrokeColor = Color;
			tk.DrawRectangle (new Point (hs, y + vs), RectSize, RectSize); 
			
			/* Draw category name */
			tk.FontSlant = FontSlant.Normal;
			tk.FontWeight = DEFAULT_FONT_WEIGHT;
			tk.FontSize = DEFAULT_FONT_SIZE;
			tk.FillColor = Config.Style.PaletteWidgets;
			tk.FontAlignment = FontAlignment.Left;
			tk.StrokeColor = Config.Style.PaletteWidgets;
			tk.DrawText (new Point (TextOffset, y), Width - TextOffset, Height, Name);
			tk.End ();
		}
	}

	public class EventTypeLabelObject: LabelObject
	{
		EventType eventType;

		public EventTypeLabelObject (EventType eventType, double width, double height, double offsetY) :
			base (width, height, offsetY)
		{
			this.eventType = eventType;
		}

		public override Color Color {
			get {
				return eventType.Color;
			}
		}

		public override string Name {
			get {
				return eventType.Name;
			}
		}
	}

	public class TimerLabelObject: LabelObject
	{
		Timer timer;

		public TimerLabelObject (Timer timer, double width, double height, double offsetY) :
			base (width, height, offsetY)
		{
			this.timer = timer;
		}

		public override string Name {
			get {
				return timer.Name;
			}
		}
	}

	public class CameraLabelObject: LabelObject
	{
		public CameraLabelObject (double width, double height, double offsetY) :
			base (width, height, offsetY)
		{
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double y = OffsetY - Math.Floor (Scroll);

			// Draw background
			tk.Begin ();
			tk.FillColor = BackgroundColor;
			tk.StrokeColor = Config.Style.PaletteWidgets;
			tk.LineWidth = 1;
			tk.DrawRectangle (new Point (0, y), Width, Height);

			/* Draw category name */
			tk.FontSlant = FontSlant.Normal;
			tk.FontWeight = FontWeight.Bold;
			tk.FontSize = StyleConf.TimelineCameraFontSize;
			tk.FillColor = Config.Style.PaletteWidgets;
			tk.FontAlignment = FontAlignment.Right;
			tk.StrokeColor = Config.Style.PaletteWidgets;
			tk.DrawText (new Point (0, y), Width - StyleConf.TimelineLabelHSpacing, Height, Name);
			tk.End ();
		}
	}
}

