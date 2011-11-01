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
using Gtk;
using Mono.Unix;

using LongoMatch.Gui;
using LongoMatch.DB;
using LongoMatch.Common;
using LongoMatch.Store;

namespace LongoMatch.Services
{
	public class Core
	{
		static DataBase db;
		static TemplatesService ts;
		static EventsManager eManager;
		static HotKeysManager hkManager;
		static MainWindow mainWindow;

		public static void Init()
		{
			Log.Debugging = Debugging;
			Log.Information("Starting " + Constants.SOFTWARE_NAME);

			SetupBaseDir();

			/* Init internationalization support */
			Catalog.Init(Constants.SOFTWARE_NAME.ToLower(),Config.RelativeToPrefix("share/locale"));

			/* Init Gtk */
			Application.Init();

			/* Check default folders */
			CheckDirs();
		}

		public static void Start(MainWindow mainWindow) {
			Core.mainWindow = mainWindow;
			StartServices(mainWindow);
			BindEvents(mainWindow);
		}
		
		public static void StartServices(MainWindow mainWindow){
			RenderingJobsManager videoRenderer;
			ProjectsManager projectsManager;
				
			/* Start TemplatesService */
			ts = new TemplatesService(Config.configDirectory);

			/* Start DB services */
			db = new DataBase(Path.Combine(Config.DBDir(),Constants.DB_FILE));
			
			/* Start the events manager */
			eManager = new EventsManager(mainWindow);

			/* Start the hotkeys manager */
			hkManager = new HotKeysManager();
			hkManager.newMarkEvent += eManager.OnNewTag;

			/* Start the rendering jobs manager */
			videoRenderer = new RenderingJobsManager(mainWindow.RenderingStateBar);
			mainWindow.NewJobEvent += (job) => {videoRenderer.AddJob(job);};
			
			projectsManager = new ProjectsManager(mainWindow);
			projectsManager.OpenedProjectChanged += OnOpenedProjectChanged;
		}
		
		public static void BindEvents(MainWindow mainWindow) {
			/* Connect player events */
			/* FIXME:
			player.Prev += OnPrev;
			player.Next += OnNext;
			player.Tick += OnTick;
			player.SegmentClosedEvent += OnSegmentClosedEvent;
			player.DrawFrame += OnDrawFrame;*/
		}

		public static void CheckDirs() {
			if(!System.IO.Directory.Exists(Config.HomeDir()))
				System.IO.Directory.CreateDirectory(Config.HomeDir());
			if(!System.IO.Directory.Exists(Config.TemplatesDir()))
				System.IO.Directory.CreateDirectory(Config.TemplatesDir());
			if(!System.IO.Directory.Exists(Config.SnapshotsDir()))
				System.IO.Directory.CreateDirectory(Config.SnapshotsDir());
			if(!System.IO.Directory.Exists(Config.PlayListDir()))
				System.IO.Directory.CreateDirectory(Config.PlayListDir());
			if(!System.IO.Directory.Exists(Config.DBDir()))
				System.IO.Directory.CreateDirectory(Config.DBDir());
			if(!System.IO.Directory.Exists(Config.VideosDir()))
				System.IO.Directory.CreateDirectory(Config.VideosDir());
			if(!System.IO.Directory.Exists(Config.TempVideosDir()))
				System.IO.Directory.CreateDirectory(Config.TempVideosDir());
		}

		public static DataBase DB {
			get {
				return db;
			}
		}
		
		public static TemplatesService TemplatesService {
			get {
				return ts;
			}
		}
		
		private static void OnOpenedProjectChanged (Project project, ProjectType projectType) {
			if (project != null) {
				hkManager.Categories=project.Categories;
				mainWindow.KeyPressEvent -= hkManager.KeyListener;
			} else {
				mainWindow.KeyPressEvent += hkManager.KeyListener;
			}
			
			eManager.OpenedProject = project;
			eManager.OpenedProjectType = projectType;
		}
		
		private static void SetupBaseDir() {
			string home;
			
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				Config.baseDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"../");
			}
			else
				Config.baseDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"../../");
			
			/* Check for the magic file PORTABLE to check if it's a portable version
			 * and the config goes in the same folder as the binaries */
			if (File.Exists(System.IO.Path.Combine(Config.baseDirectory, Constants.PORTABLE_FILE)))
				home = Config.baseDirectory;
			else
				home = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			
			Config.homeDirectory = System.IO.Path.Combine(home,Constants.SOFTWARE_NAME);
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				Config.configDirectory = Config.homeDirectory;
			else
				Config.configDirectory = System.IO.Path.Combine(home,".longomatch");
		}

		private static bool? debugging = null;	
		public static bool Debugging {
			get {
				if(debugging == null) {
					debugging = EnvironmentIsSet("LGM_DEBUG");
				}
				return debugging.Value;
			}
			set {
				debugging = value;
				Log.Debugging = Debugging;
			}
		}

		public static bool EnvironmentIsSet(string env)
		{
			return !String.IsNullOrEmpty(Environment.GetEnvironmentVariable(env));
		}
	}
}
