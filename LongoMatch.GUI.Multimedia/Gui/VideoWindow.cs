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
using System.Runtime.InteropServices;
using Gtk;
using LongoMatch.Core.Interfaces.GUI;

namespace LongoMatch.Gui
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class VideoWindow : Gtk.Bin, IViewPort
	{
		AspectFrame frame;
		DrawingArea drawingWindow;

		public event EventHandler ReadyEvent;
		public new event ExposeEventHandler ExposeEvent;
		public new event ButtonPressEventHandler ButtonPressEvent;
		public new event ScrollEventHandler ScrollEvent;

		public VideoWindow ()
		{
			this.Build ();
			frame = new AspectFrame (null, 0.5f, 0.5f, 1f, false);
			frame.Shadow = ShadowType.None;

			messageLabel.NoShowAll = true;
			drawingWindow = new DrawingArea ();
			drawingWindow.DoubleBuffered = false;
			drawingWindow.ExposeEvent += HandleExposeEvent;
			videoeventbox.ButtonPressEvent += HandleButtonPressEvent;
			videoeventbox.ScrollEvent += HandleScrollEvent;
			videoeventbox.BorderWidth = 0;
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				// Workaround for GTK bugs on Windows not showing the video window
				videoeventbox.VisibilityNotifyEvent += HandleVisibilityNotifyEvent;
			}
			frame.Add (drawingWindow);
			videoeventbox.Add (frame);
			videoeventbox.ShowAll ();
			MessageVisible = false;
		}

		void HandleVisibilityNotifyEvent (object o, VisibilityNotifyEventArgs args)
		{
			if (videoeventbox.Visible && drawingWindow.GdkWindow != null) {
				// Hack for Windows. Force video window visibility as
				// EventBox window's might prevent it to be mapped again.
				drawingWindow.GdkWindow.Show ();
			}
		}

		public IntPtr WindowHandle {
			get {
				return GetWindowHandle (drawingWindow.GdkWindow);
			}
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

		public bool Ready {
			get;
			set;
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
			if (ButtonPressEvent != null) {
				ButtonPressEvent (this, args);
			}
		}

		void HandleRealized (object sender, EventArgs e)
		{
			
		}

		[DllImport ("libcesarplayer.dll")]
		static extern IntPtr lgm_get_window_handle (IntPtr window);

		IntPtr GetWindowHandle (Gdk.Window window)
		{
			return lgm_get_window_handle (window.Handle);
		}
	}
}

