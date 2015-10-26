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
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.DB;
using NUnit.Framework;

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
		public void TestQueryPreloaded ()
		{
			storage.Store (Dashboard.DefaultTemplate (5));
			var dashboards = storage.Retrieve<Dashboard> (new QueryFilter ());
			Assert.IsFalse (dashboards.Any (d => d.IsLoaded));
		}

		[Test ()]
		public void TestQueryFull ()
		{
			var dashboard = Dashboard.DefaultTemplate (5);
			storage.Store (dashboard);
			var dashboards = storage.RetrieveFull<Dashboard> (new QueryFilter (), null);
			Assert.IsFalse (dashboards.Any (d => !d.IsLoaded));

			StorableObjectsCache cache = new StorableObjectsCache ();
			cache.AddReference (dashboard);
			var newdashboard = storage.RetrieveFull<Dashboard> (new QueryFilter (), cache).First ();
			Assert.IsTrue (Object.ReferenceEquals (dashboard, newdashboard));
		}

		[Test ()]
		public void TestQueryDashboards ()
		{
			IEnumerable<Dashboard> dashboards;

			for (int i = 0; i < 5; i++) {
				var da = Dashboard.DefaultTemplate (5);
				da.Name = "Dashboard" + (i + 1);
				storage.Store (da);
			}

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Name", "Dashboard1");
			dashboards = storage.Retrieve<Dashboard> (filter);
			Assert.AreEqual (1, dashboards.Count ());
			Assert.AreEqual ("Dashboard1", dashboards.First ().Name);

			filter.Add ("Name", "Dashboard1", "Dashboard3");
			dashboards = storage.Retrieve<Dashboard> (filter);
			Assert.AreEqual (2, dashboards.Count ());
			Assert.IsTrue (dashboards.Any (d => d.Name == "Dashboard1"));
			Assert.IsTrue (dashboards.Any (d => d.Name == "Dashboard3"));

			filter = new QueryFilter ();
			filter.Add ("Name", "Pepe");
			dashboards = storage.Retrieve<Dashboard> (filter);
			Assert.AreEqual (0, dashboards.Count ());

			filter = new QueryFilter ();
			filter.Add ("Unkown", "Pepe");
			Assert.Throws<InvalidQueryException> (
				delegate {
					dashboards = storage.Retrieve<Dashboard> (filter).ToList ();
				});
			Assert.AreEqual (0, dashboards.Count ());
		}

		[Test ()]
		public void TestQueryTeams ()
		{
			IEnumerable<Team> teams;

			for (int i = 0; i < 5; i++) {
				var da = Team.DefaultTemplate (5);
				da.Name = "Team" + (i + 1);
				storage.Store (da);
			}

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Name", "Team1");
			teams = storage.Retrieve<Team> (filter);
			Assert.AreEqual (1, teams.Count ());
			Assert.AreEqual ("Team1", teams.First ().Name);

			filter.Add ("Name", "Team1", "Team3");
			teams = storage.Retrieve<Team> (filter);
			Assert.AreEqual (2, teams.Count ());
			Assert.IsTrue (teams.Any (d => d.Name == "Team1"));
			Assert.IsTrue (teams.Any (d => d.Name == "Team3"));

			filter = new QueryFilter ();
			filter.Add ("Name", "Pepe");
			teams = storage.Retrieve<Team> (filter);
			Assert.AreEqual (0, teams.Count ());

			filter = new QueryFilter ();
			filter.Add ("Unkown", "Pepe");
			Assert.Throws<InvalidQueryException> (
				delegate {
					teams = storage.Retrieve<Team> (filter).ToList ();
				});
			Assert.AreEqual (0, teams.Count ());
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
			Assert.AreEqual (9, storage.Retrieve<Project> (filter).Count ());

			filter.Add ("Competition", "Liga", "Champions");
			Assert.AreEqual (18, storage.Retrieve<Project> (filter).Count ());

			filter.Add ("Season", "2013");
			Assert.AreEqual (6, storage.Retrieve<Project> (filter).Count ());

			filter.Add ("Season", "2013", "2015");
			Assert.AreEqual (12, storage.Retrieve<Project> (filter).Count ());

			filter = new QueryFilter ();
			filter.Add ("Season", "2013");
			filter.Add ("Competition", "Liga");
			filter.Add ("LocalName", "Complu");
			Assert.AreEqual (1, storage.Retrieve<Project> (filter).Count ());

			filter.Add ("VisitorName", "Fluendo");
			Assert.AreEqual (1, storage.Retrieve<Project> (filter).Count ());
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
			Assert.AreEqual (5, storage.Retrieve<TimelineEvent> (filter).Count ());

			filter.Add ("Player", andoni);
			Assert.AreEqual (1, storage.Retrieve<TimelineEvent> (filter).Count ());

			filter.Add ("Player", andoni, jorge, josep);
			Assert.AreEqual (3, storage.Retrieve<TimelineEvent> (filter).Count ());

			filter.Add ("Player", victor, ukelele);
			Assert.AreEqual (1, storage.Retrieve<TimelineEvent> (filter).Count ());

			filter.Add ("Player", players);
			Assert.AreEqual (5, storage.Retrieve<TimelineEvent> (filter).Count ());
		}

		[Test ()]
		public void TestQueryEventsByEventType ()
		{
			AnalysisEventType evtType1 = new AnalysisEventType { Name = "Ball lost" };
			AnalysisEventType evtType2 = new AnalysisEventType { Name = "Goal" };
			AnalysisEventType evtType3 = new AnalysisEventType { Name = "PC" };
			AnalysisEventType evtType4 = new AnalysisEventType { Name = "Recovery" };
			AnalysisEventType evtType5 = new AnalysisEventType { Name = "Unused" };

			var eventTypes = new List<AnalysisEventType> { evtType1, evtType2, evtType3, evtType4 };
			foreach (AnalysisEventType evtType in eventTypes) {
				TimelineEvent evt = new TimelineEvent ();
				evt.EventType = evtType;
				storage.Store (evt);
			}

			QueryFilter filter = new QueryFilter ();
			filter.Add ("EventType", evtType1);
			Assert.AreEqual (1, storage.Retrieve<TimelineEvent> (filter).Count ());

			filter.Add ("EventType", evtType4);
			Assert.AreEqual (1, storage.Retrieve<TimelineEvent> (filter).Count ());

			filter.Add ("EventType", evtType2, evtType3);
			Assert.AreEqual (2, storage.Retrieve<TimelineEvent> (filter).Count ());

			filter.Add ("EventType", evtType5);
			Assert.AreEqual (0, storage.Retrieve<TimelineEvent> (filter).Count ());

			filter.Add ("EventType", eventTypes);
			Assert.AreEqual (4, storage.Retrieve<TimelineEvent> (filter).Count ());
		}

		[Test ()]
		public void TestQueryEventsByTeam ()
		{
			Team devTeam, qaTeam;
			List<Project> projects;

			projects = CreateProjects ();
			devTeam = projects [0].LocalTeamTemplate;
			qaTeam = projects [0].VisitorTeamTemplate;

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Team", devTeam);
			Assert.AreEqual (125, storage.Retrieve<TimelineEvent> (filter).Count ());

			filter = new QueryFilter ();
			filter.Add ("Team", qaTeam);
			Assert.AreEqual (75, storage.Retrieve<TimelineEvent> (filter).Count ());
		}

		[Test ()]
		public void TestQueryEventsByPlayerEventTypeAndProject ()
		{
			Dashboard dashbaord;
			Team devTeam, qaTeam;
			List<Project> projects;

			projects = CreateProjects ();

			dashbaord = projects [0].Dashboard;
			devTeam = projects [0].LocalTeamTemplate;
			qaTeam = projects [0].VisitorTeamTemplate;

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Parent", projects [0], projects [1]);
			Assert.AreEqual (80, storage.Retrieve<TimelineEvent> (filter).Count ());

			filter.Add ("Player", devTeam.List [0], qaTeam.List [1]);
			Assert.AreEqual (20, storage.Retrieve<TimelineEvent> (filter).Count ());

			filter.Add ("EventType", (dashbaord.List [0] as AnalysisEventButton).EventType);
			Assert.AreEqual (4, storage.Retrieve<TimelineEvent> (filter).Count ());

			filter.Add ("Team", devTeam);
			var res = storage.Retrieve<TimelineEvent> (filter);
			Assert.AreEqual (2, storage.Retrieve<TimelineEvent> (filter).Count ());
		}

		[Test ()]
		public void TestListElementsInProjects ()
		{
			var projects = CreateProjects ();
			QueryFilter filter = new QueryFilter ();
			filter.Add ("Parent", projects);

			Assert.AreEqual (5, storage.Retrieve<EventType> (filter).Count ());
			Assert.AreEqual (2, storage.Retrieve<Team> (filter).Count ());
			Assert.AreEqual (8, storage.Retrieve<Player> (filter).Count ());
		}

		List<Project> CreateProjects ()
		{
			Player andoni = new Player { Name = "Andoni" };
			Player jorge = new Player { Name = "Jorge" };
			Player victor = new Player { Name = "Victor" };
			Player josep = new Player { Name = "Josep" };
			Player davide = new Player { Name = "Davide" };
			Player saray = new Player { Name = "Saray" };
			Player ivan = new Player { Name = "Ivan" };
			Player adria = new Player { Name = "Adria" };
			Team devteam = new Team { Name = "DevTeam" };
			Team qateam = new Team { Name = "QA" };
			devteam.List.AddRange (new List<Player> {
				andoni,
				jorge,
				victor,
				josep,
				davide
			});
			qateam.List.AddRange (new List<Player> {
				saray,
				ivan,
				adria
			});
			Dashboard dashbaord = Dashboard.DefaultTemplate (5);
			var projects = new List<Project> ();
			for (int i = 0; i < 5; i++) {
				Project p = new Project ();
				p.Dashboard = dashbaord.Clone ();
				p.LocalTeamTemplate = devteam;
				p.VisitorTeamTemplate = qateam;
				p.Description = new ProjectDescription ();
				foreach (var player in devteam.List.Concat (qateam.List)) {
					foreach (var button in p.Dashboard.List.OfType<AnalysisEventButton> ()) {
						TimelineEvent evt = p.AddEvent (button.EventType, new Time (0), new Time (10), new Time (5), null, null, null);
						evt.Players.Add (player);
					}
				}
				projects.Add (p);
				storage.Store (p);
			}
			return projects;
		}
	}
}
