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
using VAS.Core.Interfaces.Drawing;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Interfaces.GUI;
using LongoMatch.Services;
using LongoMatch.Addins;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;

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
			guiToolkitMock.Setup (g => g.RenderingStateBar).Returns (() =>
				new Mock<IRenderingStateBar> ().Object
			);
		}

		[TearDown]
		public void Delete ()
		{
			CoreServices.Stop ();
			try {
				foreach (var db in App.Current.DatabaseManager.Databases) {
					db.Reset ();
				}
				Directory.Delete (tmpPath, true);
			} catch {
			}
			SetupClass.Initialize ();
		}

		[Test]
		public void TestInitializationFromScratch ()
		{
			try {
				CoreServices.Init ();
				Assert.AreEqual (homePath, App.Current.HomeDir);
				Assert.AreEqual (homePath, App.Current.ConfigDir);
				Assert.AreEqual (homePath, Directory.GetParent (App.Current.DBDir).ToString ());
				Assert.AreEqual (homePath, Directory.GetParent (App.Current.PlayListDir).ToString ());
				Assert.AreEqual (homePath, Directory.GetParent (App.Current.SnapshotsDir).ToString ());
				Assert.AreEqual (homePath, Directory.GetParent (App.Current.VideosDir).ToString ());

				AddinsManager.Initialize (App.Current.PluginsConfigDir, App.Current.PluginsDir);
				AddinsManager.LoadConfigModifierAddins ();

				App.Current.DrawingToolkit = drawingToolkitMock.Object;
				App.Current.MultimediaToolkit = multimediaToolkitMock.Object;
				App.Current.GUIToolkit = guiToolkitMock.Object;
				AddinsManager.RegisterGStreamerPlugins ();
				AddinsManager.LoadExportProjectAddins ();
				AddinsManager.LoadMultimediaBackendsAddins (App.Current.MultimediaToolkit);
				AddinsManager.LoadUIBackendsAddins (App.Current.GUIToolkit);
				AddinsManager.LoadServicesAddins ();

				IRenderingStateBar rr = App.Current.GUIToolkit.RenderingStateBar;
				IMultimediaToolkit im = App.Current.MultimediaToolkit;

				CoreServices.Start (App.Current.GUIToolkit, App.Current.MultimediaToolkit);
				//CoreServices.Start (null, null);

				// Check database dirs
				Assert.AreEqual (Path.Combine (homePath, "db"), Directory.GetParent (App.Current.TeamsDir).ToString ());
				Assert.AreEqual (Path.Combine (homePath, "db"), Directory.GetParent (App.Current.AnalysisDir).ToString ());
				Assert.AreEqual (1, App.Current.DatabaseManager.Databases.Count);

				AddinsManager.LoadDashboards (App.Current.CategoriesTemplatesProvider);
				AddinsManager.LoadImportProjectAddins (CoreServices.ProjectsImporter);

				// Check templates and db are initialized
				Assert.AreEqual (2, App.Current.TeamTemplatesProvider.Templates.Count);
				Assert.AreEqual (1, App.Current.CategoriesTemplatesProvider.Templates.Count);
				Assert.AreEqual (0, App.Current.DatabaseManager.ActiveDB.Count<ProjectLongoMatch> ());

				CoreServices.Stop ();

				// Simulate an application restart
				CoreServices.Init ();
				CoreServices.Start (App.Current.GUIToolkit, App.Current.MultimediaToolkit);
				Assert.AreEqual (2, App.Current.TeamTemplatesProvider.Templates.Count);
				Assert.AreEqual (1, App.Current.CategoriesTemplatesProvider.Templates.Count);
				Assert.AreEqual (0, App.Current.DatabaseManager.ActiveDB.Count<ProjectLongoMatch> ());
				CoreServices.Stop ();

			} catch (Exception ex) {
			}
		}
	}
}

