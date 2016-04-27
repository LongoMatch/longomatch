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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;
using Constants = LongoMatch.Core.Common.Constants;
using EventsBroker = LongoMatch.Core.Common.EventsBroker;
using Newtonsoft.Json;

namespace LongoMatch
{
	public class Config : VAS.Config
	{
		/* State */
		public static IGUIToolkit GUIToolkit;
		public static IMultimediaToolkit MultimediaToolkit;
		public static IDrawingToolkit DrawingToolkit;
		public static ITeamTemplatesProvider TeamTemplatesProvider;
		public static ICategoriesTemplatesProvider CategoriesTemplatesProvider;
		public static EventsBroker EventsBroker;

		public static IStorageManager DatabaseManager;
		public static IRenderingJobsManager RenderingJobsManger;

		static ConfigState stateLongomatch {
			get {
				return (state as ConfigState);
			}
			set {
				state = value;
			}
		}

		public static void Init ()
		{
			string home = null;

			if (Environment.GetEnvironmentVariable ("LGM_UNINSTALLED") != null) {
				Config.baseDirectory = Path.GetFullPath (".");
				Config.dataDir = "../data";
			} else {
				if (Utils.OS == OperatingSystemID.Android) {
					Config.baseDirectory = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				} else if (Utils.OS == OperatingSystemID.iOS) {
					Config.baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
				} else {
					Config.baseDirectory = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "../");
					if (!Directory.Exists (Path.Combine (Config.baseDirectory, "share", Constants.SOFTWARE_NAME))) {
						Config.baseDirectory = Path.Combine (Config.baseDirectory, "../");
					}
				}
				if (!Directory.Exists (Path.Combine (Config.baseDirectory, "share", Constants.SOFTWARE_NAME)))
					Log.Warning ("Prefix directory not found");
				Config.dataDir = Path.Combine (Config.baseDirectory, "share", Constants.SOFTWARE_NAME.ToLower ());
			}

			if (Utils.OS == OperatingSystemID.Android) {
				home = Config.baseDirectory;
			} else if (Utils.OS == OperatingSystemID.iOS) {
				home = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "..", "Library");
			} else {
				/* Check for the magic file PORTABLE to check if it's a portable version
					* and the config goes in the same folder as the binaries */
				if (File.Exists (Path.Combine (Config.baseDirectory, Constants.PORTABLE_FILE))) {
					home = Config.baseDirectory;
				} else {
					home = Environment.GetEnvironmentVariable ("LONGOMATCH_HOME");
					if (home != null && !Directory.Exists (home)) {
						try {
							Directory.CreateDirectory (home);
						} catch (Exception ex) {
							Log.Exception (ex);
							Log.Warning (String.Format ("LONGOMATCH_HOME {0} not found", home));
							home = null;
						}
					}
					if (home == null) {
						home = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
					}
				}
			}

			Config.homeDirectory = Path.Combine (home, Constants.SOFTWARE_NAME);
			Config.configDirectory = Config.homeDirectory;

			// Migrate old config directory the home directory so that OS X users can easilly find
			// log files and config files without having to access hidden folders
			if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
				string oldHome = Path.Combine (home, "." + Constants.SOFTWARE_NAME.ToLower ()); 
				string configFilename = Constants.SOFTWARE_NAME.ToLower () + "-1.0.config";
				string configFilepath = Path.Combine (oldHome, configFilename);
				if (File.Exists (configFilepath) && !File.Exists (Config.ConfigFile)) {
					try {
						File.Move (configFilepath, Config.ConfigFile);
					} catch (Exception ex) {
						Log.Exception (ex);
					}
				}
			}
		}

		public static void Load ()
		{
			if (File.Exists (Config.ConfigFile)) {
				Log.Information ("Loading config from " + Config.ConfigFile);
				try {
					state = Serializer.Instance.LoadSafe<ConfigState> (Config.ConfigFile);
				} catch (Exception ex) {
					Log.Error ("Error loading config");
					Log.Exception (ex);
				}
			}

			if (state == null) {
				Log.Information ("Creating new config at " + Config.ConfigFile);
				state = new ConfigState ();
				Save ();
			}
			Background = Resources.LoadImage (Constants.BACKGROUND);
			Copyright = Constants.COPYRIGHT;
			License = Constants.LICENSE;
			SoftwareName = Constants.SOFTWARE_NAME;
			SoftwareIconName = Constants.LOGO_ICON;
			LatestVersionURL = Constants.LATEST_VERSION_URL;
		}

		new public static void Save ()
		{
			try {
				Serializer.Instance.Save (state, Config.ConfigFile); 
			} catch (Exception ex) {
				Log.Error ("Error saving config");
				Log.Exception (ex);
			}
		}

		new public static string ConfigFile {
			get {
				string filename = Constants.SOFTWARE_NAME.ToLower () + "-1.0.config";
				return Path.Combine (Config.ConfigDir, filename);
			}
		}

		public static string LogFile {
			get {
				string filename = Constants.SOFTWARE_NAME.ToLower () + ".log";
				return Path.Combine (Config.ConfigDir, filename);
			}
		}

		public static string LibsDir {
			get {
				return RelativeToPrefix (Path.Combine ("lib",
					Constants.SOFTWARE_NAME.ToLower ()));
			}
		}

		public static string PluginsDir {
			get {
				return RelativeToPrefix (String.Format ("lib/{0}/plugins",
					Constants.SOFTWARE_NAME.ToLower ()));
			}
		}

		public static string PluginsConfigDir {
			get {
				return Path.Combine (configDirectory, "addins-1.0");
			}
		}

		//		public static string DBDir {
		//			get {
		//				return Path.Combine (homeDirectory, "db");
		//			}
		//		}

		public static string AnalysisDir {
			get {
				return Path.Combine (DBDir, "analysis");
			}

		}

		public static string TeamsDir {
			get {
				return Path.Combine (DBDir, "teams");
			}
		}

		#region Properties

		static public Image Background {
			get;
			set;
		}

		static public string Copyright {
			get;
			set;
		}

		static public string License {
			get;
			set;
		}

		static public string SoftwareName {
			get;
			set;
		}

		static public string SoftwareIconName {
			get;
			set;
		}

		static public bool SupportsMultiCamera {
			get;
			set;
		}

		static public bool SupportsFullHD {
			get;
			set;
		}

		static public bool SupportsActionLinks {
			get;
			set;
		}

		static public bool SupportsZoom {
			get;
			set;
		}

		static public string LatestVersionURL {
			get;
			set;
		}

		public static bool FastTagging {
			get {
				return state.fastTagging;
			}
			set {
				state.fastTagging = value;
				Save ();
			}
		}

		public static bool UseGameUnits {
			get;
			set;
		}

		public static string Lang {
			get {
				return state.lang;
			}
			set {
				state.lang = value;
				Save ();
			}
		}

		public static VideoStandard CaptureVideoStandard {
			get {
				return state.captureVideoStandard;
			}
			set {
				state.captureVideoStandard = value;
				Save ();
			}
		}

		public static EncodingProfile CaptureEncodingProfile {
			get {
				return state.captureEncodingProfile;
			}
			set {
				state.captureEncodingProfile = value;
				Save ();

			}
		}

		public static EncodingQuality CaptureEncodingQuality {
			get {
				return state.captureEncodingQuality;
			}
			set {
				state.captureEncodingQuality = value;
				Save ();

			}
		}

		public static bool AutoSave {
			get {
				return state.autoSave;
			}
			set {
				state.autoSave = value;
				Save ();
			}
		}

		public static bool AutoRenderPlaysInLive {
			get {
				return state.autorender;
			}
			set {
				state.autorender = value;
				Save ();
			}
		}

		public static string AutoRenderDir {
			get {
				return state.autorenderDir;
			}
			set {
				state.autorenderDir = value;
				Save ();
			}
		}

		public static string LastDir {
			get {
				return state.lastDir;
			}
			set {
				state.lastDir = value;
				Save ();
			}
		}

		public static string LastRenderDir {
			get {
				return state.lastRenderDir;
			}
			set {
				state.lastRenderDir = value;
				Save ();
			}
		}

		public static bool ReviewPlaysInSameWindow {
			get {
				return state.reviewPlaysInSameWindow;
			}
			set {
				state.reviewPlaysInSameWindow = value;
				Save ();
			}
		}

		public static string DefaultTemplate {
			get {
				return state.defaultTemplate;
			}
			set {
				state.defaultTemplate = value;
				Save ();
			}
		}

		public static Hotkeys Hotkeys {
			get {
				return stateLongomatch.hotkeys;
			}
			set {
				stateLongomatch.hotkeys = value;
				Save ();
			}
		}

		public static ProjectSortMethod ProjectSortMethod {
			get {
				return state.projectSortMethod;
			}
			set {
				state.projectSortMethod = value;
				Save ();
			}
		}

		public static Version IgnoreUpdaterVersion {
			get {
				return state.ignoreUpdaterVersion;
			}
			set {
				state.ignoreUpdaterVersion = value;
				Save ();
			}
		}

		#endregion

	}

	[Serializable]
	[JsonConverter (typeof(LongoMatchConverter))]
	public class ConfigState : VAS.ConfigState
	{
		public Hotkeys hotkeys;

		public ConfigState () : base ()
		{
			hotkeys = new Hotkeys ();
		}
	}
}
