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

namespace LongoMatch.Services
{


	public class UpdatesNotifier
	{
		readonly Version actual;
		Version update;

		const string UPDATE_INFO_URL="http://oneplay-cdn.fluendo.com/latest.json";
		string temp_file;
		string downloadURL;

		#region Constructors
		public UpdatesNotifier()
		{
			actual = Assembly.GetExecutingAssembly().GetName().Version;
			temp_file = Path.Combine(Config.HomeDir, "latest.json");

			var thread = new Thread(new ThreadStart(CheckForUpdates));
			thread.Start();
		}
		#endregion

		#region Private methods
		void FetchNewVersion() {
			var wb = new WebClient();
			try {
				wb.DownloadFile(UPDATE_INFO_URL,temp_file);
				var fileStream = new FileStream(temp_file, FileMode.Open);
				var sr = new StreamReader (fileStream);
				JObject latest = JsonConvert.DeserializeObject<JObject> (sr.ReadToEnd ());
				fileStream.Close ();

				update = new Version (latest["version"].Value<string> ());
				downloadURL = latest["url"].Value<string> ();
			}
			catch(Exception ex) {
				Console.WriteLine("Error downloading version file:\n"+ex);
				update = actual;
			}
		}

		bool ConectionExists() {
			try {
				Dns.GetHostEntry("oneplay-cdn.fluendo.com");
				return true;
			}
			catch {
				update = actual;
				return false;
			}
		}

		bool IsOutDated() {
			if(update.Major > actual.Major)
				return true;
			if(update.Minor > actual.Minor)
				return true;
			if(update.Build > actual.Build)
				return true;
			return false;
		}

		void CheckForUpdates() {
			if(ConectionExists())
				FetchNewVersion();
			if(update != null && IsOutDated()) {
				Config.GUIToolkit.Invoke (delegate {
					Config.GUIToolkit.InfoMessage (
						string.Format (
							Catalog.GetString("Version {0} is available!\n" +
								"(You are using version {1})\n" +
								"<a href=\"{2}/index.html\">Click here to get it.</a>"),
							update, actual, downloadURL));
				});
			}
		}
		#endregion
	}
}
