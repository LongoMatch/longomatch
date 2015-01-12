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

namespace LongoMatch.Gui
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class VideoWindow : Gtk.Bin
	{
		AspectFrame frame;
		public event EventHandler ReadyEvent;
		public new event ExposeEventHandler ExposeEvent;
		public new event ButtonPressEventHandler ButtonPressEvent;
		public new event ScrollEventHandler ScrollEvent;

		public VideoWindow ()
		{
			this.Build ();
			frame = new AspectFrame (null, 0.5f, 0.5f, 1f, false);
			frame.Shadow = ShadowType.None;

			Window = new DrawingArea ();
			Window.DoubleBuffered = false;
			Window.ExposeEvent += HandleExposeEvent;
			videoeventbox.ButtonPressEvent += HandleButtonPressEvent;
			videoeventbox.ScrollEvent += HandleScrollEvent;
			videoeventbox.BorderWidth = 0;
#if OSTYPE_WINDOWS
			// Workaround for GTK bugs on Windows not showing the video window
			videoeventbox.VisibilityNotifyEvent += HandleVisibilityNotifyEvent;
#endif

			frame.Add (Window);
			videoeventbox.Add (frame);
			ShowAll ();
		}

		void HandleVisibilityNotifyEvent (object o, VisibilityNotifyEventArgs args)
		{
			if (videoeventbox.Visible && Window.GdkWindow != null) {
				// Hack for Windows. Force video window visibility as
				// EventBox window's might prevent it to be mapped again.
				Window.GdkWindow.Show ();
			}
		}

		public bool Ready {
			get;
			set;
		}

		void HandleScrollEvent (object o, ScrollEventArgs args)
		{
			if (ScrollEvent != null) {
				ScrollEvent (o, args);
			}
			
		}

		void HandleExposeEvent (object o, ExposeEventArgs args)
		{
			if (!Ready) {
				if (ReadyEvent != null) {
					ReadyEvent (o, null);
				}
				Ready = true;
			}
			if (ExposeEvent != null) {
				ExposeEvent (o, args);
			}
		}

		void HandleButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			if (ButtonPressEvent != null) {
				ButtonPressEvent (o, args);
			}
		}

		void HandleRealized (object sender, EventArgs e)
		{
			
		}
		
		public DrawingArea Window {
			get;
			protected set;
		}

		public float Ratio {
			set {
				frame.Ratio = value;
			}
		}
	}
}

