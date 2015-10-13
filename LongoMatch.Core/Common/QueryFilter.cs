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
using System.Collections;

namespace LongoMatch.Core.Common
{
	/// <summary>
	/// A filter used to retrieve objects from the database using <see cref="IStorage.Retrieve</see>"/>.
	/// </summary>
	public class QueryFilter: Dictionary<string, List<object>>
	{

		Dictionary<string, List<object>> cachedFilter;

		public QueryFilter ()
		{
			cachedFilter = new Dictionary<string, List<object>> ();
			Operator = QueryOperator.And;

		}

		/// <summary>
		/// Add a new filter constraint for an indexed property with a list of possible values.
		/// </summary>
		/// <param name="key">the name of the indexed property to filter .</param>
		/// <param name="values">A list with the available options.</param>
		public void Add (string key, params object[] values)
		{
			List<object> valuesList, existingList = null;

			if (values.Count () == 1 && values [0] is IEnumerable && !(values [0] is string)) {
				valuesList = (values [0] as IEnumerable).OfType<object> ().ToList ();
			} else {
				valuesList = values.ToList ();
			}
			cachedFilter.TryGetValue (key, out existingList);
			this [key] = valuesList;
		}

		/// <summary>
		/// Saves the last changes to the filter.
		/// </summary>
		public void SaveChanges ()
		{
			cachedFilter.Clear ();
			foreach (string key in Keys) {
				cachedFilter [key] = this [key];
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="LongoMatch.Core.Common.QueryFilter"/> has changed
		/// with respect of the last call to <see cref="SaveChanges"/>
		/// </summary>
		public bool Changed {
			get {
				if (!cachedFilter.Keys.SequenceEqualNoOrder (Keys)) {
					return true;
				}
				foreach (string key in cachedFilter.Keys) {
					if (!cachedFilter [key].SequenceEqualNoOrder (this [key])) {
						return true;
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Gets or sets the query operator type.
		/// </summary>
		public QueryOperator Operator {
			get;
			set;
		}
	}
}

