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
	[TestFixture()]
	public class TestHotKey
	{
		[Test()]
		public void TestSerialization ()
		{
			HotKey hk = new HotKey ();
			hk.Key = 2;
			hk.Modifier = 3;
			
			Utils.CheckSerialization (hk);
			
			HotKey hk2 = Utils.SerializeDeserialize (hk);
			Assert.AreEqual (hk.Key, hk2.Key);
			Assert.AreEqual (hk.Modifier, hk2.Modifier);
		}
		
		[Test()]
		public void TestDefined ()
		{
			HotKey hk = new HotKey ();
			Assert.IsFalse (hk.Defined);
			hk.Key = 2;
			Assert.IsTrue (hk.Defined);
			hk.Key = -1;
			hk.Modifier = 1;
			Assert.IsFalse (hk.Defined);
		}
		
		[Test()]
		public void TestEquality ()
		{
			HotKey k1 = new HotKey {Key = 1, Modifier = 2};
			HotKey k2 = new HotKey {Key = 1, Modifier = 3};
			Assert.AreNotEqual (k1, k2);
			Assert.IsTrue (k1 != k2);
			Assert.IsFalse (k1 == k2);
			k2.Modifier = 2;
			Assert.AreEqual (k1, k2);
			Assert.IsFalse (k1 != k2);
			Assert.IsTrue (k1 == k2);
		}
	}
}

