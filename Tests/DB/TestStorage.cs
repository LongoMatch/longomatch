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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Couchbase.Lite;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Serialization;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using LongoMatch;
using Moq;
using LongoMatch.Services;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;

namespace Tests.DB
{
	class StorableContainerTest: StorableBase
	{
		public StorableContainerTest ()
		{
			ID = Guid.NewGuid ();
		}

		public StorableImageTest Image { get; set; }
	}

	class StorableListTest: StorableBase
	{
		public StorableListTest ()
		{
			ID = Guid.NewGuid ();
		}

		public List<StorableImageTest> Images { get; set; }
	}

	class StorableListNoChildrenTest: StorableListTest
	{
		public override bool DeleteChildren {
			get {
				return false;
			}
		}
	}


	class StorableImageTest : StorableBase
	{
		public StorableImageTest ()
		{
			ID = Guid.NewGuid ();
		}

		public Image Image1 { get; set; }

		public Image Image2 { get; set; }

		public List<Image> Images { get; set; }
	}

	class StorableImageTest2: StorableImageTest
	{
	}

	[TestFixture ()]
	public class TestStorage
	{
		Database db;
		CouchbaseStorage storage;

		[TestFixtureSetUp]
		public void InitDB ()
		{
			string tmpPath = Path.GetTempPath ();
			string homePath = Path.Combine (tmpPath, "LongoMatch");
			string dbPath = Path.Combine (homePath, "db");
			if (Directory.Exists (dbPath)) {
				Directory.Delete (dbPath, true);
			}

			Directory.CreateDirectory (tmpPath);
			Directory.CreateDirectory (homePath);
			Directory.CreateDirectory (dbPath);

			storage = new CouchbaseStorage (dbPath, "test-db");
			db = storage.Database;
			// Remove the StorageInfo doc to get more understandable document count results
			db.GetDocument (Guid.Empty.ToString ()).Delete ();

			Environment.SetEnvironmentVariable ("LONGOMATCH_HOME", tmpPath);
			Environment.SetEnvironmentVariable ("LGM_UNINSTALLED", "1");
			CoreServices.Init ();
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

		[Test]
		public void TestDeleteError ()
		{
			Assert.Throws<StorageException> (() => storage.Delete<Project> (null));
		}

		[Test]
		public void TestStoreError ()
		{
			Assert.Throws<StorageException> (() => storage.Store<Project> (null));
		}

		[Test ()]
		public void TestDocType ()
		{
			StorableImageTest t = new StorableImageTest {
				ID = Guid.NewGuid (),
			};
			Document doc = db.CreateDocument ();
			SerializationContext context = new SerializationContext (db, t.GetType ());
			JObject jo = DocumentsSerializer.SerializeObject (t, doc.CreateRevision (), context);
			Assert.AreEqual (t.ID, jo.Value<Guid> ("ID"));
			Assert.AreEqual ("StorableImageTest", jo.Value<string> ("DocType"));
		}

		[Test ()]
		public void TestStoreImages ()
		{
			Image img = Utils.LoadImageFromFile ();
			StorableImageTest t = new StorableImageTest {
				Image1 = img,
				Image2 = img,
				ID = Guid.NewGuid (),
			};
			Document doc = db.CreateDocument ();
			UnsavedRevision rev = doc.CreateRevision ();
			SerializationContext context = new SerializationContext (db, t.GetType ());
			JObject jo = DocumentsSerializer.SerializeObject (t, rev, context);
			Assert.IsNotNull (jo ["ID"]);
			Assert.AreEqual ("attachment::Image1_1", jo ["Image1"].Value<string> ());
			Assert.AreEqual ("attachment::Image2_1", jo ["Image2"].Value<string> ());
			int i = 0;
			foreach (string name in rev.AttachmentNames) {
				i++;
				Assert.AreEqual (string.Format ("Image{0}_1", i), name);
			}
		}

		[Test ()]
		public void TestStoreImagesList ()
		{
			Image img = Utils.LoadImageFromFile ();
			StorableImageTest t = new StorableImageTest {
				Images = new List<Image> { img, img, img },
				ID = Guid.NewGuid (),
			};
			Document doc = db.CreateDocument ();
			UnsavedRevision rev = doc.CreateRevision ();
			SerializationContext context = new SerializationContext (db, t.GetType ());
			JObject jo = DocumentsSerializer.SerializeObject (t, rev, context);
			int i = 0;
			foreach (string name in rev.AttachmentNames) {
				i++;
				Assert.AreEqual ("Images_" + i, name);
			}
			Assert.AreEqual (3, i);
			Assert.AreEqual ("attachment::Images_1", jo ["Images"] [0].Value<string> ());
			Assert.AreEqual ("attachment::Images_2", jo ["Images"] [1].Value<string> ());
			Assert.AreEqual ("attachment::Images_3", jo ["Images"] [2].Value<string> ());
		}

		[Test ()]
		public void TestDeleteChildren ()
		{
			StorableListTest list = new StorableListTest ();
			list.Images = new List<StorableImageTest> ();
			list.Images.Add (new StorableImageTest ());
			list.Images.Add (new StorableImageTest ());
			storage.Store (list);
			Assert.AreEqual (3, db.DocumentCount);
			storage.Delete (list);
			Assert.AreEqual (0, db.DocumentCount);

			StorableListNoChildrenTest list2 = new StorableListNoChildrenTest ();
			list2.Images = new List<StorableImageTest> ();
			list2.Images.Add (new StorableImageTest ());
			list2.Images.Add (new StorableImageTest ());
			storage.Store (list2);
			Assert.AreEqual (3, db.DocumentCount);
			storage.Delete (list2);
			Assert.AreEqual (2, db.DocumentCount);
		}

		[Test ()]
		public void TestDeleteOrphanedChildrenOnDelete ()
		{
			StorableListTest list = new StorableListTest ();
			list.Images = new List<StorableImageTest> ();
			list.Images.Add (new StorableImageTest ());
			list.Images.Add (new StorableImageTest ());
			storage.Store (list);
			Assert.AreEqual (3, db.DocumentCount);
			list = storage.Retrieve<StorableListTest> (list.ID);
			list.Images.Remove (list.Images [0]);
			storage.Delete (list);
			Assert.AreEqual (0, db.DocumentCount);
		}

		[Test ()]
		public void TestDeleteOrphanedChildrenOnUpdate ()
		{
			StorableListTest list = new StorableListTest ();
			list.Images = new List<StorableImageTest> ();
			list.Images.Add (new StorableImageTest ());
			list.Images.Add (new StorableImageTest ());
			storage.Store (list);
			Assert.AreEqual (3, db.DocumentCount);
			list = storage.Retrieve<StorableListTest> (list.ID);
			list.Images.Remove (list.Images [0]);
			storage.Store (list);
			Assert.AreEqual (2, db.DocumentCount);
		}

		[Test ()]
		public void TestRetrieveImages ()
		{
			Image img = Utils.LoadImageFromFile ();
			StorableImageTest t = new StorableImageTest {
				Image1 = img,
				Image2 = img,
				ID = Guid.NewGuid (),
			};
			storage.Store (t);
			var test2 = storage.Retrieve<StorableImageTest> (t.ID);
			Assert.AreEqual (t.Image1.Width, test2.Image1.Width);
			Assert.AreEqual (t.Image1.Height, test2.Image1.Height);
			Assert.AreEqual (t.Image2.Width, test2.Image2.Width);
			Assert.AreEqual (t.Image2.Height, test2.Image2.Height);
		}

		[Test ()]
		public void TestStoreStorableByReference ()
		{
			StorableImageTest img = new StorableImageTest {
				ID = Guid.NewGuid (),
				Image1 = Utils.LoadImageFromFile (),
			};
			StorableContainerTest cont = new StorableContainerTest {
				ID = Guid.NewGuid (),
				Image = img,
			};
			Assert.AreEqual (0, db.DocumentCount);
			Document doc = db.CreateDocument ();
			UnsavedRevision rev = doc.CreateRevision ();
			SerializationContext context = new SerializationContext (db, cont.GetType ());
			JObject jo = DocumentsSerializer.SerializeObject (cont, rev, context);
			Assert.AreEqual (img.ID.ToString (), jo ["Image"].Value<String> ());
			Assert.AreEqual (1, db.DocumentCount);
			Assert.IsNotNull (storage.Retrieve<StorableImageTest> (img.ID));
			rev.Save ();
			Assert.AreEqual (2, db.DocumentCount);
		}

		[Test ()]
		public void TestRetrieveStorableByReference ()
		{
			StorableImageTest img = new StorableImageTest {
				ID = Guid.NewGuid (),
				Image1 = Utils.LoadImageFromFile (),
			};
			StorableContainerTest cont = new StorableContainerTest {
				ID = Guid.NewGuid (),
				Image = img,
			};
			storage.Store (cont);
			Assert.AreEqual (2, db.DocumentCount);
			var cont2 = storage.Retrieve <StorableContainerTest> (cont.ID);
			Assert.IsNotNull (cont2.Image);
			Assert.AreEqual (img.ID, cont2.Image.ID);
		}

		[Test ()]
		public void TestRetrieveStorableListByReference ()
		{
			StorableListTest list = new StorableListTest ();
			list.Images = new List<StorableImageTest> ();
			list.Images.Add (new StorableImageTest ());
			list.Images.Add (new StorableImageTest2 ());

			storage.Store (list);
			Assert.AreEqual (3, db.DocumentCount);

			StorableListTest list2 = storage.Retrieve<StorableListTest> (list.ID);
			Assert.AreEqual (2, list2.Images.Count);
			Assert.AreEqual (typeof(StorableImageTest), list2.Images [0].GetType ());
			Assert.AreEqual (typeof(StorableImageTest2), list2.Images [1].GetType ());
		}

		[Test ()]
		public void TestStorableIDUsesRootStorableID ()
		{
			StorableImageTest img = new StorableImageTest {
				ID = Guid.NewGuid (),
				Image1 = Utils.LoadImageFromFile (),
			};
			StorableContainerTest cont = new StorableContainerTest {
				ID = Guid.NewGuid (),
				Image = img,
			};
			Assert.AreEqual (0, db.DocumentCount);
			string newID = String.Format ("{0}&{1}", cont.ID, img.ID); 
			storage.Store (cont);
			Assert.AreEqual (2, db.DocumentCount);
			Assert.IsNotNull (db.GetExistingDocument (cont.ID.ToString ()));
			Assert.IsNotNull (db.GetExistingDocument (newID));
			cont = storage.Retrieve<StorableContainerTest> (cont.ID);
			Assert.AreEqual (img.ID, cont.Image.ID);
			storage.Delete (cont);
			Assert.AreEqual (0, db.DocumentCount);
		}


		[Test ()]
		public void TestRetrieveErrors ()
		{
			// ID does not exists
			Assert.IsNull (storage.Retrieve<Project> (Guid.Empty));
			// ID exists but for a different type;
			StorableImageTest t = new StorableImageTest {
				ID = Guid.NewGuid (),
			};
			storage.Store (t);
			Assert.IsNull (storage.Retrieve<Project> (t.ID));
		}

		[Test ()]
		public void TestIsChangedResetted ()
		{
			Team t, t1;
			ObjectChangedParser parser;
			List<IStorable> storables = null, changed = null;
			StorableNode parent = null;

			parser = new ObjectChangedParser ();
			t = Team.DefaultTemplate (10);
			storage.Store (t);

			// After loading an object
			t1 = DocumentsSerializer.LoadObject (typeof(Team), t.ID, db) as Team;
			Assert.IsTrue (parser.ParseInternal (out parent, t1, Serializer.JsonSettings));
			Assert.IsTrue (parent.ParseTree (ref storables, ref changed));
			Assert.AreEqual (0, changed.Count);
			Assert.NotNull (t1.DocumentID);

			// After filling an object
			t1 = new Team ();
			t1.ID = t.ID;
			t1.DocumentID = t.ID.ToString ();
			t1.IsChanged = true;
			DocumentsSerializer.FillObject (t1, db);
			Assert.IsTrue (parser.ParseInternal (out parent, t1, Serializer.JsonSettings));
			Assert.IsTrue (parent.ParseTree (ref storables, ref changed));
			Assert.AreEqual (0, changed.Count);
		}

		[Test ()]
		public void TestDashboards ()
		{
			Dashboard dashboard = Dashboard.DefaultTemplate (10);
			dashboard.Image = dashboard.FieldBackground = dashboard.HalfFieldBackground =
				dashboard.GoalBackground = Utils.LoadImageFromFile ();
			storage.Store (dashboard);
			Assert.AreEqual (15, db.DocumentCount);
			Assert.IsNotNull (db.GetExistingDocument (dashboard.ID.ToString ()));
			Dashboard dashboard2 = storage.Retrieve<Dashboard> (dashboard.ID);
			Assert.IsNotNull (dashboard2);
			Assert.AreEqual (dashboard.ID, dashboard2.ID);
			Assert.AreEqual (dashboard.List.Count, dashboard2.List.Count);
			Assert.IsNotNull (dashboard2.Image);
			Assert.IsNotNull (dashboard2.FieldBackground);
			Assert.IsNotNull (dashboard2.HalfFieldBackground);
			Assert.IsNotNull (dashboard2.GoalBackground);
			Assert.IsNotNull (dashboard2.DocumentID);
			Assert.AreEqual (16, dashboard2.Image.Width); 
			Assert.AreEqual (16, dashboard2.Image.Height);
			storage.Delete (dashboard);
			Assert.AreEqual (0, db.DocumentCount);
		}

		[Test ()]
		public void TestPlayer ()
		{
			Player player1 = new Player {Name = "andoni", Position = "runner",
				Number = 5, Birthday = new DateTime (1984, 6, 11),
				Nationality = "spanish", Height = 1.73f, Weight = 70,
				Playing = true, Mail = "test@test", Color = Color.Red
			};
			player1.Photo = Utils.LoadImageFromFile ();
			storage.Store (player1);
			Assert.AreEqual (1, db.DocumentCount);
			Assert.IsNotNull (db.GetExistingDocument (player1.ID.ToString ()));
			Player player2 = storage.Retrieve<Player> (player1.ID);
			Assert.AreEqual (player1.ID, player2.ID);
			Assert.AreEqual (player1.ToString (), player2.ToString ());
			Assert.AreEqual (player1.Photo.Width, player2.Photo.Width);
			Assert.IsNotNull (player2.DocumentID);
			storage.Delete (player1);
			Assert.AreEqual (0, db.DocumentCount);
		}

		[Test ()]
		public void TestTeam ()
		{
			Team team1 = Team.DefaultTemplate (10);
			storage.Store<Team> (team1);
			Assert.AreEqual (11, db.DocumentCount);
			Team team2 = storage.Retrieve<Team> (team1.ID);
			Assert.AreEqual (team1.ID, team2.ID);
			Assert.AreEqual (team1.List.Count, team2.List.Count);
			Assert.IsNotNull (team2.DocumentID);
			storage.Delete (team1);
			Assert.AreEqual (0, db.DocumentCount);
		}

		[Test ()]
		public void TestTimelineEvent ()
		{
			Player p = new Player ();
			AnalysisEventType evtType = new AnalysisEventType ();
			TimelineEvent evt = new TimelineEvent ();

			Document doc = db.GetDocument (evt.ID.ToString ());
			SerializationContext context = new SerializationContext (db, evt.GetType ());
			context.Cache.AddReference (p);
			evt.Players.Add (p);
			evt.EventType = evtType;

			doc.Update ((UnsavedRevision rev) => {
				JObject jo = DocumentsSerializer.SerializeObject (evt, rev, context);
				Assert.AreEqual (p.ID.ToString (), jo ["Players"] [0].Value<String> ());
				Assert.AreEqual (evtType.ID.ToString (), jo ["EventType"].Value<String> ());
				IDictionary<string, object> props = jo.ToObject<IDictionary<string, object>> ();
				rev.SetProperties (props);
				return true;
			});

			/* Player has not been added to the db, as it was already referenced
			 * by the IDReferenceResolver */
			Assert.AreEqual (2, db.DocumentCount);

			DocumentsSerializer.SaveObject (p, db);
			Assert.AreEqual (3, db.DocumentCount);

			TimelineEvent evt2 = storage.Retrieve <TimelineEvent> (evt.ID);
			Assert.IsNotNull (evt2.EventType);
			Assert.IsNotNull (evt2.DocumentID);

			storage.Delete (evt);
			Assert.AreEqual (2, db.DocumentCount);
			storage.Delete (p);
			Assert.AreEqual (1, db.DocumentCount);
			storage.Delete (evtType);
			Assert.AreEqual (0, db.DocumentCount);
		}

		[Test ()]
		public void TestProject ()
		{
			Project p = new Project ();
			p.Dashboard = Dashboard.DefaultTemplate (10);
			p.UpdateEventTypesAndTimers ();
			p.LocalTeamTemplate = Team.DefaultTemplate (10);
			p.VisitorTeamTemplate = Team.DefaultTemplate (12);
			MediaFile mf = new MediaFile ("path", 34000, 25, true, true, "mp4", "h264",
				               "aac", 320, 240, 1.3, null, "Test asset");
			ProjectDescription pd = new ProjectDescription ();
			pd.FileSet = new MediaFileSet ();
			pd.FileSet.Add (mf);
			p.Description = pd;
			p.AddEvent (new TimelineEvent ());

			storage.Store<Project> (p);
			Assert.AreEqual (43, db.DocumentCount);

			p = storage.RetrieveAll<Project> ().First ();
			Assert.IsNotNull (p.DocumentID);
			p.Load ();
			Assert.IsTrue (Object.ReferenceEquals (p.Description.FileSet, p.Timeline [0].FileSet));
			storage.Store (p);
			Assert.AreEqual (43, db.DocumentCount);

			storage.Delete (p);
			Assert.AreEqual (0, db.DocumentCount);
		}


		[Test ()]
		public void TestSaveProjectWithEvents ()
		{
			Project p = new Project ();
			p.Dashboard = Dashboard.DefaultTemplate (10);
			p.UpdateEventTypesAndTimers ();
			p.LocalTeamTemplate = Team.DefaultTemplate (10);
			p.VisitorTeamTemplate = Team.DefaultTemplate (12);
			MediaFile mf = new MediaFile ("path", 34000, 25, true, true, "mp4", "h264",
				               "aac", 320, 240, 1.3, null, "Test asset");
			ProjectDescription pd = new ProjectDescription ();
			pd.FileSet = new MediaFileSet ();
			pd.FileSet.Add (mf);
			p.Description = pd;

			for (int i = 0; i < 10; i++) {
				TimelineEvent evt = new TimelineEvent {
					EventType = p.EventTypes [i],
					Start = new Time (1000),
					Stop = new Time (2000),
					Players = new ObservableCollection<Player> { p.LocalTeamTemplate.List [0] }, 
				};
				p.Timeline.Add (evt);
			}

			storage.Store<Project> (p);
			Assert.AreEqual (52, db.DocumentCount);
			storage.Store<Project> (p);
			Assert.AreEqual (52, db.DocumentCount);

			Project p2 = storage.Retrieve<Project> (p.ID);
			Assert.AreEqual (p.Timeline.Count, p2.Timeline.Count);
			Assert.AreEqual (p2.LocalTeamTemplate.List [0], p2.Timeline [0].Players [0]);
			Assert.AreEqual ((p2.Dashboard.List [0] as AnalysisEventButton).EventType,
				p2.Timeline [0].EventType);
			Assert.IsNotNull (p2.DocumentID);

			storage.Delete (p);
			Assert.AreEqual (0, db.DocumentCount);
		}

		[Test ()]
		public void TestDeleteProjectItems ()
		{
			Project p = new Project ();
			p.Dashboard = Dashboard.DefaultTemplate (10);
			p.UpdateEventTypesAndTimers ();
			p.LocalTeamTemplate = Team.DefaultTemplate (10);

			for (int i = 0; i < 10; i++) {
				TimelineEvent evt = new TimelineEvent {
					EventType = p.EventTypes [i],
					Start = new Time (1000),
					Stop = new Time (2000),
				};
				p.Timeline.Add (evt);
			}

			storage.Store (p);
			p = storage.Retrieve<Project> (p.ID);

			// Removing this object should not remove the EvenType from the database, which might be referenced by
			// TimelineEvent's in the timeline
			EventType evtType = (p.Dashboard.List [0] as AnalysisEventButton).EventType;
			p.Dashboard.List.Remove (p.Dashboard.List [0]);
			storage.Store (p);
			Assert.DoesNotThrow (() => storage.Retrieve<Project> (p.ID));

			// Delete an event with a Player, a Team and an EventType, it should delete only the timeline event
			p.Timeline [0].Teams.Add (p.LocalTeamTemplate);
			p.Timeline [0].Players.Add (p.LocalTeamTemplate.List [0]);
			p.Timeline.Remove (p.Timeline [0]);
			Assert.DoesNotThrow (() => storage.Retrieve<Project> (p.ID));
		}

		[Test ()]
		public void TestPreloadPropertiesArePreserved ()
		{
			Project p1 = Utils.CreateProject (true);
			storage.Store (p1);
			Project p2 = storage.RetrieveAll<Project> ().First ();
			Assert.IsFalse (p2.IsLoaded);
			Assert.IsNotNull (p2.DocumentID);
			p2.Description.Competition = "NEW NAME";
			p2.Load ();
			Assert.AreEqual ("NEW NAME", p2.Description.Competition);
			storage.Store (p2);
			Project p3 = storage.RetrieveAll<Project> ().First ();
			Assert.IsNotNull (p3.DocumentID);
			Assert.AreEqual (p2.Description.Competition, p3.Description.Competition);
		}

		[Test ()]
		public void TestBackup ()
		{
			var res = storage.Backup ();
			Assert.IsTrue (res);
		}

	}
}
