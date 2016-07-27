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

using Image = VAS.Core.Common.Image;


namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class BackgroundWidget : Gtk.Bin
	{
		public BackgroundWidget ()
		{
			this.Build ();
			drawingarea.ExposeEvent += HandleExposeEvent;
		}

		public Pixbuf Background {
			get;
			set;
		}

		void HandleExposeEvent (object o, ExposeEventArgs args)
		{
			Pixbuf frame;
			int width, height, allocWidth, allocHeight, logoX, logoY;
			float ratio;
			
			if (Background == null)
				return;

			width = Background.Width;
			height = Background.Height;
			allocWidth = Allocation.Width;
			allocHeight = Allocation.Height;
			
			frame = new Pixbuf (Colorspace.Rgb, false, 8, this.Allocation.Width,
				this.Allocation.Height);
			
			ratio = Math.Min ((float)allocWidth / (float)width,
				(float)allocHeight / (float)height); 
				                       
			logoX = (int)((allocWidth / 2) - (width * ratio / 2));
			logoY = (int)((allocHeight / 2) - (height * ratio / 2));

			/* Scaling to available space */
			Background.Composite (frame, 0, 0, allocWidth, allocHeight,
				logoX, logoY, ratio, ratio,
				InterpType.Bilinear, 255);
			                       
			/* Drawing our frame */
			frame.RenderToDrawable (drawingarea.GdkWindow, Style.BlackGC, 0, 0,
				args.Event.Area.X, args.Event.Area.Y,
				args.Event.Area.Width, args.Event.Area.Height,
				RgbDither.Normal, args.Event.Area.X, args.Event.Area.Y);
			frame.Dispose ();
			return;
		}
	}
}

