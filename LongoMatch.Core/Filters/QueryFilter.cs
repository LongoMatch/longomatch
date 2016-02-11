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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Common;

namespace LongoMatch.Core.Filters
{
	/// <summary>
	/// A filter used to retrieve objects from the database using <see cref="IStorage.Retrieve</see>"/>.
	/// </summary>
	public class QueryFilter: Dictionary<string, List<object>>
	{

		public QueryFilter ()
		{
			Operator = QueryOperator.And;
			Children = new List<QueryFilter> ();
		}

		/// <summary>
		/// A list of children filtren to nest query filters.
		/// </summary>
		public List<QueryFilter> Children {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the query operator type.
		/// </summary>
		public QueryOperator Operator {
			get;
			set;
		}

		/// <summary>
		/// Add a new filter constraint for an indexed property with a list of possible values.
		/// </summary>
		/// <param name="key">the name of the indexed property to filter .</param>
		/// <param name="values">A list with the available options.</param>
		public void Add (string key, params object[] values)
		{
			List<object> valuesList;

			if (values.Count () == 1 && values [0] is IEnumerable && !(values [0] is string)) {
				valuesList = new List<object> ();
				foreach (object o in values[0] as IEnumerable) {
					valuesList.Add (o);
				}
			} else {
				valuesList = values.ToList ();
			}
			this [key] = valuesList;
		}
	}
}

