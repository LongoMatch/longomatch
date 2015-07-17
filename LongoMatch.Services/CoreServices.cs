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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using LongoMatch;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Services.Services;

#if OSTYPE_WINDOWS
using System.Runtime.InteropServices;

#endif
namespace LongoMatch.Services
{
	public class CoreServices
	{
		static DataBaseManager dbManager;
		static EventsManager eManager;
		static HotKeysManager hkManager;
		static RenderingJobsManager videoRenderer;
		static ProjectsManager projectsManager;
		static PlaylistManager plManager;
		static ToolsManager toolsManager;
		static TemplatesService ts;
		static UpdatesNotifier updatesNotifier;
		static List<IService> services = new List<IService> ();

		public static IProjectsImporter ProjectsImporter;
		#if OSTYPE_WINDOWS
		[DllImport("libglib-2.0-0.dll") /* willfully unmapped */ ]
		static extern bool g_setenv (String env, String val, bool overwrite);
		#endif
		public static void Init ()
		{
			Log.Debugging = Debugging;
			Log.Information ("Starting " + Constants.SOFTWARE_NAME);

			FillVersion ();

			Config.Init ();

			/* Check default folders */
			CheckDirs ();

			/* Redirects logs to a file */
			Log.SetLogFile (Config.LogFile);

			/* Load user config */
			Config.Load ();
			
			if (Config.Lang != null) {
				Environment.SetEnvironmentVariable ("LANGUAGE", Config.Lang.Replace ("-", "_"));
#if OSTYPE_WINDOWS
				g_setenv ("LANGUAGE", Config.Lang.Replace ("-", "_"), true);
#endif
			}
			InitTranslations ();

			/* Fill up the descriptions again after initializing the translations */
			Config.Hotkeys.FillActionsDescriptions ();
		}

		static void FillVersion ()
		{
			Assembly assembly = Assembly.GetExecutingAssembly ();
			FileVersionInfo info = FileVersionInfo.GetVersionInfo (assembly.Location); 
			Config.Version = assembly.GetName ().Version;
			Config.BuildVersion = info.ProductVersion;
			Log.Information (Utils.SysInfo);
		}

		public static void InitTranslations ()
		{
			string localesDir = Config.RelativeToPrefix ("share/locale");

			if (!Directory.Exists (localesDir)) {
				var cerbero_prefix = Environment.GetEnvironmentVariable ("CERBERO_PREFIX");
				if (cerbero_prefix != null) {
					localesDir = Path.Combine (cerbero_prefix, "share", "locale");
				} else {
					Log.ErrorFormat ("'{0}' does not exist. This looks like an uninstalled execution." +
					"Define CERBERO_PREFIX.", localesDir);
				}
			}
			/* Init internationalization support */
			Catalog.Init (Constants.SOFTWARE_NAME.ToLower (), localesDir);
		}

		public static void Start (IGUIToolkit guiToolkit, IMultimediaToolkit multimediaToolkit)
		{
			Config.MultimediaToolkit = multimediaToolkit;
			Config.GUIToolkit = guiToolkit;
			Config.EventsBroker.QuitApplicationEvent += HandleQuitApplicationEvent;
			RegisterServices (guiToolkit, multimediaToolkit);
			StartServices ();
		}

		public static void RegisterService (IService service)
		{
			Log.InformationFormat ("Registering service {0}", service.Name);
			services.Add (service);
		}

		public static void RegisterServices (IGUIToolkit guiToolkit, IMultimediaToolkit multimediaToolkit)
		{
			ts = new TemplatesService (new FileStorage (Config.DBDir));
			RegisterService (ts);
			Config.TeamTemplatesProvider = ts.TeamTemplateProvider;
			Config.CategoriesTemplatesProvider = ts.CategoriesTemplateProvider;

			/* Start DB services */
			dbManager = new DataBaseManager (Config.DBDir, guiToolkit);
			RegisterService (dbManager);
			Config.DatabaseManager = dbManager;

			/* Start the rendering jobs manager */
			videoRenderer = new RenderingJobsManager (multimediaToolkit, guiToolkit);
			RegisterService (videoRenderer);
			Config.RenderingJobsManger = videoRenderer;

			projectsManager = new ProjectsManager (guiToolkit, multimediaToolkit);
			RegisterService (projectsManager);

			/* State the tools manager */
			toolsManager = new ToolsManager (guiToolkit, dbManager);
			RegisterService (toolsManager);
			ProjectsImporter = toolsManager;

			/* Start the events manager */
			eManager = new EventsManager (guiToolkit, videoRenderer);
			RegisterService (eManager);

			/* Start the hotkeys manager */
			hkManager = new HotKeysManager ();
			RegisterService (hkManager);

			/* Start playlists manager */
			plManager = new PlaylistManager (Config.GUIToolkit, videoRenderer);
			RegisterService (plManager);

			/* Start the Update Notifier */
			updatesNotifier = new UpdatesNotifier ();
			RegisterService (updatesNotifier);
		}

		public static void StartServices ()
		{
			foreach (IService service in services.OrderBy (s => s.Level)) {
				if (service.Start ()) {
					Log.InformationFormat ("Started service {0} successfully", service.Name);
				} else {
					Log.InformationFormat ("Failed starting service {0}", service.Name);
				}
			}
		}

		public static void StopServices ()
		{
			foreach (IService service in services.OrderByDescending (s => s.Level)) {
				if (service.Stop ()) {
					Log.InformationFormat ("Stopped service {0} successfully", service.Name);
				} else {
					Log.InformationFormat ("Failed stopping service {0}", service.Name);
				}
			}
		}

		public static void CheckDirs ()
		{
			if (!System.IO.Directory.Exists (Config.HomeDir))
				System.IO.Directory.CreateDirectory (Config.HomeDir);
			if (!System.IO.Directory.Exists (Config.TemplatesDir))
				System.IO.Directory.CreateDirectory (Config.TemplatesDir);
			if (!System.IO.Directory.Exists (Config.SnapshotsDir))
				System.IO.Directory.CreateDirectory (Config.SnapshotsDir);
			if (!System.IO.Directory.Exists (Config.PlayListDir))
				System.IO.Directory.CreateDirectory (Config.PlayListDir);
			if (!System.IO.Directory.Exists (Config.DBDir))
				System.IO.Directory.CreateDirectory (Config.DBDir);
			if (!System.IO.Directory.Exists (Config.VideosDir))
				System.IO.Directory.CreateDirectory (Config.VideosDir);
			if (!System.IO.Directory.Exists (Config.TempVideosDir))
				System.IO.Directory.CreateDirectory (Config.TempVideosDir);
		}

		static bool? debugging = null;

		public static bool Debugging {
			get {
				if (debugging == null) {
					debugging = EnvironmentIsSet ("LGM_DEBUG");
				}
				return debugging.Value;
			}
			set {
				debugging = value;
				Log.Debugging = Debugging;
			}
		}

		public static bool EnvironmentIsSet (string env)
		{
			return !String.IsNullOrEmpty (Environment.GetEnvironmentVariable (env));
		}

		static void HandleQuitApplicationEvent ()
		{
			if (videoRenderer.PendingJobs.Count > 0) {
				string msg = Catalog.GetString ("A rendering job is running in the background. Do you really want to quit?");
				if (!Config.GUIToolkit.QuestionMessage (msg, null)) {
					return;
				}
			}
			Config.GUIToolkit.Quit ();
		}
	}
}
