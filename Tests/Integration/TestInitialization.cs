//
//  Copyright (C) 2015 Fluendo S.A.
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
using LongoMatch;
using LongoMatch.Addins;
using LongoMatch.Core.Store;
using LongoMatch.Services;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.License;
using VAS.Core.Interfaces.Multimedia;

namespace Tests.Integration
{
	[TestFixture ()]
	public class TestInitialization
	{
		Mock<IDrawingToolkit> drawingToolkitMock;
		Mock<IMultimediaToolkit> multimediaToolkitMock;
		Mock<IGUIToolkit> guiToolkitMock;
		Mock<ILicenseManager> mockLicenseManager;
		Mock<ILicenseStatus> mockLicenseStatus;
		Mock<ILicenseLimitationsService> mockLicenseLimitationService;
		string tmpPath, homePath;
		ILicenseManager currentLicenseManager;
		ILicenseLimitationsService currentLimitationService;

		[OneTimeSetUp]
		public void FixtureSetup ()
		{
			Directory.SetCurrentDirectory (TestContext.CurrentContext.TestDirectory);
			currentLicenseManager = App.Current.LicenseManager;
			currentLimitationService = App.Current.LicenseLimitationsService;
		}

		[OneTimeTearDown]
		public void TestFixtureTearDown ()
		{
			App.Current.LicenseManager = currentLicenseManager;
			App.Current.LicenseLimitationsService = currentLimitationService;
		}

		[SetUp]
		public void Init ()
		{
			tmpPath = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			homePath = Path.Combine (tmpPath, "LongoMatch");
			Directory.CreateDirectory (tmpPath);
			Environment.SetEnvironmentVariable ("LONGOMATCH_HOME", tmpPath);
			Environment.SetEnvironmentVariable ("LGM_UNINSTALLED", "1");
			drawingToolkitMock = new Mock<IDrawingToolkit> ();
			multimediaToolkitMock = new Mock<IMultimediaToolkit> ();
			guiToolkitMock = new Mock<IGUIToolkit> ();
			mockLicenseManager = new Mock<ILicenseManager> ();
			mockLicenseStatus = new Mock<ILicenseStatus> ();
			mockLicenseManager.SetupGet ((lm) => lm.LicenseStatus).Returns (mockLicenseStatus.Object);
			mockLicenseLimitationService = new Mock<ILicenseLimitationsService> ();
		}

		[TearDown]
		public void Delete ()
		{
			try {
				foreach (var db in App.Current.DatabaseManager.Databases) {
					db.Reset ();
				}
				Directory.Delete (tmpPath, true);
			} catch {
			}
			App.Current.StopServices ();
			SetupClass.SetUp ();
		}

		[Test]
		public void TestInitializationFromScratch ()
		{
			CoreServices.Init ();
			App.Init ();
			Assert.AreEqual (homePath, App.Current.HomeDir);
			Assert.AreEqual (homePath, App.Current.ConfigDir);
			Assert.AreEqual (homePath, Directory.GetParent (App.Current.DBDir).ToString ());
			Assert.AreEqual (homePath, Directory.GetParent (App.Current.PlayListDir).ToString ());
			Assert.AreEqual (homePath, Directory.GetParent (App.Current.SnapshotsDir).ToString ());
			Assert.AreEqual (homePath, Directory.GetParent (App.Current.VideosDir).ToString ());

			AddinsManager.Initialize (App.Current.PluginsConfigDir, App.Current.PluginsDir);

			App.Current.DrawingToolkit = drawingToolkitMock.Object;
			App.Current.MultimediaToolkit = multimediaToolkitMock.Object;
			App.Current.GUIToolkit = guiToolkitMock.Object;
			App.Current.LicenseManager = mockLicenseManager.Object;
			App.Current.LicenseLimitationsService = mockLicenseLimitationService.Object;

			App.Current.StartServices ();

			// Check database dirs
			Assert.AreEqual (Path.Combine (homePath, "db"), Directory.GetParent (App.Current.TeamsDir).ToString ());
			Assert.AreEqual (Path.Combine (homePath, "db"), Directory.GetParent (App.Current.AnalysisDir).ToString ());
			Assert.AreEqual (1, App.Current.DatabaseManager.Databases.Count);

			// Check templates and db are initialized
			Assert.AreEqual (2, App.Current.TeamTemplatesProvider.Templates.Count);
			Assert.AreEqual (1, App.Current.CategoriesTemplatesProvider.Templates.Count);
			Assert.AreEqual (0, App.Current.DatabaseManager.ActiveDB.Count<LMProject> ());

			App.Current.StopServices ();

			// Simulate an application restart
			CoreServices.Init ();
			App.Current.StartServices ();
			Assert.AreEqual (2, App.Current.TeamTemplatesProvider.Templates.Count);
			Assert.AreEqual (1, App.Current.CategoriesTemplatesProvider.Templates.Count);
			Assert.AreEqual (0, App.Current.DatabaseManager.ActiveDB.Count<LMProject> ());
		}
	}
}