//
//  Copyright (C) 2015 FLUENDO S.A.
//

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAS.Core.Common;

namespace LongoMatch.Services
{


	public class UpdatesNotifier: IService
	{
		static public bool FetchNewVersion (string url, string filename)
		{
			var userAgent = String.Format ("{0}/{1} ({2};{3};{4})",
				                Config.SoftwareName,
				                Config.Version,
				                Utils.OS,
				                Environment.OSVersion.VersionString,
				                Config.BuildVersion);
			try {
				var wb = new WebClient ();
				wb.Headers.Add ("User-Agent", userAgent);
				wb.DownloadFile (url, filename);
			} catch (Exception ex) {
				Log.WarningFormat ("UpdatesNotifier: Error downloading version file from {0} to {1} (User-Agent: {2})",
					url, filename, userAgent);
				Log.Exception (ex);
				return false;
			}
			Log.InformationFormat ("UpdatesNotifier: Downloaded latest version from {0} to {1} (User-Agent: {2})",
				url, filename, userAgent);
			return true;
		}

		static public bool ParseNewVersion (string filename, out Version latestVersion, out string downloadURL, out string changeLog)
		{
			latestVersion = null;
			downloadURL = null;
			changeLog = null;
			try {
				var fileStream = new FileStream (filename, FileMode.Open);
				var sr = new StreamReader (fileStream);
				JObject latestObject = JsonConvert.DeserializeObject<JObject> (sr.ReadToEnd ());
				fileStream.Close ();

				latestVersion = new Version (latestObject ["version"].Value<string> ());
				downloadURL = latestObject ["url"].Value<string> ();
				changeLog = latestObject ["changes"].Value<string> ();
			} catch (Exception ex) {
				Log.WarningFormat ("UpdatesNotifier: Error parsing version file {0}", filename);
				Log.Exception (ex);
				return false;
			}
			Log.InformationFormat ("UpdatesNotifier: Latest version is {0}", latestVersion);
			return true;
		}

		static public bool IsOutDated (Version currentVersion, Version latestVersion)
		{
			return latestVersion > currentVersion;
		}

		static public void CheckForUpdates ()
		{
			string tempFile = Path.Combine (Config.HomeDir, "latest.json");
			if (!FetchNewVersion (Config.LatestVersionURL, tempFile))
				return;

			Version latestVersion;
			string downloadURL;
			string changeLog;
			if (!ParseNewVersion (tempFile, out latestVersion, out downloadURL, out changeLog))
				return;

			Version currentVersion = Assembly.GetExecutingAssembly ().GetName ().Version;
			if (!IsOutDated (currentVersion, latestVersion)) {
				Log.InformationFormat ("UpdatesNotifier: Current version is {0} and latest available is {1}: Update not needed.",
					currentVersion, latestVersion);
				return;
			}
			Log.InformationFormat ("UpdatesNotifier: Current version is {0} and latest available is {1}: Update needed.",
				currentVersion, latestVersion);

			if (latestVersion == Config.IgnoreUpdaterVersion) {
				Log.InformationFormat ("UpdatesNotifier: Version {0} has been silenced. Not warning user about update.",
					latestVersion);
				return;
			}

			Config.GUIToolkit.Invoke (async delegate {
				bool ignore = await Config.GUIToolkit.NewVersionAvailable (currentVersion, latestVersion,
					              downloadURL, changeLog, null);
				if (ignore) {
					/* User requested to ignore this version */
					Log.InformationFormat ("UpdatesNotifier: Marking version {0} as silenced.", latestVersion);
					Config.IgnoreUpdaterVersion = latestVersion;
				}
			});
		}

		#region IService

		public int Level {
			get {
				return 90;
			}
		}

		public string Name {
			get {
				return "Updates notifier";
			}
		}

		public bool Start ()
		{
			var thread = new Thread (new ThreadStart (CheckForUpdates));
			thread.Start ();

			return true;
		}

		public bool Stop ()
		{
			return true;
		}

		#endregion
	}
}
