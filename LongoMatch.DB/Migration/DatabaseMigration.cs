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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Migration;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.DB
{
	/// <summary>
	/// Migrates a file-based database from LongoMatch &lt; 1.2 to a Couchbase.Lite database.
	/// </summary>
	public class DatabaseMigration
	{
		Dictionary<string, List<string>> databases;
		List<string> databasesDirectories;
		IProgressReport progress;
		readonly IDictionary<string, Guid> scoreNameToID;
		readonly IDictionary<string, Guid> penaltyNameToID;
		readonly IDictionary<string, Guid> teamNameToID;
		readonly IDictionary<string, Guid> dashboardNameToID;

		public DatabaseMigration (IProgressReport progress)
		{
			this.progress = progress;
			databases = new Dictionary <string, List<string>> ();
			databasesDirectories = new List<string> ();
			scoreNameToID = new ConcurrentDictionary <string, Guid> ();
			penaltyNameToID = new ConcurrentDictionary<string, Guid> ();
			teamNameToID = new ConcurrentDictionary<string, Guid> ();
			dashboardNameToID = new ConcurrentDictionary<string, Guid> ();
		}

		public void Start ()
		{
			MigrateProjectsDatabases ();
			MigrateTeamsAndDashboards ();
		}

		bool MigrateTeamsAndDashboards ()
		{
			bool ret = true;
			float count;
			float percent = 0;
			List<string> teamFiles;
			List<string> dashboardFiles;
			Guid id = Guid.NewGuid ();
			List<Team> teams = new List<Team> ();
			List<Dashboard> dashboards = new List<Dashboard> ();
			List<Task> tasks = new List<Task> ();

			progress.Report (0, "Migrating teams and dashboards", id);

			if (!Directory.Exists (Path.Combine (Config.DBDir, "teams"))) {
				return false;
			}

			teamFiles = Directory.EnumerateFiles (Path.Combine (Config.DBDir, "teams")).
				Where (f => f.EndsWith (".ltt")).ToList ();
			dashboardFiles = Directory.EnumerateFiles (Path.Combine (Config.DBDir, "analysis")).
				Where (f => f.EndsWith (".lct")).ToList ();

			if (teamFiles.Count == 0 && dashboardFiles.Count == 0) {
				progress.Report (1, "Migrating teams and dashboards", id);
				return true;
			}

			count = (teamFiles.Count + dashboardFiles.Count) * 2 + 1;

			// We can't use the FileStorage here, since it will migate the Team or Dashboard
			foreach (string teamFile in teamFiles) {
				try {
					Team team = Serializer.Instance.Load<Team> (teamFile);
					percent += 1 / count;
					progress.Report (percent, "Imported team " + team.Name, id);
					teams.Add (team);
				} catch (Exception ex) {
					Log.Exception (ex);
				}
			}

			foreach (string dashboardFile in dashboardFiles) {
				try {
					Dashboard dashboard = Serializer.Instance.Load<Dashboard> (dashboardFile);
					percent += 1 / count;
					progress.Report (percent, "Imported dashboard " + dashboard.Name, id);
					dashboards.Add (dashboard);
				} catch (Exception ex) {
					Log.Exception (ex);
				}
			}

			foreach (Team team in teams) {
				var migrateTask = Task.Run (() => {
					try {
						Log.Information ("Migrating team " + team.Name);
						TeamMigration.Migrate0 (team, teamNameToID);
						Config.TeamTemplatesProvider.Save (team);
						percent += 1 / count;
						progress.Report (percent, "Migrated team " + team.Name, id);
					} catch (Exception ex) {
						Log.Exception (ex);
						ret = false;
					}
				});
				tasks.Add (migrateTask);
			}

			foreach (Dashboard dashboard in dashboards) {
				var migrateTask = Task.Run (() => {
					try {
						Log.Information ("Migrating dashboard " + dashboard.Name);
						DashboardMigration.Migrate0 (dashboard, scoreNameToID, penaltyNameToID);
						Config.CategoriesTemplatesProvider.Save (dashboard);
						percent += 1 / count;
						progress.Report (percent, "Migrated team " + dashboard.Name, id);
					} catch (Exception ex) {
						Log.Exception (ex);
						ret = false;
					}
				});
				tasks.Add (migrateTask);
			}

			Task.WaitAll (tasks.ToArray ());

			string backupDir = Path.Combine (Config.TemplatesDir, "backup");
			if (!Directory.Exists (backupDir)) {
				Directory.CreateDirectory (backupDir);
			}

			foreach (string templateFile in Directory.EnumerateFiles (Path.Combine (Config.DBDir, "teams")).Concat(
				Directory.EnumerateFiles (Path.Combine (Config.DBDir, "analysis")))) {
				File.Move (templateFile, Path.Combine (backupDir, Path.GetFileName (templateFile)));
			}

			progress.Report (1, "Teams and dashboards migrated", id);
			return ret;
		}

		void MigrateProjectsDatabases ()
		{
			Guid id = Guid.NewGuid ();
			progress.Report (0, "Migrating databases", id);
			// Collect all the databases and projects to migrate for progress updates
			foreach (var directory in Directory.EnumerateDirectories (Config.DBDir)) {
				if (!directory.EndsWith (".ldb")) {
					continue;
				}
				databasesDirectories.Add (directory);
				var projects = new List<string> ();
				databases [Path.GetFileNameWithoutExtension (directory)] = projects;
				foreach (string projectfile in Directory.EnumerateFiles (directory)) {
					if (!projectfile.EndsWith (".ldb")) {
						projects.Add (projectfile);
					}
				}
			}

			// Start migrating databases
			foreach (var kv in databases) {
				MigrateDB (Config.DatabaseManager, kv.Key, kv.Value);
			}
			// Now that all the databases have been migrated, move the old databases to a backup directory
			string backupDir = Path.Combine (Config.DBDir, "old");
			if (!Directory.Exists (backupDir)) {
				Directory.CreateDirectory (backupDir);
			}
			foreach (string dbdir in databasesDirectories) {
				string destDir = Path.Combine (backupDir, Path.GetFileName (dbdir));
				if (Directory.Exists (destDir)) {
					Directory.Delete (destDir, true);
				}
				Directory.Move (dbdir, destDir);
			}
			progress.Report (1, "Databases migrated", id);
		}

		bool MigrateDB (IDataBaseManager manager, string databaseName, List<string> projectFiles)
		{
			IDatabase database;
			Guid id = Guid.NewGuid ();
			float totalProjects = projectFiles.Count * 2;
			float percent = 0;
			List<Task> tasks = new List<Task> ();
			ConcurrentQueue<Project> projects = new ConcurrentQueue<Project> ();
			bool ret = true;

			Log.Information ("Start migrating " + databaseName);
			try {
				database = manager.Add (databaseName);
			} catch {
				database = manager.Databases.FirstOrDefault (d => d.Name == databaseName);
			}

			if (database == null) {
				Log.Error ("Database with name " + databaseName + " is null");
				return false;
			}

			foreach (string projectFile in projectFiles) {
				var importTask = Task.Run (() => {
					Project project = null;
					try {
						Log.Information ("Migrating project " + projectFile);
						project = Serializer.Instance.Load<Project> (projectFile);
						projects.Enqueue (project);
					} catch (Exception ex) {
						Log.Exception (ex);
						ret = false;
					}
					percent += 1 / totalProjects;
					progress.Report (percent, "Imported project " + project?.Description.Title, id);
				});
				tasks.Add (importTask);
			}
			Task.WaitAll (tasks.ToArray ());

			foreach (Project project in projects) {
				if (project.LocalTeamTemplate.ID != Guid.Empty) {
					teamNameToID [project.LocalTeamTemplate.Name] = project.LocalTeamTemplate.ID;
				}
				if (project.VisitorTeamTemplate.ID != Guid.Empty) {
					teamNameToID [project.VisitorTeamTemplate.Name] = project.VisitorTeamTemplate.ID;
				}
			}

			foreach (Project project in projects) {
				var importTask = Task.Run (() => {
					try {
						ProjectMigration.Migrate0 (project, scoreNameToID, penaltyNameToID, teamNameToID, dashboardNameToID);
						database.AddProject (project);
					} catch (Exception ex) {
						Log.Exception (ex);
						ret = false;
					}
					percent += 1 / totalProjects;
					progress.Report (percent, "Migrated project " + project?.Description.Title, id);
				});
				tasks.Add (importTask);
			}
			Task.WaitAll (tasks.ToArray ());
			Log.Information ("Database " + databaseName + " migrated correctly");
			return ret;
		}
	}
}

