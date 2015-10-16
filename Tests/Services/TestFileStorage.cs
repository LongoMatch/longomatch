//
//  Copyright (C) 2015 jl
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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using LongoMatch.DB;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using System.Linq;

namespace Tests.Services
{
	[TestFixture ()]
	public class TestFileStorage
	{

		FileStorage fs;

		private class TestStorable : StorableBase
		{
			public string memberString;

			public TestStorable (string memberString)
			{
				this.memberString = memberString;
				ID = Guid.NewGuid ();
			}
		}

		[SetUp]
		public void CreateStorage ()
		{
			fs = new FileStorage (Path.Combine (Path.GetTempPath (), "TestFileStorage"));
		}

		[TearDown]
		public void RemoveStorage ()
		{
			try {
				Directory.Delete (Path.Combine (Path.GetTempPath (), "TestFileStorage"), true);
			} catch {
			}
		}

		[Test ()]
		public void TestCase ()
		{
			TestStorable ts1 = new TestStorable ("first");

			fs.Store<TestStorable> (ts1);
			List<TestStorable> lts = fs.RetrieveAll<TestStorable> ().ToList ();

			// Check that we have stored one object
			Assert.AreEqual (lts.Count, 1);
			TestStorable ts2 = lts [0];
			Assert.AreNotSame (ts2, null);

			// Check that the object is the same
			Assert.AreEqual (ts2.memberString, ts1.memberString);

			// Get based on memberString
			QueryFilter filter = new QueryFilter ();
			filter.Add ("memberString", "first");
			lts = fs.Retrieve<TestStorable> (filter).ToList ();

			// Check that we have stored one object
			Assert.AreEqual (lts.Count, 1);

			// Check that the returned object is the one we are looking for
			ts2 = lts [0];
			Assert.AreNotSame (ts2, null);

			// Check that the storage is empty
			fs.Delete<TestStorable> (ts2);
			lts = fs.RetrieveAll<TestStorable> ().ToList ();
			Assert.AreEqual (lts.Count, 0);
		}

		[Test ()]
		public void TestRetrieveByID ()
		{
			TestStorable ts1 = new TestStorable ("first");
			fs.Store<TestStorable> (ts1);

			TestStorable ts2 = fs.Retrieve<TestStorable> (ts1.ID);
			Assert.IsNotNull (ts2);
			Assert.AreEqual (ts1.ID, ts2.ID);

			Assert.IsNull (fs.Retrieve<TestStorable> (Guid.NewGuid ()));
		}

		[Test ()]
		public void TestRetrieveFiltered ()
		{
			TestStorable ts1 = new TestStorable ("first");
			fs.Store<TestStorable> (ts1);

			/* Test with a dictionary combination that exists */
			QueryFilter filter = new QueryFilter ();
			filter.Add ("memberString", "first");
			Assert.AreEqual (1, fs.Retrieve<TestStorable> (filter).Count ());

			/* Test with a dictionary combination that doesn't exist */
			filter ["memberString"] [0] = "second";
			Assert.AreEqual (0, fs.Retrieve<TestStorable> (filter).Count ());
		}
	}
}
