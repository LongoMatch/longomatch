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
using LongoMatch;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Services;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces;
using VAS.Core.Store;

namespace Tests.Services
{
	[TestFixture]
	public class TestToolsManager
	{
		ToolsManager toolsManager;
		ProjectImporter importer;
		Mock<IStorageManager> dbManagerMock;
		Mock<IStorage> dbMock;
		Mock<IGUIToolkit> guiToolkitMock;

		[SetUp]
		public void SetUp ()
		{
			guiToolkitMock = new Mock<IGUIToolkit> ();
			Config.GUIToolkit = guiToolkitMock.Object;

			dbMock = new Mock<IStorage> ();
			dbManagerMock = new Mock<IStorageManager> ();
			dbManagerMock.Setup (d => d.ActiveDB).Returns (dbMock.Object);
			Config.DatabaseManager = dbManagerMock.Object;

			Config.EventsBroker = new EventsBroker ();

			toolsManager = new ToolsManager ();
			importer = new ProjectImporter {
				Description = "",
				ImportFunction = () => null,
				FilterName = "",
				Extensions = new string [] { },
				NeedsEdition = false,
				CanOverwrite = false,
			};
			toolsManager.ProjectImporters.Add (importer);
			toolsManager.Start ();
		}

		[Test]
		public void TestRegister ()
		{
			var toolsManager = new ToolsManager ();
			toolsManager.RegisterImporter (() => new ProjectLongoMatch (), "", "", null, false, false);
			Assert.AreEqual (1, toolsManager.ProjectImporters.Count);
		}

		[Test]
		public void TestNoImporters ()
		{
			var toolsManager = new ToolsManager ();
			toolsManager.Start ();
			Config.EventsBroker.EmitImportProject ();
			guiToolkitMock.Verify (g => g.ErrorMessage (It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());
		}

		[Test]
		public void TestImportProjectWithFailure ()
		{
			// Returns null
			importer.ImportFunction = () => null;

			Config.EventsBroker.EmitImportProject ();
			dbMock.Verify (db => db.Store<ProjectLongoMatch> (It.IsAny<ProjectLongoMatch> (), It.IsAny<bool> ()), Times.Never ());

			// Throws Exception
			importer.ImportFunction = () => {
				throw new Exception ();
			};

			Config.EventsBroker.EmitImportProject ();
			guiToolkitMock.Verify (g => g.ErrorMessage (It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());
		}

		[Test]
		public void TestImportProject ()
		{
			bool openned = false;
			ProjectLongoMatch p = new ProjectLongoMatch ();

			Config.EventsBroker.OpenProjectIDEvent += (project_id, project) => {
				if (project == p) {
					openned = true;
				}
			};
			importer.ImportFunction = () => p;
			Config.EventsBroker.EmitImportProject ();
			dbMock.Verify (db => db.Store<ProjectLongoMatch> (p, true), Times.Once ());
			Assert.IsTrue (openned);
		}

		[Test]
		public void TestImportFakeLiveProject ()
		{
			bool openned = false;
			ProjectLongoMatch p = new ProjectLongoMatch ();
			p.Description = new ProjectDescription ();
			p.Description.FileSet = new MediaFileSet ();
			p.Description.FileSet.Add (new MediaFile { FilePath = Constants.FAKE_PROJECT });

			Config.EventsBroker.OpenProjectIDEvent += (project_id, project) => {
				openned |= project == p;
			};

			importer.ImportFunction = () => p;
			Config.EventsBroker.EmitImportProject ();
			dbMock.Verify (db => db.Store<ProjectLongoMatch> (p, true), Times.Once ());
			guiToolkitMock.Verify (g => g.SelectMediaFiles (It.IsAny<MediaFileSet> ()), Times.Never ());
			Assert.IsTrue (openned);
		}

		[Test]
		public void TestImportProjectThatNeedsEdition ()
		{
			bool openned = false;
			ProjectLongoMatch p = new ProjectLongoMatch ();

			Config.EventsBroker.NewProjectEvent += project => {
				openned |= project == p;
			};
			importer.ImportFunction = () => p;
			importer.NeedsEdition = true;
			Config.EventsBroker.EmitImportProject ();
			dbMock.Verify (db => db.Store<ProjectLongoMatch> (p, true), Times.Never ());
			Assert.IsTrue (openned);
		}

	}
}

