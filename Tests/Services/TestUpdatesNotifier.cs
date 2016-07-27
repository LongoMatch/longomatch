//
//  Copyright (C) 2015 Fluendo S.A.
//
using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using VAS.Services;

namespace Tests.Services
{
	[TestFixture ()]
	public class TestUpdatesNotifier
	{
		[Test ()]
		public void TestParser ()
		{
			// Extract the file from the resources
			string tmpFile = Path.GetTempFileName ();
			using (Stream resource = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("latest-test.json")) {
				using (Stream output = File.OpenWrite (tmpFile)) {
					resource.CopyTo (output);
				}
			}

			// Parse the file and check that the content is correct
			Version latestVersion;
			string downloadUrl;
			string changeLog;
			UpdatesNotifier.ParseNewVersion (tmpFile, out latestVersion, out downloadUrl, out changeLog);
			Assert.AreEqual (latestVersion.Major, 9);
			Assert.AreEqual (latestVersion.Minor, 8);
			Assert.AreEqual (latestVersion.Build, 7);
			Assert.AreEqual (downloadUrl, "test-url.com");
			Assert.AreEqual (changeLog, "none");
		}

		[Test ()]
		public void TestIsOutDated ()
		{
			Assert.IsTrue (UpdatesNotifier.IsOutDated (new Version ("1.0.2"), new Version ("1.0.3")));
			Assert.IsTrue (UpdatesNotifier.IsOutDated (new Version ("1.0.2"), new Version ("1.1.1")));
			Assert.IsTrue (UpdatesNotifier.IsOutDated (new Version ("1.9.2"), new Version ("2.0.1")));
			Assert.IsTrue (UpdatesNotifier.IsOutDated (new Version ("1.0.2"), new Version ("1.1")));
			Assert.IsTrue (UpdatesNotifier.IsOutDated (new Version ("1.0.2"), new Version ("3.0")));
			Assert.IsTrue (UpdatesNotifier.IsOutDated (new Version ("2.1"), new Version ("3.0")));
			Assert.IsTrue (UpdatesNotifier.IsOutDated (new Version ("1.1"), new Version ("1.2")));

			Assert.IsFalse (UpdatesNotifier.IsOutDated (new Version ("1.0.3"), new Version ("1.0.2")));
			Assert.IsFalse (UpdatesNotifier.IsOutDated (new Version ("1.1.1"), new Version ("1.0.2")));
			Assert.IsFalse (UpdatesNotifier.IsOutDated (new Version ("2.0.1"), new Version ("1.9.2")));
			Assert.IsFalse (UpdatesNotifier.IsOutDated (new Version ("1.1"), new Version ("1.0.2")));
			Assert.IsFalse (UpdatesNotifier.IsOutDated (new Version ("3.0"), new Version ("1.0.2")));
			Assert.IsFalse (UpdatesNotifier.IsOutDated (new Version ("3.0"), new Version ("2.1")));
			Assert.IsFalse (UpdatesNotifier.IsOutDated (new Version ("1.2"), new Version ("1.1")));
		}
	}
}

