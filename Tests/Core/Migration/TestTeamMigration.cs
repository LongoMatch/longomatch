//
//  Copyright (C) 2015 Fluendo S.A.
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
using System.Reflection;
using LongoMatch.Core.Common;
using LongoMatch.Core.Migration;
using LongoMatch.Core.Store.Templates;
using NUnit.Framework;

namespace Tests.Core.Migration
{
	#pragma warning disable 0618

	[TestFixture ()]
	public class TestTeamMigration
	{
		[Test ()]
		public void TestMigrateFromV0 ()
		{
			Team team;
			Team origTeam;

			using (Stream resource = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("spain.ltt")) {
				origTeam = Serializer.Instance.Load <Team> (resource);
			}

			team = origTeam.Clone ();
			team.ID = Guid.Empty;
			TeamMigration.Migrate (team);
			Assert.AreNotEqual (Guid.Empty, team.ID);
			Assert.AreEqual (1, team.Version);

			team = origTeam.Clone ();
			team.ID = Guid.Empty;
			var teamNameToID = new Dictionary<string , Guid> ();
			Guid id = Guid.NewGuid ();
			teamNameToID [team.TeamName] = id;
			TeamMigration.Migrate0 (team, teamNameToID);
			Assert.AreEqual (id, team.ID);
		}
	}
}

