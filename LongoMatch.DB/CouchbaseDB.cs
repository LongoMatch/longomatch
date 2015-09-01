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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;

namespace LongoMatch.DB
{
	public class CouchbaseDB:IDatabase
	{
		readonly CouchbaseStorage storage;

		public CouchbaseDB (Manager manager, string name)
		{
			storage = new CouchbaseStorage (manager, name);
		}

		#region IDatabase implementation

		public List<Project> GetAllProjects ()
		{
			return storage.Retrieve<Project> (null);
		}

		public Project GetProject (Guid id)
		{
			return storage.Retrieve<Project> (id);
		}

		public void AddProject (Project project)
		{
			storage.Store (project, true);
		}

		public bool RemoveProject (Project project)
		{
			storage.Delete<Project> (project);
			return true;
		}

		public void UpdateProject (Project project)
		{
			storage.Store (project);
		}

		public bool Exists (Project project)
		{
			// FIXME: add faster API to storage for that or index ID's
			return storage.Retrieve<Project> (new QueryFilter()).Any (p => p.ID == project.ID);
		}

		public bool Backup ()
		{
			return true;
		}

		public bool Delete ()
		{
			storage.Reset ();
			return true;
		}

		public void Reload ()
		{
		}

		public string Name {
			get {
				return storage.Info.Name;
			}
		}

		public DateTime LastBackup {
			get {
				return storage.Info.LastBackup;
			}
		}

		public int Count {
			get {
				return storage.Retrieve<Project> (new QueryFilter()).Count ();
			}
		}

		public Version Version {
			get {
				return storage.Info.Version;
			}
			set {
				storage.Info.Version = value;
			}
		}

		#endregion
	}
}

