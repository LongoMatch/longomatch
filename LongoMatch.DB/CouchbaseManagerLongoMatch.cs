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
using System.Text;
using System.Text.RegularExpressions;
using Couchbase.Lite;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.DB;

namespace LongoMatch.DB
{
	public class CouchbaseManagerLongoMatch : CouchbaseManager
	{
		public CouchbaseManagerLongoMatch (string dbDir) : base (dbDir)
		{
		}

		public override IStorage Add (string name)
		{
			// Couchbase doesn't accept uppercase databases.
			name = SanitizeDBName (name);
			var storage = Add (name, false);
			if (storage != null) {
				EventsAggregator.Publish (new DatabaseCreatedEvent {
					Name = name
				});
			}
			return storage;
		}

		protected override IStorage Add (string name, bool check)
		{
			if (check && manager.AllDatabaseNames.Contains (name)) {
				throw new Exception ("A database with the same name already exists");
			}
			try {
				Log.Information ("Creating new database " + name);
				IStorage db = new CouchbaseStorageLongoMatch (manager, name);
				Databases.Add (db);
				return db;
			} catch (Exception ex) {
				Log.Exception (ex);
				return null;
			}
		}
	}
}

