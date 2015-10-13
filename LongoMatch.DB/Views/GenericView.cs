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
using System.Linq;
using System.Collections.Generic;
using Couchbase.Lite;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store.Templates;
using System.Reflection;
using Newtonsoft.Json.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Serialization;

namespace LongoMatch.DB.Views
{
	public abstract class GenericView<T>: IQueryView <T> where T : IStorable, new()
	{
		Database db;
		CouchbaseStorage storage;

		public GenericView (CouchbaseStorage storage)
		{
			this.storage = storage;
			db = storage.Database;
			GetView ();
			PreviewProperties = typeof(T).GetProperties().
				Where(prop => Attribute.IsDefined(prop, typeof(LongoMatchPropertyPreload))).
				Select (p => p.Name).ToList ();

			FilterProperties = typeof(T).GetProperties().
				Select (p => new { P = p, A = p.GetCustomAttributes(typeof(LongoMatchPropertyIndex), true)}).
				Where (x => x.A.Length == 1).
				OrderBy (x => (x.A[0] as LongoMatchPropertyIndex).Index).
				Select (x => x.P.Name).ToList();
		}

		protected virtual List<string> FilterProperties {
			get;
			private set;
		}

		protected virtual List<string> PreviewProperties {
			get;
			private set;
		}

		abstract protected string ViewVersion {
			get;
		}

		virtual protected object GenKeys (IDictionary<string, object> document)
		{
			List<object> keys;

			if (FilterProperties.Count == 0)
				return null;

			keys = new List<object> ();
			foreach (string propName in FilterProperties) {
				keys.Add (document [propName]);
			}
			return new PropertyKey (keys);
		}

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

		virtual protected MapDelegate GetMap (string docType)
		{
			return (document, emitter) => {
				if (docType.Equals (document [DocumentsSerializer.DOC_TYPE])) {
					emitter (GenKeys (document), GenValue (document));
				}
			};
		}

		View GetView ()
		{
			string docType = typeof(T).Name; 
			View view = db.GetView (docType);
			if (view.Map == null) {
				view.SetMap (GetMap (docType), ViewVersion);
			}
			return view;
		}

		public List<T> Query (QueryFilter filter)
		{
			List<T> elements = new List<T> ();
			View view = GetView ();

			Query q = view.CreateQuery ();
			if (filter != null && filter.Count > 0 && FilterProperties.Count > 0) {
				string sql = "";
				int i = 0, j = 0;

				/* FIXME: add support for the OR operator */
				foreach (string propName in FilterProperties) {
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
				T d = new T ();
				d.ID = Guid.Parse (row.DocumentId);
				Revision rev = row.Document.CurrentRevision;

				d = DocumentsSerializer.DeserializeFromJson<T> (
					row.Value as string, db, rev);
				d.ID = Guid.Parse (row.DocumentId);
				d.IsLoaded = false;
				d.Storage = storage;
				elements.Add (d);
			}
			return elements;
		}
	}
}

