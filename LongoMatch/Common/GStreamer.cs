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
using System.Runtime.InteropServices;
using Gtk;
using LongoMatch.Gui;

namespace LongoMatch.Common
{
	public class GStreamer
	{
		
		[DllImport("libgstreamer-0.10.dll") /* willfully unmapped */ ]
		static extern void gst_init (string argv);
		[DllImport("libgstreamer-0.10.dll") /* willfully unmapped */ ]
		static extern IntPtr gst_registry_get_default ();
		[DllImport("libgstreamer-0.10.dll") /* willfully unmapped */ ]
		static extern IntPtr gst_registry_lookup_feature (IntPtr raw, string name);
		[DllImport("libgstreamer-0.10.dll") /* willfully unmapped */ ]
		static extern void gst_object_unref (IntPtr raw);
		
		private const string GST_DIRECTORY = ".gstreamer-0.10";
		private const string REGISTRY_PATH = "registry.bin";
		
		public static void Init() {
			SetUpEnvironment ();
			gst_init ("");
		}
		
		public static bool CheckInstallation () {
			/* This check only makes sense on windows */
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				return true;
			
			if (!CheckBasicPlugins()) {
				HandleInstallationError();
				return false;
			}
			return true;
		}
		
		private static void SetUpEnvironment () {
			string gstDirectory, registryPath;
			
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				return;
			
			gstDirectory = GetGstDirectory();
			registryPath = GetRegistryPath();
			
			if (!Directory.Exists (gstDirectory))
				Directory.CreateDirectory (gstDirectory);
			
			/* Use a custom path for the registry in Windows */
			Environment.SetEnvironmentVariable("GST_REGISTRY", registryPath);
			Environment.SetEnvironmentVariable("GST_PLUGIN_PATH",MainClass.RelativeToPrefix("lib\\gstreamer-0.10"));
		}
		
		private static string GetGstDirectory () {
			return Path.Combine (MainClass.HomeDir(), GST_DIRECTORY);
		}
		
		private static string GetRegistryPath() {
			return Path.Combine (GetGstDirectory(), REGISTRY_PATH);
		}
		
		private static bool CheckBasicPlugins () {
			IntPtr registry = gst_registry_get_default();
			
			/* After software updates, sometimes the registry is not regenerated properly
			 * and plugins appears to be missing. We only check for a few plugins for now */
			if (!ElementExists (registry, "ffdec_h264"))
				return false;
			if (!ElementExists (registry, "d3dvideosink"))
				return false;
			return true;
		}
		
		private static bool ElementExists (IntPtr registry, string element_name) {
			bool ret = false;
			
			Console.WriteLine ("Checking plugin: " + element_name);
			var feature = gst_registry_lookup_feature (registry, element_name);
			if (feature != IntPtr.Zero){
				ret = true;
				gst_object_unref (feature);
			}
			Console.WriteLine ("Checking plugin: " + ret);
			return ret;
		}
		
		private static void HandleInstallationError () {
			File.Delete(GetRegistryPath());
			
			MessagePopup.PopupMessage(null, MessageType.Error, "An error has been detected in the current " +
				"installation.\n Try restarting " + Constants.SOFTWARE_NAME + " and contact with the " +
			    "development team if the problem persists.");
		}
	}
}

