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
using System.IO;
using Couchbase.Lite;
using LongoMatch.Core.Store.Templates;
using LongoMatch.DB;
using NUnit.Framework;
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;

namespace Tests.DB
{
	[TestFixture ()]
	public class TestViews
	{
		CouchbaseStorage storage;
		Database db;

		[TestFixtureSetUp]
		public void InitDB ()
		{
			string dbPath = Path.Combine (Path.GetTempPath (), "TestDB");
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
		public void TestListDashboards ()
		{
			Dashboard d = Dashboard.DefaultTemplate (5);
			d.Name = "Dashboard1";
			storage.Store (d);

			List<Dashboard> dashboards = storage.RetrieveAll<Dashboard> (); 
			Assert.AreEqual (1, dashboards.Count);
			Assert.AreEqual (d.ID, dashboards [0].ID);
			Assert.AreEqual (d.Name, dashboards [0].Name);

			for (int i = 0; i < 5; i++) {
				var da = Dashboard.DefaultTemplate (5);
				da.Name = "Dashboard" + (i + 2);
				storage.Store (da);
			}

			dashboards = storage.RetrieveAll<Dashboard> (); 
			Assert.AreEqual (6, dashboards.Count);

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Name", "Dashboard1");
			dashboards = storage.Retrieve<Dashboard> (filter);
			Assert.AreEqual (1, dashboards.Count);
			Assert.AreEqual (d.ID, dashboards [0].ID);
			Assert.AreEqual (d.Name, dashboards [0].Name);

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
		public void TestListTeams ()
		{
			Team t = Team.DefaultTemplate (5);
			t.Name = "Team1";
			t.Shield = Utils.LoadImageFromFile ();
			storage.Store (t);

			List<Team> teams = storage.RetrieveAll<Team> (); 
			Assert.AreEqual (1, teams.Count);
			Assert.AreEqual (t.ID, teams [0].ID);
			Assert.AreEqual (t.Name, teams [0].Name);
			Assert.IsNotNull (teams [0].Shield);

			for (int i = 0; i < 5; i++) {
				var te = Team.DefaultTemplate (5);
				te.Name = "Team" + (i + 2);
				storage.Store (te);
			}

			teams = storage.RetrieveAll<Team> (); 
			Assert.AreEqual (6, teams.Count);

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Name", "Team1");
			teams = storage.Retrieve<Team> (filter);
			Assert.AreEqual (1, teams.Count);
			Assert.AreEqual (t.ID, teams [0].ID);
			Assert.AreEqual (t.Name, teams [0].Name);

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
		public void TestListProjects ()
		{
			Project p = Utils.CreateProject ();
			try {
				p.Description.Group = "GRP";
				p.Description.Competition = "COMP";
				storage.Store (p);

				List<Project> projects = storage.RetrieveAll<Project> ();
				Assert.AreEqual (1, projects.Count);
				Assert.AreNotEqual (p.Timeline.Count, projects [0].Timeline.Count);
				Assert.AreEqual ("GRP", p.Description.Group);
				Assert.AreEqual ("COMP", p.Description.Competition);

				projects = storage.Retrieve<Project> (null);
				Assert.AreEqual (1, projects.Count);

				var filter = new QueryFilter ();
				filter.Add ("Competition", "COMP");
				projects = storage.Retrieve<Project> (filter);
				Assert.AreEqual (1, projects.Count);

			} finally {
				Utils.DeleteProject (p);
			}
		}

		[Test ()]
		public void TestListPlayers ()
		{
			foreach (string n in new []{"andoni", "aitor", "xabi", "iñaki"}) {
				foreach (string f in new []{"gorriti", "zabala", "otegui"}) {
					foreach (string r in new []{"cholo", "bobi", "tolai"}) {
						Player p = new Player { Name = n, LastName = f, NickName = r };
						storage.Store (p);
					}
				}
			}

			List<Player> players = storage.RetrieveAll<Player> (); 
			Assert.AreEqual (36, players.Count);

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Name", "andoni");
			players = storage.Retrieve<Player> (filter);
			Assert.AreEqual (9, players.Count);

			filter = new QueryFilter ();
			filter.Add ("Name", "andoni");
			filter.Add ("LastName", "zabala");
			players = storage.Retrieve<Player> (filter);
			Assert.AreEqual (3, players.Count);

			filter = new QueryFilter ();
			filter.Add ("Name", "andoni", "aitor");
			players = storage.Retrieve<Player> (filter);
			Assert.AreEqual (18, players.Count);

			filter = new QueryFilter ();
			filter.Add ("Name", "andoni", "aitor");
			filter.Add ("LastName", "zabala");
			players = storage.Retrieve<Player> (filter);
			Assert.AreEqual (6, players.Count);
		}
	}
}

