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
using System.IO;
using Gdk;
using Gtk;
using LongoMatch.Core.Handlers.Drawing;
using LongoMatch.Core.Interfaces.Drawing;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using CursorType = VAS.Core.Common.CursorType;
using GCursorType = Gdk.CursorType;
using Image = VAS.Core.Common.Image;
using Point = VAS.Core.Common.Point;
using Rectangle = Gdk.Rectangle;

namespace LongoMatch.Drawing.Cairo
{
	public class WidgetWrapper: IWidget
	{
		public event DrawingHandler DrawEvent;
		public event ButtonPressedHandler ButtonPressEvent;
		public event ButtonReleasedHandler ButtonReleasedEvent;
		public event MotionHandler MotionEvent;
		public event ShowTooltipHandler ShowTooltipEvent;
		public event LongoMatch.Core.Handlers.Drawing.SizeChangedHandler SizeChangedEvent;

		DrawingArea widget;
		int currentWidth, currentHeight;
		double lastX, lastY;
		bool canMove, inButtonPress;
		uint moveTimerID, hoverTimerID, lastButtonTime;

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
					new Gdk.Rectangle ((int)area.Start.X - 1, (int)area.Start.Y - 1,
						(int)Math.Ceiling (area.Width) + 2,
						(int)Math.Ceiling (area.Height) + 2),
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
			string cursorStr = null;
			Gdk.Cursor cursor = null;

			if (widget.GdkWindow == null) {
				return;
			}
			
			switch (tool) {
			case DrawTool.Line:
				cursorStr = "arrow";
				break;
			case DrawTool.Cross:
				cursorStr = "cross";
				break;
			case DrawTool.Text:
				cursorStr = "text";
				break;
			case DrawTool.Counter:
				cursorStr = "number";
				break;
			case DrawTool.Ellipse:
			case DrawTool.CircleArea:
				cursorStr = "ellipse";
				break;
			case DrawTool.Rectangle:
			case DrawTool.RectangleArea:
				cursorStr = "rect";
				break;
			case DrawTool.Angle:
				cursorStr = "angle";
				break;
			case DrawTool.Pen:
				cursorStr = "freehand";
				break;
			case DrawTool.Eraser:
				cursorStr = "eraser";
				break;
			case DrawTool.Player:
				cursorStr = "player";
				break;
			case DrawTool.Zoom:
				cursorStr = "zoom";
				break;
			case DrawTool.CanMove:
				cursorStr = "hand_opened";
				break;
			case DrawTool.Move:
				cursorStr = "hand_closed";
				break;
			case DrawTool.Selection:
				cursorStr = "hand_select";
				break;
			default:
				cursor = null;
				break;
			}
			if (cursorStr == null) {
				widget.GdkWindow.Cursor = cursor;
			} else {
				Image img = Resources.LoadImage (Path.Combine ("images/cursors", cursorStr));
				Cursor c = new Cursor (widget.Display, img.Value, 0, 0);
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
#if OSTYPE_OS_X
			case ModifierType.Mod2Mask:
				bm = ButtonModifier.Control;
				break;
			case ModifierType.ControlMask:
				bm = ButtonModifier.Meta;
				break;
#else
			case ModifierType.ControlMask:
				bm = ButtonModifier.Control;
				break;
#endif
			case ModifierType.ShiftMask:
				bm = ButtonModifier.Shift;
				break;
			default:
				bm = ButtonModifier.None;
				break;
			}
			return bm;
		}

		ButtonRepetition ParseButtonRepetition (EventType type)
		{
			ButtonRepetition br;

			switch (type) {
			case EventType.TwoButtonPress:
				br = ButtonRepetition.Double;
				break;
			case EventType.ThreeButtonPress:
				br = ButtonRepetition.Triple;
				break;
			default:
				br = ButtonRepetition.Single;
				break;
			}
			return br;
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

			/* Grab the focus if it's required */
			if (widget.CanFocus && !widget.HasFocus)
				widget.GrabFocus ();
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
				ButtonRepetition br;
				uint time = args.Event.Time;
				
				bt = ParseButtonType (args.Event.Button);
				bm = ParseButtonModifier (args.Event.State);
				br = ParseButtonRepetition (args.Event.Type);

				// Ignore the second single click event when there's a double click
				var nextEvent = EventHelper.Peek ();
				if (nextEvent != null) {
					try {
						var nextEventButton = nextEvent as EventButton;
						if (nextEventButton?.Time == time &&
						    ParseButtonRepetition (nextEventButton.Type) != ButtonRepetition.Single) {
							return;
						}
					} finally {
						EventHelper.Free (nextEvent);
					}
				}

				ButtonPressEvent (new Point (args.Event.X, args.Event.Y),
					time, bt, bm, br);
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

