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
using LongoMatch.Core.Events;
using LongoMatch.Core.Store;
using LongoMatch.Services;
using LongoMatch.Services.State;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
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
		Mock<IDialogs> mockDialogs;

		[SetUp]
		public void SetUp ()
		{
			guiToolkitMock = new Mock<IGUIToolkit> ();
			App.Current.GUIToolkit = guiToolkitMock.Object;
			mockDialogs = new Mock<IDialogs> ();
			App.Current.Dialogs = mockDialogs.Object;

			dbMock = new Mock<IStorage> ();
			dbManagerMock = new Mock<IStorageManager> ();
			dbManagerMock.Setup (d => d.ActiveDB).Returns (dbMock.Object);
			App.Current.DatabaseManager = dbManagerMock.Object;

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

		[TearDown]
		public void TearDown ()
		{
			toolsManager.Stop ();
		}

		[Test]
		public void TestRegister ()
		{
			var toolsManager = new ToolsManager ();
			toolsManager.RegisterImporter (() => new LMProject (), "", "", null, false, false);
			Assert.AreEqual (1, toolsManager.ProjectImporters.Count);
		}

		[Test]
		public void TestNoImporters ()
		{
			var toolsManager = new ToolsManager ();
			toolsManager.Start ();
			App.Current.EventsBroker.Publish<ImportProjectEvent> (new ImportProjectEvent ());
			mockDialogs.Verify (g => g.ErrorMessage (It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());
			toolsManager.Stop ();
		}

		[Test]
		public void TestImportProjectWithFailure ()
		{
			// Returns null
			importer.ImportFunction = () => null;

			App.Current.EventsBroker.Publish<ImportProjectEvent> (new ImportProjectEvent ());
			dbMock.Verify (db => db.Store<LMProject> (It.IsAny<LMProject> (), It.IsAny<bool> ()), Times.Never ());

			// Throws Exception
			importer.ImportFunction = () => {
				throw new Exception ();
			};

			App.Current.EventsBroker.Publish<ImportProjectEvent> (new ImportProjectEvent ());
			mockDialogs.Verify (g => g.ErrorMessage (It.IsAny<string> (), It.IsAny<object> ()), Times.Once ());
		}

		[Test]
		public void TestImportProject ()
		{
			bool openned = false;
			LMProject p = new LMProject ();

			EventToken et = App.Current.EventsBroker.Subscribe<OpenProjectIDEvent> ((OpenProjectIDEvent e) => {
				if (e.Project == p) {
					openned = true;
				}
			});

			importer.ImportFunction = () => p;
			App.Current.EventsBroker.Publish<ImportProjectEvent> (new ImportProjectEvent ());
			dbMock.Verify (db => db.Store<LMProject> (p, true), Times.Once ());
			Assert.IsTrue (openned);

			App.Current.EventsBroker.Unsubscribe<OpenProjectIDEvent> (et);
		}

		[Test]
		public void TestImportFakeLiveProject ()
		{
			bool openned = false;
			LMProject p = new LMProject ();
			p.Description = new ProjectDescription ();
			p.Description.FileSet = new MediaFileSet ();
			p.Description.FileSet.Add (new MediaFile { FilePath = Constants.FAKE_PROJECT });

			EventToken et = App.Current.EventsBroker.Subscribe<OpenProjectIDEvent> ((OpenProjectIDEvent e) => {
				openned |= e.Project == p;
			});

			importer.ImportFunction = () => p;
			App.Current.EventsBroker.Publish<ImportProjectEvent> (new ImportProjectEvent ());
			dbMock.Verify (db => db.Store<LMProject> (p, true), Times.Once ());
			guiToolkitMock.Verify (g => g.SelectMediaFiles (It.IsAny<MediaFileSet> ()), Times.Never ());
			Assert.IsTrue (openned);

			App.Current.EventsBroker.Unsubscribe<OpenProjectIDEvent> (et);
		}

		[Test]
		public void TestImportProjectThatNeedsEdition ()
		{
			bool openned = false;
			LMProject p = new LMProject ();

			EventToken et = App.Current.EventsBroker.Subscribe<NavigationEvent> ((e) => {
				openned |= e.Name == NewProjectState.NAME;
			});

			importer.ImportFunction = () => p;
			importer.NeedsEdition = true;
			App.Current.EventsBroker.Publish (new ImportProjectEvent ());
			dbMock.Verify (db => db.Store (p, true), Times.Never ());
			Assert.IsTrue (openned);

			App.Current.EventsBroker.Unsubscribe<NavigationEvent> (et);
		}

	}
}

