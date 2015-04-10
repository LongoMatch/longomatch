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

namespace LongoMatch.Services
{


	public class UpdatesNotifier
	{
		readonly Version currentVersion;
		Version latestVersion;

		const string UPDATE_INFO_URL="http://oneplay-cdn.fluendo.com/latest.json";
		string tempFile;
		string downloadURL;

		#region Constructors
		public UpdatesNotifier()
		{
			currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
			tempFile = Path.Combine(Config.HomeDir, "latest.json");

			var thread = new Thread(new ThreadStart(CheckForUpdates));
			thread.Start();
		}
		#endregion

		#region Private methods
		void FetchNewVersion() {
			var wb = new WebClient();
			try {
				wb.DownloadFile(UPDATE_INFO_URL,tempFile);
				var fileStream = new FileStream(tempFile, FileMode.Open);
				var sr = new StreamReader (fileStream);
				JObject latestObject = JsonConvert.DeserializeObject<JObject> (sr.ReadToEnd ());
				fileStream.Close ();

				latestVersion = new Version (latestObject["version"].Value<string> ());
				downloadURL = latestObject["url"].Value<string> ();
			}
			catch(Exception ex) {
				Log.Warning("Error processing version file: " + UPDATE_INFO_URL);
				Log.Exception (ex);
				latestVersion = currentVersion;
			}
		}

		bool ConectionExists() {
			try {
				Dns.GetHostEntry("oneplay-cdn.fluendo.com");
				return true;
			}
			catch {
				latestVersion = currentVersion;
				return false;
			}
		}

		bool IsOutDated() {
			if(latestVersion.Major > currentVersion.Major)
				return true;
			if(latestVersion.Minor > currentVersion.Minor)
				return true;
			if(latestVersion.Build > currentVersion.Build)
				return true;
			return false;
		}

		void CheckForUpdates() {
			if(ConectionExists())
				FetchNewVersion();
			Log.InformationFormat ("UpdatesNotifier: Current version is {0} and latest available is {1}",
				currentVersion, latestVersion);
			if(IsOutDated()) {
				Config.GUIToolkit.Invoke (delegate {
					Config.GUIToolkit.InfoMessage (
						string.Format (
							Catalog.GetString("Version {0} is available!\n" +
								"(You are using version {1})\n" +
								"<a href=\"{2}\">Click here to get it.</a>"),
							latestVersion, currentVersion, downloadURL));
				});
			}
		}
		#endregion
	}
}
