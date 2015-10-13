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
using NUnit.Framework;
using System.IO;
using System.Linq;
using LongoMatch.DB;
using Couchbase.Lite;
using LongoMatch.Core.Store.Templates;
using System.Collections.Generic;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;

namespace Tests.DB
{
	[TestFixture ()]
	public class TestQueries
	{
		CouchbaseStorage storage;
		Database db;

		[TestFixtureSetUp]
		public void InitDB ()
		{
			string dbPath = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			if (Directory.Exists (dbPath)) {
				Directory.Delete (dbPath, true);
			}
			try {
				storage = new CouchbaseStorage (dbPath, "test-db");
			} catch (Exception ex) {
				throw ex;
			}
			db = storage.Database;
		}

		[TestFixtureTearDown]
		public void DeleteDB ()
		{
			Directory.Delete (db.Manager.Directory, true);
		}

		[TearDown]
		public void CleanDB ()
		{
			db.RunInTransaction (() => {
				foreach (var d in db.CreateAllDocumentsQuery ().Run()) {
					db.GetDocument (d.DocumentId).Delete ();
				}
				return true;
			});
		}

		[Test ()]
		public void TestQueryDashboards ()
		{
			List<Dashboard> dashboards;

			for (int i = 0; i < 5; i++) {
				var da = Dashboard.DefaultTemplate (5);
				da.Name = "Dashboard" + (i + 1);
				storage.Store (da);
			}

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Name", "Dashboard1");
			dashboards = storage.Retrieve<Dashboard> (filter);
			Assert.AreEqual (1, dashboards.Count);
			Assert.AreEqual ("Dashboard1", dashboards [0].Name);

			filter.Add ("Name", "Dashboard1", "Dashboard3");
			dashboards = storage.Retrieve<Dashboard> (filter);
			Assert.AreEqual (2, dashboards.Count);
			Assert.AreEqual ("Dashboard1", dashboards [0].Name);
			Assert.AreEqual ("Dashboard3", dashboards [1].Name);

			filter = new QueryFilter ();
			filter.Add ("Name", "Pepe");
			dashboards = storage.Retrieve<Dashboard> (filter);
			Assert.AreEqual (0, dashboards.Count);

			filter = new QueryFilter ();
			filter.Add ("Unkown", "Pepe");
			Assert.Throws<InvalidQueryException> (
				delegate {
					dashboards = storage.Retrieve<Dashboard> (filter);
				});
			Assert.AreEqual (0, dashboards.Count);
		}

		[Test ()]
		public void TestQueryTeams ()
		{
			List<Team> teams;

			for (int i = 0; i < 5; i++) {
				var da = Team.DefaultTemplate (5);
				da.Name = "Team" + (i + 1);
				storage.Store (da);
			}

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Name", "Team1");
			teams = storage.Retrieve<Team> (filter);
			Assert.AreEqual (1, teams.Count);
			Assert.AreEqual ("Team1", teams [0].Name);

			filter.Add ("Name", "Team1", "Team3");
			teams = storage.Retrieve<Team> (filter);
			Assert.AreEqual (2, teams.Count);
			Assert.AreEqual ("Team1", teams [0].Name);
			Assert.AreEqual ("Team3", teams [1].Name);

			filter = new QueryFilter ();
			filter.Add ("Name", "Pepe");
			teams = storage.Retrieve<Team> (filter);
			Assert.AreEqual (0, teams.Count);

			filter = new QueryFilter ();
			filter.Add ("Unkown", "Pepe");
			Assert.Throws<InvalidQueryException> (
				delegate {
					teams = storage.Retrieve<Team> (filter);
				});
			Assert.AreEqual (0, teams.Count);
		}

		[Test ()]
		public void TestQueryProjects ()
		{
			QueryFilter filter;

			foreach (string comp in new []{"Liga", "Champions", "Copa"}) {
				foreach (string season in new []{"2013", "2014", "2015"}) {
					foreach (string team in new []{"Barça", "Complu", "Santomera"}) {
						Project p = new Project ();
						p.Description = new ProjectDescription ();
						p.Description.Season = season;
						p.Description.Competition = comp;
						p.Description.LocalName = team;
						p.Description.VisitorName = "Fluendo";
						storage.Store (p);
					}
				}
			}

			filter = new QueryFilter ();
			filter.Add ("Competition", "Liga");
			Assert.AreEqual (9, storage.Retrieve<Project> (filter).Count);

			filter.Add ("Competition", "Liga", "Champions");
			Assert.AreEqual (18, storage.Retrieve<Project> (filter).Count);

			filter.Add ("Season", "2013");
			Assert.AreEqual (6, storage.Retrieve<Project> (filter).Count);

			filter.Add ("Season", "2013", "2015");
			Assert.AreEqual (12, storage.Retrieve<Project> (filter).Count);

			filter = new QueryFilter ();
			filter.Add ("Season", "2013");
			filter.Add ("Competition", "Liga");
			filter.Add ("LocalName", "Complu");
			Assert.AreEqual (1, storage.Retrieve<Project> (filter).Count);

			filter.Add ("VisitorName", "Fluendo");
			Assert.AreEqual (1, storage.Retrieve<Project> (filter).Count);
		}

		[Test ()]
		public void TestQueryEventsByPlayer ()
		{
			Player andoni = new Player { Name = "Andoni" };
			Player jorge = new Player { Name = "Jorge" };
			Player victor = new Player { Name = "Victor" };
			Player josep = new Player { Name = "Josep" };
			Player davide = new Player { Name = "Davide" };
			Player messi = new Player { Name = "Messi" };
			Player ukelele = new Player { Name = "ukelele" };

			var players = new List<Player> { andoni, jorge, victor, josep, davide };
			foreach (Player player in players) {
				TimelineEvent evt = new TimelineEvent ();
				evt.Players.Add (player);
				evt.Players.Add (messi);
				storage.Store (evt);
			}

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Player", messi);
			Assert.AreEqual (5, storage.Retrieve<TimelineEvent> (filter).Count);

			filter.Add ("Player", andoni);
			Assert.AreEqual (1, storage.Retrieve<TimelineEvent> (filter).Count);

			filter.Add ("Player", andoni, jorge, josep);
			Assert.AreEqual (3, storage.Retrieve<TimelineEvent> (filter).Count);

			filter.Add ("Player", victor, ukelele);
			Assert.AreEqual (1, storage.Retrieve<TimelineEvent> (filter).Count);

			filter.Add ("Player", players);
			Assert.AreEqual (5, storage.Retrieve<TimelineEvent> (filter).Count);
		}

	}
}
