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
using LongoMatch;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.DB;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.DB;

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
		public void TestQueryPreloaded ()
		{
			storage.Store (DashboardLongoMatch.DefaultTemplate (5));
			var dashboards = storage.Retrieve<DashboardLongoMatch> (new QueryFilter ());
			Assert.IsFalse (dashboards.Any (d => d.IsLoaded));
		}

		[Test ()]
		public void TestQueryFull ()
		{
			var dashboard = DashboardLongoMatch.DefaultTemplate (5);
			storage.Store (dashboard);
			var dashboards = storage.RetrieveFull<DashboardLongoMatch> (new QueryFilter (), null);
			Assert.IsFalse (dashboards.Any (d => !d.IsLoaded));

			StorableObjectsCache cache = new StorableObjectsCache ();
			cache.AddReference (dashboard);
			var newdashboard = storage.RetrieveFull<DashboardLongoMatch> (new QueryFilter (), cache).First ();
			Assert.IsTrue (Object.ReferenceEquals (dashboard, newdashboard));
		}

		[Test ()]
		public void TestQueryDashboards ()
		{
			IEnumerable<DashboardLongoMatch> dashboards;

			for (int i = 0; i < 5; i++) {
				var da = DashboardLongoMatch.DefaultTemplate (5);
				da.Name = "Dashboard" + (i + 1);
				storage.Store (da);
			}

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Name", "Dashboard1");
			dashboards = storage.Retrieve<DashboardLongoMatch> (filter);
			Assert.AreEqual (1, dashboards.Count ());
			Assert.AreEqual ("Dashboard1", dashboards.First ().Name);

			filter.Add ("Name", "Dashboard1", "Dashboard3");
			dashboards = storage.Retrieve<DashboardLongoMatch> (filter);
			Assert.AreEqual (2, dashboards.Count ());
			Assert.IsTrue (dashboards.Any (d => d.Name == "Dashboard1"));
			Assert.IsTrue (dashboards.Any (d => d.Name == "Dashboard3"));

			filter = new QueryFilter ();
			filter.Add ("Name", "Pepe");
			dashboards = storage.Retrieve<DashboardLongoMatch> (filter);
			Assert.AreEqual (0, dashboards.Count ());

			filter = new QueryFilter ();
			filter.Add ("Unkown", "Pepe");
			Assert.Throws<InvalidQueryException> (
				delegate {
					dashboards = storage.Retrieve<DashboardLongoMatch> (filter).ToList ();
				});
			Assert.AreEqual (0, dashboards.Count ());
		}

		[Test ()]
		public void TestQueryTeams ()
		{
			IEnumerable<SportsTeam> teams;

			for (int i = 0; i < 5; i++) {
				var da = SportsTeam.DefaultTemplate (5);
				da.Name = "Team" + (i + 1);
				storage.Store (da);
			}

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Name", "Team1");
			teams = storage.Retrieve<SportsTeam> (filter);
			Assert.AreEqual (1, teams.Count ());
			Assert.AreEqual ("Team1", teams.First ().Name);

			filter.Add ("Name", "Team1", "Team3");
			teams = storage.Retrieve<SportsTeam> (filter);
			Assert.AreEqual (2, teams.Count ());
			Assert.IsTrue (teams.Any (d => d.Name == "Team1"));
			Assert.IsTrue (teams.Any (d => d.Name == "Team3"));

			filter = new QueryFilter ();
			filter.Add ("Name", "Pepe");
			teams = storage.Retrieve<SportsTeam> (filter);
			Assert.AreEqual (0, teams.Count ());

			filter = new QueryFilter ();
			filter.Add ("Unkown", "Pepe");
			Assert.Throws<InvalidQueryException> (
				delegate {
					teams = storage.Retrieve<SportsTeam> (filter).ToList ();
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
						ProjectLongoMatch p = new ProjectLongoMatch ();
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
			Assert.AreEqual (9, storage.Retrieve<ProjectLongoMatch> (filter).Count ());

			filter.Add ("Competition", "Liga", "Champions");
			Assert.AreEqual (18, storage.Retrieve<ProjectLongoMatch> (filter).Count ());

			filter.Add ("Season", "2013");
			Assert.AreEqual (6, storage.Retrieve<ProjectLongoMatch> (filter).Count ());

			filter.Add ("Season", "2013", "2015");
			Assert.AreEqual (12, storage.Retrieve<ProjectLongoMatch> (filter).Count ());

			filter = new QueryFilter ();
			filter.Add ("Season", "2013");
			filter.Add ("Competition", "Liga");
			filter.Add ("LocalName", "Complu");
			Assert.AreEqual (1, storage.Retrieve<ProjectLongoMatch> (filter).Count ());

			filter.Add ("VisitorName", "Fluendo");
			Assert.AreEqual (1, storage.Retrieve<ProjectLongoMatch> (filter).Count ());
		}

		[Test ()]
		public void TestQueryEventsByPlayer ()
		{
			PlayerLongoMatch andoni = new PlayerLongoMatch { Name = "Andoni" };
			PlayerLongoMatch jorge = new PlayerLongoMatch { Name = "Jorge" };
			PlayerLongoMatch victor = new PlayerLongoMatch { Name = "Victor" };
			PlayerLongoMatch josep = new PlayerLongoMatch { Name = "Josep" };
			PlayerLongoMatch davide = new PlayerLongoMatch { Name = "Davide" };
			PlayerLongoMatch messi = new PlayerLongoMatch { Name = "Messi" };
			PlayerLongoMatch ukelele = new PlayerLongoMatch { Name = "ukelele" };

			var players = new List<PlayerLongoMatch> { andoni, jorge, victor, josep, davide };
			foreach (PlayerLongoMatch player in players) {
				TimelineEventLongoMatch evt = new TimelineEventLongoMatch ();
				evt.Players.Add (player);
				evt.Players.Add (messi);
				storage.Store (evt);
			}

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Player", messi);
			Assert.AreEqual (5, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter.Add ("Player", andoni);
			Assert.AreEqual (1, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter.Add ("Player", andoni, jorge, josep);
			Assert.AreEqual (3, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter.Add ("Player", victor, ukelele);
			Assert.AreEqual (1, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter.Add ("Player", players);
			Assert.AreEqual (5, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());
		}

		[Test ()]
		public void TestQueryEventsByEventType ()
		{
			AnalysisEventType evtType1 = new AnalysisEventType { Name = "Ball lost" };
			AnalysisEventType evtType2 = new AnalysisEventType { Name = "PC" };
			AnalysisEventType evtType3 = new AnalysisEventType { Name = "Recovery" };
			AnalysisEventType evtType4 = new AnalysisEventType { Name = "Throw-in" };
			AnalysisEventType evtType5 = new AnalysisEventType { Name = "Unused" };
			ScoreEventType score = new ScoreEventType { Name = "Goal" };

			var eventTypes = new List<EventType> { evtType1, evtType2, evtType3, evtType4, score };
			foreach (EventType evtType in eventTypes) {
				TimelineEventLongoMatch evt = new TimelineEventLongoMatch ();
				evt.EventType = evtType;
				storage.Store (evt);
			}

			QueryFilter filter = new QueryFilter ();
			filter.Add ("EventType", evtType1);
			Assert.AreEqual (1, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter.Add ("EventType", evtType4);
			Assert.AreEqual (1, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter.Add ("EventType", evtType2, evtType3);
			Assert.AreEqual (2, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter.Add ("EventType", eventTypes);
			Assert.AreEqual (5, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter.Add ("EventType", evtType5);
			Assert.AreEqual (0, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter.Add ("EventType", score);
			Assert.AreEqual (1, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());
		}

		[Test ()]
		public void TestQueryEventsByNoPlayerOrTeam ()
		{
			PlayerLongoMatch messi = new PlayerLongoMatch { Name = "Messi" };
			TimelineEventLongoMatch evt = new TimelineEventLongoMatch ();
			evt.Players.Add (messi);
			storage.Store (evt);
			evt = new TimelineEventLongoMatch ();
			storage.Store (evt);

			QueryFilter filter = new QueryFilter ();
			PlayerLongoMatch nullPlayer = null;
			SportsTeam nullTeam = null;

			filter.Add ("Player", nullPlayer);
			Assert.AreEqual (1, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter = new QueryFilter ();
			filter.Add ("Team", nullTeam);
			Assert.AreEqual (2, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter = new QueryFilter ();
			QueryFilter teamsAndPlayersFilter = new QueryFilter { Operator = QueryOperator.Or };
			filter.Children.Add (teamsAndPlayersFilter);
			teamsAndPlayersFilter.Add ("Team", nullTeam);
			teamsAndPlayersFilter.Add ("Player", nullPlayer);
			filter.Operator = QueryOperator.Or;
			Assert.AreEqual (2, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());
		}

		[Test ()]
		public void TestQueryEventsByTeam ()
		{
			SportsTeam devTeam, qaTeam;
			List<ProjectLongoMatch> projects;

			projects = CreateProjects ();
			devTeam = projects [0].LocalTeamTemplate;
			qaTeam = projects [0].VisitorTeamTemplate;

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Team", devTeam);
			Assert.AreEqual (125, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter = new QueryFilter ();
			filter.Add ("Team", qaTeam);
			Assert.AreEqual (75, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());
		}

		[Test ()]
		public void TestQueryEventsByPlayerEventTypeAndProject ()
		{
			Dashboard dashbaord;
			SportsTeam devTeam, qaTeam;
			List<ProjectLongoMatch> projects;

			projects = CreateProjects ();

			dashbaord = projects [0].Dashboard;
			devTeam = projects [0].LocalTeamTemplate;
			qaTeam = projects [0].VisitorTeamTemplate;

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Parent", projects [0], projects [1]);
			Assert.AreEqual (80, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter.Add ("Player", devTeam.List [0], qaTeam.List [1]);
			Assert.AreEqual (20, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter.Add ("EventType", (dashbaord.List [0] as AnalysisEventButton).EventType);
			Assert.AreEqual (4, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			filter.Add ("Team", devTeam);
			var res = storage.Retrieve<TimelineEventLongoMatch> (filter);
			Assert.AreEqual (2, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());
		}

		[Test ()]
		public void TestListElementsInProjects ()
		{
			var projects = CreateProjects ();
			QueryFilter filter = new QueryFilter ();
			filter.Add ("Parent", projects);

			Assert.AreEqual (9, storage.Retrieve<EventType> (filter).Count ());
			Assert.AreEqual (2, storage.Retrieve<SportsTeam> (filter).Count ());
			Assert.AreEqual (8, storage.Retrieve<PlayerLongoMatch> (filter).Count ());
		}

		[Test ()]
		public void TestNestedQueries ()
		{
			SportsTeam devTeam, qaTeam;
			List<ProjectLongoMatch> projects;

			projects = CreateProjects ();
			devTeam = projects [0].LocalTeamTemplate;
			qaTeam = projects [0].VisitorTeamTemplate;

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Parent", projects);

			Assert.AreEqual (200, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			QueryFilter teamsPlayersFilter = new QueryFilter { Operator = QueryOperator.Or };
			teamsPlayersFilter.Add ("Team", qaTeam);
			filter.Children.Add (teamsPlayersFilter);

			Assert.AreEqual (75, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());

			teamsPlayersFilter.Add ("Player", devTeam.List [0]);
			Assert.AreEqual (100, storage.Retrieve<TimelineEventLongoMatch> (filter).Count ());
		}

		List<ProjectLongoMatch> CreateProjects ()
		{
			PlayerLongoMatch andoni = new PlayerLongoMatch { Name = "Andoni" };
			PlayerLongoMatch jorge = new PlayerLongoMatch { Name = "Jorge" };
			PlayerLongoMatch victor = new PlayerLongoMatch { Name = "Victor" };
			PlayerLongoMatch josep = new PlayerLongoMatch { Name = "Josep" };
			PlayerLongoMatch davide = new PlayerLongoMatch { Name = "Davide" };
			PlayerLongoMatch saray = new PlayerLongoMatch { Name = "Saray" };
			PlayerLongoMatch ivan = new PlayerLongoMatch { Name = "Ivan" };
			PlayerLongoMatch adria = new PlayerLongoMatch { Name = "Adria" };
			SportsTeam devteam = new SportsTeam { Name = "DevTeam" };
			SportsTeam qateam = new SportsTeam { Name = "QA" };
			devteam.List.AddRange (new List<PlayerLongoMatch> {
				andoni,
				jorge,
				victor,
				josep,
				davide
			});
			qateam.List.AddRange (new List<PlayerLongoMatch> {
				saray,
				ivan,
				adria
			});
			DashboardLongoMatch dashbaord = DashboardLongoMatch.DefaultTemplate (5);
			var projects = new List<ProjectLongoMatch> ();
			for (int i = 0; i < 5; i++) {
				ProjectLongoMatch p = new ProjectLongoMatch ();
				p.Dashboard = dashbaord.Clone ();
				p.LocalTeamTemplate = devteam;
				p.VisitorTeamTemplate = qateam;
				p.Description = new ProjectDescription ();
				foreach (var player in devteam.List.Concat (qateam.List)) {
					foreach (var button in p.Dashboard.List.OfType<AnalysisEventButton> ()) {
						TimelineEventLongoMatch evt = p.AddEvent (button.EventType, new Time (0), new Time (10),
							                              new Time (5), null) as TimelineEventLongoMatch;
						evt.Players.Add (player);
						if (qateam.List.Contains (player)) {
							evt.Teams.Add (qateam);
						} else {
							evt.Teams.Add (devteam);
						}
					}
				}
				projects.Add (p);
				storage.Store (p);
			}
			return projects;
		}
	}
}
