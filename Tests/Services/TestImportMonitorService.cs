﻿//
//  Copyright (C) 2016 Fluendo S.A.
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
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers.Misc;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.DB;
using LongoMatch.Services;
using Moq;
using NUnit.Framework;
using VAS.DB;

namespace Tests.Services
{

	class DummyMonitor: IDirectoryMonitor
	{
		#region IDirectoryMonitor implementation

		public event FileChangedHandler FileChangedEvent;

		public void Start ()
		{
		}

		public void Stop ()
		{
		}

		public string DirectoryPath {
			get;
			set;
		}

		#endregion

		public void AddFile (string path)
		{
			FileChangedEvent (FileChangeType.Created, path);
		}
	}

	[TestFixture]
	public class TestImportMonitorService
	{
		string tmpDir;
		ImportMonitorServices service;
		DummyMonitor monitor;
		Mock<ICategoriesTemplatesProvider> dashboardsProviderMock;
		Mock<ITeamTemplatesProvider> teamsProviderMock;
		Mock<IStorageManager> storageManagerMock;
		Mock<IStorage> storageMock;

		[SetUp]
		public void Setup ()
		{
			dashboardsProviderMock = new Mock<ICategoriesTemplatesProvider> ();
			teamsProviderMock = new Mock<ITeamTemplatesProvider> ();
			storageManagerMock = new Mock<IStorageManager> ();
			storageManagerMock.SetupAllProperties ();
			storageMock = new Mock<IStorage> ();
			storageManagerMock.Object.ActiveDB = storageMock.Object;
			var uiMock = new Mock<IGUIToolkit> ();
			uiMock.Setup (m => m.Invoke (It.IsAny<EventHandler> ())).Callback<EventHandler> (e => e (null, null));
			Config.EventsBroker = new EventsBroker ();
			Config.CategoriesTemplatesProvider = dashboardsProviderMock.Object;
			Config.TeamTemplatesProvider = teamsProviderMock.Object;
			Config.DatabaseManager = storageManagerMock.Object;
			Config.GUIToolkit = uiMock.Object;
			tmpDir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			Directory.CreateDirectory (tmpDir);
			monitor = new DummyMonitor ();
			service = new ImportMonitorServices (tmpDir, monitor);
		}

		[TearDown]
		public void TearDown ()
		{
			try {
				Directory.Delete (tmpDir, true);
			} catch {
			}
		}

		[Test]
		public void TestInvalidDirectory ()
		{
			service = new ImportMonitorServices ("/" + Path.GetRandomFileName (), new DummyMonitor ());
			Assert.IsFalse (service.Start ());
		}

		[Test]
		public void TestInvalidFile ()
		{
			service.Start ();
			string outPath = Path.Combine (tmpDir, "test" + Constants.CAT_TEMPLATE_EXT);
			FileStream file = File.OpenWrite (outPath);
			file.Write (new byte[] { 1, 2, 3, 4 }, 0, 4);
			file.Flush ();
			file.Close ();
			monitor.AddFile (outPath);
			Assert.IsTrue (File.Exists (outPath));
			service.Stop ();
		}

		[Test]
		public void TestImportFilesAtStartup ()
		{
			Dashboard dashboard = Dashboard.DefaultTemplate (1);
			string outPath = Path.Combine (tmpDir, "test" + Constants.CAT_TEMPLATE_EXT);
			FileStorage.StoreAt (dashboard, outPath);
			dashboardsProviderMock.Verify (s => s.Add (dashboard), Times.Never ());
			service.Start ();
			dashboardsProviderMock.Verify (s => s.Add (dashboard), Times.Once ());
			Assert.IsFalse (File.Exists (outPath));
			service.Stop ();
		}

		[Test]
		public void TestAddDashboard ()
		{
			service.Start ();
			Dashboard dashboard = Dashboard.DefaultTemplate (1);
			string outPath = Path.Combine (tmpDir, "test" + Constants.CAT_TEMPLATE_EXT);
			FileStorage.StoreAt (dashboard, outPath);
			monitor.AddFile (outPath);
			dashboardsProviderMock.Verify (s => s.Add (dashboard), Times.Once ());
			Assert.IsFalse (File.Exists (outPath));
			service.Stop ();
		}

		[Test]
		public void TestAddTeam ()
		{
			service.Start ();
			Team team = Team.DefaultTemplate (1);
			string outPath = Path.Combine (tmpDir, "test" + Constants.TEAMS_TEMPLATE_EXT);
			FileStorage.StoreAt (team, outPath);
			monitor.AddFile (outPath);
			teamsProviderMock.Verify (s => s.Add (team), Times.Once ());
			Assert.IsFalse (File.Exists (outPath));
			service.Stop ();
		}

		[Test]
		public void TestAddProject ()
		{
			service.Start ();
			Project project = new Project ();
			string outPath = Path.Combine (tmpDir, "test" + Constants.PROJECT_EXT);
			FileStorage.StoreAt (project, outPath);
			monitor.AddFile (outPath);
			storageMock.Verify (s => s.Store <Project> (project, true));
			Assert.IsFalse (File.Exists (outPath));
			service.Stop ();
		}
	}
}

