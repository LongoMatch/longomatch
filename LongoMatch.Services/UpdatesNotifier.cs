//
//  Copyright (C) 2015 FLUENDO S.A.
//

using System;
using System.IO;
using System.Reflection;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Mono.Unix;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;

namespace LongoMatch.Services
{


	public class UpdatesNotifier: IService
	{
		readonly Version currentVersion;
		Version latestVersion;

		string tempFile;
		string downloadURL;
		string changeLog;

		#region Constructors

		public UpdatesNotifier ()
		{
			currentVersion = Assembly.GetExecutingAssembly ().GetName ().Version;
			tempFile = Path.Combine (Config.HomeDir, "latest.json");
		}

		#endregion

		#region Private methods

		void FetchNewVersion ()
		{
			var wb = new WebClient ();
			try {
				wb.DownloadFile (Config.LatestVersionURL, tempFile);
				var fileStream = new FileStream (tempFile, FileMode.Open);
				var sr = new StreamReader (fileStream);
				JObject latestObject = JsonConvert.DeserializeObject<JObject> (sr.ReadToEnd ());
				fileStream.Close ();
				Log.InformationFormat ("UpdatesNotifier: Got latest version from {0}",
					Config.LatestVersionURL);

				latestVersion = new Version (latestObject ["version"].Value<string> ());
				downloadURL = latestObject ["url"].Value<string> ();
				changeLog = latestObject["changes"].Value<string> ();
			} catch (Exception ex) {
				Log.Warning ("Error processing version file: " + Config.LatestVersionURL);
				Log.Exception (ex);
				latestVersion = currentVersion;
			}
		}

		bool IsOutDated ()
		{
			if (latestVersion.Major > currentVersion.Major)
				return true;
			if (latestVersion.Minor > currentVersion.Minor)
				return true;
			if (latestVersion.Build > currentVersion.Build)
				return true;
			return false;
		}

		void CheckForUpdates ()
		{
			FetchNewVersion ();
			Log.InformationFormat ("UpdatesNotifier: Current version is {0} and latest available is {1}",
				currentVersion, latestVersion);
			if (latestVersion == Config.IgnoreUpdaterVersion) {
				Log.InformationFormat ("UpdatesNotifier: Version {0} has been silenced. Not warning user about update.",
					latestVersion);
				return;
			}
			if (IsOutDated ()) {
				Config.GUIToolkit.Invoke (delegate {
					bool ignore = Config.GUIToolkit.NewVersionAvailable (currentVersion, latestVersion,
						downloadURL, changeLog, null);
					if (ignore) {
						/* User requested to ignore this version */
						Log.InformationFormat ("UpdatesNotifier: Marking version {0} as silenced.", latestVersion);
						Config.IgnoreUpdaterVersion = latestVersion;
					}
				});
			}
		}

		#endregion

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
