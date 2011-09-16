// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using Gtk;
using Gdk;
using Mono.Unix;

namespace LongoMatch.Common
{
	public class ImageUtils
	{
		public static Pixbuf Scale(Pixbuf pixbuf, int max_width, int max_height) {
			int ow,oh,h,w;

			h = ow = pixbuf.Height;
			w = oh = pixbuf.Width;
			ow = max_width;
			oh = max_height;

			if(w>max_width || h>max_height) {
				Pixbuf scalledPixbuf;
				double rate = (double)w/(double)h;
				
				if(h>w)
					ow = (int)(oh * rate);
				else
					oh = (int)(ow / rate);
				scalledPixbuf = pixbuf.ScaleSimple(ow,oh,Gdk.InterpType.Bilinear);
				pixbuf.Dispose();
				return scalledPixbuf;
			} else {
				return pixbuf;
			}
		}
		
		public static byte[] Serialize(Pixbuf pixbuf) {
			return pixbuf.SaveToBuffer("png");
		}
		
		public static FileFilter GetFileFilter() {
			FileFilter filter = new FileFilter();
			filter.Name = "Images";
			filter.AddPattern("*.png");
			filter.AddPattern("*.jpg");
			filter.AddPattern("*.jpeg");
			return filter;
		}

		public static Pixbuf OpenImage(Gtk.Window toplevel) {
			Pixbuf pimage = null;
			StreamReader file;
			FileChooserDialog fChooser;
			
			fChooser = new FileChooserDialog(Catalog.GetString("Choose an image"),
			                                 toplevel, FileChooserAction.Open,
			                                 "gtk-cancel",ResponseType.Cancel,
			                                 "gtk-open",ResponseType.Accept);
			fChooser.AddFilter(GetFileFilter());
			if(fChooser.Run() == (int)ResponseType.Accept)	{
				// For Win32 compatibility we need to open the image file
				// using a StreamReader. Gdk.Pixbuf(string filePath) uses GLib to open the
				// input file and doesn't support Win32 files path encoding
				file = new StreamReader(fChooser.Filename);
				pimage= new Gdk.Pixbuf(file.BaseStream);
				file.Close();
			}
			fChooser.Destroy();
			return pimage;
		}
	}
}

