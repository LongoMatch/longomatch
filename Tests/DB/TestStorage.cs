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

	class StorableImageTest : IStorable 
	{
		public Guid ID { get; set; }
		public Image Image1 { get; set; }
		public Image Image2 { get; set; }
		public List<Image> Images { get; set;}
	}

	[TestFixture ()]
	public class TestStorage
	{
		Database db;
		CouchbaseStorage storage;

		[TestFixtureSetUp]
		public void InitDB () {
			string dbPath = Path.Combine (Path.GetTempPath (), "TestDB");
			storage = new CouchbaseStorage (dbPath, "test-db");
			db = storage.Database;
		}

		[TestFixtureTearDown]
		public void DeleteDB () {
			Directory.Delete (db.Manager.Directory, true);
		}

		[TearDown]
		public void CleanDB () {
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
			JObject jo = DocumentsSerializer.SerializeObject (t, doc.CreateRevision (), null);
			Assert.AreEqual (t.ID, jo.Value<Guid> ("ID"));
			Assert.AreEqual ("StorableImageTest", jo.Value<string> ("DocType"));
		}

		[Test()]
		public void TestSerializeImages () {
			Image img = Utils.LoadImageFromFile ();
			StorableImageTest t = new StorableImageTest {
				Image1 = img,
				Image2 = img,
				ID = Guid.NewGuid(),
			};
			Document doc = db.CreateDocument ();
			UnsavedRevision rev = doc.CreateRevision ();
			JObject jo = DocumentsSerializer.SerializeObject (t, rev, null);
			Assert.IsNotNull (jo ["ID"]);
			Assert.AreEqual ("attachment::Image1_1", jo ["Image1"].Value<string> ());
			Assert.AreEqual ("attachment::Image2_1", jo ["Image2"].Value<string> ());
			int i = 0;
			foreach (string name in rev.AttachmentNames) {
				i++;
				Assert.AreEqual (string.Format ("Image{0}_1", i), name);
			}
		}

		[Test()]
		public void TestSerializeImagesList () {
			Image img = Utils.LoadImageFromFile ();
			StorableImageTest t = new StorableImageTest {
				Images = new List<Image> {img, img, img},
				ID = Guid.NewGuid(),
			};
			Document doc = db.CreateDocument ();
			UnsavedRevision rev = doc.CreateRevision ();
			JObject jo = DocumentsSerializer.SerializeObject (t, rev, null);
			int i = 0;
			foreach (string name in rev.AttachmentNames) {
				i++;
				Assert.AreEqual ("Images_" + i, name);
			}
			Assert.AreEqual (3, i);
			Assert.AreEqual ("attachment::Images_1", jo ["Images"][0].Value<string>());
			Assert.AreEqual ("attachment::Images_2", jo ["Images"][1].Value<string>());
			Assert.AreEqual ("attachment::Images_3", jo ["Images"][2].Value<string>());
		}

		[Test()]
		public void TestDeserializeImages () {
			Image img = Utils.LoadImageFromFile ();
			StorableImageTest t = new StorableImageTest {
				Image1 = img,
				Image2 = img,
				ID = Guid.NewGuid(),
			};
			storage.Store (t);
			var test2 = storage.Retrieve<StorableImageTest> (t.ID);
			Assert.AreEqual (t.Image1.Width, test2.Image1.Width);
			Assert.AreEqual (t.Image1.Height, test2.Image1.Height);
			Assert.AreEqual (t.Image2.Width, test2.Image2.Width);
			Assert.AreEqual (t.Image2.Height, test2.Image2.Height);
		}

		[Test ()]
		public void TestSaveDashboard ()
		{
			Dashboard dashboard = Dashboard.DefaultTemplate (10);
			storage.Store (dashboard);
			Assert.AreEqual (1, db.DocumentCount);
			Assert.IsNotNull (db.GetExistingDocument (dashboard.ID.ToString()));
		}

		[Test ()]
		public void TestLoadDashboard ()
		{
			Dashboard dashboard = Dashboard.DefaultTemplate (10);
			dashboard.Image = dashboard.GoalBackground =
				dashboard.HalfFieldBackground = dashboard.FieldBackground = Utils.LoadImageFromFile();
			storage.Store (dashboard);
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
		public void TestSavePlayer ()
		{
			Player player = new Player {Name = "andoni", Position = "runner",
				Number = 5, Birthday = new DateTime (1984, 6, 11),
				Nationality = "spanish", Height = 1.73f, Weight = 70,
				Playing = true, Mail = "test@test", Color = Color.Red
			};
			storage.Store (player);
			Assert.AreEqual (1, db.DocumentCount);
			Assert.IsNotNull (db.GetExistingDocument (player.ID.ToString()));
		}

		[Test ()]
		public void TestLoadPlayer ()
		{
		}
	}
}

