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
using LongoMatch.DB;
using LongoMatch.Gui;
using LongoMatch.Gui.Dialog;
using LongoMatch.Services;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Plugins;
using VAS.Core.Store;
using VAS.Drawing.Cairo;
using VAS.Multimedia.Utils;
using VAS.Services;
using VAS.UI.Dialog;
using VAS.UI.Helpers;
using VAS.Video;
using Constants = LongoMatch.Core.Common.Constants;
using VASUi = VAS.UI;

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
			App.Init ();
			CoreServices.Init ();
			InitGtk ();
			var splashScreen = new SplashScreen (Resources.LoadImage (Constants.SPLASH));
			splashScreen.Show ();
			Application.Invoke (async (s, e) => await Init (splashScreen));
			Application.Run ();
			try {
				AddinsManager.ShutdownMultimediaBackends ();
			} catch (Exception e) {
				Log.Exception (e);
			}
		}

		static async Task Init (SplashScreen splashScreen)
		{
			IProgressReport progress = splashScreen;

			try {
				bool haveCodecs = false;
				App.Current.DrawingToolkit = new CairoBackend ();
				App.Current.MultimediaToolkit = new MultimediaToolkit ();
				App.Current.GUIToolkit = GUIToolkit.Instance;
				App.Current.GUIToolkit.Register<IPlayerView, VASUi.PlayerView> (0);
				App.Current.Dialogs = VASUi.Dialogs.Instance;

				Task gstInit = Task.Factory.StartNew (() => InitGStreamer (progress));

				InitAddins (progress);
				CoreServices.RegisterService (new UpdatesNotifier ());
				CoreServices.Start (App.Current.GUIToolkit, App.Current.MultimediaToolkit);
				AddinsManager.LoadDashboards (App.Current.CategoriesTemplatesProvider);
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
				foreach (IProjectExporter exporter in
						 App.Current.Registry.RetrieveAll<IProjectExporter> (InstanceType.Default)) {
					(GUIToolkit.Instance.MainController as MainWindow).AddExportEntry (exporter.Description,
									new Func<Project, bool, Task> (exporter.Export));

				}
				App.Current.GUIToolkit.Welcome ();
			} catch (Exception ex) {
				ProcessExecutionError (ex);
			}
		}

		static void InitAddins (IProgressReport progress)
		{
			Guid id = Guid.NewGuid ();
			progress.Report (0.1f, "Initializing addins", id);
			AddinsManager.Initialize (App.Current.PluginsConfigDir, App.Current.PluginsDir);
			progress.Report (0.5f, "Addins parsed", id);
			AddinsManager.LoadConfigModifierAddins ();
			AddinsManager.LoadExportProjectAddins ();
			AddinsManager.LoadMultimediaBackendsAddins (App.Current.MultimediaToolkit);
			AddinsManager.LoadUIBackendsAddins (App.Current.GUIToolkit);
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
			if (Utils.OS == OperatingSystemID.OSX) {
				MenuItem quit;
				GtkOSXApplication app;

				app = new GtkOSXApplication ();
				MainWindow window = App.Current.GUIToolkit.MainController as MainWindow;
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
			Rc.AddDefaultFile (Utils.GetDataFilePath (Path.Combine ("theme", "gtk-2.0", "gtkrc")));
			App.Current.Style = StyleConf.Load (Utils.GetDataFilePath (Path.Combine ("theme", "longomatch-dark.json")));

			/* We are having some race condition with XCB resulting on an invalid
			 * message and thus an abort of the program, we better activate the
			 * thread sae X11
			 */
			if (Utils.OS == OperatingSystemID.Linux)
				XInitThreads ();

			Application.Init ();

			IconTheme.Default.PrependSearchPath (Utils.GetDataDirPath ("icons"));
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
			if (ex is AddinRequestShutdownException) {
				Application.Quit ();
				return;
			}

			string logFile = Constants.SOFTWARE_NAME + "-" + DateTime.Now + ".log";
			logFile = Utils.SanitizePath (logFile, ' ', ':');
			logFile = Path.Combine (App.Current.HomeDir, logFile);
			Log.Exception (ex);
			try {
				if (File.Exists (logFile)) {
					File.Delete (logFile);
				}
				File.Copy (App.Current.LogFile, logFile);
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
