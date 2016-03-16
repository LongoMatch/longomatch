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
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Drawables;

namespace LongoMatch.Drawing.CanvasObjects.Timeline
{
	public class TimeNodeObject: CanvasObject, ICanvasSelectableObject
	{
		ISurface needle;
		SelectionPosition movingPos;
		const int MAX_TIME_SPAN = 1000;

		public TimeNodeObject (TimeNode node)
		{
			TimeNode = node;
			SelectionMode = NodeSelectionMode.All;
			DraggingMode = NodeDraggingMode.All;
			LineColor = Config.Style.PaletteBackgroundLight;
			Height = StyleConf.TimelineCategoryHeight;
			ClippingMode = NodeClippingMode.Strict;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (needle != null) {
				needle.Dispose ();
				needle = null;
			}
		}

		public TimeNode TimeNode {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the selection mode.
		/// </summary>
		/// <value>The selection mode.</value>
		public NodeSelectionMode SelectionMode {
			get;
			set;
		}

		public NodeDraggingMode DraggingMode {
			get;
			set;
		}

		public Color LineColor {
			get;
			set;
		}

		public bool ShowName {
			get;
			set;
		}

		public Time MaxTime {
			set;
			protected get;
		}

		public double OffsetY {
			get;
			set;
		}

		public double Height {
			get;
			set;
		}

		public double SecondsPerPixel {
			set;
			protected get;
		}

		public double StartX {
			get {
				return Utils.TimeToPos (TimeNode.Start, SecondsPerPixel);
			}
		}

		public double StopX {
			get {
				return Utils.TimeToPos (TimeNode.Stop, SecondsPerPixel);
			}
		}

		public double CenterX {
			get {
				return Utils.TimeToPos (TimeNode.Start + TimeNode.Duration / 2,
					SecondsPerPixel);
			}
		}

		public NodeClippingMode ClippingMode {
			get;
			set;
		}

		Area Area {
			get {
				double ls = StyleConf.TimelineLineSize;
				return new Area (new Point (StartX - ls, OffsetY),
					(StopX - StartX) + 2 * ls, Height);
			}
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			if (SelectionMode == NodeSelectionMode.Borders || SelectionMode == NodeSelectionMode.All) {
				double accuracy;
				if (point.Y >= OffsetY && point.Y < OffsetY + Height) {
					if (Drawable.MatchAxis (point.X, StartX, precision, out accuracy)) {
						return new Selection (this, SelectionPosition.Left, accuracy);
					} else if (Drawable.MatchAxis (point.X, StopX, precision, out accuracy)) {
						return new Selection (this, SelectionPosition.Right, accuracy);
					}
				}
			}

			if (SelectionMode == NodeSelectionMode.Segment || SelectionMode == NodeSelectionMode.All) {
				if (point.Y >= OffsetY && point.Y < OffsetY + Height) {
					if (point.X > StartX && point.X < StopX) {
						return new Selection (this, SelectionPosition.All, Math.Abs (CenterX - point.X));
					}
				}
			}
			return null;
		}

		public void Move (Selection sel, Point p, Point start)
		{
			double diffX;

			// Apply dragging restrictions
			if (DraggingMode == NodeDraggingMode.None)
				return;
			switch (sel.Position) {
			case SelectionPosition.Left:
			case SelectionPosition.Right:
				if (DraggingMode == NodeDraggingMode.Segment)
					return;
				break;
			case SelectionPosition.All:
				if (DraggingMode == NodeDraggingMode.Borders)
					return;
				break;
			}

			Time newTime = Utils.PosToTime (p, SecondsPerPixel);
			diffX = p.X - start.X;

			if (p.X < 0) {
				p.X = 0;
			} else if (newTime > MaxTime) {
				p.X = Utils.TimeToPos (MaxTime, SecondsPerPixel);
			}
			newTime = Utils.PosToTime (p, SecondsPerPixel);

			if (TimeNode is StatEvent) {
				TimeNode.EventTime = newTime;
				return;
			}

			switch (sel.Position) {
			case SelectionPosition.Left:
				if (newTime.MSeconds + MAX_TIME_SPAN > TimeNode.Stop.MSeconds) {
					TimeNode.Start.MSeconds = TimeNode.Stop.MSeconds - MAX_TIME_SPAN;
				} else {
					TimeNode.Start = newTime;
				}
				break;
			case SelectionPosition.Right:
				if (newTime.MSeconds - MAX_TIME_SPAN < TimeNode.Start.MSeconds) {
					TimeNode.Stop.MSeconds = TimeNode.Start.MSeconds + MAX_TIME_SPAN;
				} else {
					TimeNode.Stop = newTime;
				}
				break;
			case SelectionPosition.All:
				Time tstart, tstop;
				Time diff = Utils.PosToTime (new Point (diffX, p.Y), SecondsPerPixel);
				bool ok = false;

				tstart = TimeNode.Start;
				tstop = TimeNode.Stop;

				switch (ClippingMode) {
				case NodeClippingMode.None:
					ok = true;
					break;
				case NodeClippingMode.NoStrict:
					ok = ((tstop + diff) >= new Time (0) && (tstart + diff) < MaxTime);
					break;
				case NodeClippingMode.LeftStrict:
					ok = ((tstart + diff) >= new Time (0) && (tstart + diff) < MaxTime);
					break;
				case NodeClippingMode.RightStrict:
					ok = (tstop + diff) >= new Time (0) && ((tstop + diff) < MaxTime);
					break;
				case NodeClippingMode.Strict:
					ok = ((tstart + diff) >= new Time (0) && (tstop + diff) < MaxTime);
					break;
				}

				if (ok) {
					TimeNode.Start += diff;
					TimeNode.Stop += diff;
				}
				break;
			}
			movingPos = sel.Position;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double linepos;

			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			tk.Begin ();
			if (needle == null) {
				Image img = Resources.LoadImage (StyleConf.TimelineNeedleUP);
				needle = tk.CreateSurface (img.Width, img.Height, img);
			}
			
			if (Selected) {
				tk.FillColor = Config.Style.PaletteActive;
				tk.StrokeColor = Config.Style.PaletteActive;
			} else {
				tk.FillColor = LineColor;
				tk.StrokeColor = LineColor;
			}
			tk.LineWidth = StyleConf.TimelineLineSize;
			
			linepos = OffsetY + Height / 2 + StyleConf.TimelineLineSize / 2;
			
			if (StopX - StartX <= needle.Width / 2) {
				double c = movingPos == SelectionPosition.Left ? StopX : StartX;
				tk.DrawSurface (new Point (c - needle.Width / 2, linepos - 9), StyleConf.TimelineNeedleUpWidth, StyleConf.TimelineNeedleUpHeight, needle, ScaleMode.AspectFit);
			} else {
				tk.DrawLine (new Point (StartX, linepos),
					new Point (StopX, linepos));
				tk.DrawSurface (new Point (StartX - needle.Width / 2, linepos - 9), StyleConf.TimelineNeedleUpWidth, StyleConf.TimelineNeedleUpHeight, needle, ScaleMode.AspectFit);
				tk.DrawSurface (new Point (StopX - needle.Width / 2, linepos - 9), StyleConf.TimelineNeedleUpWidth, StyleConf.TimelineNeedleUpHeight, needle, ScaleMode.AspectFit);
			}
			

			if (ShowName) {
				tk.FontSize = StyleConf.TimelineFontSize;
				tk.FontWeight = FontWeight.Bold;
				tk.FillColor = Config.Style.PaletteActive;
				tk.StrokeColor = Config.Style.PaletteActive;
				tk.DrawText (new Point (StartX, OffsetY), StopX - StartX, Height / 2, TimeNode.Name);
			}
			tk.End ();
		}
	}

	public class TimerTimeNodeObject: TimeNodeObject
	{
		public TimerTimeNodeObject (Timer t, TimeNode tn) : base (tn)
		{
			Timer = t;
		}

		public Timer Timer {
			get;
			set;
		}
	}
}
