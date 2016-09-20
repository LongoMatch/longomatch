//
//  Copyright (C) 2016 FLUENDO S.A.
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
using System.IO;
using LongoMatch.Core.Interfaces;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Serialization;
using Constants = LongoMatch.Core.Common.Constants;

namespace LongoMatch
{
	public class App : VAS.App
	{
		/* FIXME: We should split Config into:
		*  - App: store aplication states, default directories, etc...
		*  - Config: stores application configuration.
		*  - Move some constants such as SoftwareName in a way that they can be easilly configured by the apllication, maybe in the initialization function of the library.
		*  - Constants classes shouldn't be inheriting from other static classes of constants.
		*/

		/* State */
		public ITeamTemplatesProvider TeamTemplatesProvider;
		public ICategoriesTemplatesProvider CategoriesTemplatesProvider;

		new public static App Current {
			get {
				return (App)VAS.App.Current;
			}
			set {
				VAS.App.Current = value;
			}
		}

		public static void Init ()
		{
			App app = new App ();
			Init (app, "LGM_UNINSTALLED", Constants.SOFTWARE_NAME, Constants.PORTABLE_FILE, "LONGOMATCH_HOME");
			InitConstants ();
			Load ();
		}

		internal static void InitConstants ()
		{
			Current.Copyright = Constants.COPYRIGHT;
			Current.License = Constants.LICENSE;
			Current.SoftwareName = Constants.SOFTWARE_NAME;
			Current.SoftwareIconName = Constants.LOGO_ICON;
			Current.LatestVersionURL = Constants.LATEST_VERSION_URL;
			Current.DefaultDBName = Constants.DEFAULT_DB_NAME;
			Current.ProjectExtension = Constants.PROJECT_EXT;
			Current.LowerRate = 1;
			Current.UpperRate = 30;
			Current.RatePageIncrement = 3;
			Current.RateList = new List<double> { 0.04, 0.08, 0.12, 0.16, 0.20, 0.24, 0.28, 0.32, 0.36, 0.40, 0.44,
				0.48, 0.52, 0.56, 0.60, 0.64, 0.68, 0.72, 0.76, 0.80, 0.84, 0.88, 0.92, 0.96, 1, 2, 3, 4, 5
			};
			Current.DefaultRate = 25;
		}

		protected static void Load ()
		{
			if (File.Exists (Current.ConfigFile)) {
				Log.Information ("Loading config from " + Current.ConfigFile);
				try {
					Current.Config = Serializer.Instance.LoadSafe<Config> (Current.ConfigFile);
				} catch (Exception ex) {
					Log.Error ("Error loading config");
					Log.Exception (ex);
				}
			}

			if (Current.Config == null) {
				Log.Information ("Creating new config at " + Current.ConfigFile);
				Current.Config = new Config ();
				Current.Config.Save ();
			}
			Current.Background = Resources.LoadImage (Constants.BACKGROUND);
			Current.Config.CurrentDatabase = Constants.DEFAULT_DB_NAME;
		}

		Config config;

		public Config Config {
			get {
				return config;
			}
			set {
				config = value;
				VAS.App.Current.Config = config;
			}
		}

		new public string ConfigFile {
			get {
				string filename = Constants.SOFTWARE_NAME.ToLower () + "-1.0.config";
				return Path.Combine (App.Current.ConfigDir, filename);
			}
		}

		public string LogFile {
			get {
				string filename = Constants.SOFTWARE_NAME.ToLower () + ".log";
				return Path.Combine (App.Current.ConfigDir, filename);
			}
		}

		public string LibsDir {
			get {
				return RelativeToPrefix (Path.Combine ("lib",
					Constants.SOFTWARE_NAME.ToLower ()));
			}
		}

		public string PluginsDir {
			get {
				return RelativeToPrefix (String.Format ("lib/{0}/plugins",
					Constants.SOFTWARE_NAME.ToLower ()));
			}
		}

		public string PluginsConfigDir {
			get {
				return Path.Combine (configDirectory, "addins-1.0");
			}
		}

		public string AnalysisDir {
			get {
				return Path.Combine (DBDir, "analysis");
			}

		}

		public string TeamsDir {
			get {
				return Path.Combine (DBDir, "teams");
			}
		}
	}
}

