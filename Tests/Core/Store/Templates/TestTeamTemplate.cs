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
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using NUnit.Framework;
using VAS.Core.Common;
using Constants = LongoMatch.Core.Common.Constants;

namespace Tests.Core.Store.Templates
{
	[TestFixture ()]
	public class TestTeamTemplate
	{
		[Test ()]
		public void TestSerialization ()
		{
			LMTeam t = new LMTeam ();

			Utils.CheckSerialization (t);

			t.Name = "test";
			t.TeamName = "team";
			t.Shield = Utils.LoadImageFromFile ();
			t.List.Add (new LMPlayer { Name = "P1" });
			t.List.Add (new LMPlayer { Name = "P2" });
			t.List.Add (new LMPlayer { Name = "P3" });


			Utils.CheckSerialization (t);

			LMTeam newt = Utils.SerializeDeserialize (t);

			Assert.AreEqual (t.ID, newt.ID);
			Assert.AreEqual (t.Name, newt.Name);
			Assert.AreEqual (t.TeamName, newt.TeamName);
			Assert.AreEqual (t.Shield.Width, 16);
			Assert.AreEqual (t.Shield.Height, 16);
			Assert.AreEqual (t.Players.Count, newt.Players.Count);
			Assert.AreEqual (t.Players [0].Name, newt.Players [0].Name);
			Assert.AreEqual (t.Players [1].Name, newt.Players [1].Name);
			Assert.AreEqual (t.Players [2].Name, newt.Players [2].Name);
		}

		[Test]
		public void TestVersion ()
		{
			Assert.AreEqual (Constants.DB_VERSION, new LMTeam ().Version);
			Assert.AreEqual (Constants.DB_VERSION, LMTeam.DefaultTemplate (1).Version);
		}

		[Test ()]
		public void TestColor ()
		{
			LMTeam t = new LMTeam ();
			Assert.AreEqual (t.Color, t.Colors [0]);
			t.ActiveColor = -1;
			Assert.AreEqual (t.Color, t.Colors [0]);
			t.ActiveColor = t.Colors.Length + 1;
			Assert.AreEqual (t.Color, t.Colors [0]);
			t.ActiveColor = 1;
			Assert.AreEqual (t.Color, t.Colors [1]);
		}

		[Test ()]
		public void TestPlayingPlayers ()
		{
			LMTeam t = new LMTeam ();
			LMPlayer p1, p2, p3;

			t.Name = "test";
			t.TeamName = "team";

			Assert.AreEqual (t.CalledPlayersList.Count, 0);

			p1 = new LMPlayer { Name = "P1", Playing = true };
			p2 = new LMPlayer { Name = "P2", Playing = false };
			p3 = new LMPlayer { Name = "P3", Playing = true };
			t.List.Add (p1);
			Assert.AreEqual (t.CalledPlayersList.Count, 1);
			t.List.Add (p2);
			Assert.AreEqual (t.CalledPlayersList.Count, 1);
			t.List.Add (p3);
			Assert.AreEqual (t.CalledPlayersList.Count, 2);
			Assert.AreEqual (t.CalledPlayersList [0], p1);
			Assert.AreEqual (t.CalledPlayersList [1], p3);
		}

		[Test ()]
		public void TestCreateDefaultTemplate ()
		{
			LMTeam t = LMTeam.DefaultTemplate (10);

			Assert.AreEqual (t.Players.Count, 10);
			t.AddDefaultItem (8);
			Assert.AreEqual (t.Players.Count, 11);
		}

		[Test ()]
		public void TestFormation ()
		{
			LMTeam t = LMTeam.DefaultTemplate (1);
			t.FormationStr = "1-2-3-4";
			Assert.AreEqual (t.Formation.Length, 4);
			Assert.AreEqual (t.Formation [0], 1);
			Assert.AreEqual (t.Formation [1], 2);
			Assert.AreEqual (t.Formation [2], 3);
			Assert.AreEqual (t.Formation [3], 4);
			Assert.Throws<FormatException> (
				delegate {
					t.FormationStr = "1;as";
				});
		}

		[Test ()]
		public void TestBenchPlayers ()
		{
			LMTeam t = LMTeam.DefaultTemplate (15);
			t.FormationStr = "1-2-3-4";
			Assert.AreEqual (5, t.BenchPlayersList.Count);
			Assert.AreEqual (t.Players [10], t.BenchPlayersList [0]);
			Assert.AreEqual (t.Players [14], t.BenchPlayersList [4]);
			t.FormationStr = "10-2-3-4";
			Assert.AreEqual (0, t.BenchPlayersList.Count);
		}

		[Test ()]
		public void TestStartingPlayers ()
		{
			LMTeam t = LMTeam.DefaultTemplate (15);
			t.FormationStr = "1-2-3-4";
			Assert.AreEqual (10, t.StartingPlayers);
			Assert.AreEqual (10, t.StartingPlayersList.Count);
			Assert.AreEqual (t.Players [0], t.StartingPlayersList [0]);
			Assert.AreEqual (t.Players [9], t.StartingPlayersList [9]);

			/* Players not playing are skipped */
			t.Players [0].Playing = false;
			Assert.AreEqual (t.Players [1], t.StartingPlayersList [0]);
			Assert.AreEqual (t.Players [10], t.StartingPlayersList [9]);

			/* Unless we are editing the team */
			t.TemplateEditorMode = true;
			Assert.AreEqual (t.Players [0], t.StartingPlayersList [0]);
			Assert.AreEqual (t.Players [9], t.StartingPlayersList [9]);
			t.TemplateEditorMode = false;

			/* If the list of playing players is smaller than the formation
			 * the list of starting players is of the same size as the list
			 * of playing players */
			for (int i = 0; i < 8; i++) {
				t.Players [i].Playing = false;
			}
			Assert.AreEqual (7, t.StartingPlayersList.Count);
			Assert.AreEqual (t.Players [8], t.StartingPlayersList [0]);
		}

		[Test ()]
		public void TestRemovePlayers ()
		{
			LMTeam t = LMTeam.DefaultTemplate (15);
			t.FormationStr = "1-2-3-4";

			/* Removing a player from the starting list must be swapped
			 * with the first player in the bench to keep the same lineup */
			t.RemovePlayers (new List<LMPlayer> { t.Players [0] }, false);
			Assert.AreEqual (15, t.Players.Count);
			Assert.AreEqual (11, t.Players [0].Number);
			Assert.AreEqual (2, t.Players [1].Number);
			Assert.AreEqual (1, t.Players [14].Number);
			t.RemovePlayers (new List<LMPlayer> { t.Players [0] }, true);
			Assert.AreEqual (14, t.Players.Count);
			Assert.AreEqual (12, t.Players [0].Number);

			t.RemovePlayers (new List<LMPlayer> { new LMPlayer () }, true);
			Assert.AreEqual (14, t.Players.Count);

			t.RemovePlayers (new List<LMPlayer> { new LMPlayer (), t.Players [12] }, true);
			Assert.AreEqual (13, t.Players.Count);
		}

		[Test ()]
		public void TestResetPlayers ()
		{
			LMTeam t = LMTeam.DefaultTemplate (10);
			for (int i = 0; i < 5; i++) {
				t.Players [0].Playing = false;
			}
			t.ResetPlayers ();
			Assert.IsEmpty (t.Players.Where (p => !p.Playing));
		}

		[Test ()]
		public void TestIsChanged ()
		{
			LMTeam t = LMTeam.DefaultTemplate (10);
			Assert.IsTrue (t.IsChanged);
			t.IsChanged = false;
			t.List.Remove (t.Players [0]);
			Assert.IsTrue (t.IsChanged);
			t.IsChanged = false;
			t.List.Add (new LMPlayer ());
			Assert.IsTrue (t.IsChanged);
			t.IsChanged = false;
			t.FormationStr = "1-2";
			Assert.IsTrue (t.IsChanged);
			t.IsChanged = false;
			t.Colors = null;
			Assert.IsTrue (t.IsChanged);
			t.IsChanged = false;
			t.Shield = new Image (10, 10);
			Assert.IsTrue (t.IsChanged);
			t.IsChanged = false;
			t.TeamName = "new";
			Assert.IsTrue (t.IsChanged);
			t.IsChanged = false;
			t.Name = "new";
			Assert.IsTrue (t.IsChanged);
			t.IsChanged = false;
		}

		[Test ()]
		public void TestCopy ()
		{
			LMTeam team = LMTeam.DefaultTemplate (10);
			LMTeam copy = (LMTeam)team.Copy ("newName");
			Assert.AreNotEqual (team.ID, copy.ID);
			for (int i = 0; i < team.List.Count; i++) {
				Assert.AreNotEqual (team.List [i].ID, copy.List [i].ID);
			}
			Assert.AreEqual ("newName", copy.Name);
			Assert.AreNotEqual (team.Name, copy.Name);
		}
	}
}
