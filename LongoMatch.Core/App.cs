//
//  Copyright (C) 2016 dfernandez
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
using LongoMatch.Core.Interfaces;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Serialization;
using Constants = LongoMatch.Core.Common.Constants;
using LongoMatch.Core.Common;

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

		//		App (Config config) : base (config)
		//		{
		//
		//		}

		public static App Current { get; set; }

		public static void Init ()
		{
			App app = new App ();
			VAS.App.Init (app, "LGM_UNINSTALLED", Constants.SOFTWARE_NAME, Constants.PORTABLE_FILE, "LONGOMATCH_HOME");
			Load (app);
		}

		internal static void Load (App app)
		{
			Current = app;
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
			Current.Copyright = Constants.COPYRIGHT;
			Current.License = Constants.LICENSE;
			Current.SoftwareName = Constants.SOFTWARE_NAME;
			Current.SoftwareIconName = Constants.LOGO_ICON;
			Current.LatestVersionURL = Constants.LATEST_VERSION_URL;
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

		#region Properties

		public Image Background {
			get;
			set;
		}

		public string Copyright {
			get;
			set;
		}

		public string License {
			get;
			set;
		}

		public string SoftwareName {
			get;
			set;
		}

		public string SoftwareIconName {
			get;
			set;
		}

		public bool SupportsMultiCamera {
			get;
			set;
		}

		public bool SupportsFullHD {
			get;
			set;
		}

		public bool SupportsActionLinks {
			get;
			set;
		}

		public bool SupportsZoom {
			get;
			set;
		}

		public string LatestVersionURL {
			get;
			set;
		}

		#endregion
	}
}

