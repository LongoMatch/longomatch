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
using Mono.Addins;
using System;
using System.IO;
using System.Runtime.InteropServices;
using LongoMatch.Addins.ExtensionPoints;
using VAS.Core.Common;
using VAS.Core;

namespace LongoMatch.Plugins.GStreamer
{
	[Extension]
	public class GStreamerRestricted: IGStreamerPluginsProvider
	{
		[DllImport ("libgstreamer-0.10.dll")]
		static extern bool gst_registry_scan_path (IntPtr registry, IntPtr path);

		[DllImport ("libgstreamer-0.10.dll")]
		static extern IntPtr gst_registry_get_default ();

		public string Name {
			get {
				return Catalog.GetString ("GStreamer open source plugins");
			}
		}

		public string Description {
			get {
				return Catalog.GetString ("GStreamer open source plugins with patents issues");
			}
		}

		public void RegisterPlugins ()
		{
			string gstdir = Path.Combine (Config.PluginsDir, "gstreamer-0.10");
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				Environment.SetEnvironmentVariable ("PATH",
					Environment.GetEnvironmentVariable ("PATH") + ";" + gstdir);
			}
			Log.Information ("Registering plugins in directory " + gstdir);
			IntPtr p = GLib.Marshaller.StringToPtrGStrdup (gstdir);
			IntPtr reg = gst_registry_get_default ();
			gst_registry_scan_path (reg, p);
			GLib.Marshaller.Free (p);
		}
	}
}
