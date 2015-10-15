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
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using LongoMatch;

namespace Tests
{
	public class LocalDatabase: IDatabase
	{
		Dictionary<Guid, Project> projects;

		public LocalDatabase ()
		{
			projects = new Dictionary<Guid, Project> ();
		}

		public  List<Project> GetAllProjects ()
		{
			return projects.Values.ToList ();
		}

		public Project GetProject (Guid id)
		{
			return projects [id];
		}

		public void AddProject (Project project)
		{
			projects [project.ID] = project;
		}

		public bool RemoveProject (Project project)
		{
			projects.Remove (project.ID);
			return true;
		}

		public void UpdateProject (Project project)
		{
			projects [project.ID] = project;
		}

		public bool Exists (Project project)
		{
			return projects.ContainsKey (project.ID);
		}

		public bool Backup ()
		{
			return true;
		}

		public bool Delete ()
		{
			return true;
		}

		public void Reload ()
		{
		}

		public string Name {
			get;
			set;
		}

		public DateTime LastBackup {
			get { return DateTime.UtcNow; }
		}

		public int Count {
			get { return projects.Count; }
		}

		public Version Version {
			get {
				return Config.Version;
			}
			set {
			}
		}

		public IStorage Storage {
			get;
			set;
		}
	}


	public class LocalDatabaseManager: IDataBaseManager
	{
		Dictionary<string, IDatabase> databases;

		public LocalDatabaseManager ()
		{
			databases = new Dictionary<string, IDatabase> ();
			Add ("Test");
			SetActiveByName ("Test");
		}

		public void SetActiveByName (string name)
		{
			ActiveDB = databases [name];
		}

		public IDatabase Add (string name)
		{
			var db = new LocalDatabase ();
			db.Name = name;
			databases.Add (name, db);
			return db;
		}

		public bool Delete (IDatabase db)
		{
			databases.Remove (db.Name);
			return true;
		}

		public void UpdateDatabases ()
		{
		}

		public IDatabase ActiveDB {
			get;
			set;
		}

		public List<IDatabase> Databases {
			get {
				return databases.Values.ToList ();
			}
			set {
			}
		}
	}
}

