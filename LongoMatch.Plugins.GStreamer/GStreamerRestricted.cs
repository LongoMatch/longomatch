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
using System.Reflection;
using Mono.Addins;
using LongoMatch.Addins.ExtensionPoints;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Unix;

namespace LongoMatch.Plugins.GStreamer
{
	[Extension]
	public class GStreamerRestricted: IGStreamerPluginsProvider
	{
		string assemblyDir;

		[DllImport("libgstreamer-0.10.dll")]
		static extern bool gst_registry_scan_path (IntPtr registry, IntPtr path);

		[DllImport("libgstreamer-0.10.dll")]
		static extern IntPtr gst_registry_get_default ();

		public GStreamerRestricted ()
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			assemblyDir = Path.GetDirectoryName(path);
		}
		
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
			IntPtr p = GLib.Marshaller.StringToPtrGStrdup (Path.Combine (assemblyDir, "gstreamer-0.10"));
			IntPtr reg = gst_registry_get_default ();
			gst_registry_scan_path (reg, p);
			GLib.Marshaller.Free (p);
		}
	}
}
