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
using NUnit.Framework;
using System;
using LongoMatch.Core.Store;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestTag
	{
		[Test ()]
		public void TestSerialization ()
		{
			Tag tag = new Tag ("name", "grp");
			tag.HotKey = new HotKey { Modifier = 2, Key = 1 };
			Utils.CheckSerialization (tag);

			Tag tag2 = Utils.SerializeDeserialize (tag);
			Assert.AreEqual (tag.Value, tag2.Value);
			Assert.AreEqual (tag.Group, tag2.Group);
			Assert.AreEqual (tag.HotKey, tag2.HotKey);
		}

		[Test ()]
		public void TestEquals ()
		{
			Tag tag1 = new Tag ("name", "grp");
			Tag tag2 = new Tag ("name", "grp");
			Tag tag3 = new Tag ("name1", "grp");
			Tag tag4 = new Tag ("name", "grp1");

			Assert.IsFalse (tag1.Equals (null));
			Assert.IsFalse (tag1.Equals ("string"));
			Assert.IsFalse (tag1.Equals (tag3));
			Assert.IsFalse (tag1.Equals (tag4));
			Assert.IsTrue (tag1.Equals (tag2));
		}
	}
}

