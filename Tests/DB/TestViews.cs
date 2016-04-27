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
using System.Linq;
using Couchbase.Lite;
using NUnit.Framework;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.Store;
using LongoMatch.DB;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Serialization;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.DB;
using VAS.DB.Views;

namespace Tests.DB
{
	public class PropertiesTest: StorableBase
	{
		[LongoMatchPropertyIndex (1)]
		[LongoMatchPropertyPreload]
		public string Key1 { get; set; }

		[LongoMatchPropertyIndex (0)]
		public string Key2 { get; set; }

		[LongoMatchPropertyPreload]
		public string Key3 { get; set; }

		protected override void CheckIsLoaded ()
		{
			IsLoaded = true;
		}
	}

	public class TestView: GenericView<PropertiesTest>
	{

		public TestView (CouchbaseStorage storage) : base (storage)
		{
		}

		protected override string ViewVersion { get { return "1"; } }

		public List<string> PreloadProperties { get { return PreviewProperties; } }

		public List<string> IndexedProperties { get { return FilterProperties.Keys.OfType<string> ().ToList (); } }
	}


	[TestFixture ()]
	public class TestViews
	{
		CouchbaseStorageLongoMatch storage;
		Database db;

		[TestFixtureSetUp]
		public void InitDB ()
		{
			string dbPath = Path.Combine (Path.GetTempPath (), "TestDB");
			if (Directory.Exists (dbPath)) {
				Directory.Delete (dbPath, true);
			}
			try {
				storage = new CouchbaseStorageLongoMatch (dbPath, "test-db");
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
			DashboardLongoMatch d = DashboardLongoMatch.DefaultTemplate (5);
			d.Name = "Dashboard1";
			storage.Store (d);

			List<DashboardLongoMatch> dashboards = storage.RetrieveAll<DashboardLongoMatch> ().ToList ();
			Assert.AreEqual (1, dashboards.Count);
			Assert.AreEqual (d.ID, dashboards [0].ID);
			Assert.AreEqual (d.Name, dashboards [0].Name);
			Assert.IsTrue (dashboards.All (i => i.DocumentID != null));

			for (int i = 0; i < 5; i++) {
				var da = DashboardLongoMatch.DefaultTemplate (5);
				da.Name = "Dashboard" + (i + 2);
				storage.Store (da);
			}

			dashboards = storage.RetrieveAll<DashboardLongoMatch> ().ToList ();
			Assert.IsTrue (dashboards.All (i => i.DocumentID != null));
			Assert.AreEqual (6, dashboards.Count);
		}

		[Test ()]
		public void TestLoadDashboards ()
		{
			DashboardLongoMatch d = DashboardLongoMatch.DefaultTemplate (5);
			d.Name = "Dashboard1";
			// Make PenaltyCardEventType and ScoreEventType the same object so that both are serialized
			// as references and Utils.AreEquals can check the rest correctly
			(d.List [8] as PenaltyCardButton).EventType = (d.List [7] as PenaltyCardButton).EventType;
			(d.List [10] as ScoreButton).EventType = (d.List [9] as ScoreButton).EventType;
			storage.Store (d);
			DashboardLongoMatch d1 = storage.Retrieve<DashboardLongoMatch> (new QueryFilter ()).First ();
			d1.IsLoaded = true;
			Utils.AreEquals (d, d1, false);
			d1.IsLoaded = false;
			Utils.AreEquals (d, d1);
		}

		[Test ()]
		public void TestListTeams ()
		{
			Team t = Team.DefaultTemplate (5);
			t.Name = "Team1";
			t.Shield = Utils.LoadImageFromFile ();
			storage.Store (t);

			List<Team> teams = storage.RetrieveAll<Team> ().ToList (); 
			Assert.AreEqual (1, teams.Count);
			Assert.AreEqual (t.ID, teams [0].ID);
			Assert.AreEqual (t.Name, teams [0].Name);
			Assert.IsNotNull (teams [0].Shield);
			Assert.IsTrue (teams.All (i => i.DocumentID != null));

			for (int i = 0; i < 5; i++) {
				var te = Team.DefaultTemplate (5);
				te.Name = "Team" + (i + 2);
				storage.Store (te);
			}

			Assert.AreEqual (6, storage.RetrieveAll<Team> ().Count ());
		}

		[Test ()]
		public void TestLoadTeam ()
		{
			Team t = Team.DefaultTemplate (5);
			t.Name = "Team1";
			t.Shield = Utils.LoadImageFromFile ();
			storage.Store (t);
			Team t1 = storage.Retrieve<Team> (new QueryFilter ()).First ();
			t1.IsLoaded = true;
			Utils.AreEquals (t, t1, false);
			t1.IsLoaded = false;
			Utils.AreEquals (t, t1);
			Assert.IsNotNull (t1.DocumentID);
		}

		[Test ()]
		public void TestListProjects ()
		{
			ProjectLongoMatch p = Utils.CreateProject ();
			try {
				p.Description.Group = "GRP";
				p.Description.Competition = "COMP";
				storage.Store (p);

				List<ProjectLongoMatch> projects = storage.RetrieveAll<ProjectLongoMatch> ().ToList ();
				Assert.AreEqual (1, projects.Count);
				Assert.AreEqual (p.Timeline.Count, projects [0].Timeline.Count);
				Assert.AreEqual ("GRP", p.Description.Group);
				Assert.AreEqual ("COMP", p.Description.Competition);
				Assert.IsTrue (projects.All (i => i.DocumentID != null));

				Assert.AreEqual (1, storage.Retrieve<ProjectLongoMatch> (null).Count ());

				var filter = new QueryFilter ();
				filter.Add ("Competition", "COMP");
				Assert.AreEqual (1, storage.Retrieve<ProjectLongoMatch> (filter).Count ());

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
						PlayerLongoMatch p = new PlayerLongoMatch { Name = n, LastName = f, NickName = r };
						storage.Store (p);
					}
				}
			}

			IEnumerable<PlayerLongoMatch> players = storage.RetrieveAll<PlayerLongoMatch> ();
			Assert.AreEqual (36, players.Count ());

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Name", "andoni");
			players = storage.Retrieve<PlayerLongoMatch> (filter);
			Assert.AreEqual (9, players.Count ());

			filter = new QueryFilter ();
			filter.Add ("Name", "andoni");
			filter.Add ("LastName", "zabala");
			players = storage.Retrieve<PlayerLongoMatch> (filter);
			Assert.AreEqual (3, players.Count ());

			filter = new QueryFilter ();
			filter.Add ("Name", "andoni", "aitor");
			players = storage.Retrieve<PlayerLongoMatch> (filter);
			Assert.AreEqual (18, players.Count ());

			filter = new QueryFilter ();
			filter.Add ("Name", "andoni", "aitor");
			filter.Add ("LastName", "zabala");
			players = storage.Retrieve<PlayerLongoMatch> (filter);
			Assert.AreEqual (6, players.Count ());
		}
	}
}
