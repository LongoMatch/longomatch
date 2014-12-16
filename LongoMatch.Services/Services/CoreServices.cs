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
using LongoMatch;
using LongoMatch.DB;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using Mono.Unix;

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
				

		public static IProjectsImporter ProjectsImporter;
		#if OSTYPE_WINDOWS
		[DllImport("libglib-2.0-0.dll") /* willfully unmapped */ ]
		static extern bool g_setenv (String env, String val, bool overwrite);
		#endif
		public static void Init ()
		{
			Log.Debugging = Debugging;
			Log.Information ("Starting " + Constants.SOFTWARE_NAME);

			Config.Init ();

			/* Check default folders */
			CheckDirs ();
			
			/* Load user config */
			Config.Load ();
			
			if (Config.Lang != null) {
				Environment.SetEnvironmentVariable ("LANGUAGE", Config.Lang);
#if OSTYPE_WINDOWS
				g_setenv ("LANGUAGE", Config.Lang.Replace ("-", "_"), true);
#endif
			}
			
			/* Init internationalization support */
			Catalog.Init (Constants.SOFTWARE_NAME.ToLower (), Config.RelativeToPrefix ("share/locale"));

		}

		public static void Start (IGUIToolkit guiToolkit, IMultimediaToolkit multimediaToolkit)
		{
			Config.MultimediaToolkit = multimediaToolkit;
			Config.GUIToolkit = guiToolkit;
			Config.EventsBroker.QuitApplicationEvent += HandleQuitApplicationEvent;
			StartServices (guiToolkit, multimediaToolkit);
		}

		public static void StartServices (IGUIToolkit guiToolkit, IMultimediaToolkit multimediaToolkit)
		{
			ts = new TemplatesService ();
			Config.TeamTemplatesProvider = ts.TeamTemplateProvider;
			Config.CategoriesTemplatesProvider = ts.CategoriesTemplateProvider;

			/* Start DB services */
			dbManager = new DataBaseManager (Config.DBDir, guiToolkit);
			dbManager.SetActiveByName (Config.CurrentDatabase);
			Config.DatabaseManager = dbManager;
			
			/* Start the rendering jobs manager */
			videoRenderer = new RenderingJobsManager (multimediaToolkit, guiToolkit);
			Config.RenderingJobsManger = videoRenderer;
			
			projectsManager = new ProjectsManager (guiToolkit, multimediaToolkit, ts);
			
			/* State the tools manager */
			toolsManager = new ToolsManager (guiToolkit, dbManager);
			ProjectsImporter = toolsManager;
			
			/* Start the events manager */
			eManager = new EventsManager (guiToolkit, videoRenderer);
			
			/* Start the hotkeys manager */
			hkManager = new HotKeysManager ();

			/* Start playlists manager */
			plManager = new PlaylistManager (Config.GUIToolkit, videoRenderer);
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
