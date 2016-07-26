//
//  Copyright (C) 2016 Fluendo S.A.
//
//
using System.IO;
using System.Linq;
using LongoMatch;
using LongoMatch.Core.Events;
using LongoMatch.Core.Store;
using LongoMatch.Plugins;
using LongoMatch.Services;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;

namespace Tests.Plugins.Importers
{
	[TestFixture ()]
	public class TestLongoMatchImporter
	{
		LongoMatchImporter importer;
		Mock<IGUIToolkit> mockGUI;
		ProjectsManager manager;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			importer = new LongoMatchImporter ();
			mockGUI = new Mock<IGUIToolkit> ();
			manager = new ProjectsManager ();
			manager.Start ();
			App.Current.ProjectExtension = ".tmp";

		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			manager.Stop ();
		}

		[Test ()]
		public void TestImport ()
		{
			// Arrange
			string path = Path.GetTempFileName ();
			string videopath = Path.GetTempFileName ();
			string originalPath = Path.Combine ("non-existing-path", Path.GetFileName (videopath));

			mockGUI.Setup (g => g.OpenFile (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string[]> ())).Returns (path);
			mockGUI.Setup (g => g.BusyDialog (It.IsAny<string> (), It.IsAny<object> ())).Returns (new DummyBusyDialog ());
			App.Current.GUIToolkit = mockGUI.Object;

			ProjectLongoMatch p = Utils.CreateProject ();
			p.FileSet.First ().FilePath = originalPath;
			Assert.IsFalse (p.FileSet.CheckFiles ());

			Project.Export (p, path);

			bool called = false;

			App.Current.EventsBroker.Subscribe<ProjectImportEvent> (ev => {
				called = true;
			});


			try {
				// Act
				Project imported = importer.ImportProject ();

				// Assert
				Assert.AreEqual (p, imported);
				Assert.IsTrue (called);
				Assert.AreEqual (videopath, imported.FileSet.First ().FilePath);
				Assert.IsTrue (imported.FileSet.CheckFiles ());
			} finally {
				File.Delete (path);
			}
		}
	}
}