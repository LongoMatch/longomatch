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

		[OneTimeSetUp]
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

		[OneTimeTearDown]
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
			storage.Store (LMDashboard.DefaultTemplate (5));
			var dashboards = storage.Retrieve<Dashboard> (new QueryFilter ());
			Assert.IsFalse (dashboards.Any (d => d.IsLoaded));
		}

		[Test ()]
		public void TestQueryFull ()
		{
			var dashboard = LMDashboard.DefaultTemplate (5);
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
				var da = LMDashboard.DefaultTemplate (5);
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
				var da = LMTeam.DefaultTemplate (5);
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
						LMProject p = new LMProject ();
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
			Assert.AreEqual (9, storage.Retrieve<LMProject> (filter).Count ());

			filter.Add ("Competition", "Liga", "Champions");
			Assert.AreEqual (18, storage.Retrieve<LMProject> (filter).Count ());

			filter.Add ("Season", "2013");
			Assert.AreEqual (6, storage.Retrieve<LMProject> (filter).Count ());

			filter.Add ("Season", "2013", "2015");
			Assert.AreEqual (12, storage.Retrieve<LMProject> (filter).Count ());

			filter = new QueryFilter ();
			filter.Add ("Season", "2013");
			filter.Add ("Competition", "Liga");
			filter.Add ("LocalName", "Complu");
			Assert.AreEqual (1, storage.Retrieve<LMProject> (filter).Count ());

			filter.Add ("VisitorName", "Fluendo");
			Assert.AreEqual (1, storage.Retrieve<LMProject> (filter).Count ());
		}

		[Test ()]
		public void TestQueryEventsByPlayer ()
		{
			LMPlayer andoni = new LMPlayer { Name = "Andoni" };
			LMPlayer jorge = new LMPlayer { Name = "Jorge" };
			LMPlayer victor = new LMPlayer { Name = "Victor" };
			LMPlayer josep = new LMPlayer { Name = "Josep" };
			LMPlayer davide = new LMPlayer { Name = "Davide" };
			LMPlayer messi = new LMPlayer { Name = "Messi" };
			LMPlayer ukelele = new LMPlayer { Name = "ukelele" };

			var players = new List<LMPlayer> { andoni, jorge, victor, josep, davide };
			foreach (LMPlayer player in players) {
				LMTimelineEvent evt = new LMTimelineEvent ();
				evt.Players.Add (player);
				evt.Players.Add (messi);
				storage.Store (evt);
			}

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Player", messi);
			Assert.AreEqual (5, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter.Add ("Player", andoni);
			Assert.AreEqual (1, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter.Add ("Player", andoni, jorge, josep);
			Assert.AreEqual (3, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter.Add ("Player", victor, ukelele);
			Assert.AreEqual (1, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter.Add ("Player", players);
			Assert.AreEqual (5, storage.Retrieve<LMTimelineEvent> (filter).Count ());
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
				LMTimelineEvent evt = new LMTimelineEvent ();
				evt.EventType = evtType;
				storage.Store (evt);
			}

			QueryFilter filter = new QueryFilter ();
			filter.Add ("EventType", evtType1);
			Assert.AreEqual (1, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter.Add ("EventType", evtType4);
			Assert.AreEqual (1, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter.Add ("EventType", evtType2, evtType3);
			Assert.AreEqual (2, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter.Add ("EventType", eventTypes);
			Assert.AreEqual (5, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter.Add ("EventType", evtType5);
			Assert.AreEqual (0, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter.Add ("EventType", score);
			Assert.AreEqual (1, storage.Retrieve<LMTimelineEvent> (filter).Count ());
		}

		[Test ()]
		public void TestQueryEventsByNoPlayerOrTeam ()
		{
			LMPlayer messi = new LMPlayer { Name = "Messi" };
			LMTimelineEvent evt = new LMTimelineEvent ();
			evt.Players.Add (messi);
			storage.Store (evt);
			evt = new LMTimelineEvent ();
			storage.Store (evt);

			QueryFilter filter = new QueryFilter ();
			LMPlayer nullPlayer = null;
			LMTeam nullTeam = null;

			filter.Add ("Player", nullPlayer);
			Assert.AreEqual (1, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter = new QueryFilter ();
			filter.Add ("Team", nullTeam);
			Assert.AreEqual (2, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter = new QueryFilter ();
			QueryFilter teamsAndPlayersFilter = new QueryFilter { Operator = QueryOperator.Or };
			filter.Children.Add (teamsAndPlayersFilter);
			teamsAndPlayersFilter.Add ("Team", nullTeam);
			teamsAndPlayersFilter.Add ("Player", nullPlayer);
			filter.Operator = QueryOperator.Or;
			Assert.AreEqual (2, storage.Retrieve<LMTimelineEvent> (filter).Count ());
		}

		[Test ()]
		public void TestQueryEventsByTeam ()
		{
			LMTeam devTeam, qaTeam;
			List<LMProject> projects;

			projects = CreateProjects ();
			devTeam = projects [0].LocalTeamTemplate;
			qaTeam = projects [0].VisitorTeamTemplate;

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Team", devTeam);
			Assert.AreEqual (125, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter = new QueryFilter ();
			filter.Add ("Team", qaTeam);
			Assert.AreEqual (75, storage.Retrieve<LMTimelineEvent> (filter).Count ());
		}

		[Test ()]
		public void TestQueryEventsByPlayerEventTypeAndProject ()
		{
			Dashboard dashbaord;
			LMTeam devTeam, qaTeam;
			List<LMProject> projects;

			projects = CreateProjects ();

			dashbaord = projects [0].Dashboard;
			devTeam = projects [0].LocalTeamTemplate;
			qaTeam = projects [0].VisitorTeamTemplate;

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Parent", projects [0], projects [1]);
			Assert.AreEqual (80, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter.Add ("Player", devTeam.List [0], qaTeam.List [1]);
			Assert.AreEqual (20, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter.Add ("EventType", (dashbaord.List [0] as AnalysisEventButton).EventType);
			Assert.AreEqual (4, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			filter.Add ("Team", devTeam);
			var res = storage.Retrieve<LMTimelineEvent> (filter);
			Assert.AreEqual (2, storage.Retrieve<LMTimelineEvent> (filter).Count ());
		}

		[Test ()]
		public void TestListElementsInProjects ()
		{
			var projects = CreateProjects ();
			QueryFilter filter = new QueryFilter ();
			filter.Add ("Parent", projects);

			Assert.AreEqual (9, storage.Retrieve<EventType> (filter).Count ());
			Assert.AreEqual (2, storage.Retrieve<Team> (filter).Count ());
			Assert.AreEqual (8, storage.Retrieve<LMPlayer> (filter).Count ());
		}

		[Test ()]
		public void TestNestedQueries ()
		{
			LMTeam devTeam, qaTeam;
			List<LMProject> projects;

			projects = CreateProjects ();
			devTeam = projects [0].LocalTeamTemplate;
			qaTeam = projects [0].VisitorTeamTemplate;

			QueryFilter filter = new QueryFilter ();
			filter.Add ("Parent", projects);

			Assert.AreEqual (200, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			QueryFilter teamsPlayersFilter = new QueryFilter { Operator = QueryOperator.Or };
			teamsPlayersFilter.Add ("Team", qaTeam);
			filter.Children.Add (teamsPlayersFilter);

			Assert.AreEqual (75, storage.Retrieve<LMTimelineEvent> (filter).Count ());

			teamsPlayersFilter.Add ("Player", devTeam.List [0]);
			Assert.AreEqual (100, storage.Retrieve<LMTimelineEvent> (filter).Count ());
		}

		List<LMProject> CreateProjects ()
		{
			LMPlayer andoni = new LMPlayer { Name = "Andoni" };
			LMPlayer jorge = new LMPlayer { Name = "Jorge" };
			LMPlayer victor = new LMPlayer { Name = "Victor" };
			LMPlayer josep = new LMPlayer { Name = "Josep" };
			LMPlayer davide = new LMPlayer { Name = "Davide" };
			LMPlayer saray = new LMPlayer { Name = "Saray" };
			LMPlayer ivan = new LMPlayer { Name = "Ivan" };
			LMPlayer adria = new LMPlayer { Name = "Adria" };
			LMTeam devteam = new LMTeam { Name = "DevTeam" };
			LMTeam qateam = new LMTeam { Name = "QA" };
			devteam.List.AddRange (new List<LMPlayer> {
				andoni,
				jorge,
				victor,
				josep,
				davide
			});
			qateam.List.AddRange (new List<LMPlayer> {
				saray,
				ivan,
				adria
			});
			LMDashboard dashbaord = LMDashboard.DefaultTemplate (5);
			var projects = new List<LMProject> ();
			for (int i = 0; i < 5; i++) {
				LMProject p = new LMProject ();
				p.Dashboard = dashbaord.Clone ();
				p.LocalTeamTemplate = devteam;
				p.VisitorTeamTemplate = qateam;
				p.Description = new ProjectDescription ();
				foreach (var player in devteam.List.Concat (qateam.List)) {
					foreach (var button in p.Dashboard.List.OfType<AnalysisEventButton> ()) {
						LMTimelineEvent evt = p.AddEvent (button.EventType, new Time (0), new Time (10),
							                              new Time (5), null) as LMTimelineEvent;
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
