//
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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.Store;
using VAS.DB;

namespace LongoMatch.Services
{
	/// <summary>
	/// This services monitors a directory in the filesystem looking for <see cref="Dashboard"/>
	/// <see cref="Team"/> or <see cref="Project"/> and imports them into the database.
	/// </summary>
	public class ImportMonitorServices: IService
	{
		IDirectoryMonitor monitor;

		public ImportMonitorServices (string directoryPath, IDirectoryMonitor monitor)
		{
			this.monitor = monitor;
			DirectoryPath = directoryPath;
			ImportTeams = true;
			ImportDashboards = true;
			ImportProjects = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the importer import projects.
		/// </summary>
		/// <value>if <c>true</c>, import projects; otherwise, projects are ignored.</value>
		public bool ImportProjects {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the importer import dashboards.
		/// </summary>
		/// <value>if <c>true</c>, import dashboards; otherwise, dashboards are ignored.</value>
		public bool ImportDashboards {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the importer import teams.
		/// </summary>
		/// <value>if <c>true</c>, import teams; otherwise, teams are ignored.</value>
		public bool ImportTeams {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the directory path to monitor.
		/// </summary>
		/// <value>The directory path.</value>
		public string DirectoryPath {
			get;
			set;
		}

		#region IService implementation

		public int Level {
			get {
				return 11;
			}
		}

		public string Name {
			get {
				return "Import monitor";
			}
		}

		public bool Start ()
		{
			if (!Directory.Exists (DirectoryPath)) {
				return false;
			}
			monitor.DirectoryPath = DirectoryPath;
			monitor.FileChangedEvent += HandleFileChangedEvent;
			foreach (string path in Directory.GetFiles (DirectoryPath)) {
				HandleFileChangedEvent (FileChangeType.Created, path);
			}
			monitor.Start ();
			return true;
		}

		public bool Stop ()
		{
			monitor.FileChangedEvent -= HandleFileChangedEvent;
			monitor.Stop ();
			return false;
		}

		#endregion

		void HandleFileChangedEvent (FileChangeType eventType, string path)
		{
			// Event from the monitor usually comes from non-UI threads.
			Config.GUIToolkit.Invoke ((s, e) => {
				if (eventType == FileChangeType.Created) {
					string ext = Path.GetExtension (path);
					try {
						if (ImportTeams && ext == Constants.TEAMS_TEMPLATE_EXT) {
							Team team = FileStorage.RetrieveFrom<Team> (path);
							Config.TeamTemplatesProvider.Add (team);
							File.Delete (path);
						} else if (ImportDashboards && ext == Constants.CAT_TEMPLATE_EXT) {
							Dashboard team = FileStorage.RetrieveFrom<Dashboard> (path);
							Config.CategoriesTemplatesProvider.Add (team);
							File.Delete (path);
						} else if (ImportProjects && ext == Constants.PROJECT_EXT) {
							Project project = FileStorage.RetrieveFrom<Project> (path);
							Config.DatabaseManager.ActiveDB.Store<Project> (project, true);
							File.Delete (path);
						}
					} catch (Exception ex) {
						Log.Exception (ex);
					}
				}
			});
		}

	}
}

