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
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Interfaces.Drawing;

namespace LongoMatch
{
	public class Config
	{
		public static string homeDirectory;
		public static string baseDirectory;
		public static string configDirectory;
		public static string dataDir;
		
		/* State */
		public static IGUIToolkit GUIToolkit;
		public static IMultimediaToolkit MultimediaToolkit;
		public static IDrawingToolkit DrawingToolkit;
		public static ITeamTemplatesProvider TeamTemplatesProvider;
		public static ICategoriesTemplatesProvider CategoriesTemplatesProvider;
		public static EventsBroker EventsBroker;
		
		public static IDataBaseManager DatabaseManager;
		public static IRenderingJobsManager RenderingJobsManger;
		
		public static StyleConf Style;

		static ConfigState state;
		
		public static void Load () {
			if (File.Exists(Config.ConfigFile)) {
				Log.Information ("Loading config from " + Config.ConfigFile);
				try {
					state = Serializer.LoadSafe<ConfigState>(Config.ConfigFile);
				} catch (Exception ex) {
					Log.Error ("Error loading config");
					Log.Exception (ex);
				}
			}
			
			if (state == null) {
				Log.Information ("Creating new config at " + Config.ConfigFile);
				state = new ConfigState();
				Save ();
			}
		}
		
		public static void Save () {
			try {
				Serializer.Save(state, Config.ConfigFile); 
			} catch (Exception ex) {
				Log.Error ("Errro saving config");
				Log.Exception (ex);
			}
		}
		
		public static string ConfigFile {
			get {
				string filename = Constants.SOFTWARE_NAME.ToLower() + ".config";
				return Path.Combine(Config.ConfigDir, filename);
			}
		}
		
		public static string HomeDir {
			get {
				return homeDirectory;
			}
		}

		public static string BaseDir {
			set {
				baseDirectory = value;
			}
		}
		
		public static string ConfigDir {
			set {
				configDirectory = value;
			}
			get {
				return configDirectory;
			}
		}
		
		public static string PlayListDir {
			get {
				return Path.Combine(homeDirectory, "playlists");
			}
		}

		public static string SnapshotsDir {
			get {
				return Path.Combine(homeDirectory, "snapshots");
			}
		}

		public static string TemplatesDir {
			get {
				return Path.Combine(homeDirectory, "templates");
			}
		}

		public static string VideosDir {
			get {
				return Path.Combine(homeDirectory, "videos");
			}
		}

		public static string TempVideosDir {
			get {
				return Path.Combine(configDirectory, "temp");
			}
		}

		public static string ImagesDir {
			get {
				return Path.Combine (dataDir, "images");
			}
		}
		
		public static string IconsDir {
			get {
				return Path.Combine (dataDir, "icons");
			}
		}
		
		public static string PluginsDir {
			get {
				return RelativeToPrefix(String.Format("lib/{0}/plugins",
				                                      Constants.SOFTWARE_NAME.ToLower()));
			}
		}
		
		public static string PluginsConfigDir {
			get {
				return Path.Combine(configDirectory, "addins");
			}
		}

		public static string DBDir {
			get {
				return Path.Combine(homeDirectory, "db");
			}
		}

		public static string AnalysisDir {
			get {
				return Path.Combine(DBDir, "analysis");
			}
			
		}
		public static string TeamsDir {
			get {
				return Path.Combine(DBDir, "teams");
			}
		}
		
		public static string RelativeToPrefix(string relativePath) {
			return Path.Combine(baseDirectory, relativePath);
		}
		
		#region Properties
		static public Image FieldBackground {
			get {
				return new Image (Path.Combine (Config.ImagesDir, Constants.FIELD_BACKGROUND));
			}
		}
		
		static public Image HalfFieldBackground {
			get {
				return new Image (Path.Combine (Config.ImagesDir, Constants.HALF_FIELD_BACKGROUND));
			}
		}
		
		static public Image HHalfFieldBackground {
			get {
				return new Image (Path.Combine (Config.ImagesDir, Constants.HHALF_FIELD_BACKGROUND));
			}
		}
		
		static public Image GoalBackground {
			get {
				return new Image (Path.Combine (Config.ImagesDir, Constants.GOAL_BACKGROUND));
			}
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
		
		public static string CurrentDatabase {
			get {
				return state.currentDatabase;
			}
			set {
				state.currentDatabase = value;
				Save ();
			}
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
			set{
				state.captureEncodingProfile = value;
				Save ();
				
			}
		}
		
		public static VideoStandard RenderVideoStandard {
			get {
				return state.renderVideoStandard;
			}
			set {
				state.renderVideoStandard = value;
				Save ();
			}
		}
		
		public static EncodingProfile RenderEncodingProfile {
			get {
				return state.renderEncodingProfile;
			}
			set{
				state.renderEncodingProfile = value;
				Save ();
				
			}
		}
		
		public static EncodingQuality CaptureEncodingQuality {
			get {
				return state.captureEncodingQuality;
			}
			set{
				state.captureEncodingQuality = value;
				Save ();
				
			}
		}
			
		public static EncodingQuality RenderEncodingQuality {
			get {
				return state.renderEncodingQuality;
			}
			set {
				state.renderEncodingQuality = value;
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
		
		public static bool OverlayTitle {
			get {
				return state.overlayTitle;
			}
			set {
				state.overlayTitle = value;
				Save ();
			}
		}
		
		public static bool EnableAudio {
			get {
				return state.enableAudio;
			}
			set {
				state.enableAudio = value;
				Save ();
			}
		}
		
		public static uint FPS_N {
			get {
				return state.fps_n;
			}
			set {
				state.fps_n = value;
				Save ();
			}
		}
		
		public static uint FPS_D {
			get {
				return state.fps_d;
			}
			set {
				state.fps_d = value;
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
		#endregion

	}
	
	[Serializable]
	public class ConfigState{
		public bool fastTagging;
		public bool autoSave;
		public string currentDatabase;
		public string lang;
		public uint fps_n;
		public uint fps_d;
		public VideoStandard captureVideoStandard;
		public VideoStandard renderVideoStandard;
		public EncodingProfile captureEncodingProfile;
		public EncodingProfile renderEncodingProfile;
		public EncodingQuality captureEncodingQuality;
		public EncodingQuality renderEncodingQuality;
		public bool overlayTitle;
		public bool enableAudio;
		public bool autorender;
		public string autorenderDir;
		public string lastRenderDir;
		public bool reviewPlaysInSameWindow;
		public string defaultTemplate;
		
		public ConfigState () {
			/* Set default values */
			fastTagging = false;
			currentDatabase = Constants.DEFAULT_DB_NAME;
			lang = null;
			autoSave = false;
			captureVideoStandard = VideoStandards.P480_16_9;
			captureEncodingProfile = EncodingProfiles.MP4;
			captureEncodingQuality = EncodingQualities.Medium;
			renderVideoStandard = VideoStandards.P720_16_9;
			renderEncodingProfile = EncodingProfiles.MP4;
			renderEncodingQuality = EncodingQualities.High;
			overlayTitle = true;
			enableAudio = false;
			fps_n = 25;
			fps_d = 1;
			autorender = false;
			autorenderDir = null;
			lastRenderDir = null;
			reviewPlaysInSameWindow = true;
			defaultTemplate = null;
		}
	}
}
