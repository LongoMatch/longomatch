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
using LongoMatch.Services.Services;
using LongoMatch.Core.Interfaces;

namespace Tests.Services
{
	[TestFixture()]
	public class TestFileStorage
	{
		private class TestStorable : IStorable
		{
			public string memberString;

			public TestStorable(string memberString)
			{
				this.memberString = memberString;
				ID = Guid.NewGuid ();
			}

			public Guid ID {
				get;
				set;
			}
		}

		[Test()]
		public void TestCase ()
		{
			FileStorage fs = new FileStorage("/tmp/TestFileStorage/", true);
			TestStorable ts1 = new TestStorable("first");

			fs.Store<TestStorable>(ts1);
			List<TestStorable> lts = fs.RetrieveAll<TestStorable>();

			// Check that we have stored one object
			Assert.AreEqual(lts.Count, 1);
			TestStorable ts2 = lts[0];
			Assert.AreNotSame(ts2, null);

			// Check that the object is the same
			Assert.AreEqual(ts2.memberString, ts1.memberString);

			// Check that the storage is empty
			fs.Delete<TestStorable>(ts2);
			lts = fs.RetrieveAll<TestStorable>();
			Assert.AreEqual(lts.Count, 0);
		}
	}
}

