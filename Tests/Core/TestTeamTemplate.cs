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
using NUnit.Framework;
using LongoMatch.Common;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace Tests.Core
{
	[TestFixture()]
	public class TestTeamTemplate
	{
		[Test()]
		public void TestSerialization ()
		{
			TeamTemplate t = new TeamTemplate();
			
			Utils.CheckSerialization(t);
			
			t.Name = "test";
			t.Version = new Version (1, 2);
			t.TeamName = "team";
			t.Add (new Player {Name="P1"});
			t.Add (new Player {Name="P2"});
			t.Add (new Player {Name="P3"});
			
			
			Utils.CheckSerialization (t);
			
			TeamTemplate newt = Utils.SerializeDeserialize(t);
			
			Assert.AreEqual (t.Name, newt.Name);
			Assert.AreEqual (t.Version, newt.Version);
			Assert.AreEqual (t.TeamName, newt.TeamName);
			Assert.AreEqual (t.Count, newt.Count);
			Assert.AreEqual (t[0].Name, newt[0].Name);
			Assert.AreEqual (t[1].Name, newt[1].Name);
			Assert.AreEqual (t[2].Name, newt[2].Name);
		}
		
		
		[Test()]
		public void TestPlayingPlayers ()
		{
			TeamTemplate t = new TeamTemplate();
			Player p1, p2, p3;
			
			t.Name = "test";
			t.Version = new Version (1, 2);
			t.TeamName = "team";
			
			Assert.AreEqual (t.PlayingPlayersList.Count, 0);
			
			p1 = new Player {Name="P1", Playing = true};
			p2 = new Player {Name="P2", Playing = false};
			p3 = new Player {Name="P3", Playing = true};
			t.Add (p1);
			Assert.AreEqual (t.PlayingPlayersList.Count, 1);
			t.Add (p2);
			Assert.AreEqual (t.PlayingPlayersList.Count, 1);
			t.Add (p3);
			Assert.AreEqual (t.PlayingPlayersList.Count, 2);
			Assert.AreEqual (t.PlayingPlayersList[0], p1);
			Assert.AreEqual (t.PlayingPlayersList[1], p3);
		}
		
		[Test()]
		public void TestCreateDefaultTemplate ()
		{
			TeamTemplate t = TeamTemplate.DefaultTemplate (10);
			
			Assert.AreEqual (t.Count, 10);
			t.AddDefaultItem (8);
			Assert.AreEqual (t.Count, 11);
		}
		
	}
}
