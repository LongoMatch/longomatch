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
using Gtk;
using Gdk;
using LongoMatch.Common;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Handlers.Drawing;
using Rectangle = Gdk.Rectangle;
using Point = LongoMatch.Common.Point;
using CursorType = LongoMatch.Common.CursorType;
using GCursorType = Gdk.CursorType;

namespace LongoMatch.Drawing.Cairo
{
	public class WidgetWrapper: IWidget
	{
		public event DrawingHandler DrawEvent;
		public event ButtonPressedHandler ButtonPressEvent;
		public event ButtonReleasedHandler ButtonReleasedEvent;
		public event MotionHandler MotionEvent;
		public event ShowTooltipHandler ShowTooltipEvent;
		public event LongoMatch.Handlers.Drawing.SizeChangedHandler SizeChangedEvent;

		DrawingArea widget;
		int currentWidth, currentHeight;
		double lastX, lastY;
		bool canMove, inButtonPress;
		uint moveTimerID, hoverTimerID;

		public WidgetWrapper (DrawingArea widget)
		{
			this.widget = widget;
			MoveWaitMS = 200;
			widget.AddEvents ((int)EventMask.PointerMotionMask);
			widget.AddEvents ((int)EventMask.ButtonPressMask);
			widget.AddEvents ((int)EventMask.ButtonReleaseMask);
			widget.AddEvents ((int)EventMask.KeyPressMask);
			widget.ExposeEvent += HandleExposeEvent;
			widget.ButtonPressEvent += HandleButtonPressEvent;
			widget.ButtonReleaseEvent += HandleButtonReleaseEvent;
			widget.MotionNotifyEvent += HandleMotionNotifyEvent;
		}
		
		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				if (moveTimerID != 0) {
					GLib.Source.Remove (moveTimerID);
					moveTimerID = 0;
				}
				if (hoverTimerID != 0) {
					GLib.Source.Remove (hoverTimerID);
					
					hoverTimerID = 0;
				}
			}
		}

		public uint MoveWaitMS {
			get;
			set;
		}
		
		public double Width {
			get {
				return currentWidth;
			}
			set {
				widget.WidthRequest = (int)value;
			}
		}

		public double Height {
			get {
				return currentHeight;
			}
			set {
				widget.HeightRequest = (int)value;
			}
		}

		public void ReDraw (Area area = null)
		{
			if (widget.GdkWindow == null) {
				return;
			}
			if (area == null) {
				Gdk.Region region = widget.GdkWindow.ClipRegion;
				widget.GdkWindow.InvalidateRegion (region, true);
			} else {
				widget.GdkWindow.InvalidateRect (
					new Gdk.Rectangle ((int)area.Start.X, (int)area.Start.Y,
				                    (int)area.Width, (int)area.Height),
					true);
			}
			widget.GdkWindow.ProcessUpdates (true);
		}

		public void ReDraw (IMovableObject drawable)
		{
			/* FIXME: get region from drawable */
			ReDraw ();
		}

		public void ShowTooltip (string text)
		{
			widget.HasTooltip = true;
			widget.TooltipText = text;
		}

		public void SetCursor (CursorType type)
		{
			GCursorType gtype;
			switch (type) {
			case CursorType.Arrow:
				gtype = GCursorType.Arrow;
				break;
			case CursorType.DoubleArrow:
				gtype = GCursorType.SbHDoubleArrow;
				break;
			case CursorType.Selection:
				gtype = GCursorType.Fleur;
				break;
			case CursorType.Cross:
				gtype = GCursorType.Cross;
				break;
			default:
				gtype = GCursorType.Arrow;
				break;
			}
			widget.GdkWindow.Cursor = new Cursor (gtype);
		}

		public void SetCursorForTool (DrawTool tool)
		{
			string cursor;
			
			switch (tool) {
			case DrawTool.Line:
				cursor = "arrow";
				break;
			case DrawTool.Cross:
				cursor = "cross";
				break;
			case DrawTool.Text:
				cursor = "text";
				break;
			case DrawTool.Counter:
				cursor = "number";
				break;
			case DrawTool.Ellipse:
			case DrawTool.CircleArea:
				cursor = "ellipse";
				break;
			case DrawTool.Rectangle:
			case DrawTool.RectangleArea:
				cursor = "rect";
				break;
			case DrawTool.Angle:
				cursor = "angle";
				break;
			case DrawTool.Pen:
				cursor = "freehand";
				break;
			case DrawTool.Eraser:
				cursor = "eraser";
				break;
			case DrawTool.Selection:
			default:
				cursor = null;
				break;
			}
			if (cursor == null) {
				widget.GdkWindow.Cursor = null;
			} else {
				Cursor c = new Cursor (widget.Display,
				                       Gdk.Pixbuf.LoadFromResource (cursor), 0, 0);
				widget.GdkWindow.Cursor = c;
			}
		}

		void Draw (Area area)
		{
			if (DrawEvent != null) {
				using (CairoContext c = new CairoContext (widget.GdkWindow)) {
					global::Cairo.Context cc = c.Value as global::Cairo.Context;
					if (area == null) {
						Rectangle r = widget.GdkWindow.ClipRegion.Clipbox;
						area = new Area (new Point (r.X, r.Y), r.Width, r.Height);
					}
					cc.Rectangle (area.Start.X, area.Start.Y, area.Width, area.Height);
					cc.Clip ();
					DrawEvent (c, area);
				}
			}
		}

		ButtonType ParseButtonType (uint button)
		{
			ButtonType bt;
			
			switch (button) {
			case 1:
				bt = ButtonType.Left;
				break;
			case 2:
				bt = ButtonType.Center;
				break;
			case 3:
				bt = ButtonType.Right;
				break;
			default:
				bt = ButtonType.None;
				break;
			}
			return bt;
		}

		ButtonModifier ParseButtonModifier (ModifierType modifier)
		{
			ButtonModifier bm;
			
			switch (modifier) {
			case ModifierType.ControlMask:
				bm = ButtonModifier.Control;
				break;
			case ModifierType.ShiftMask:
				bm = ButtonModifier.Shift;
				break;
			default:
				bm = ButtonModifier.None;
				break;
			}
			return bm;
		}

		bool ReadyToMove ()
		{
			canMove = true;
			moveTimerID = 0;
			return false;
		}

		bool EmitShowTooltip ()
		{
			if (ShowTooltipEvent != null) {
				ShowTooltipEvent (new Point (lastX, lastY));
			}
			hoverTimerID = 0;
			return false;
		}

		void HandleMotionNotifyEvent (object o, MotionNotifyEventArgs args)
		{
			if (hoverTimerID != 0) {
				GLib.Source.Remove (hoverTimerID);
				hoverTimerID = 0;
			}
			hoverTimerID = GLib.Timeout.Add (100, EmitShowTooltip);
			widget.HasTooltip = false;
			
			lastX = args.Event.X;
			lastY = args.Event.Y;

			if (MotionEvent != null) {
				if (!inButtonPress || canMove) {
					MotionEvent (new Point (lastX, lastY));
				}
			}
		}

		void HandleButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{
			if (moveTimerID != 0) {
				GLib.Source.Remove (moveTimerID);
				moveTimerID = 0;
			}
			
			if (ButtonReleasedEvent != null) {
				ButtonType bt;
				ButtonModifier bm;
				
				bt = ParseButtonType (args.Event.Button);
				bm = ParseButtonModifier (args.Event.State);
				ButtonReleasedEvent (new Point (args.Event.X, args.Event.Y), bt, bm);
			}
			inButtonPress = false;
		}

		void HandleButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			/* Fast button clicks sometimes produced a small move that
			 * should be ignored. Start moving only when the button has been
			 * pressed for more than 200ms */
			canMove = false;
			inButtonPress = true;
			moveTimerID = GLib.Timeout.Add (MoveWaitMS, ReadyToMove);
			if (ButtonPressEvent != null) {
				ButtonType bt;
				ButtonModifier bm;
				
				bt = ParseButtonType (args.Event.Button);
				bm = ParseButtonModifier (args.Event.State);
				ButtonPressEvent (new Point (args.Event.X, args.Event.Y),
				                  args.Event.Time, bt, bm);
			}
		}

		void HandleExposeEvent (object o, ExposeEventArgs args)
		{
			Rectangle r;
			Area a;
			bool size_changed;
			
			size_changed = widget.Allocation.Height != currentHeight;
			size_changed |= widget.Allocation.Width != currentWidth;
			currentWidth = widget.Allocation.Width;
			currentHeight = widget.Allocation.Height;
			if (size_changed && SizeChangedEvent != null) {
				SizeChangedEvent ();
			}
			
			r = args.Event.Area;
			a = new Area (new Point (r.X, r.Y), r.Width, r.Height);
			Draw (a);
		}
	}
}

