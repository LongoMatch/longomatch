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
using Gtk;
using Gdk;
using Cairo;
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
		public event LongoMatch.Handlers.Drawing.SizeChangedHandler SizeChangedEvent;

		DrawingArea widget;
		int currentWidth, currentHeight;
		bool canMove;
		uint timerID;
		
		public WidgetWrapper (DrawingArea widget)
		{
			this.widget = widget;
			widget.Events |= EventMask.PointerMotionMask;
			widget.Events |= EventMask.ButtonPressMask;
			widget.Events |= EventMask.ButtonReleaseMask ;
			widget.ExposeEvent += HandleExposeEvent;
			widget.ButtonPressEvent += HandleButtonPressEvent;
			widget.ButtonReleaseEvent += HandleButtonReleaseEvent;
			widget.MotionNotifyEvent += HandleMotionNotifyEvent;
		}

		public double Width {
			get {
				return currentWidth;
			}
			set {
				widget.WidthRequest = (int) value;
			}
		}

		public double Height {
			get {
				return currentHeight;
			}
			set {
				widget.HeightRequest = (int) value;
			}
		}
		
		public void ReDraw (Area area = null) {
			if (widget.GdkWindow == null) {
				return;
			}
			if (area == null) {
				Gdk.Region region = widget.GdkWindow.ClipRegion;
				widget.GdkWindow.InvalidateRegion(region,true);
			} else {
				widget.GdkWindow.InvalidateRect (
					new Gdk.Rectangle ((int)area.Start.X, (int)area.Start.Y,
				                   (int)area.Width, (int)area.Height),
					true);
			}
			widget.GdkWindow.ProcessUpdates(true);
		}
		
		public void ReDraw (IDrawable drawable) {
			/* FIXME: get region from drawable */
			ReDraw ();
		}
		
		public void SetCursor (CursorType type) {
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
		
		void Draw (Area area) {
			if (DrawEvent != null) {
				using (Context c = CairoHelper.Create (widget.GdkWindow)) {
					if (area == null) {
						area = new Area (new Point (0, 0), Width, Height);
					}
					DrawEvent (c, area);
				}
			}
		}
		
		ButtonType ParseButtonType (uint button) {
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
		
		ButtonModifier ParseButtonModifier (ModifierType modifier) {
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
		
		bool ReadyToMove () {
			canMove = true;
			return false;
			timerID = 0;
		}

		void HandleMotionNotifyEvent (object o, MotionNotifyEventArgs args)
		{
			if (MotionEvent != null && canMove) {
				MotionEvent (new Point (args.Event.X, args.Event.Y));
			}
		}

		void HandleButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{
			if (timerID != 0) {
				GLib.Source.Remove (timerID);
				timerID = 0;
			}
			
			if (ButtonReleasedEvent != null) {
				ButtonType bt;
				ButtonModifier bm;
				
				bt = ParseButtonType (args.Event.Button);
				bm = ParseButtonModifier (args.Event.State);
				ButtonReleasedEvent (new Point (args.Event.X, args.Event.Y), bt, bm);
			}
		}

		void HandleButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			/* Fast button clicks sometimes produced a small move that
			 * should be ignored. Start moving only when the button has been
			 * pressed for more than 200ms */
			canMove = false;
			timerID = GLib.Timeout.Add (200, ReadyToMove);
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

