//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using NUnit.Framework;

using LongoMatch.DB;
using LongoMatch.Core.Store;

namespace Tests.Services
{
	[TestFixture()]
	public class TestDatabase
	{
		string tmpdir;
		
		[SetUp] public void CreateDBDir()
		{
			do {
				tmpdir = Path.GetTempPath() + Guid.NewGuid().ToString();
			} while (Directory.Exists(tmpdir));
		}

		[TearDown] public void DeleteDBDir()
		{
			try {
				Directory.Delete (tmpdir);
			} catch (Exception) {
			}
		}
		
		[Test()]
		public void TestCreateEmptyDatabase ()
		{
			string dbdir = Path.Combine (tmpdir, "test.ldb");
			DataBase db = new DataBase (dbdir);
			Assert.IsTrue (Directory.Exists (dbdir));
			Assert.IsTrue (File.Exists (Path.Combine (dbdir, "test.ldb")));
			Assert.AreEqual (db.Count, 0);
		}
		
		[Test()]
		public void TestLoadExistingDatabase ()
		{
			string dbdir = Path.Combine (tmpdir, "test.ldb");
			DataBase db = new DataBase (dbdir);
			DataBase newdb = new DataBase (dbdir);
			Assert.AreEqual (db.LastBackup, newdb.LastBackup);
		}
		
		[Test()]
		public void TestReloadDB ()
		{
			string dbdir = Path.Combine (tmpdir, "test.ldb");
			DataBase db = new DataBase (dbdir);
			db.AddProject (new ProjectLongoMatch {Description = new ProjectDescriptionLongoMatch()});
			File.Delete (Path.Combine (dbdir, "test.ldb"));
			db = new DataBase (dbdir);
			Assert.IsTrue (File.Exists (Path.Combine (dbdir, "test.ldb")));
			Assert.AreEqual (db.Count, 1);
		}
		
		[Test()]
		public void TestDBWithErrorProject ()
		{
			string dbdir = Path.Combine (tmpdir, "test.ldb");
			DataBase db = new DataBase (dbdir);
			db.AddProject (new ProjectLongoMatch {Description = new ProjectDescriptionLongoMatch()});
			var writer = File.CreateText (Path.Combine (dbdir, "wrongfile"));
			writer.WriteLine("TEST&%&$&%");
			writer.WriteLine("}~4");
			writer.Flush();
			writer.Close();
			File.Delete (Path.Combine (dbdir, "test.ldb"));
			db = new DataBase (dbdir);
			Assert.AreEqual (db.Count, 1);
		}
		
		[Test()]
		public void TestGetAllProjects()
		{
			string dbdir = Path.Combine (tmpdir, "test.ldb");
			DataBase db = new DataBase (dbdir);
			ProjectDescriptionLongoMatch pd1 = new ProjectDescriptionLongoMatch ();
			ProjectDescriptionLongoMatch pd2 = new ProjectDescriptionLongoMatch ();
			ProjectLongoMatch p1 = new ProjectLongoMatch {Description = pd1};
			ProjectLongoMatch p2 = new ProjectLongoMatch {Description = pd2};
			db.AddProject (p1);
			db.AddProject (p2);
			Assert.AreEqual (db.Count, 2);
			List<ProjectDescriptionLongoMatch> projects = db.GetAllProjects ();
			Assert.AreEqual (db.Count, 2);
			Assert.AreEqual (projects.Count, 2);
			Assert.AreEqual (projects[0], pd1);
			Assert.AreEqual (projects[1], pd2);
		}

		[Test()]
		public void TestGetProject()
		{
			string dbdir = Path.Combine (tmpdir, "test.ldb");
			DataBase db = new DataBase (dbdir);
			ProjectDescriptionLongoMatch pd1 = new ProjectDescriptionLongoMatch ();
			ProjectLongoMatch p1 = new ProjectLongoMatch {Description = pd1};
			db.AddProject (p1);
			ProjectLongoMatch p2 = db.GetProject (p1.ID);
			Assert.AreEqual (p1.ID, p2.ID);
			Assert.IsNull (db.GetProject (new Guid()));
		}
		
		[Test()]
		public void TestAddProject()
		{
			string dbdir = Path.Combine (tmpdir, "test.ldb");
			DataBase db = new DataBase (dbdir);
			ProjectDescriptionLongoMatch pd1 = new ProjectDescriptionLongoMatch ();
			ProjectLongoMatch p1 = new ProjectLongoMatch {Description = pd1};
			Assert.IsTrue (db.AddProject (p1));
			Assert.IsTrue (File.Exists (Path.Combine (dbdir, p1.ID.ToString())));
			Assert.IsTrue (db.AddProject (p1));
			Assert.AreEqual (db.Count, 1);
			db = new DataBase (dbdir);
			Assert.AreEqual (db.Count, 1);
		}
		
		[Test()]
		public void TestRemoveProject()
		{
			string dbdir = Path.Combine (tmpdir, "test.ldb");
			DataBase db = new DataBase (dbdir);
			ProjectDescriptionLongoMatch pd1 = new ProjectDescriptionLongoMatch ();
			ProjectLongoMatch p1 = new ProjectLongoMatch {Description = pd1};
			Assert.IsTrue (db.AddProject (p1));
			Assert.IsTrue (File.Exists (Path.Combine (dbdir, p1.ID.ToString())));
			Assert.AreEqual (db.Count, 1);
			Assert.IsTrue (db.RemoveProject (p1.ID));
			Assert.IsFalse (File.Exists (Path.Combine (dbdir, p1.ID.ToString())));
			Assert.AreEqual (db.Count, 0);
			Assert.IsFalse (db.RemoveProject (p1.ID));
			db = new DataBase (dbdir);
			Assert.AreEqual (db.Count, 0);
		}
		
		[Test()]
		public void TestUpdateProject()
		{
			string dbdir = Path.Combine (tmpdir, "test.ldb");
			DataBase db = new DataBase (dbdir);
			ProjectDescriptionLongoMatch pd1 = new ProjectDescriptionLongoMatch ();
			ProjectLongoMatch p1 = new ProjectLongoMatch {Description = pd1};
			DateTime lastModified = p1.Description.LastModified;
			Assert.IsTrue (db.AddProject (p1));
			Assert.IsTrue (db.UpdateProject (p1));
			Assert.AreNotEqual (p1.Description.LastModified, lastModified);
		}
		
		[Test()]
		public void TestExists()
		{
			string dbdir = Path.Combine (tmpdir, "test.ldb");
			DataBase db = new DataBase (dbdir);
			ProjectDescriptionLongoMatch pd1 = new ProjectDescriptionLongoMatch ();
			ProjectLongoMatch p1 = new ProjectLongoMatch {Description = pd1};
			Assert.IsFalse (db.Exists (p1));
			db.AddProject (p1);
			Assert.IsTrue (db.Exists (p1));
		}
		
		[Test()]
		public void TestBackup ()
		{
		}
		
		[Test()]
		public void TestDelete ()
		{
			string dbdir = Path.Combine (tmpdir, "test.ldb");
			DataBase db = new DataBase (dbdir);
			Assert.IsTrue (Directory.Exists (dbdir));
			db.Delete ();
			Assert.IsFalse (Directory.Exists (dbdir));
			db.Delete ();
		}
	}
}

