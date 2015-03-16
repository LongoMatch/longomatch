//
//  Copyright (C) 2015 Andoni Morales Alastruey
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
using Newtonsoft.Json.Linq;

namespace LongoMatch.DB
{
	public class CouchbaseStorage: IStorage
	{
		Database db;

		public CouchbaseStorage (string databaseDir, string databaseName)
		{
			Manager manager = new Manager (new System.IO.DirectoryInfo (databaseDir),
				ManagerOptions.Default);
			db = manager.GetDatabase (databaseName);
		}

		internal Database Database {
			get {
				return db;
			}
		}
		#region IStorage implementation

		public List<T> RetrieveAll<T> () where T : IStorable
		{
			throw new NotImplementedException ();
		}

		public T Retrieve<T> (Guid id) where T : IStorable
		{
			Document doc = db.GetExistingDocument (id.ToString());
			return DocumentsSerializer.DeserializeObject<T> (db, doc); 
		}

		public List<T> Retrieve<T> (Dictionary<string, object> filter) where T : IStorable
		{
			throw new NotImplementedException ();
		}

		public void Store<T> (T t) where T : IStorable
		{
			Document doc = db.GetDocument (t.ID.ToString ());
			doc.Update((UnsavedRevision newRevision) => 
				{
					JObject jo = DocumentsSerializer.SerializeObject (t, newRevision, null);
					IDictionary<string, object> props = jo.ToObject<IDictionary<string, object>>();
					/* SetProperties sets a new properties dictionary, removing the attachments we
					 * added in the serialization */
					if (newRevision.Properties.ContainsKey ("_attachments")) {
						props["_attachments"] = newRevision.Properties["_attachments"];
					}
					newRevision.SetProperties (props);
					return true;
				});
		}

		public void Delete<T> (T t) where T : IStorable
		{
			db.GetDocument (t.ID.ToString ()).Delete ();
		}

		public void Reset ()
		{
			db.Manager.ForgetDatabase (db);
		}

		#endregion
	}
}

