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
using System.IO;
using LongoMatch.Core.Interfaces;
using VAS.Core.Common;
using Constants = LongoMatch.Core.Common.Constants;

namespace LongoMatch
{
	public class App : VAS.App
	{
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

		public static void Init (App app = null)
		{
			if (app == null) {
				app = new App ();
			}

			Init (app, Constants.SOFTWARE_NAME, Constants.LOGO_ICON, Constants.PORTABLE_FILE, "LONGOMATCH_HOME", "LGM");
			App.Current.InitConstants ();
			App.Current.DataDir.Add (Path.Combine (Path.GetFullPath ("."), "../data"));

			/* Redirects logs to a file */
			Log.SetLogFile (App.Current.LogFile);
			Log.Information ("Starting " + Constants.SOFTWARE_NAME);
			Log.Information (Utils.SysInfo);

			/* Fill up the descriptions again after initializing the translations */
			App.Current.Config.Hotkeys.FillActionsDescriptions ();
		}

		public new Config Config {
			get {
				return base.Config as Config;
			}
			set {
				base.Config = value;
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

		protected override VAS.Config CreateConfig ()
		{
			return new Config ();
		}

		internal void InitConstants ()
		{
			Current.Copyright = Constants.COPYRIGHT;
			Current.License = Constants.LICENSE;
			Current.LatestVersionURL = Constants.LATEST_VERSION_URL;
			Current.DefaultDBName = Constants.DEFAULT_DB_NAME;
			Current.ProjectExtension = Constants.PROJECT_EXT;
			Current.Website = Constants.WEBSITE;
			Current.Translators = Constants.TRANSLATORS;
		}
	}
}

