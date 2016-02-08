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
using System.Linq;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;
using LongoMatch.Core.Migration;
using System.Collections.ObjectModel;
using LongoMatch.Core.Store.Templates;

namespace Tests.Core.Migration
{
	#pragma warning disable 0618
	[TestFixture ()]
	public class TestProjectMigration
	{
		[Test ()]
		public void TestMigrateFromV0 ()
		{
			Project project;

			using (Stream resource = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("spain_france_test.lgm")) {
				project = Serializer.Instance.Load <Project> (resource);
			}

			Assert.AreEqual (0, project.Version);
			Assert.AreEqual (0, project.Dashboard.Version);
			Assert.AreEqual (0, project.LocalTeamTemplate.Version);
			Assert.AreEqual (0, project.VisitorTeamTemplate.Version);

			ProjectMigration.Migrate (project);

			// Check that dashboard and teams are migrated
			Assert.AreEqual (1, project.Version);
			Assert.AreEqual (1, project.Dashboard.Version);
			Assert.AreEqual (1, project.LocalTeamTemplate.Version);
			Assert.AreEqual (1, project.VisitorTeamTemplate.Version);

			Assert.AreEqual (3, project.Timeline.Count (e => e.Teams.Contains (project.LocalTeamTemplate)));
			Assert.AreEqual (2, project.Timeline.Count (e => e.Teams.Contains (project.VisitorTeamTemplate)));
			// Check that team tags have changed from TeamType to List<Team> correctly
			foreach (TimelineEvent evt in project.Timeline) {
				if (evt.Team == TeamType.LOCAL) {
					Assert.AreEqual (evt.Teams, new ObservableCollection<Team> { project.LocalTeamTemplate });
				} else if (evt.Team == TeamType.VISITOR) {
					Assert.AreEqual (evt.Teams, new ObservableCollection<Team> { project.VisitorTeamTemplate });
				} else if (evt.Team == TeamType.BOTH) {
					Assert.AreEqual (evt.Teams,
						new ObservableCollection<Team> { project.LocalTeamTemplate, project.VisitorTeamTemplate });
				} else if (evt.Team == TeamType.NONE) {
					Assert.AreEqual (evt.Teams, new ObservableCollection<Team> ());
				}
			}

			// Check that ScoreEvents and PenaltyCardEvents have now different Score and PenaltyCatd EventType instead
			// of the generic one.
			Assert.AreEqual (6, project.ScoreEvents.GroupBy (e => e.EventType).Count ());
			Assert.AreEqual (2, project.PenaltyCardsEvents.GroupBy (e => e.EventType).Count ());

			// Check that all the timeline events have a FileSet
			Assert.IsFalse (project.Timeline.Any (e => e.FileSet == null));
		}
	}
}

