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
using System.Linq;
using System.IO;
using Gtk;
using Gdk;
using Mono.Unix;

using LongoMatch.Common;
using LColor = LongoMatch.Common.Color; 
using Color = Gdk.Color;
using System.Collections.Generic;

namespace LongoMatch.Gui.Helpers
{
	public class Misc
	{
		public static string lastFilename;
		
		public static FileFilter GetFileFilter() {
			FileFilter filter = new FileFilter();
			filter.Name = "Images";
			filter.AddPattern("*.png");
			filter.AddPattern("*.jpg");
			filter.AddPattern("*.jpeg");
			return filter;
		}

		public static Pixbuf OpenImage(Widget widget) {
			Gtk.Window toplevel = widget.Toplevel as Gtk.Window;
			Pixbuf pimage = null;
			StreamReader file;
			FileChooserDialog fChooser;
			string lastDir;
			
			 if (lastFilename != null) {
				lastDir = Path.GetDirectoryName (lastFilename);
			 } else {
				lastDir = Config.HomeDir;
			 }
			
			fChooser = new FileChooserDialog(Catalog.GetString("Choose an image"),
			                                 toplevel, FileChooserAction.Open,
			                                 "gtk-cancel",ResponseType.Cancel,
			                                 "gtk-open",ResponseType.Accept);
			fChooser.AddFilter(GetFileFilter());
			fChooser.SetCurrentFolder (lastDir);
			if(fChooser.Run() == (int)ResponseType.Accept)	{
				// For Win32 compatibility we need to open the image file
				// using a StreamReader. Gdk.Pixbuf(string filePath) uses GLib to open the
				// input file and doesn't support Win32 files path encoding
				lastFilename = fChooser.Filename;
				file = new StreamReader(fChooser.Filename);
				pimage= new Gdk.Pixbuf(file.BaseStream);
				file.Close();
			}
			fChooser.Destroy();
			return pimage;
		}
		
		public static Pixbuf Scale(Pixbuf pixbuf, int max_width, int max_height, bool dispose=true) {
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
				if (dispose)
					pixbuf.Dispose();
				return scalledPixbuf;
			} else {
				return pixbuf;
			}
		}
		
		static public double ShortToDouble (ushort val) {
			return (double) (val) / ushort.MaxValue;
		}
		
		static public double ByteToDouble (byte val) {
			return (double) (val) / byte.MaxValue;
		}
		
		public static Color ToGdkColor(LColor color) {
			return new Color (color.R, color.G, color.B);
		}
		
		public static LColor ToLgmColor(Color color) {
			return LColor.ColorFromUShort (color.Red, color.Green, color.Blue);
		}
		
		public static ListStore FillImageFormat (ComboBox formatBox, VideoStandard def) {
			ListStore formatStore;
			int index = 0, active = 0;
			
			formatStore = new ListStore(typeof(string), typeof (VideoStandard));
			foreach (VideoStandard std in VideoStandards.Rendering) {
				formatStore.AppendValues (std.Name, std);
				if (std.Equals(def))
					active = index;
				index ++;
			} 
			formatBox.Model = formatStore;
			formatBox.Active = active;
			return formatStore;
		}

		public static ListStore FillEncodingFormat (ComboBox encodingBox, EncodingProfile def) {
			ListStore encodingStore;
			int index = 0, active = 0;
			
			encodingStore = new ListStore(typeof(string), typeof (EncodingProfile));
			foreach (EncodingProfile prof in EncodingProfiles.Render) {
				encodingStore.AppendValues(prof.Name, prof);
				if (prof.Equals(def))
					active = index;
				index++;
			}
			encodingBox.Model = encodingStore;
			encodingBox.Active = active;
			return encodingStore;
		}
		
		public static ListStore FillQuality (ComboBox qualityBox, EncodingQuality def) {
			ListStore qualityStore;
			int index = 0, active = 0;
			
			qualityStore = new ListStore(typeof(string), typeof (EncodingQuality));
			foreach (EncodingQuality qual in EncodingQualities.All) {
				qualityStore.AppendValues(qual.Name, qual);
				if (qual.Equals(def)) {
					active = index;
				}
				index++;
			}
			qualityBox.Model = qualityStore;
			qualityBox.Active = active;
			return qualityStore;
		}
		
		public static Gdk.Pixbuf LoadIcon (Gtk.Widget widget, string name, Gtk.IconSize size)
		{
			Gdk.Pixbuf res = widget.RenderIcon (name, size, null);
			if ((res != null)) {
				return res;
			} else {
				int sz;
				int sy;
				global::Gtk.Icon.SizeLookup (size, out  sz, out  sy);
				try {
					return Gtk.IconTheme.Default.LoadIcon (name, sz, 0);
				} catch (System.Exception) {
					if ((name != "gtk-missing-image")) {
						return LoadIcon (widget, "gtk-missing-image", size);
					} else {
						Gdk.Pixmap pmap = new Gdk.Pixmap (Gdk.Screen.Default.RootWindow, sz, sz);
						Gdk.GC gc = new Gdk.GC (pmap);
						gc.RgbFgColor = new Gdk.Color (255, 255, 255);
						pmap.DrawRectangle (gc, true, 0, 0, sz, sz);
						gc.RgbFgColor = new Gdk.Color (0, 0, 0);
						pmap.DrawRectangle (gc, false, 0, 0, (sz - 1), (sz - 1));
						gc.SetLineAttributes (3, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
						gc.RgbFgColor = new Gdk.Color (255, 0, 0);
						pmap.DrawLine (gc, (sz / 4), (sz / 4), ((sz - 1) - (sz / 4)), ((sz - 1) - (sz / 4)));
						pmap.DrawLine (gc, ((sz - 1) - (sz / 4)), (sz / 4), (sz / 4), ((sz - 1) - (sz / 4)));
						return Gdk.Pixbuf.FromDrawable (pmap, pmap.Colormap, 0, 0, 0, 0, sz, sz);
					}
				}
			}
		}
		
		public static void DisableFocus (Container w, params Type[] skipTypes) {
			w.CanFocus = false;
			foreach (Widget child in w.AllChildren) {
				if (child is Container) {
					DisableFocus (child as Container);
				} else {
					if (!skipTypes.Contains (child.GetType())) {
						child.CanFocus = false;
					}
				}
			}
		}
	}
}

