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
using System.Runtime.InteropServices;
using System.Threading;
using Gtk;
using LongoMatch.Addins;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Drawing.Cairo;
using LongoMatch.Gui;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Helpers;
using LongoMatch.Multimedia.Utils;
using LongoMatch.Services;
using LongoMatch.Video;
using Mono.Unix;

namespace LongoMatch
{
	class MainClass
	{
		[DllImport("libX11", CallingConvention=CallingConvention.Cdecl)]
		private static extern int XInitThreads();

		public static void Main (string[] args)
		{
			// Replace the current synchronization context with a GTK synchronization context
			// that continues tasks in the main UI thread instead of a random thread from the pool.
			SynchronizationContext.SetSynchronizationContext (new GtkSynchronizationContext ());
			GLib.ExceptionManager.UnhandledException += new GLib.UnhandledExceptionHandler (OnException);

			CoreServices.Init ();
			InitGtk ();
			var splashScreen = new SplashScreen ();
			splashScreen.Show ();
			Application.Invoke ((s, e) => Init (splashScreen));
			Application.Run ();
		}

		static void Init (SplashScreen splashScreen)
		{
			IProgressReport progress = splashScreen;

			/* Init GStreamer */
			GStreamer.Init ();

			try {
				AddinsManager.Initialize (Config.PluginsConfigDir, Config.PluginsDir);
				AddinsManager.LoadConfigModifierAddins ();
				Config.DrawingToolkit = new CairoBackend ();
				Config.MultimediaToolkit = new MultimediaToolkit ();
				Config.GUIToolkit = new GUIToolkit ();
				bool haveCodecs = AddinsManager.RegisterGStreamerPlugins ();
				AddinsManager.LoadExportProjectAddins (Config.GUIToolkit.MainController);
				AddinsManager.LoadMultimediaBackendsAddins (Config.MultimediaToolkit);
				AddinsManager.LoadUIBackendsAddins (Config.GUIToolkit);
				AddinsManager.LoadServicesAddins ();
				Config.GUIToolkit.Register<IPlayerView, PlayerView> (0);
				if (!haveCodecs) {
					CodecsChoiceDialog ccd = new CodecsChoiceDialog ();
					int response = ccd.Run ();
					if (response == (int)ResponseType.Accept) {
						try {
							System.Diagnostics.Process.Start (Constants.WEBSITE);
						} catch {
						}
					}
					ccd.Destroy ();
				}
				try {
					CoreServices.Start (Config.GUIToolkit, Config.MultimediaToolkit);
				} catch (DBLockedException locked) {
					string msg = Catalog.GetString ("The database seems to be locked by another instance and " +
					             "the application will be closed.");
					Config.GUIToolkit.ErrorMessage (msg);
					Log.Exception (locked);
					return;
				}
				AddinsManager.LoadDashboards (Config.CategoriesTemplatesProvider);
				AddinsManager.LoadImportProjectAddins (CoreServices.ProjectsImporter);
				ConfigureOSXApp ();
				splashScreen.Destroy ();
				Config.GUIToolkit.Welcome ();
			} catch (AddinRequestShutdownException arse) {
				// Abort gracefully
			} catch (Exception ex) {
				ProcessExecutionError (ex);
			} finally {
				try {
					AddinsManager.ShutdownMultimediaBackends ();
				} catch {
				}
			}
		}

		static void ConfigureOSXApp ()
		{
			if (Utils.RunningPlatform () == PlatformID.MacOSX) {
				MenuItem quit;
				GtkOSXApplication app;

				app = new GtkOSXApplication ();
				MainWindow window = Config.GUIToolkit.MainController as MainWindow;
				app.NSApplicationBlockTermination += (o, a) => {
					a.RetVal = window.CloseAndQuit ();
				};

				quit = window.QuitMenu;
				quit.Visible = false;
				app.SetMenuBar (window.Menu);
				app.InsertAppMenuItem (window.AboutMenu, 0);
				app.InsertAppMenuItem (new SeparatorMenuItem (), 1);
				app.InsertAppMenuItem (window.PreferencesMenu, 2);
				window.Menu.Visible = false;
				app.UseQuartzAccelerators = false;
				app.Ready ();
			}
		}

		static void InitGtk ()
		{
			string gtkRC, iconsDir, styleConf;
			
			gtkRC = Path.Combine (Config.dataDir, "theme", "gtk-2.0", "gtkrc");
			if (File.Exists (gtkRC)) {
				Rc.AddDefaultFile (gtkRC);
			}
			
			styleConf = Path.Combine (Config.dataDir, "theme", "longomatch-dark.json");
			Config.Style = StyleConf.Load (styleConf);

			/* We are having some race condition with XCB resulting on an invalid
			 * message and thus an abort of the program, we better activate the
			 * thread sae X11
			 */
			if (Utils.RunningPlatform () == PlatformID.Unix)
				XInitThreads ();

			Application.Init ();

			iconsDir = Path.Combine (Config.dataDir, "icons");
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
			logFile = Utils.SanitizePath (logFile, ' ', ':');
			logFile = Path.Combine (Config.HomeDir, logFile);
			Log.Exception (ex);
			try {
				if (File.Exists (logFile)) {
					File.Delete (logFile);
				}
				File.Copy (Config.LogFile, logFile);
			} catch (Exception ex1) {
				Log.Exception (ex1);
			}

			MessagesHelpers.ErrorMessage (null,
				Catalog.GetString ("The application has finished with an unexpected error.") + "\n" +
				Catalog.GetString ("A log has been saved at: ") +
				"<a href=\"" + logFile + "\">" + logFile + "</a>\n" +
				Catalog.GetString ("Please, fill a bug report "));
			Application.Quit ();
		}
	}
}
