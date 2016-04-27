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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Store;

namespace LongoMatch.Drawing.CanvasObjects.Timeline
{
	public class CameraObject: TimeNodeObject
	{
		MediaFile mediaFile;

		public CameraObject (MediaFile mf) :
			base (new TimeNode { Start = new Time (-mf.Offset.MSeconds),
				Stop = mf.Duration - mf.Offset, Name = mf.Name
			})
		{
			mediaFile = mf;
			// Video boundaries can't be changed, only the segment can move.
			DraggingMode = NodeDraggingMode.Segment;
			SelectionMode = NodeSelectionMode.Segment;
			ClippingMode = NodeClippingMode.LeftStrict;
		}

		public MediaFile MediaFile {
			get {
				return mediaFile;
			}
		}

		public override string Description {
			get {
				return mediaFile.Name;
			}
		}

		Area Area {
			get {
				return new Area (new Point (StartX, OffsetY),
					(StopX - StartX), Height);
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			tk.Begin ();

			tk.StrokeColor = Config.Style.PaletteBackgroundDark;
			if (Selected) {
				tk.FillColor = Config.Style.PaletteActive;
			} else {
				tk.FillColor = LineColor;
			}
			tk.LineWidth = 1;

			tk.DrawRoundedRectangle (new Point (StartX, OffsetY), StopX - StartX, Height, 5);

			if (ShowName) {
				tk.FontSize = 16;
				tk.FontWeight = FontWeight.Bold;
				tk.FillColor = Config.Style.PaletteActive;
				tk.StrokeColor = Config.Style.PaletteActive;
				tk.DrawText (new Point (StartX, OffsetY), StopX - StartX,
					Height - StyleConf.TimelineLineSize,
					TimeNode.Name);
			}
			tk.End ();
		}
	}
}

