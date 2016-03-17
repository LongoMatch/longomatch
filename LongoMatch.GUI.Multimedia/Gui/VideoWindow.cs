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

using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Multimedia.Utils;

namespace LongoMatch.Gui
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class VideoWindow : Gtk.Bin, IViewPort
	{
		AspectFrame frame;
		DrawingArea drawingWindow;
		bool dragStarted;

		public event EventHandler ReadyEvent;
		public new event ExposeEventHandler ExposeEvent;
		public new event ButtonPressEventHandler ButtonPressEvent;
		public new event ButtonReleaseEventHandler ButtonReleaseEvent;
		public new event ScrollEventHandler ScrollEvent;
		public event ButtonPressEventHandler VideoDragStarted;
		public event ButtonReleaseEventHandler VideoDragStopped;
		public event MotionNotifyEventHandler VideoDragged;

		public VideoWindow ()
		{
			this.Build ();

			frame = new AspectFrame (null, 0.5f, 0.5f, 1f, false);
			frame.Shadow = ShadowType.None;

			messageLabel.NoShowAll = true;

			drawingWindow = new DrawingArea ();
			drawingWindow.DoubleBuffered = false;
			drawingWindow.ExposeEvent += HandleExposeEvent;
			drawingWindow.MotionNotifyEvent += HandleMotionNotifyEvent;
			drawingWindow.ButtonPressEvent += HandleButtonPressEvent;
			drawingWindow.ButtonReleaseEvent += HandleButtonReleaseEvent;
			drawingWindow.Realized += HandleRealized;
			drawingWindow.AddEvents ((int)(Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask | Gdk.EventMask.PointerMotionMask | Gdk.EventMask.ScrollMask));

			videoeventbox.ButtonPressEvent += HandleButtonPressEvent;
			videoeventbox.ButtonReleaseEvent += HandleButtonReleaseEvent;
			videoeventbox.ScrollEvent += HandleScrollEvent;
			videoeventbox.BorderWidth = 0;

			frame.Add (drawingWindow);
			videoeventbox.Add (frame);
			videoeventbox.ShowAll ();

			MessageVisible = false;
			messageLabel.Ellipsize = Pango.EllipsizeMode.End;
		}

		public object WindowHandle {
			get;
			private set;
		}

		public string Message {
			set {
				messageLabel.Text = value;
			}
		}

		public bool MessageVisible {
			set {
				videoeventbox.Visible = !value;
				messageLabel.Visible = value;
			}
		}

		public float Ratio {
			set {
				frame.Ratio = value;
			}
			get {
				return frame.Ratio;
			}
		}

		public float Xalign {
			set {
				frame.Xalign = value;
			}
		}

		public float Yalign {
			set {
				frame.Yalign = value;
			}
		}

		public bool Ready {
			get;
			set;
		}

		public Cursor Cursor {
			set {
				drawingWindow.GdkWindow.Cursor = value;
			}
		}

		void HandleMotionNotifyEvent (object o, MotionNotifyEventArgs args)
		{
			if (dragStarted == true) {
				if (VideoDragged != null) {
					VideoDragged (this, args);
				}
			}
		}

		void HandleScrollEvent (object o, ScrollEventArgs args)
		{
			if (ScrollEvent != null) {
				ScrollEvent (this, args);
			}
		}

		void HandleExposeEvent (object o, ExposeEventArgs args)
		{
			if (!Ready) {
				Ready = true;
				if (ReadyEvent != null) {
					ReadyEvent (this, null);
				}
			}
			if (ExposeEvent != null) {
				ExposeEvent (this, args);
			}
		}

		void HandleButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			if (o == drawingWindow) {
				dragStarted = true;
				if (VideoDragStarted != null) {
					VideoDragStarted (this, args);
				}
			} else {
				if (ButtonPressEvent != null) {
					ButtonPressEvent (this, args);
				}
			}
		}

		void HandleButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{
			if (o == drawingWindow) {
				dragStarted = false;
				if (VideoDragStopped != null) {
					VideoDragStopped (this, args);
				}
			} else {
				if (ButtonReleaseEvent != null) {
					ButtonReleaseEvent (this, args);
				}
			}
		}

		void HandleRealized (object sender, EventArgs e)
		{
			WindowHandle = drawingWindow.GdkWindow.GetWindowHandle ();
		}
	}
}

