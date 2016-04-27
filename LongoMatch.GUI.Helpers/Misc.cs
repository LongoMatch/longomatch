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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gdk;
using Gtk;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using VAS.Core.Common;
using VAS.Core.Store;
using Color = Gdk.Color;
using LColor = VAS.Core.Common.Color;
using VAS.Core;

namespace LongoMatch.Gui.Helpers
{
	public class Misc
	{
		public static string lastFilename;
		public static Hashtable missingIcons = new Hashtable ();

		public static FileFilter GetFileFilter ()
		{
			FileFilter filter = new FileFilter ();
			filter.Name = "Images";
			filter.AddPattern ("*.png");
			filter.AddPattern ("*.jpg");
			filter.AddPattern ("*.jpeg");
			filter.AddPattern ("*.svg");
			return filter;
		}

		public static Pixbuf OpenImage (Widget widget)
		{
			Pixbuf pimage = null;
			StreamReader file;
			string filename;

			filename = Config.GUIToolkit.OpenFile (Catalog.GetString ("Choose an image"),
				null, null, "Images", new string[] { "*.png", "*.jpg", "*.jpeg", "*.svg" }); 
			if (filename != null) {
				// For Win32 compatibility we need to open the image file
				// using a StreamReader. Gdk.Pixbuf(string filePath) uses GLib to open the
				// input file and doesn't support Win32 files path encoding
				try {
					file = new StreamReader (filename);
					pimage = new Gdk.Pixbuf (file.BaseStream);
					file.Close ();
				} catch (Exception ex) {
					Log.Exception (ex);
					Config.GUIToolkit.ErrorMessage (Catalog.GetString ("Image file format not supported"), widget);
				}
			}
			return pimage;
		}

		public static Pixbuf Scale (Pixbuf pixbuf, int max_width, int max_height, bool dispose = true)
		{
			int ow, oh, h, w;

			h = ow = pixbuf.Height;
			w = oh = pixbuf.Width;
			ow = max_width;
			oh = max_height;

			if (w > max_width || h > max_height) {
				Pixbuf scalledPixbuf;
				double rate = (double)w / (double)h;
				
				if (h > w)
					ow = (int)(oh * rate);
				else
					oh = (int)(ow / rate);
				scalledPixbuf = pixbuf.ScaleSimple (ow, oh, Gdk.InterpType.Bilinear);
				if (dispose)
					pixbuf.Dispose ();
				return scalledPixbuf;
			} else {
				return pixbuf;
			}
		}

		static public double ShortToDouble (ushort val)
		{
			return (double)(val) / ushort.MaxValue;
		}

		static public double ByteToDouble (byte val)
		{
			return (double)(val) / byte.MaxValue;
		}

		public static Color ToGdkColor (LColor color)
		{
			return new Color (color.R, color.G, color.B);
		}

		public static LColor ToLgmColor (Color color, ushort alpha = ushort.MaxValue)
		{
			return LColor.ColorFromUShort (color.Red, color.Green, color.Blue, alpha);
		}

		public static ListStore FillImageFormat (ComboBox formatBox, List<VideoStandard> standards,
		                                         VideoStandard def)
		{
			ListStore formatStore;
			int index = 0, active = 0;
			
			formatStore = new ListStore (typeof(string), typeof(VideoStandard));
			foreach (VideoStandard std in standards) {
				formatStore.AppendValues (std.Name, std);
				if (std.Equals (def))
					active = index;
				index++;
			} 
			formatBox.Model = formatStore;
			formatBox.Active = active;
			return formatStore;
		}

		public static ListStore FillEncodingFormat (ComboBox encodingBox, EncodingProfile def)
		{
			ListStore encodingStore;
			int index = 0, active = 0;
			
			encodingStore = new ListStore (typeof(string), typeof(EncodingProfile));
			foreach (EncodingProfile prof in EncodingProfiles.Render) {
				encodingStore.AppendValues (prof.Name, prof);
				if (prof.Equals (def))
					active = index;
				index++;
			}
			encodingBox.Model = encodingStore;
			encodingBox.Active = active;
			return encodingStore;
		}

		public static ListStore FillQuality (ComboBox qualityBox, EncodingQuality def)
		{
			ListStore qualityStore;
			int index = 0, active = 0;
			
			qualityStore = new ListStore (typeof(string), typeof(EncodingQuality));
			foreach (EncodingQuality qual in EncodingQualities.All) {
				qualityStore.AppendValues (qual.Name, qual);
				if (qual.Equals (def)) {
					active = index;
				}
				index++;
			}
			qualityBox.Model = qualityStore;
			qualityBox.Active = active;
			return qualityStore;
		}

		/// <summary>
		/// Loads the missing icon for a given Gtk.IconSize.
		/// </summary>
		/// <returns>The missing icon. This function uses a cache internally.</returns>
		/// <param name="size">Size as a Gtk.IconSize.</param>
		public static Gdk.Pixbuf LoadMissingIcon (Gtk.IconSize size)
		{
			int sz, sy;
			global::Gtk.Icon.SizeLookup (size, out  sz, out  sy);
			return LoadMissingIcon (sz);
		}

		/// <summary>
		/// Loads the missing icon for a given size in pixels.
		/// </summary>
		/// <returns>The missing icon. This function uses a cache internally.</returns>
		/// <param name="sz">Size in pixels.</param>
		public static Gdk.Pixbuf LoadMissingIcon (int sz)
		{
			if (!missingIcons.ContainsKey (sz)) {
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
				missingIcons [sz] = Gdk.Pixbuf.FromDrawable (pmap, pmap.Colormap, 0, 0, 0, 0, sz, sz);
			}
			return (Gdk.Pixbuf)missingIcons [sz];
		}

		/// <summary>
		/// Loads the icon for a given name and size.
		/// </summary>
		/// <returns>The icon as a Gdk.Pixbuf or missing image icon if not found.</returns>
		/// <param name="name">Icon Name.</param>
		/// <param name="size">Icon Size in pixels.</param>
		/// <param name="flags">Lookup Flags like ForceSVG.</param>
		public static Gdk.Pixbuf LoadIcon (string name, int size, IconLookupFlags flags = IconLookupFlags.ForceSvg)
		{
			try {
				IconInfo icon_info = Gtk.IconTheme.Default.LookupIcon (name, size, flags);
				Gdk.Pixbuf res = new Gdk.Pixbuf (icon_info.Filename, size, size, true);
				return res;
			} catch (Exception e) {
				Log.Error (String.Format ("Icon {0} not found. Error: {1}", name, e.Message));
				return LoadMissingIcon (size);
			}
		}

		/// <summary>
		/// Loads the icon for a given name and size.
		/// </summary>
		/// <returns>The icon as a Gdk.Pixbuf or missing image icon if not found.</returns>
		/// <param name="name">Icon Name.</param>
		/// <param name="size">Icon Size as a Gtk.IconSize.</param>
		/// <param name="flags">Lookup Flags like ForceSVG.</param>
		public static Gdk.Pixbuf LoadIcon (string name, Gtk.IconSize size, IconLookupFlags flags = IconLookupFlags.ForceSvg)
		{
			int sz, sy;
			global::Gtk.Icon.SizeLookup (size, out  sz, out  sy);
			return LoadIcon (name, sz, flags);
		}

		/// <summary>
		/// Loads the stock icon for a given name and size.
		/// </summary>
		/// <returns>The stock icon.</returns>
		/// <param name="widget">Widget to get the icon for. Themes can modify the stock icon for a specific widget.</param>
		/// <param name="name">Name.</param>
		/// <param name="size">Size as Gtk.IconSize.</param>
		public static Gdk.Pixbuf LoadStockIcon (Gtk.Widget widget, string name, Gtk.IconSize size)
		{
			Gdk.Pixbuf res = widget.RenderIcon (name, size, null);
			if ((res != null)) {
				return res;
			} else {
				return LoadMissingIcon (size);
			}
		}

		static bool IsSkipedType (Widget w, Type[] skipTypes)
		{
			return skipTypes.Any (t => t.IsInstanceOfType (w));
		}

		public static void SetFocus (Container w, bool canFocus, params Type[] skipTypes)
		{
			if (IsSkipedType (w, skipTypes)) {
				return;
			}
			w.CanFocus = canFocus;
			foreach (Widget child in w.AllChildren) {
				if (child is Container) {
					SetFocus (child as Container, canFocus, skipTypes);
				} else {
					if (!IsSkipedType (child, skipTypes)) {
						child.CanFocus = canFocus;
					}
				}
			}
		}

		public static MediaFile DiscoverFile (string filename, object parent)
		{
			MediaFile mediaFile = null;
			IBusyDialog busy = null;

			try {
				Exception ex = null;
				busy = Config.GUIToolkit.BusyDialog (Catalog.GetString ("Analyzing video file:") + "\n" +
				filename, parent);
				System.Action action = () => {
					try {
						mediaFile = Config.MultimediaToolkit.DiscoverFile (filename);
					} catch (Exception e) {
						ex = e;
					}
				};
				busy.ShowSync (action);
				
				if (ex != null) {
					throw ex;
				} else if (mediaFile == null) {
					throw new Exception (Catalog.GetString ("Timeout parsing file."));
				} else if (!mediaFile.HasVideo || mediaFile.VideoCodec == "") {
					throw new Exception (Catalog.GetString ("This file doesn't contain a video stream."));
				} else if (mediaFile.HasVideo && mediaFile.Duration.MSeconds == 0) {
					throw new Exception (Catalog.GetString ("This file contains a video stream but its length is 0."));
				}
			} catch (Exception ex) {
				busy.Destroy ();
				Config.GUIToolkit.ErrorMessage (ex.Message, parent);
				return null;
			}
			return mediaFile;
		}

		public static MediaFile OpenFile (object parent)
		{
			MediaFile mediaFile;
			IGUIToolkit gui = Config.GUIToolkit;
			IMultimediaToolkit multimedia = Config.MultimediaToolkit;
			string filename;
			
			filename = gui.OpenFile (Catalog.GetString ("Open file"), null, null);
			if (filename == null)
				return null;
			
			mediaFile = DiscoverFile (filename, parent);
			if (mediaFile != null) {
				try {
					if (multimedia.FileNeedsRemux (mediaFile)) {
						// HACK: We only authorize remuxing in the pro version.
						if (!Config.SupportsFullHD) {
							string msg = Catalog.GetString ("This file is not in a supported format, " +
							             "convert it with the video conversion tool");
							throw new Exception (msg);
						} else {
							string q = Catalog.GetString ("This file needs to be converted into a more suitable format." +
							           "(This step will only take a few minutes)");
							if (gui.QuestionMessage (q, null, parent).Result) {
								string newFilename = multimedia.RemuxFile (mediaFile, parent);
								if (newFilename != null) {
									mediaFile = multimedia.DiscoverFile (newFilename);
								} else {
									mediaFile = null;
								}
							} else {
								mediaFile = null;
							}
						}
					}
				} catch (Exception ex) {
					gui.ErrorMessage (ex.Message, parent);
					mediaFile = null;
				}
			}
			return mediaFile;
		}

		public static bool RightButtonClicked (Gdk.EventButton evnt)
		{
			if (evnt.Type != Gdk.EventType.ButtonPress)
				return false;
#if OSTYPE_OS_X
			return evnt.Button == 3 || (evnt.Button == 1 && evnt.State == ModifierType.ControlMask);
#else
			return evnt.Button == 3;
#endif
		}
	}
}

