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
using LongoMatch.Core.Migration;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;
using VAS.Core.Store.Templates;

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
			List<string> teamFiles = new List<string> ();
			List<string> dashboardFiles = new List<string> ();
			Guid id = Guid.NewGuid ();
			ConcurrentQueue<LMTeam> teams = new ConcurrentQueue<LMTeam> ();
			ConcurrentQueue<Dashboard> dashboards = new ConcurrentQueue<Dashboard> ();
			List<Task> tasks = new List<Task> ();

			progress.Report (0, "Migrating teams and dashboards", id);

			try {
				teamFiles = Directory.EnumerateFiles (Path.Combine (App.Current.DBDir, "teams")).
					Where (f => f.EndsWith (".ltt")).ToList ();
			} catch (DirectoryNotFoundException ex) {
				percent += 0.5f;
				progress.Report (percent, "Migrated teams", id);
			}
			try {
				dashboardFiles = Directory.EnumerateFiles (Path.Combine (App.Current.DBDir, "analysis")).
					Where (f => f.EndsWith (".lct")).ToList ();
			} catch (DirectoryNotFoundException ex) {
				percent += 0.5f;
				progress.Report (percent, "Migrated dashboards", id);
			}
			if (teamFiles.Count == 0 && dashboardFiles.Count == 0) {
				progress.Report (1, "Migrated teams and dashboards", id);
				return true;
			}
			count = (teamFiles.Count + dashboardFiles.Count) * 2 + 1;

			// We can't use the FileStorage here, since it will migate the Team or Dashboard
			foreach (string teamFile in teamFiles) {
				try {
					LMTeam team = Serializer.Instance.Load<LMTeam> (teamFile);
					percent += 1 / count;
					progress.Report (percent, "Imported team " + team.Name, id);
					teams.Enqueue (team);
				} catch (Exception ex) {
					Log.Exception (ex);
				}
			}

			foreach (string dashboardFile in dashboardFiles) {
				try {
					Dashboard dashboard = Serializer.Instance.Load<Dashboard> (dashboardFile);
					percent += 1 / count;
					progress.Report (percent, "Imported dashboard " + dashboard.Name, id);
					dashboards.Enqueue (dashboard);
				} catch (Exception ex) {
					Log.Exception (ex);
				}
			}

			foreach (LMTeam team in teams) {
				var migrateTask = Task.Run (() => {
					try {
						Log.Information ("Migrating team " + team.Name);
						TeamMigration.Migrate0 (team, teamNameToID);
						App.Current.TeamTemplatesProvider.Save (team);
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
						App.Current.CategoriesTemplatesProvider.Save (dashboard as LMDashboard);
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

			try {
				string backupDir = Path.Combine (App.Current.TemplatesDir, "backup");
				if (!Directory.Exists (backupDir)) {
					Directory.CreateDirectory (backupDir);
				}

				foreach (string templateFile in Directory.EnumerateFiles (Path.Combine (App.Current.DBDir, "teams")).Concat(
					Directory.EnumerateFiles (Path.Combine (App.Current.DBDir, "analysis")))) {
					string outputFile = Path.Combine (backupDir, Path.GetFileName (templateFile));
					if (File.Exists (outputFile)) {
						File.Delete (outputFile);
					}
					File.Move (templateFile, outputFile);
				}
			} catch (Exception ex) {
				Log.Error ("Error moving templates to the backup directory.");
				Log.Exception (ex);
			}

			progress.Report (1, "Teams and dashboards migrated", id);
			return ret;
		}

		void MigrateProjectsDatabases ()
		{
			Guid id = Guid.NewGuid ();
			progress.Report (0, "Migrating databases", id);
			// Collect all the databases and projects to migrate for progress updates
			foreach (var directory in Directory.EnumerateDirectories (App.Current.DBDir)) {
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
				MigrateDB (App.Current.DatabaseManager, kv.Key, kv.Value);
			}
			// Now that all the databases have been migrated, move the old databases to a backup directory
			string backupDir = Path.Combine (App.Current.DBDir, "old");
			if (!Directory.Exists (backupDir)) {
				Directory.CreateDirectory (backupDir);
			}
			try {
				foreach (string dbdir in databasesDirectories) {
					string destDir = Path.Combine (backupDir, Path.GetFileName (dbdir));
					if (Directory.Exists (destDir)) {
						Directory.Delete (destDir, true);
					}
					Directory.Move (dbdir, destDir);
				}
			} catch (Exception ex) {
				Log.Error ("Error moving database to the backup directory");
				Log.Exception (ex);
			}
			progress.Report (1, "Databases migrated", id);
		}

		bool MigrateDB (IStorageManager manager, string databaseName, List<string> projectFiles)
		{
			IStorage database;
			Guid id = Guid.NewGuid ();
			int indexSteps = 4;
			float step = (float)1 / (projectFiles.Count * 2 + indexSteps);
			float percent = 0;
			List<Task> tasks = new List<Task> ();
			bool ret = true;

			Log.Information ("Start migrating " + databaseName);
			try {
				database = manager.Add (databaseName);
			} catch {
				database = manager.Databases.FirstOrDefault (d => d.Info.Name == databaseName);
			}

			if (database == null) {
				Log.Error ("Database with name " + databaseName + " is null");
				return false;
			}

			foreach (string projectFile in projectFiles) {
				var importTask = Task.Run (() => {
					LMProject project = null;
					try {
						Log.Information ("Migrating project " + projectFile);
						project = Serializer.Instance.Load<LMProject> (projectFile);
					} catch (Exception ex) {
						Log.Exception (ex);
						ret = false;
					}
					percent += step;
					progress.Report (percent, "Imported project " + project?.Description.Title, id);

					if (project != null) {
						if (project.LocalTeamTemplate.ID != Guid.Empty) {
							teamNameToID [project.LocalTeamTemplate.Name] = project.LocalTeamTemplate.ID;
						}
						if (project.VisitorTeamTemplate.ID != Guid.Empty) {
							teamNameToID [project.VisitorTeamTemplate.Name] = project.VisitorTeamTemplate.ID;
						}
						try {
							ProjectMigration.Migrate0 (project, scoreNameToID, penaltyNameToID, teamNameToID, dashboardNameToID);
							database.Info.Version = new Version (2, 0);
							database.Store (project, true);
						} catch (Exception ex) {
							Log.Exception (ex);
							ret = false;
						}
						percent += step;
						progress.Report (percent, "Migrated project " + project?.Description.Title, id);
						project.Dispose ();
						project = null;
						GC.Collect ();
					}
				});
				tasks.Add (importTask);
			}
			Task.WaitAll (tasks.ToArray ());

			// Create a query and print the result to traverse the iterator
			Log.Information ("Projects index created:" + database.RetrieveAll<LMProject> ().Count ());
			percent += step;
			progress.Report (percent, "Projects index created", id);

			Log.Information ("Timeline events index created:" + database.RetrieveAll<LMTimelineEvent> ().Count ());
			percent += step;
			progress.Report (percent, "Events index created", id);

			Log.Information ("Teams index created:" + database.RetrieveAll<Team> ().Count ());
			percent += step;
			progress.Report (percent, "Teams index created", id);

			Log.Information ("DAshboards index created:" + database.RetrieveAll<Dashboard> ().Count ());
			percent += step;
			progress.Report (percent, "Dashboards index created", id);

			Log.Information ("Database " + databaseName + " migrated correctly");
			return ret;
		}
	}
}

