//
//  Copyright (C) 2018 
using System;
using System.Threading.Tasks;
using VAS.Core.Interfaces;
using LongoMatch.Core.Migration;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.DB.Migration
{
	public class TemplatesMigration
	{
		IProgressReport progress;
		bool teamsMigrated;
		bool dashboardsMigrated;

		public TemplatesMigration (IProgressReport progress)
		{
			this.progress = progress;
		}

		public async Task Start ()
		{
			IStorage templatesStorage = App.Current.CategoriesTemplatesProvider.Storage;
			await Migrate (templatesStorage);
		}

		async Task Migrate (IStorage storage)
		{
			// Apply all the migration steps starting from the current version
			if (storage.Info.Version <= new Version (1, 0)) {
				await Migrate1_0_to_1_1 (storage);
			} else {
				return;
			}
			await Migrate (storage);
		}

		async Task Migrate1_0_to_1_1 (IStorage storage)
		{
			Guid id = Guid.NewGuid ();

			progress.Report (0.1f, "Migrating teams", id);
			MigrateTeams ();

			progress.Report (0.5f, "Migrating dashboards", id);
			MigrateDashboards ();

			progress.Report (1f, "Migrated database objects", id);
			storage.Info.Version = new Version (1, 1);
			storage.Store (storage.Info);
		}

		void MigrateTeams ()
		{
			if (teamsMigrated) {
				return;
			}

			var teams = App.Current.TeamTemplatesProvider.Templates;
			foreach (LMTeam t in teams) {
				TeamMigration.Migrate (t);
				App.Current.TeamTemplatesProvider.Save (t);
			}

			teamsMigrated = true;
		}

		void MigrateDashboards ()
		{
			if (dashboardsMigrated) {
				return;
			}

			var dashboards = App.Current.CategoriesTemplatesProvider.Templates;
			foreach (var dashboard in dashboards) {
				DashboardMigration.Migrate (dashboard);
				App.Current.CategoriesTemplatesProvider.Save (dashboard);
			}

			dashboardsMigrated = true;
		}

	}
}
