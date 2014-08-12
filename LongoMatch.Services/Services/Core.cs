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
using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Interfaces.Multimedia;
using Mono.Unix;

#if OSTYPE_WINDOWS
using System.Runtime.InteropServices;

#endif
namespace LongoMatch.Services
{
	public class Core
	{
		static DataBaseManager dbManager;
		static EventsManager eManager;
		static HotKeysManager hkManager;
		static RenderingJobsManager videoRenderer;
		public static IProjectsImporter ProjectsImporter;
		#if OSTYPE_WINDOWS
		[DllImport("libglib-2.0-0.dll") /* willfully unmapped */ ]
		static extern void g_setenv (String env, String val);

		
		
		
		
		
		
		
		
		
		
		
		#endif
		public static void Init ()
		{
			Log.Debugging = Debugging;
			Log.Information ("Starting " + Constants.SOFTWARE_NAME);

			SetupBaseDir ();

			/* Check default folders */
			CheckDirs ();
			
			/* Load user config */
			Config.Load ();
			
			if (Config.Lang != null) {
				Environment.SetEnvironmentVariable ("LANGUAGE", Config.Lang);
#if OSTYPE_WINDOWS
				g_setenv ("LANGUAGE", Config.Lang);
#endif
			}
			
			/* Init internationalization support */
			Catalog.Init (Constants.SOFTWARE_NAME.ToLower (), Config.RelativeToPrefix ("share/locale"));

		}

		public static void Start (IGUIToolkit guiToolkit, IMultimediaToolkit multimediaToolkit)
		{
			Config.MultimediaToolkit = multimediaToolkit;
			Config.GUIToolkit = guiToolkit;
			Config.EventsBroker.QuitApplicationEvent += () => Config.GUIToolkit.Quit ();
			StartServices (guiToolkit, multimediaToolkit);
		}

		public static void StartServices (IGUIToolkit guiToolkit, IMultimediaToolkit multimediaToolkit)
		{
			ProjectsManager projectsManager;
			PlaylistManager plManager;
			ToolsManager toolsManager;
			TemplatesService ts;
				
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
			toolsManager = new ToolsManager (guiToolkit, multimediaToolkit, ts);
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

		static void SetupBaseDir ()
		{
			string home;
			
			Config.baseDirectory = System.IO.Path.Combine (System.AppDomain.CurrentDomain.BaseDirectory, "../");
			if (!System.IO.Directory.Exists (System.IO.Path.Combine (Config.baseDirectory, "share", Constants.SOFTWARE_NAME))) {
				Config.baseDirectory = System.IO.Path.Combine (Config.baseDirectory, "../");
			}
			if (!System.IO.Directory.Exists (System.IO.Path.Combine (Config.baseDirectory, "share", Constants.SOFTWARE_NAME)))
				Log.Warning ("Prefix directory not found");
			
			/* Check for the magic file PORTABLE to check if it's a portable version
			 * and the config goes in the same folder as the binaries */
			if (File.Exists (System.IO.Path.Combine (Config.baseDirectory, Constants.PORTABLE_FILE)))
				home = Config.baseDirectory;
			else
				home = System.Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			
			Config.homeDirectory = System.IO.Path.Combine (home, Constants.SOFTWARE_NAME);
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				Config.configDirectory = Config.homeDirectory;
			else
				Config.configDirectory = System.IO.Path.Combine (home, "." +
					Constants.SOFTWARE_NAME.ToLower ());
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
	}
}
