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
using LongoMatch.Core.Interfaces;
using System.Collections.Generic;
using System.Globalization;

namespace LongoMatch.DB
{
	public abstract class ObjectsCache<T, W>
	{
		protected readonly Dictionary<T, W> idToObjects;
		protected readonly Dictionary<W, T> objectsToId;

		public ObjectsCache ()
		{
			idToObjects = new Dictionary<T, W> ();
			objectsToId = new Dictionary<W, T> ();
		}

		public bool IsCached (T id)
		{
			return idToObjects.ContainsKey (id);
		}

		public bool IsCached (W obj)
		{
			return objectsToId.ContainsKey (obj);
		}

		public W ResolveReference (T id)
		{
			W p;
			idToObjects.TryGetValue (id, out p);
			return p;
		}

		public void Clear ()
		{
			idToObjects.Clear ();
			objectsToId.Clear ();
		}

		public abstract T GetReference (W obj);

		public void AddReference (W value)
		{
			T id = GetReference (value);
			idToObjects [id] = value;
			objectsToId [value] = id;
		}
	}

	public class GenericObjectsCache: ObjectsCache<string, object>
	{
		int _references;

		public GenericObjectsCache ()
		{
			_references = 0;
		}

		public override string GetReference (object obj)
		{
			string idStr;

			if (!objectsToId.TryGetValue (obj, out idStr)) {
				_references++;
				idStr = _references.ToString (CultureInfo.InvariantCulture); 
			}

			return idStr;
		}
	}

	public class StorableObjectsCache: ObjectsCache<Guid, IStorable>, IStorableObjectsCache
	{
		public override Guid GetReference (IStorable obj)
		{
			return obj != null ? obj.ID: Guid.Empty;
		}
	}
}

