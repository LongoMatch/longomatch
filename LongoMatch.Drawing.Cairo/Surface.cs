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
using Cairo;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Common;

namespace LongoMatch.Drawing.Cairo
{
	public class Surface: ISurface
	{
		ImageSurface surface;

		public Surface (int width, int height, Image image)
		{
			surface = new ImageSurface (Format.ARGB32, width, height);
			if (image != null) {
				using (Context context = new Context(surface)) {
					Gdk.CairoHelper.SetSourcePixbuf (context, image.Value, 0, 0);
					context.Paint ();
				}
			}
		}

		public Surface (string filename)
		{
			surface = new ImageSurface (filename);
		}

		public object Value {
			get {
				return surface;
			}
		}

		public IContext Context {
			get {
				return new CairoContext (surface);
			}
		}

		public int Width {
			get {
				return surface.Width;
			}
		}
		
		public int Height {
			get {
				return surface.Height;
			}
		}
		
		public Image Copy ()
		{
			string tempFile = System.IO.Path.GetTempFileName ();
			surface.WriteToPng (tempFile);
			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (tempFile);
			return new Image (pixbuf);
		}

		public void Dispose ()
		{
			surface.Dispose ();
		}
	}
}

