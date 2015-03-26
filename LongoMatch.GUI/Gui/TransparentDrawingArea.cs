//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using Gdk;
using Gtk;
using Cairo;

namespace LongoMatch.Gui.Popup
{


	[System.ComponentModel.ToolboxItem (true)]
	public partial class TransparentDrawingArea : Gtk.Window
	{
		//Pixmpas and shapes
		private Pixmap pixmap;
		private Pixmap shape;
		private Gdk.GC shapeGC;
		private Gdk.Color transparent;
		private Gdk.Color opaque;

		//Mouse motion
		private double lastx = -1;
		private double lasty = -1;

		//Reshaping timeout
		private uint timeoutId;

		//Status
		private bool modified;
		private bool hardGrab;

		//Drawing Properties
		private Gdk.Color foreground;
		private int lineWidth;

		//"Parent" Widget we want to draw over
		private Widget targetWidget;


		public TransparentDrawingArea (Widget targetWidget) : base (Gtk.WindowType.Toplevel)
		{
			this.Build ();

			ExtensionEvents = ExtensionMode.All;
			Gdk.Color.Parse ("red", ref foreground);
			LineColor = foreground;
			lineWidth = 6;
			modified = false;
			this.targetWidget = targetWidget;
		}

		public int LineWidth {
			set {
				lineWidth = value;
			}
		}

		public Gdk.Color LineColor {
			set {
				foreground = value;
			}
		}

		public void ToggleGrab ()
		{
			if (hardGrab)
				ReleaseGrab ();
			else
				AcquireGrab ();
		}

		public void ReleaseGrab ()
		{
			if (hardGrab) {
				hardGrab = false;
				Pointer.Ungrab (Gtk.Global.CurrentEventTime);
			}
		}

		public void AcquireGrab ()
		{
			GrabStatus stat;
			if (!hardGrab) {
				stat = Pointer.Grab (drawingarea.GdkWindow, false,
					EventMask.ButtonMotionMask |
					EventMask.ButtonPressMask |
					EventMask.ButtonReleaseMask,
					targetWidget.GdkWindow,
					new Gdk.Cursor (Gdk.CursorType.Pencil) /* data->paint_cursor */,
					Gtk.Global.CurrentEventTime);
				if (stat == GrabStatus.Success) {
					hardGrab = true;
				}
			}
		}

		public void ToggleVisibility ()
		{
			Visible = !Visible;
		}

		public void Clear ()
		{
			//Clear shape
			shapeGC.Foreground = transparent;
			shape.DrawRectangle (shapeGC, true, 0, 0, Allocation.Width, Allocation.Height);
			shapeGC.Background = opaque;

			ShapeCombineMask (shape, 0, 0);

			//Clear pixmap
			pixmap.DrawRectangle (drawingarea.Style.BlackGC, true, 0, 0, Allocation.Width, Allocation.Height);
		}

		private bool Reshape ()
		{
			if (modified) {
				ShapeCombineMask (shape, 0, 0);
				modified = false;
			}
			return true;
		}

		private void CreatePixmaps ()
		{
			GCValues shapeGCV;

			//Create a 1 depth pixmap used as a shape
			//that will contain the info about transparency
			shape = new Pixmap (null, Gdk.Screen.Default.Width, Gdk.Screen.Default.Height, 1);
			shapeGC = new Gdk.GC (shape);
			shapeGCV = new GCValues ();
			shapeGC.GetValues (shapeGCV);
			transparent = shapeGCV.Foreground;
			opaque = shapeGCV.Background;
			shapeGC.Foreground = transparent;
			shape.DrawRectangle (shapeGC, true, 0, 0, Gdk.Screen.Default.Width, Gdk.Screen.Default.Height);
			shapeGC.Background = opaque;

			ShapeCombineMask (shape, 0, 0);

			//Create the pixmap that will contain the real drawing
			//Used on Expose event to redraw the drawing area
			pixmap = new Pixmap (drawingarea.GdkWindow, Gdk.Screen.Default.Width, Gdk.Screen.Default.Height);
			pixmap.DrawRectangle (drawingarea.Style.BlackGC, true, 0, 0, Gdk.Screen.Default.Width, Gdk.Screen.Default.Height);
		}

		private void  DrawCairoLine (Context c, int x1, int y1, int x2, int y2, Gdk.Color color)
		{
			c.Color = new Cairo.Color (color.Red, color.Green, color.Blue, 1);
			c.MoveTo (x1, y1);
			c.LineTo (x2, y2);
			c.LineWidth = lineWidth;
			c.LineCap = LineCap.Round;
			c.LineJoin = LineJoin.Round;
			c.Stroke ();
			c.Fill ();
		}

		private void DrawLine (int x1, int y1, int x2, int y2)
		{
			Cairo.Rectangle rect = new Cairo.Rectangle (Math.Min (x1, x2) - lineWidth / 2,
				                       Math.Min (y1, y2) - lineWidth / 2,
				                       Math.Abs (x1 - x2) + lineWidth,
				                       Math.Abs (y1 - y2) + lineWidth);

			using (Context c = CairoHelper.Create (drawingarea.GdkWindow)) {
				c.Color = new Cairo.Color (foreground.Red, foreground.Green, foreground.Blue, 1);
				c.Rectangle (rect);
				c.LineWidth = lineWidth;
				c.LineCap = LineCap.Round;
				c.LineJoin = LineJoin.Round;
				c.StrokePreserve ();
				c.Fill ();
			}

			using (Context c = CairoHelper.Create (shape)) {
				DrawCairoLine (c, x1, y1, x2, y2, opaque);
			}

			using (Context c = CairoHelper.Create (pixmap)) {
				DrawCairoLine (c, x1, y1, x2, y2, foreground);
			}
			modified = true;
		}

		protected virtual void OnDrawingareaExposeEvent (object o, Gtk.ExposeEventArgs args)
		{
			EventExpose evnt = args.Event;
			drawingarea.GdkWindow.DrawDrawable (drawingarea.Style.ForegroundGCs [(int)drawingarea.State],
				pixmap,
				evnt.Area.X, evnt.Area.Y,
				evnt.Area.X, evnt.Area.Y,
				evnt.Area.Width, evnt.Area.Height);
		}

		protected virtual void OnDrawingareaButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			if (!hardGrab)
				return;
			lastx = args.Event.X;
			lasty = args.Event.Y;

			if (args.Event.Button == 1)
				DrawLine ((int)args.Event.X, (int)args.Event.Y, (int)args.Event.X, (int)args.Event.Y);
		}

		protected virtual void OnDrawingareaMotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args)
		{
			if (!hardGrab)
				return;

			if (lastx == -1 || lasty == -1) {
				lastx = args.Event.X;
				lasty = args.Event.Y;
			}
			DrawLine ((int)lastx, (int)lasty, (int)args.Event.X, (int)args.Event.Y);
			lastx = args.Event.X;
			lasty = args.Event.Y;
		}


		protected virtual void OnDrawingareaButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args)
		{
			drawingarea.QueueDraw ();
			lastx = -1;
			lasty = -1;
		}

		protected override void OnHidden ()
		{
			GLib.Source.Remove (timeoutId);
			base.OnHidden ();
		}

		protected override void  OnShown ()
		{
			//Prevent a dirty flash when the
			//Window is created and hidden
			if (targetWidget != null) {
				base.OnShown ();
				timeoutId = GLib.Timeout.Add (20, Reshape);
			}
		}

		protected virtual void OnDrawingareaConfigureEvent (object o, Gtk.ConfigureEventArgs args)
		{
			this.TransientFor = (Gtk.Window)targetWidget.Toplevel;
			this.Resize (Gdk.Screen.Default.Width, Gdk.Screen.Default.Height);
			CreatePixmaps ();
		}
	}
}
