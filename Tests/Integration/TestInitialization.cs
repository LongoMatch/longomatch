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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Services;
using Moq;
using NUnit.Framework;

namespace Tests.Integration
{
	[TestFixture ()]
	public class TestInitialization
	{
		Mock<IDrawingToolkit> drawingToolkitMock;
		Mock<IMultimediaToolkit> multimediaToolkitMock;
		Mock<IGUIToolkit> guiToolkitMock;
		string tmpPath, homePath;

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
			guiToolkitMock.Setup (g => g.RenderingStateBar).Returns (() => new Mock<IRenderingStateBar> ().Object);
		}

		[TearDown]
		public void Delete ()
		{
			CoreServices.Stop ();
			try {
				foreach (var db in Config.DatabaseManager.Databases) {
					db.Delete ();
				}
				Directory.Delete (tmpPath, true);
			} catch {
			}
		}

		[Test]
		public void TestInitializationFromScratch ()
		{
			CoreServices.Init ();
			Assert.AreEqual (homePath, Config.HomeDir);
			Assert.AreEqual (homePath, Config.ConfigDir);
			Assert.AreEqual (homePath, Directory.GetParent (Config.DBDir).ToString ());
			Assert.AreEqual (homePath, Directory.GetParent (Config.PlayListDir).ToString ());
			Assert.AreEqual (homePath, Directory.GetParent (Config.SnapshotsDir).ToString ());
			Assert.AreEqual (homePath, Directory.GetParent (Config.VideosDir).ToString ());

			AddinsManager.Initialize (Config.PluginsConfigDir, Config.PluginsDir);
			AddinsManager.LoadConfigModifierAddins ();

			Config.EventsBroker = new EventsBroker ();
			Config.DrawingToolkit = drawingToolkitMock.Object;
			Config.MultimediaToolkit = multimediaToolkitMock.Object;
			Config.GUIToolkit = guiToolkitMock.Object;
			AddinsManager.RegisterGStreamerPlugins ();
			AddinsManager.LoadExportProjectAddins (Config.GUIToolkit.MainController);
			AddinsManager.LoadMultimediaBackendsAddins (Config.MultimediaToolkit);
			AddinsManager.LoadUIBackendsAddins (Config.GUIToolkit);
			AddinsManager.LoadServicesAddins ();
			CoreServices.Start (Config.GUIToolkit, Config.MultimediaToolkit);

			// Check database dirs
			Assert.AreEqual (Path.Combine (homePath, "db"), Directory.GetParent (Config.TeamsDir).ToString ());
			Assert.AreEqual (Path.Combine (homePath, "db"), Directory.GetParent (Config.AnalysisDir).ToString ());
			Assert.AreEqual (1, Config.DatabaseManager.Databases.Count);

			AddinsManager.LoadDashboards (Config.CategoriesTemplatesProvider);
			AddinsManager.LoadImportProjectAddins (CoreServices.ProjectsImporter);

			// Check templates and db are initialized
			Assert.AreEqual (2, Config.TeamTemplatesProvider.Templates.Count);
			Assert.AreEqual (1, Config.CategoriesTemplatesProvider.Templates.Count);
			Assert.AreEqual (0, Config.DatabaseManager.ActiveDB.Count);

			CoreServices.Stop ();

			// Simulate an application restart
			CoreServices.Init ();
			CoreServices.Start (Config.GUIToolkit, Config.MultimediaToolkit);
			Assert.AreEqual (2, Config.TeamTemplatesProvider.Templates.Count);
			Assert.AreEqual (1, Config.CategoriesTemplatesProvider.Templates.Count);
			Assert.AreEqual (0, Config.DatabaseManager.ActiveDB.Count);
			CoreServices.Stop ();
		}
	}
}

