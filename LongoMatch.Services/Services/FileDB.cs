// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;

namespace LongoMatch.DB
{
	public class DataBase: IDatabase
	{
		LiteDB projectsDB;
		string dbDirPath;
		string dbPath;
		string dbName;
		TimeSpan maxDaysWithoutBackup = new TimeSpan (5, 0, 0, 0);

		public DataBase (string dbDirPath)
		{
			dbName = Path.GetFileNameWithoutExtension (dbDirPath);
			dbPath = Path.Combine (dbDirPath, Path.GetFileName (dbDirPath));
			this.dbDirPath = dbDirPath;
			
			if (!Directory.Exists (dbDirPath)) {
				Directory.CreateDirectory (dbDirPath);
			}
			if (File.Exists (dbPath)) {
				try {
					projectsDB = Serializer.Load<LiteDB> (dbPath);
					projectsDB.DBPath = dbPath;
				} catch (Exception e) {
					Log.Exception (e);
				}
			}
			if (projectsDB == null) {
				ReloadDB ();
			}
			DateTime now = DateTime.UtcNow;
			if (projectsDB.LastBackup + maxDaysWithoutBackup < now) {
				Backup ();
			}
		}

		/// <value>
		/// The database version
		/// </value>
		public Version Version {
			get {
				return projectsDB.Version;
			}
			set {
				projectsDB.Version = value;
			}
		}

		public string Name {
			get {
				return dbName;
			}
		}

		public DateTime LastBackup {
			get {
				return projectsDB.LastBackup;
			}
		}

		public int Count {
			get {
				return projectsDB.Projects.Count;
			}
		}

		public bool Exists (Project project)
		{
			bool ret = false;
			if (projectsDB.ProjectsDict.ContainsKey (project.ID)) {
				if (File.Exists (Path.Combine (dbDirPath, project.ID.ToString ()))) {
					ret = true;
				}
			}
			return ret;
		}

		public bool Backup ()
		{
			DirectoryInfo backupDir, dbDir;
			FileInfo[] files;
			
			dbDir = new DirectoryInfo (dbDirPath);
			backupDir = new DirectoryInfo (dbDirPath + ".backup");
			try {
				if (backupDir.Exists) {
					backupDir.Delete ();
				}
				backupDir.Create ();
				files = dbDir.GetFiles ();
				foreach (FileInfo file in files) {
					string temppath = Path.Combine (backupDir.FullName, file.Name);
					file.CopyTo (temppath, false);
				}
				projectsDB.LastBackup = DateTime.UtcNow;
				projectsDB.Save ();
				return true;
			} catch {
				return false;
			}
		}

		public bool Delete ()
		{
			try {
				Directory.Delete (dbDirPath, true);
				return true;
			} catch (Exception ex) {
				Log.Exception (ex);
				return false;
			}
		}

		public List<ProjectDescription> GetAllProjects ()
		{
			return projectsDB.Projects;
		}

		public Project GetProject (Guid id)
		{
			string projectFile = Path.Combine (dbDirPath, id.ToString ());

			if (File.Exists (projectFile)) {
				try {
					return Serializer.Load<Project> (projectFile);
				} catch (Exception ex) {
					throw new ProjectDeserializationException (ex);
				}
			} else {
				throw new ProjectNotFoundException (projectFile);
			}
		}

		public void AddProject (Project project)
		{
			string projectFile;
			
			projectFile = Path.Combine (dbDirPath, project.ID.ToString ());
			project.Description.LastModified = DateTime.UtcNow;
			projectsDB.Add (project.Description);
			try {
				if (File.Exists (projectFile))
					File.Delete (projectFile);
				Serializer.Save (project, projectFile);
			} catch (Exception ex) {
				Log.Exception (ex);
				projectsDB.Delete (project.Description.ID);
			}
		}

		public bool RemoveProject (Guid id)
		{
			string projectFile;
			
			projectFile = Path.Combine (dbDirPath, id.ToString ());
			if (File.Exists (projectFile)) {
				File.Delete (projectFile);
			}
			return projectsDB.Delete (id);
		}

		public void UpdateProject (Project project)
		{
			project.Description.LastModified = DateTime.UtcNow;
			AddProject (project);
		}

		void ReloadDB ()
		{
			projectsDB = new LiteDB (dbPath);
			DirectoryInfo dbDir = new DirectoryInfo (dbDirPath);
			foreach (FileInfo file in dbDir.GetFiles ()) {
				if (file.FullName == dbPath) {
					continue;
				}
				try {
					Project project = Serializer.Load<Project> (file.FullName);
					projectsDB.Add (project.Description);
				} catch (Exception ex) {
					Log.Exception (ex);
				}
			}
			projectsDB.Save ();
		}
	}

	[Serializable]
	class LiteDB
	{
		
		public LiteDB (string dbPath)
		{
			DBPath = dbPath;
			ProjectsDict = new Dictionary <Guid, ProjectDescription> ();
			Version = new System.Version (Constants.DB_MAYOR_VERSION,
			                              Constants.DB_MINOR_VERSION);
			LastBackup = DateTime.UtcNow;
		}

		public LiteDB ()
		{
		}

		public string DBPath {
			get;
			set;
		}

		public Version Version { get; set; }

		public Dictionary<Guid, ProjectDescription> ProjectsDict { get; set; }

		public DateTime LastBackup { get; set; }

		public List<ProjectDescription> Projects {
			get {
				return ProjectsDict.Select (d => d.Value).ToList ();
			}
		}

		public bool Add (ProjectDescription desc)
		{
			if (ProjectsDict.ContainsKey (desc.ID)) {
				ProjectsDict [desc.ID] = desc;
			} else {
				ProjectsDict.Add (desc.ID, desc);
			}
			return Save ();
		}

		public bool Delete (Guid uuid)
		{
			if (ProjectsDict.ContainsKey (uuid)) {
				ProjectsDict.Remove (uuid);
				return Save ();
			}
			return false;
		}

		public bool Save ()
		{
			bool ret = false;
			
			try {
				Serializer.Save (this, DBPath);
				ret = true;
			} catch (Exception ex) {
				Log.Exception (ex);
			}
			return ret;
		}
	}
}

