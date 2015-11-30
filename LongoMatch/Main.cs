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
using System.Threading.Tasks;
using Gtk;
using LongoMatch.Addins;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.DB;
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
		[DllImport ("libX11", CallingConvention = CallingConvention.Cdecl)]
		private static extern int XInitThreads ();

		public static void Main (string[] args)
		{
			// Replace the current synchronization context with a GTK synchronization context
			// that continues tasks in the main UI thread instead of a random thread from the pool.
			SynchronizationContext.SetSynchronizationContext (new GtkSynchronizationContext ());
			GLib.ExceptionManager.UnhandledException += HandleException;
			CoreServices.Init ();
			InitGtk ();
			var splashScreen = new SplashScreen ();
			splashScreen.Show ();
			Application.Invoke (async (s, e) => await Init (splashScreen));
			Application.Run ();
		}

		static async Task Init (SplashScreen splashScreen)
		{
			IProgressReport progress = splashScreen;

			try {
				bool haveCodecs = false;
				Config.DrawingToolkit = new CairoBackend ();
				Config.MultimediaToolkit = new MultimediaToolkit ();
				Config.GUIToolkit = new GUIToolkit ();
				Config.GUIToolkit.Register<IPlayerView, PlayerView> (0);

				Task addinsTask = Task.Run (() => InitAddins (progress));
				Task gstInit = Task.Factory.StartNew (() => InitGStreamer (progress));

				// Wait until the addins are initialized to start the services
				await addinsTask;
				CoreServices.Start (Config.GUIToolkit, Config.MultimediaToolkit);
				AddinsManager.LoadDashboards (Config.CategoriesTemplatesProvider);
				AddinsManager.LoadImportProjectAddins (CoreServices.ProjectsImporter);

				// Migrate the old databases now that the DB and Templates services have started
				DatabaseMigration dbMigration = new DatabaseMigration (progress);
				Task dbInit = Task.Factory.StartNew (dbMigration.Start);

				// Wait for Migration and the GStreamer initialization
				try {
					await Task.WhenAll (gstInit, dbInit);
				} catch (AggregateException ae) {
					throw ae.Flatten ();
				}

				if (!AddinsManager.RegisterGStreamerPlugins ()) {
					ShowCodecsDialog ();
				}

				splashScreen.Destroy ();
				ConfigureOSXApp ();
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

		static void InitAddins (IProgressReport progress)
		{
			Guid id = Guid.NewGuid ();
			progress.Report (0.1f, "Initializing addins", id);
			AddinsManager.Initialize (Config.PluginsConfigDir, Config.PluginsDir);
			progress.Report (0.5f, "Addins parsed", id);
			AddinsManager.LoadConfigModifierAddins ();
			AddinsManager.LoadExportProjectAddins (Config.GUIToolkit.MainController);
			AddinsManager.LoadMultimediaBackendsAddins (Config.MultimediaToolkit);
			AddinsManager.LoadUIBackendsAddins (Config.GUIToolkit);
			AddinsManager.LoadServicesAddins ();
			progress.Report (1, "Addins initialized", id);
		}


		static void InitGStreamer (IProgressReport progress)
		{
			Guid id = Guid.NewGuid ();
			progress.Report (0.1f, "Initializing GStreamer", id);
			GStreamer.Init ();
			progress.Report (1f, "GStreamer initialized", id);
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

		static void ShowCodecsDialog ()
		{
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

		static void HandleException (GLib.UnhandledExceptionArgs args)
		{
			ProcessExecutionError ((Exception)args.ExceptionObject);
		}

		static void ProcessExecutionError (Exception ex)
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
