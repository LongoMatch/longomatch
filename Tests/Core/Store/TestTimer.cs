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
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using NUnit.Framework;
using VAS.Core.Store;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestTimer
	{
		[Test ()]
		public void TestSerialization ()
		{
			LMTimer timer = new LMTimer ();
			Utils.CheckSerialization (timer);

			timer.Name = "test";
			timer.Team = TeamType.LOCAL;
			LMTimer timer2 = Utils.SerializeDeserialize (timer);
			Assert.AreEqual (timer.Name, timer2.Name);
			Assert.AreEqual (timer.Nodes, timer2.Nodes);
			Assert.AreEqual (timer.Team, timer2.Team);
		}
	}
}

