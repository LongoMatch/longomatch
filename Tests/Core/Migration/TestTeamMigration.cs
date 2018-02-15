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
using LongoMatch;
using LongoMatch.Core.Migration;
using LongoMatch.Core.Store.Templates;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;

namespace Tests.Core.Migration
{
	#pragma warning disable 0618

	[TestFixture ()]
	public class TestTeamMigration
	{
		Mock<IPreviewService> mockPreview;

		[OneTimeSetUp]
		public void FixtureSetUp () {
			mockPreview = new Mock<IPreviewService> ();
			App.Current.PreviewService = mockPreview.Object;
		}

		[Test]
		public void Migrate_FromV0ToV2_Ok ()
		{
			// Arrange
			LMTeam team;
			LMTeam origTeam;

			using (Stream resource = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("spain.ltt")) {
				origTeam = Serializer.Instance.Load <LMTeam> (resource);
			}

			team = origTeam.Clone ();
			team.ID = Guid.Empty;

			mockPreview.Setup (p => p.CreatePreview (team)).Returns (new Image (1, 1));

			// Act
			Assert.AreEqual (0, team.Version);
			TeamMigration.Migrate (team);

			// Assert
			Assert.AreNotEqual (Guid.Empty, team.ID);
			Assert.AreEqual (2, team.Version);
			Assert.IsNotNull (team.Preview);

			team = origTeam.Clone ();
			team.ID = Guid.Empty;
			var teamNameToID = new Dictionary<string , Guid> ();
			Guid id = Guid.NewGuid ();
			teamNameToID [team.TeamName] = id;
			TeamMigration.Migrate0 (team, teamNameToID);

			Assert.AreEqual (id, team.ID);
		}

		[Test]
		public void MigrateV1_AlreadyMigrated_DoNothing ()
		{
			// Arrange
			LMTeam team;
			LMTeam origTeam;

			using (Stream resource = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("spain.ltt")) {
				origTeam = Serializer.Instance.Load<LMTeam> (resource);
			}

			team = origTeam.Clone ();
			TeamMigration.Migrate (team);

			mockPreview.Setup (p => p.CreatePreview (team)).Returns (new Image (1, 1));

			// Act
			Image preview = team.Preview;
			TeamMigration.Migrate1 (team);

			// Assert
			Assert.AreSame (preview, team.Preview);
			Assert.AreEqual (2, team.Version);
		}

		[Test]
		public void NewTeam_NothingToMigrate_DoNothing ()
		{
			// Arrange
			LMTeam team = new LMTeam ();
			mockPreview.Setup (p => p.CreatePreview (team)).Returns (new Image (1, 1));

			// Act
			TeamMigration.Migrate (team);

			// Assert
			Assert.IsNull (team.Preview);
			Assert.AreEqual (2, team.Version);
		}
	}
}

