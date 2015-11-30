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
using Couchbase.Lite;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Common;

namespace LongoMatch.DB
{
	public class CouchbaseManager: IDataBaseManager
	{
		readonly Manager manager;
		IDatabase activeDB;

		public CouchbaseManager (string dbDir)
		{
			manager = new Manager (new System.IO.DirectoryInfo (dbDir),
				ManagerOptions.Default);
			Databases = new List<IDatabase> ();
		}

		#region IDataBaseManager implementation

		public void SetActiveByName (string name)
		{
			IDatabase db = Databases.FirstOrDefault (d => d.Name == name);
			if (db != null) {
				ActiveDB = db;
			} else {
				ActiveDB = Add (name);
			}
		}

		public IDatabase Add (string name)
		{
			return Add (name, false);
		}


		public bool Delete (IDatabase db)
		{
			// Leave at least one
			if (Databases.Count <= 1) {
				return false;
			}
			Databases.Remove (db);
			return db.Delete ();
		}

		public void UpdateDatabases ()
		{
			foreach (string dbName in manager.AllDatabaseNames) {
				if (dbName != "templates") {
					Add (dbName, false);
				}
			}
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

		#endregion

		IDatabase Add (string name, bool check)
		{
			if (check && manager.AllDatabaseNames.Contains (name)) {
				throw new Exception ("A database with the same name already exists");
			}
			try {
				Log.Information ("Creating new database " + name);
				IDatabase db = new CouchbaseDB (manager, name);
				Databases.Add (db);
				return db;
			} catch (Exception ex) {
				Log.Exception (ex);
				return null;
			}
		}
	}
}

