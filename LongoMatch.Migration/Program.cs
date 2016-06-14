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
using System.IO;
using VAS.Core.Common;
using Catalog = LongoMatch.Core.Catalog;

namespace LongoMatch.Migration
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			SetupBasedir ();
			Catalog.Init ("longomatch", LongoMatch.App.Current.RelativeToPrefix ("share/locale"));
			InitGtk ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Invoke (delegate {
				win.Load ();
			});
			Application.Run ();
		}

		static void SetupBasedir ()
		{
			string home, homeDirectory, baseDirectory, configDirectory;
			
			baseDirectory = System.IO.Path.Combine (System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
			home = System.Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			homeDirectory = System.IO.Path.Combine (home, "LongoMatch");
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				configDirectory = homeDirectory;
			else {
				configDirectory = System.IO.Path.Combine (home, "." + "longomatch");
			}
			LongoMatch.App.Current.ConfigDir = configDirectory;
			LongoMatch.App.Current.homeDirectory = homeDirectory;
			
			if (Environment.GetEnvironmentVariable ("LGM_UNINSTALLED") != null) {
				LongoMatch.App.Current.baseDirectory = ".";
				LongoMatch.App.Current.dataDir = "../../data";
			} else {
				LongoMatch.App.Current.baseDirectory = baseDirectory;
				LongoMatch.App.Current.dataDir = System.IO.Path.Combine (LongoMatch.App.Current.baseDirectory, "share", "longomatch");
			}
//			LongoMatch.App.Current.Load ();
			var styleConf = Path.Combine (App.Current.dataDir, "theme", "longomatch-dark.json");
			LongoMatch.App.Current.Style = StyleConf.Load (styleConf);
		}

		static	void InitGtk ()
		{
			string gtkRC, iconsDir;
			
			gtkRC = Path.Combine (App.Current.dataDir, "theme", "gtk-2.0", "gtkrc");
			if (File.Exists (gtkRC)) {
				Rc.AddDefaultFile (gtkRC);
			}
			
			Application.Init ();
			
			iconsDir = Path.Combine (App.Current.dataDir, "icons");
			if (Directory.Exists (iconsDir)) {
				IconTheme.Default.PrependSearchPath (iconsDir);
			}
		}
	}
}
