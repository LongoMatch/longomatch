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
using System.Collections.Specialized;
using System.Linq;
using Couchbase.Lite;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Serialization;
using Newtonsoft.Json.Linq;

namespace LongoMatch.DB.Views
{
	/// <summary>
	/// Generic View for the Couchbase database that indexes properties with
	/// the <see cref="LongoMatchPropertyIndex"/> attribute and make it possible
	/// to perform queries using a <see cref="QueryFilter"/>.
	/// The view also stores a preloaded version of the object that is returned in the
	/// query using the properties with the attribute <see cref="LongoMatchPropertyPreload"/>
	/// </summary>
	public abstract class GenericView<T>: IQueryView <T> where T : IStorable, new()
	{
		readonly Database db;
		readonly CouchbaseStorage storage;

		protected GenericView (CouchbaseStorage storage)
		{
			this.storage = storage;
			db = storage.Database;
			GetView ();

			// List all properties that will are included in the preloaded version of the object
			// returned in the queries
			PreviewProperties = typeof(T).GetProperties ().
				Where (prop => Attribute.IsDefined (prop, typeof(LongoMatchPropertyPreload))).
				Select (p => p.Name).ToList ();

			// List all properties that are indexed for the queries sorted by Index
			FilterProperties = new OrderedDictionary ();
			FilterProperties.Add (DocumentsSerializer.PARENT_PROPNAME, true);
			foreach (var prop in typeof(T).GetProperties ().
				Select (p => new { P = p, A = p.GetCustomAttributes (typeof(LongoMatchPropertyIndex), true)}).
				Where (x => x.A.Length == 1).
				OrderBy (x => (x.A [0] as LongoMatchPropertyIndex).Index)) {
				FilterProperties.Add (prop.P.Name, typeof(IStorable).IsAssignableFrom (prop.P.PropertyType));
			}
		}

		/// <summary>
		/// An ordered dictionary that store as keys the name of the property
		/// and as value a <c>boolean</c> indicating if the property returns an
		/// <see cref="IStorable"/>.
		/// </summary>
		protected virtual OrderedDictionary FilterProperties {
			get;
			private set;
		}

		/// <summary>
		/// A list with the names of the properties that are included in the preloaded
		/// version of the object.
		/// </summary>
		protected virtual List<string> PreviewProperties {
			get;
			private set;
		}

		/// <summary>
		/// The version of the view. It needs to be changed each time the Map function changes
		/// to re-index the view when this function changes.
		/// </summary>
		abstract protected string ViewVersion {
			get;
		}

		/// <summary>
		/// Creates a list of values to be indexed for the queries as a <see cref="PropertyKey"/>
		/// </summary>
		/// <returns>The key to emit in the map function.</returns>
		/// <param name="document">The database document.</param>
		virtual protected object GenKeys (IDictionary<string, object> document)
		{
			List<object> keys;

			if (FilterProperties.Count == 0)
				return null;

			keys = new List<object> ();
			foreach (string propName in FilterProperties.Keys) {
				object value;

				if (!document.TryGetValue (propName, out value)) {
					keys.Add (null);
					continue;
				}
				// If the property is an IStorable, store the object ID which will be used in the queries
				if ((bool)FilterProperties [propName]) {
					keys.Add (DocumentsSerializer.IDStringFromString (value as string));
				} else {
					keys.Add (value);
				}
			}
			return new PropertyKey (keys);
		}

		/// <summary>
		/// Return a serialized string with the preloaded version of the object using the properties in
		/// <see cref="PreviewProperties"/>
		/// </summary>
		/// <returns>A JSON string representation of  the object.</returns>
		/// <param name="document">The database document.</param>
		virtual protected string GenValue (IDictionary<string, object> document)
		{
			JObject jo;

			if (FilterProperties.Count == 0)
				return null;

			jo = new JObject ();
			foreach (string propName in PreviewProperties) {
				object obj;
				if (document.TryGetValue (propName, out obj)) {
					if (obj is JObject) {
						jo [propName] = obj as JObject;
					} else {
						jo [propName] = new JValue (obj);
					}
				}
			}
			return jo.ToString ();
		}

		/// <summary>
		/// Gets the map function to be run in this view.
		/// </summary>
		/// <returns>The map function.</returns>
		/// <param name="docType">Document type.</param>
		virtual protected MapDelegate GetMap (string docType)
		{
			return (document, emitter) => {
				if (docType.Equals (document [DocumentsSerializer.DOC_TYPE])) {
					emitter (GenKeys (document), GenValue (document));
				}
			};
		}

		/// <summary>
		/// Creates a new view in the database if it does not exists and it sets the map funcion on it.
		/// </summary>
		/// <returns>The view.</returns>
		View GetView ()
		{
			string docType = typeof(T).Name; 
			View view = db.GetView (docType);
			if (view.Map == null) {
				view.SetMap (GetMap (docType), ViewVersion);
			}
			return view;
		}

		/// <summary>
		/// Performs a query on the view with a <see cref="QueryFilter"/> whose keys
		/// must be in the list of <see cref="FilterProperties"/>
		/// </summary>
		/// <param name="filter">Filter.</param>
		public List<T> Query (QueryFilter filter)
		{
			List<T> elements = new List<T> ();
			View view = GetView ();

			Query q = view.CreateQuery ();
			if (filter != null && filter.Count > 0 && FilterProperties.Count > 0) {
				string sql = "";
				int i = 0, j = 0;

				foreach (string propName in FilterProperties.Keys) {
					List<object> values;

					if (filter.TryGetValue (propName, out values)) {
						string ope = "";
						string key = "key";

						/* Set the operator between keys */
						if (j != 0) {
							ope = filter.Operator == QueryOperator.And ? "AND" : "OR";
						}

						/* Set the key name */
						if (i != 0) {
							key += i;
						}

						/* Transform IStorable objects into ID's for the query since they are not indexed
						* as objects but with their ID */
						for (int w = 0; w < values.Count; w++) {
							IStorable storable = values [w] as IStorable;
							if (storable != null) {
								values [w] = storable.ID;
							}
						}

						if (values.Count == 1) {
							sql += String.Format (" {0} {1}='\"{2}\"' ", ope, key, values [0]);
						} else {
							string vals = String.Join (" , ", values.Select (x => "'\"" + x + "\"'"));
							sql += String.Format (" {0} {1} IN ({2}) ", ope, key, vals);
						}
						j++;
					}
					i++;
				}
				if (j == 0) {
					throw new InvalidQueryException ();
				} else {
					q.SQLSearch = sql;
				}
			}

			QueryEnumerator ret = q.Run ();
			foreach (QueryRow row in ret) {
				Revision rev = row.Document.CurrentRevision;
				T d = DocumentsSerializer.DeserializeFromJson<T> (
					      row.Value as string, db, rev);
				d.ID = DocumentsSerializer.IDFromString (row.DocumentId);
				d.DocumentID = row.DocumentId;
				d.IsLoaded = false;
				d.Storage = storage;
				elements.Add (d);
			}
			return elements;
		}
	}
}

