// Main.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using System.IO;
using System.Reflection;
using Gtk;
using LongoMatch.Addins;
using LongoMatch.Core.Common;
using LongoMatch.Drawing.Cairo;
using LongoMatch.Gui;
using LongoMatch.Gui.Helpers;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Multimedia.Utils;
using LongoMatch.Services;
using LongoMatch.Video;
using Mono.Unix;

namespace LongoMatch
{
	class MainClass
	{
		
		public static void Main (string[] args)
		{
			AddinsManager manager = null;
			CoreServices.Init ();

			InitGtk ();

			/* Init GStreamer */
			GStreamer.Init ();
			if (!GStreamer.CheckInstallation ())
				return;

			GLib.ExceptionManager.UnhandledException += new GLib.UnhandledExceptionHandler (OnException);
			Version version = Assembly.GetExecutingAssembly ().GetName ().Version;

			try {
				manager = new AddinsManager (Config.PluginsConfigDir, Config.PluginsDir);
				manager.LoadConfigModifierAddins ();
				Config.DrawingToolkit = new CairoBackend ();
				Config.EventsBroker = new EventsBroker ();
				IMultimediaToolkit multimediaToolkit = new MultimediaToolkit ();
				Config.MultimediaToolkit = multimediaToolkit;
				GUIToolkit guiToolkit = new GUIToolkit (version);
				manager.LoadExportProjectAddins (guiToolkit.MainController);
				manager.LoadMultimediaBackendsAddins (multimediaToolkit);
				try {
					CoreServices.Start (guiToolkit, multimediaToolkit);
				} catch (DBLockedException locked) {
					string msg = Catalog.GetString ("The database seems to be locked by another instance and " +
						"the application will closed.");
					guiToolkit.ErrorMessage (msg);
					Log.Exception (locked);
					return;
				}
				manager.LoadImportProjectAddins (CoreServices.ProjectsImporter);
				Application.Run ();
			} catch (Exception ex) {
				ProcessExecutionError (ex);
			} finally {
				if (manager != null) {
					try {
						manager.ShutdownMultimediaBackends ();
					} catch (Exception ex) {
					}
				}
			}
		}

		static void InitGtk ()
		{
			string dataDir, gtkRC, iconsDir, styleConf;
			IconTheme theme;
			
			if (Environment.GetEnvironmentVariable ("LGM_UNINSTALLED") != null) {
				dataDir = "../data";
			} else {
				dataDir = Path.Combine (Config.baseDirectory, "share",
				                        Constants.SOFTWARE_NAME.ToLower ());
			}
			Config.dataDir = dataDir;
			
			gtkRC = Path.Combine (dataDir, "theme", "gtk-2.0", "gtkrc");
			if (File.Exists (gtkRC)) {
				Rc.AddDefaultFile (gtkRC);
			}
			
			styleConf = Path.Combine (dataDir, "theme", "longomatch-dark.json");
			Config.Style = StyleConf.Load (styleConf);

			Application.Init ();

			iconsDir = Path.Combine (dataDir, "icons");
			if (Directory.Exists (iconsDir)) {
				IconTheme.Default.PrependSearchPath (iconsDir);
			}
			
		}

		private static void OnException (GLib.UnhandledExceptionArgs args)
		{
			ProcessExecutionError ((Exception)args.ExceptionObject);
		}

		private static void ProcessExecutionError (Exception ex)
		{
			string logFile = Constants.SOFTWARE_NAME + "-" + DateTime.Now + ".log";
			string message;

			logFile = logFile.Replace ("/", "-");
			logFile = logFile.Replace (" ", "-");
			logFile = logFile.Replace (":", "-");
			logFile = System.IO.Path.Combine (Config.HomeDir, logFile);

			message = SysInfo.PrintInfo (Assembly.GetExecutingAssembly ().GetName ().Version);
			if (ex.InnerException != null)
				message += String.Format ("{0}\n{1}\n{2}\n{3}\n{4}", ex.Message, ex.InnerException.Message, ex.Source, ex.StackTrace, ex.InnerException.StackTrace);
			else
				message += String.Format ("{0}\n{1}\n{2}", ex.Message, ex.Source, ex.StackTrace);

			using (StreamWriter s = new StreamWriter(logFile)) {
				s.WriteLine (message);
				s.WriteLine ("\n\n\nStackTrace:");
				s.WriteLine (System.Environment.StackTrace);
			}
			Log.Exception (ex);
			//TODO Add bug reports link
			MessagesHelpers.ErrorMessage (null,
			                              Catalog.GetString ("The application has finished with an unexpected error.") + "\n" +
				Catalog.GetString ("A log has been saved at: ") + logFile + "\n" +
				Catalog.GetString ("Please, fill a bug report "));

			Application.Quit ();
		}
	}
}
