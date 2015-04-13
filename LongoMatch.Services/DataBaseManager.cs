// 
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using Mono.Unix;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;

namespace LongoMatch.DB
{
	public class DataBaseManager: IDataBaseManager, IService
	{
		string DBDir;
		IGUIToolkit guiToolkit;
		IDatabase activeDB;
		const int SUPPORTED_MAJOR_VERSION = Constants.DB_MAYOR_VERSION;

		public DataBaseManager (string DBDir, IGUIToolkit guiToolkit)
		{
			this.DBDir = DBDir;
			this.guiToolkit = guiToolkit;
			Config.EventsBroker.ManageDatabasesEvent += HandleManageDatabase;
			Config.EventsBroker.OpenedProjectChanged += (p, pt, f, a) => {
				OpenedProject = p;
			};
			UpdateDatabases ();
		}

		public Project OpenedProject {
			get;
			set;
		}

		public void SetActiveByName (string name)
		{
			foreach (IDatabase db in Databases) {
				if (db.Name == name) {
					Log.Information ("Selecting active database " + db.Name);
					ActiveDB = db;
					return;
				}
			}
			
			IDatabase newdb = new DataBase (NameToFile (name));
			Log.Information ("Creating new database " + newdb.Name);
			Databases.Add (newdb);
			ActiveDB = newdb;
		}

		public IDatabase Add (string name)
		{
			if (Databases.Where (db => db.Name == name).Count () != 0) {
				throw new Exception ("A database with the same name already exists");
			}
			try {
				IDatabase newdb = new DataBase (NameToFile (name));
				Log.Information ("Creating new database " + newdb.Name);
				Databases.Add (newdb);
				return newdb;
			} catch (Exception ex) {
				Log.Exception (ex);
				return null;
			}
		}

		public bool Delete (IDatabase db)
		{
			/* Leave at least one database */
			if (Databases.Count < 2) {
				return false;
			}
			return db.Delete ();
		}

		public IDatabase ActiveDB {
			get {
				return activeDB;
			}
			set {
				activeDB = value;
				Config.CurrentDatabase = value.Name;
				Config.Save ();
			}
		}

		public List<IDatabase> Databases {
			get;
			set;
		}

		public void UpdateDatabases ()
		{
			Databases = new List<IDatabase> ();
			DirectoryInfo dbdir = new DirectoryInfo (DBDir);
			
			foreach (DirectoryInfo subdir in dbdir.GetDirectories()) {
				if (subdir.FullName.EndsWith (".ldb")) {
					IDatabase db = new DataBase (subdir.FullName);	
					if (db != null) {
						Log.Information ("Found database " + db.Name);
						Databases.Add (db);
					}
				}
			}
		}

		string NameToFile (string name)
		{
			return Path.Combine (DBDir, name + '.' + Extension);
		}

		string FileToName (string path)
		{
			return Path.GetFileName (path).Replace ("." + Extension, "");
		}

		string Extension {
			get {
				return "ldb";
			}
		}

		void HandleManageDatabase ()
		{
			if (OpenedProject != null) {
				var msg = Catalog.GetString ("Close the current project to open the database manager");
				guiToolkit.ErrorMessage (msg);
			} else {
				guiToolkit.OpenDatabasesManager ();
			}
		}

		#region IService

		public int Level {
			get {
				return 20;
			}
		}

		public string Name {
			get {
				return "Database manager";
			}
		}

		public bool Start ()
		{
			return true;
		}

		public bool Stop ()
		{
			return true;
		}

		#endregion
	}
}

