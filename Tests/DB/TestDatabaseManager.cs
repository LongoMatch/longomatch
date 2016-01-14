//
//  Copyright (C) 2016 Fluendo S.A.
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
using LongoMatch.Core.Interfaces;
using LongoMatch.DB;
using NUnit.Framework;

namespace Tests.DB
{
	[TestFixture ()]
	public class TestDatabaseManager
	{
		IStorageManager storageManager;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
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

			storageManager = new CouchbaseManager (dbPath);
		}

		[TearDown]
		public void TearDown ()
		{
			foreach (var db in storageManager.Databases) {
				db.Reset ();
			}
			storageManager.Databases = new List<IStorage> ();
		}

		[Test ()]
		public void TestSanitizeDBNameOK ()
		{
			string name = "ok-name_$()+-/";
			var storage = storageManager.Add (name);
			Assert.AreEqual (name, storage.Info.Name);
		}

		[Test ()]
		public void TestSanitizeDBNameAccent ()
		{
			string name = "námê";
			var storage = storageManager.Add (name);
			Assert.AreEqual ("na_me_", storage.Info.Name);
		}

		[Test ()]
		public void TestSanitizeDBNameSymbols ()
		{
			string name = "na?me*";
			var storage = storageManager.Add (name);
			Assert.AreEqual ("na_me_", storage.Info.Name);
		}

		[Test ()]
		public void TestSanitizeDBNameStartSymbol ()
		{
			string name = "?name*";
			var storage = storageManager.Add (name);
			Assert.AreEqual ("db_name_", storage.Info.Name);
		}
	}
}
