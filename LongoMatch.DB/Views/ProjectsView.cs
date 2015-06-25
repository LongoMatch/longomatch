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
using Couchbase.Lite;
using System.Collections.Generic;
using LongoMatch.Core.Store;
using Newtonsoft.Json.Linq;

namespace LongoMatch.DB.Views
{
	public class ProjectsView: GenericView <Project>
	{
		const string VERSION = "1";

		public ProjectsView (Database db) : base (db)
		{
		}

		protected override string ViewVersion {
			get {
				return VERSION;
			}
		}

		protected override List<string> FilterProperties {
			get {
				return new List<string> { "Title", "Season", "Competition", "LocalTeamName", "VisitorTeamName" };
			}
		}

		protected override object GenKeys (IDictionary<string, object> document)
		{
			List<object> keys = new List<object> ();
			JObject desc = document ["Description"] as JObject;

			keys.Add (desc ["Title"]);
			keys.Add (desc ["Season"]);
			keys.Add (desc ["Competition"]);
			keys.Add (desc ["LocalTeamName"]);
			keys.Add (desc ["VisitorTeamName"]);
			return new PropertyKey (keys);
		}
	}
}

