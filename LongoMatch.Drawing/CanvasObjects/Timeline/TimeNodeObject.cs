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
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Drawing.CanvasObjects.Timeline;

namespace LongoMatch.Drawing.CanvasObjects.Timeline
{
	public class LTimeNodeView : TimeNodeView
	{
		public LTimeNodeView ()
		{
		}

		public override void Move (Selection sel, Point p, Point start)
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

			Time newTime = VAS.Drawing.Utils.PosToTime (p, SecondsPerPixel);
			diffX = p.X - start.X;

			if (p.X < 0) {
				p.X = 0;
			} else if (newTime > MaxTime) {
				p.X = VAS.Drawing.Utils.TimeToPos (MaxTime, SecondsPerPixel);
			}
			newTime = VAS.Drawing.Utils.PosToTime (p, SecondsPerPixel);

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
				Time diff = VAS.Drawing.Utils.PosToTime (new Point (diffX, p.Y), SecondsPerPixel);
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
	}
}
