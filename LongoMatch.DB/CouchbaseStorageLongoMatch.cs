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
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using VAS.DB;

namespace LongoMatch.DB
{
	public class CouchbaseStorageLongoMatch: CouchbaseStorage
	{
		static CouchbaseStorageLongoMatch ()
		{
			#pragma warning disable 0618
			DocumentsSerializerHelper.AddTypeTranslation (typeof(TeamTemplate), typeof(SportsTeam));
			DocumentsSerializerHelper.AddTypeTranslation (typeof(TimelineEventLongoMatch), typeof(TimelineEventLongoMatch));
			#pragma warning restore 0618
		}

		public CouchbaseStorageLongoMatch (Database db) : base (db)
		{
		}

		public CouchbaseStorageLongoMatch (Manager manager, string storageName) : base (manager, storageName)
		{
		}

		public CouchbaseStorageLongoMatch (string dbDir, string storageName) : base (dbDir, storageName)
		{
		}


		protected override void InitializeViews ()
		{
			base.InitializeViews ();
			AddView (typeof(DashboardLongoMatch), new LongoMatch.DB.Views.DashboardsView (this));
			AddView (typeof(SportsTeam), new LongoMatch.DB.Views.TeamsView (this));
			AddView (typeof(ProjectLongoMatch), new LongoMatch.DB.Views.ProjectsView (this));
			AddView (typeof(PlayerLongoMatch), new LongoMatch.DB.Views.PlayersView (this));
			AddView (typeof(TimelineEventLongoMatch), new LongoMatch.DB.Views.TimelineEventsView (this));
		}
	}
}

