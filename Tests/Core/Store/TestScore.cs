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
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using NUnit.Framework;
using VAS.Core.Common;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestScore
	{
		[Test ()]
		public void TestSerialization ()
		{
			Score s = new Score ();
			s.Color = Color.Red;
			s.Name = "test";
			s.Points = 2;
			
			Utils.CheckSerialization (s);
			
			Score s2 = Utils.SerializeDeserialize (s);
			Assert.AreEqual (s.Color, s2.Color);
			Assert.AreEqual (s.Name, s2.Name);
			Assert.AreEqual (s.Points, s2.Points);
		}
	}
}

