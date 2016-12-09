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
using System.Collections.Generic;
using System.Collections.Specialized;
using Couchbase.Lite;
using LongoMatch.Core.Store;
using Newtonsoft.Json.Linq;
using VAS.DB.Views;

namespace LongoMatch.DB.Views
{
	public class ProjectsView: GenericView <LMProject>
	{
		const string VERSION = "1";

		public ProjectsView (CouchbaseStorageLongoMatch storage) : base (storage)
		{
			DocumentType = "Project";
		}

		protected override string ViewVersion {
			get {
				return VERSION;
			}
		}

		protected override  OrderedDictionary FilterProperties {
			get {
				return new OrderedDictionary { { "Title", false }, { "Season", false },
					{ "Competition", false }, { "LocalName", false }, { "VisitorName", false }
				};
			}
		}

		protected override object GenKeys (IDictionary<string, object> document)
		{
			List<object> keys = new List<object> ();
			JObject desc = document ["Description"] as JObject;

			keys.Add (desc ["Title"]);
			keys.Add (desc ["Season"]);
			keys.Add (desc ["Competition"]);
			keys.Add (desc ["LocalName"]);
			keys.Add (desc ["VisitorName"]);
			return new PropertyKey (keys);
		}
	}
}

