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
using System.Linq;
using LongoMatch.Store;
using LongoMatch.Common;
using LongoMatch.Interfaces.Drawing;
using System.Runtime.Remoting;
using LongoMatch.Store.Drawables;

namespace LongoMatch.Drawing
{
	public class Utils
	{
		public static double Round (double n, int multiple)
		{
			if (n % multiple > multiple / 2) {
				return RoundUp (n, multiple);
			} else {
				return RoundDown (n, multiple);
			}
		}

		public static double RoundUp (double n, int multiple)
		{
			return  (multiple - n % multiple) + n;
		}

		public static double RoundDown (double n, int multiple)
		{
			return n - n % multiple; 
		}

		public static double TimeToPos (Time time, double secondsPerPixel)
		{
			return (double)time.MSeconds / 1000 / secondsPerPixel;
		}

		public static Time PosToTime (Point p, double secondsPerPixel)
		{
			return new Time ((int)(p.X * 1000 * secondsPerPixel));
		}

		public static ICanvasSelectableObject CanvasFromDrawableObject (IBlackboardObject drawable)
		{
			string objecttype = String.Format ("LongoMatch.Drawing.CanvasObject.{0}Object",
			                                   drawable.GetType ().ToString ().Split ('.').Last ());
			ObjectHandle handle = Activator.CreateInstance (null, objecttype);
			ICanvasDrawableObject d = (ICanvasDrawableObject)handle.Unwrap ();
			d.IDrawableObject = drawable;
			return d;
		}

		protected static Image RenderFrameDrawing (IDrawingToolkit tk, int width, int height, FrameDrawing fd, Image image)
		{
			Image img;
			ISurface surface;
			
			surface = tk.CreateSurface (image.Width, image.Height, image);
			using (IContext c = surface.Context) {
				tk.Context = c;
				foreach (Drawable d in fd.Drawables) {
					ICanvasSelectableObject obj = CanvasFromDrawableObject (d);
					obj.Draw (tk, null);
				}
				tk.DrawImage (fd.Freehand);
			}
			img = surface.Copy ();
			surface.Dispose ();
			return img;
		}
		
		public static Image RenderFrameDrawing (IDrawingToolkit tk, int width, int height, FrameDrawing fd)
		{
			return RenderFrameDrawing (tk, width, height, fd, null);
		}
		
		public static Image RenderFrameDrawingToImage (IDrawingToolkit tk, Image image, FrameDrawing fd)
		{
			return RenderFrameDrawing (tk, image.Width, image.Height, fd, image);
		}
		
		public static Point ToUserCoords (Point p, Point offset, double scaleX, double scaleY)
		{
			return new Point ((p.X - offset.X) / scaleX,
			                  (p.Y - offset.Y) / scaleY);
		
		}
	}
}

