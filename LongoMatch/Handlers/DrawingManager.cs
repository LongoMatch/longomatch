//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using Gdk;
using Gtk;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Popup;
using LongoMatch.Handlers;
namespace LongoMatch.Handlers
{


	public class DrawingManager
	{

		TransparentDrawingArea drawingArea;
		DrawingToolBox toolBox;

		public DrawingManager(DrawingToolBox toolBox, Widget targetWidget)
		{
			drawingArea = new TransparentDrawingArea(targetWidget);
			drawingArea.Hide();
			this.toolBox=toolBox;
			toolBox.ColorChanged += new ColorChangedHandler(OnColorChanged);
			toolBox.LineWidthChanged += new LineWidthChangedHandler(OnLineWidthChanged);
			toolBox.VisibilityChanged += new VisibilityChangedHandler(OnVisibilityChanged);
			toolBox.ClearDrawing += new ClearDrawingHandler(OnClearDrawing);
			toolBox.ToolsVisible=false;
		}

		public  void OnKeyPressEvent(object o, Gtk.KeyPressEventArgs args)
		{
			if(!toolBox.Visible)
				return;
			if(args.Event.Key== Gdk.Key.d) {
				drawingArea.ToggleGrab();
			}
			else if(args.Event.Key== Gdk.Key.c) {
				drawingArea.Clear();
			}
			else if(args.Event.Key== Gdk.Key.s) {
				drawingArea.ToggleVisibility();
			}
		}

		protected virtual void OnColorChanged(Gdk.Color color) {
			drawingArea.LineColor = color;
		}

		protected virtual void OnLineWidthChanged(int width) {
			drawingArea.LineWidth = width;
		}

		protected virtual void OnVisibilityChanged(bool visible) {
			drawingArea.Visible = visible;
			if(!visible)
				drawingArea.Clear();
		}

		protected virtual void OnClearDrawing() {
			drawingArea.Clear();
		}
	}
}