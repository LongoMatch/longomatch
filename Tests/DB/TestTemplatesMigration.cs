//
//  Copyright (C) 2018 
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store.Templates;
using LongoMatch.DB;
using LongoMatch.DB.Migration;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;
using VAS.Core.Store.Templates;
using VAS.Services;
using Image = VAS.Core.Common.Image;

namespace Tests.DB
{
	public class TestTemplatesMigration
	{
		TemplatesMigration migration;
		Mock<IProgressReport> progressMock;
		CouchbaseStorageLongoMatch storage;
		Mock<IPreviewService> mockPreview;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			SetupClass.SetUp ();
		}

		[SetUp]
		public void Setup ()
		{
			SetupClass.SetUp ();
			string tmpPath = Path.GetTempPath ();
			string homePath = Path.Combine (tmpPath, "LongoMatch");
			string dbPath = Path.Combine (homePath, "db");

			if (Directory.Exists (dbPath)) {
				Directory.Delete (dbPath, true);
			}
			Directory.CreateDirectory (tmpPath);
			Directory.CreateDirectory (homePath);
			Directory.CreateDirectory (dbPath);

			storage = new CouchbaseStorageLongoMatch (dbPath, "test-db");
			progressMock = new Mock<IProgressReport> ();
			migration = new TemplatesMigration (progressMock.Object);

			mockPreview = new Mock<IPreviewService> ();
			App.Current.PreviewService = mockPreview.Object;
		}

		[Test]
		public async Task MigrateTeams_From1_0_MigratedTo1_1 ()
		{
			// Arrange
			LMDashboard dashboard1_0 = Serializer.Instance.Load<LMDashboard> (
				App.Current.ResourcesLocator.GetEmbeddedResourceFileStream ("Tests.data.templates.dashboard_1_0.lct"));
			Team team1_0 = Serializer.Instance.Load<Team> (
				App.Current.ResourcesLocator.GetEmbeddedResourceFileStream ("Tests.data.templates.team_1_0.ltt"));
			
			DummyDashboardsProvider.defaultTemplates = dashboard1_0.Clone ();
			Assert.AreEqual (1, dashboard1_0.Version);
			var dprovider = new DummyDashboardsProvider (storage);
			App.Current.CategoriesTemplatesProvider = dprovider;
			dprovider.Storage.Info.Version = new Version (1, 0);
			Assert.AreEqual (1, App.Current.CategoriesTemplatesProvider.Templates.FirstOrDefault ().Version);

			DummyTeamsProvider.defaultTemplates = team1_0.Clone ();
			Assert.AreEqual (1, team1_0.Version);
			var tprovider = new DummyTeamsProvider (storage);
			App.Current.TeamTemplatesProvider = tprovider;
			tprovider.Storage.Info.Version = new Version (1, 0);
			Assert.AreEqual (1, App.Current.TeamTemplatesProvider.Templates.FirstOrDefault ().Version);

			// Act
			await migration.Start ();

			// Assert
			Assert.AreEqual (new Version (1, 1), storage.Info.Version);
			var teamTemplate = App.Current.TeamTemplatesProvider.Templates.FirstOrDefault ();
			var dashboardTemplate = App.Current.CategoriesTemplatesProvider.Templates.FirstOrDefault ();
			Assert.AreEqual (2, teamTemplate.Version);
			Assert.AreEqual (2, dashboardTemplate.Version);
		}

		class DummyDashboardsProvider : TemplatesProvider<Dashboard>, ICategoriesTemplatesProvider
		{
			public static Dashboard defaultTemplates;

			public DummyDashboardsProvider (IStorage storage) : base (storage)
			{
				Add (defaultTemplates);
			}

			protected override Dashboard CreateDefaultTemplate (int count)
			{
				return defaultTemplates;
			}
		}

		class DummyTeamsProvider : TemplatesProvider<Team>, ITeamTemplatesProvider
		{
			public static Team defaultTemplates;

			public DummyTeamsProvider (IStorage storage) : base (storage)
			{
				Add (defaultTemplates);
			}

			protected override Team CreateDefaultTemplate (int count)
			{
				return defaultTemplates;
			}
		}
	}
}
