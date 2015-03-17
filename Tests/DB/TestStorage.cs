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
using System.IO;
using Couchbase.Lite;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.DB;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.DB
{
	class StorableContainerTest: IStorable
	{
		public Guid ID { get; set; }

		public StorableImageTest Image { get; set; }
	}

	class StorableImageTest : IStorable
	{
		public Guid ID { get; set; }

		public Image Image1 { get; set; }

		public Image Image2 { get; set; }

		public List<Image> Images { get; set; }
	}

	[TestFixture ()]
	public class TestStorage
	{
		Database db;
		CouchbaseStorage storage;

		[TestFixtureSetUp]
		public void InitDB ()
		{
			string dbPath = Path.Combine (Path.GetTempPath (), "TestDB");
			if (Directory.Exists (dbPath)) {
				Directory.Delete (dbPath, true);
			}
			storage = new CouchbaseStorage (dbPath, "test-db");
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
			foreach (var d in db.CreateAllDocumentsQuery ().Run()) {
				db.GetDocument (d.DocumentId).Delete ();
			}
		}

		[Test ()]
		public void TestDocType ()
		{
			StorableImageTest t = new StorableImageTest {
				ID = Guid.NewGuid (),
			};
			Document doc = db.CreateDocument ();
			JObject jo = DocumentsSerializer.SerializeObject (t, doc.CreateRevision (), db, null);
			Assert.AreEqual (t.ID, jo.Value<Guid> ("ID"));
			Assert.AreEqual ("StorableImageTest", jo.Value<string> ("DocType"));
		}

		[Test ()]
		public void TestSerializeImages ()
		{
			Image img = Utils.LoadImageFromFile ();
			StorableImageTest t = new StorableImageTest {
				Image1 = img,
				Image2 = img,
				ID = Guid.NewGuid (),
			};
			Document doc = db.CreateDocument ();
			UnsavedRevision rev = doc.CreateRevision ();
			JObject jo = DocumentsSerializer.SerializeObject (t, rev, db, null);
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
		public void TestSerializeImagesList ()
		{
			Image img = Utils.LoadImageFromFile ();
			StorableImageTest t = new StorableImageTest {
				Images = new List<Image> { img, img, img },
				ID = Guid.NewGuid (),
			};
			Document doc = db.CreateDocument ();
			UnsavedRevision rev = doc.CreateRevision ();
			JObject jo = DocumentsSerializer.SerializeObject (t, rev, db, null);
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
		public void TestDeserializeImages ()
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
		public void TestSerializeStorableByReference ()
		{
			StorableImageTest img = new StorableImageTest {
				ID = Guid.NewGuid (),
				Image1 = Utils.LoadImageFromFile (),
			};
			StorableContainerTest cont = new StorableContainerTest {
				ID = Guid.NewGuid (),
				Image = img,
			};
			Document doc = db.CreateDocument ();
			UnsavedRevision rev = doc.CreateRevision ();
			JObject jo = DocumentsSerializer.SerializeObject (cont, rev, db, null);
			Assert.AreEqual (img.ID, jo ["Image"].Value<Guid> ());
			Assert.AreEqual (1, db.DocumentCount);
			Assert.IsNotNull (storage.Retrieve<StorableImageTest> (img.ID));
			rev.Save ();
			Assert.AreEqual (2, db.DocumentCount);
		}

		[Test ()]
		public void TestDeserializeStorableByReference ()
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
			var cont2 = storage.Retrieve <StorableContainerTest> (cont.ID);
			Assert.IsNotNull (cont2.Image);
			Assert.AreEqual (img.ID, cont2.Image.ID);
		}

		[Test ()]
		public void TestSaveLoadDashboard ()
		{
			Dashboard dashboard = Dashboard.DefaultTemplate (10);
			dashboard.Image = dashboard.FieldBackground = dashboard.HalfFieldBackground =
				dashboard.GoalBackground = Utils.LoadImageFromFile ();
			storage.Store (dashboard);
			Assert.AreEqual (1, db.DocumentCount);
			Assert.IsNotNull (db.GetExistingDocument (dashboard.ID.ToString ()));
			Dashboard dashboard2 = storage.Retrieve<Dashboard> (dashboard.ID);
			Assert.IsNotNull (dashboard2);
			Assert.AreEqual (dashboard.ID, dashboard2.ID);
			Assert.AreEqual (dashboard.List.Count, dashboard2.List.Count);
			Assert.IsNotNull (dashboard2.Image);
			Assert.IsNotNull (dashboard2.FieldBackground);
			Assert.IsNotNull (dashboard2.HalfFieldBackground);
			Assert.IsNotNull (dashboard2.GoalBackground);
			Assert.AreEqual (16, dashboard2.Image.Width); 
			Assert.AreEqual (16, dashboard2.Image.Height); 
		}

		[Test ()]
		public void TestSaveLoadPlayer ()
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
		}

		[Test ()]
		public void TestSaveLoadTeam ()
		{
			Team team1 = Team.DefaultTemplate (10);
			storage.Store<Team> (team1);
			Assert.AreEqual (11, db.DocumentCount);
			Team team2 = storage.Retrieve<Team> (team1.ID);
			Assert.AreEqual (team1.ID, team2.ID);
			Assert.AreEqual (team1.List.Count, team2.List.Count);
		}

		[Test ()]
		public void TestSaveLoadProjectDescription ()
		{
			MediaFile mf = new MediaFile ("path", 34000, 25, true, true, "mp4", "h264",
				               "aac", 320, 240, 1.3, null, "Test asset");
			ProjectDescription pd1 = new ProjectDescription ();
			pd1.FileSet = new MediaFileSet ();
			pd1.FileSet.Add (mf);
			storage.Store (pd1);
			Assert.AreEqual (1, db.DocumentCount);

			ProjectDescription pd2 = storage.Retrieve<ProjectDescription> (pd1.ID);
			Assert.AreEqual (pd1.ID, pd2.ID);
		}

		[Test ()]
		public void TestSaveLoadProject ()
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

			storage.Store<Project> (p);
			/* 1 Project + 1 ProjectDescription 
			 * Teams and Dashboard are serialized locally in the project
			 */ 
			Assert.AreEqual (1 + 1, db.DocumentCount);
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
				p.AddEvent (p.EventTypes [i], new Time (1000), new Time (2000), null, null, null, null);
			}

			storage.Store<Project> (p);
			Assert.AreEqual (1 + 1 + 10, db.DocumentCount);
			storage.Store<Project> (p);
			Assert.AreEqual (1 + 1 + 10, db.DocumentCount);

			Project p2 = storage.Retrieve<Project> (p.ID);
			Assert.AreEqual (p.Timeline.Count, p2.Timeline.Count);
		}
	}
}

