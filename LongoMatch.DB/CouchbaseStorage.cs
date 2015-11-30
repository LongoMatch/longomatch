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
using Couchbase.Lite;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Serialization;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.DB.Views;

namespace LongoMatch.DB
{
	public class CouchbaseStorage: IStorage
	{
		Database db;
		Dictionary<Type, object> views;
		string storageName;
		object mutex;

		public CouchbaseStorage (Database db)
		{
			this.db = db;
			Init ();
		}

		public CouchbaseStorage (Manager manager, string storageName)
		{
			this.storageName = storageName;
			db = manager.GetDatabase (storageName);
			Init ();
		}

		public CouchbaseStorage (string dbDir, string storageName)
		{
			this.storageName = storageName;
			Manager manager = new Manager (new System.IO.DirectoryInfo (dbDir),
				                  ManagerOptions.Default);
			db = manager.GetDatabase (storageName);
			Init ();
		}

		void Init ()
		{
			// Only keep one revision for each document until we support replication and can handle conflicts
			db.MaxRevTreeDepth = 1;
			mutex = new object ();
			FetchInfo ();
			Compact ();
			InitializeViews ();
		}

		internal Database Database {
			get {
				return db;
			}
		}

		void FetchInfo ()
		{
			Info = Retrieve<StorageInfo> (Guid.Empty);
			if (Info == null) {
				Info = new StorageInfo {
					Name = storageName,
					LastBackup = DateTime.UtcNow,
					LastCleanup = DateTime.UtcNow,
					Version = new Version (Constants.DB_VERSION, 0),
				};
				Store (Info);
			}
		}

		void Compact ()
		{
			if ((DateTime.UtcNow - Info.LastCleanup).Days > 2) {
				db.Compact ();
				Info.LastCleanup = DateTime.UtcNow;
				Store (Info);
			}
		}

		void InitializeViews ()
		{
			views = new Dictionary <Type, object> ();
			views.Add (typeof(Dashboard), new DashboardsView (this));
			views.Add (typeof(Team), new TeamsView (this));
			views.Add (typeof(Project), new ProjectsView (this));
			views.Add (typeof(Player), new PlayersView (this));
			views.Add (typeof(TimelineEvent), new TimelineEventsView (this));
			views.Add (typeof(EventType), new EventTypeView (this));
		}

		public object Retrieve (Type type, Guid id)
		{
			return DocumentsSerializer.LoadObject (type, id, db);
		}

		#region IStorage implementation

		public StorageInfo Info {
			get;
			set;
		}

		public void Fill (IStorable storable)
		{
			lock (mutex) {
				db.RunInTransaction (() => {
					DocumentsSerializer.FillObject (storable, db);
					return true;
				});
			}
		}

		public IEnumerable<T> RetrieveAll<T> () where T : IStorable
		{
			lock (mutex) {
				IQueryView<T> qview = views [typeof(T)] as IQueryView <T>;
				return qview.Query (null);
			}
		}

		public T Retrieve<T> (Guid id) where T : IStorable
		{
			lock (mutex) {
				return (T)Retrieve (typeof(T), id);
			}
		}

		public IEnumerable<T> Retrieve<T> (QueryFilter filter) where T : IStorable
		{
			lock (mutex) {
				IQueryView<T> qview = views [typeof(T)] as IQueryView <T>;
				return qview.Query (filter);
			}
		}

		public IEnumerable<T> RetrieveFull<T> (QueryFilter filter, IStorableObjectsCache cache) where T : IStorable
		{
			lock (mutex) {
				IQueryView<T> qview = views [typeof(T)] as IQueryView <T>;
				return qview.QueryFull (filter, cache);
			}
		}

		public void Store<T> (T t, bool forceUpdate = false) where T : IStorable
		{
			lock (mutex) {
				db.RunInTransaction (() => {
					StorableNode node;
					ObjectChangedParser parser = new ObjectChangedParser ();
					parser.Parse (out node, t, Serializer.JsonSettings);

					if (!forceUpdate) {
						Update (node);
					} else {
						DocumentsSerializer.SaveObject (t, db, saveChildren: true);
					}
					foreach (IStorable storable in node.OrphanChildren) {
						db.GetDocument (DocumentsSerializer.StringFromID (storable.ID, t.ID)).Delete ();
					}
					return true;
				});
			}
		}

		/// <summary>
		/// Delete the specified storable object from the database. If the object is configured to delete its children,
		/// we perform a bulk deletion of all the documents with the the storable.ID prefix, which is faster than
		/// parsing the object and it ensures that we don't leave orphaned documents in the DB
		/// </summary>
		/// <param name="storable">The object to delete.</param>
		/// <typeparam name="T">The type of the object to delete.</typeparam>
		public void Delete<T> (T storable) where T : IStorable
		{
			lock (mutex) {
				db.RunInTransaction (() => {
					if (storable.DeleteChildren) {
						Query query = db.CreateAllDocumentsQuery ();
						// This should work, but raises an Exception in Couchbase.Lite
						//query.StartKey = t.ID;
						//query.EndKey = t.ID + "\uefff";

						// In UTF-8, from all the possible values in a GUID string, '0' is the first char in the
						// list and 'f' would be the last one
						string sepchar = DocumentsSerializer.ID_SEP_CHAR.ToString ();
						query.StartKey = storable.ID + sepchar + "00000000-0000-0000-0000-000000000000";
						query.EndKey = storable.ID + sepchar + "ffffffff-ffff-ffff-ffff-ffffffffffff";
						query.InclusiveEnd = true;
						foreach (var row in query.Run ()) {
							row.Document.Delete ();
						}
					}
					db.GetDocument (storable.ID.ToString ()).Delete ();
					return true;
				});
			}
		}

		public void Reset ()
		{
			lock (mutex) {
				db.Manager.ForgetDatabase (db);
			}
		}

		void Delete (StorableNode node, Guid rootID)
		{
			Guid id = node.Storable.ID;
			db.GetDocument (DocumentsSerializer.StringFromID (id, rootID)).Delete ();
			foreach (StorableNode child in node.Children) {
				Delete (child, rootID);
			}
		}

		void Update (StorableNode node, SerializationContext context = null)
		{
			if (context == null) {
				context = new SerializationContext (db, node.Storable.GetType ());
				context.RootID = node.Storable.ID;
			}
			if (node.IsChanged) {
				DocumentsSerializer.SaveObject (node.Storable, db, context, false);
			}

			/* Since we are saving single objects manually, we need to keep the stack
			 * updated to avoid problems with circular references */
			context.Stack.Push (node.Storable);
			foreach (StorableNode child in node.Children) {
				Update (child, context);
			}
			context.Stack.Pop ();
		}

		#endregion
	}
}

