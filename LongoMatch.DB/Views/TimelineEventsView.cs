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
using Couchbase.Lite;
using LongoMatch.Core.Store;
using Newtonsoft.Json.Linq;
using VAS.DB.Views;
using VAS.DB;

namespace LongoMatch.DB.Views
{
	public class TimelineEventsView: GenericView <TimelineEventLongoMatch>
	{
		public TimelineEventsView (CouchbaseStorageLongoMatch storage) : base (storage)
		{
			/* We emit 1 row per player changing the Players property to Player */
			FilterProperties.Remove ("Players");
			FilterProperties.Remove ("Teams");
			FilterProperties.Add ("Player", true);
			FilterProperties.Add ("Team", true);
		}

		protected override object GenKeys (IDictionary<string, object> document)
		{
			var keys = new List<object> ();
			foreach (string propName in FilterProperties.Keys) {
				if (propName == "Player" || propName == "Team") {
					continue;
				}
				if ((bool)FilterProperties [propName]) {
					keys.Add (DocumentsSerializer.IDStringFromString (document [propName] as string));
				} else {
					keys.Add (document [propName]);
				}
			}
			return new PropertyKey (keys);
		}

		protected override MapDelegate GetMap (string docType)
		{
			return (document, emitter) => {
				if (docType.Equals (document [DocumentsSerializer.DOC_TYPE])) {
					PropertyKey keys = GenKeys (document) as PropertyKey;
					int playerKeyIndex = keys.Keys.Count;
					int teamKeyIndex = keys.Keys.Count + 1;

					/* Initialize the Player key in case there are no players. */
					keys.Keys.Add (null);
					/* Initialize the Team key in case there are no players. */
					keys.Keys.Add (null);

					IList teams = document ["Teams"] as IList;
					IList players = document ["Players"] as IList;

					if (teams.Count == 0) {
						teams.Add (null);
					}
					if (players.Count == 0) {
						players.Add (null);
					}

					/* iterate over players and teams and emit a row for each player and team combination */
					foreach (object teamObject in teams) {
						string id;
						if (teamObject != null) {
							id = DocumentsSerializer.IDStringFromString ((teamObject as JValue).Value as string);
							keys.Keys [teamKeyIndex] = id;
						}
						foreach (object playerObject in players) {
							if (playerObject != null) {
								id = DocumentsSerializer.IDStringFromString ((playerObject as JValue).Value as string);
								keys.Keys [playerKeyIndex] = id;
							}
							emitter (keys, GenValue (document));
						}
					}
				}
			};
		}

		protected override string ViewVersion {
			get {
				return "1";
			}
		}
	}
}

