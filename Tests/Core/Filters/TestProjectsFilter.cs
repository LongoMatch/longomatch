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
using System.Collections.Generic;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using NUnit.Framework;
using VAS.Core.Common;

namespace Tests.Core.Filters
{
	[TestFixture ()]
	public class TestProjectsFilter
	{
		List<LMProject> projects;
		ProjectsFilter projectsFilter;

		[OneTimeSetUp]
		public void FillProjects ()
		{
			projects = new List<LMProject> ();
			AddProject (null, null, "Complu", "Club");
			AddProject ("Liga", null, "Complu", "Club");
			AddProject (null, "2015", "Complu", "Club");
			AddProject ("Liga", "2015", "Complu", "Club");
			AddProject ("Liga", "2015", "Complu", "Sanse");
			AddProject ("Liga", "2015", "Complu", "Tenis");
			AddProject ("Liga", "2016", "Complu", "Club");
			AddProject ("Liga", "2016", "Complu", "Sanse");
			AddProject ("Liga", "2016", "Complu", "Tenis");
			AddProject ("EHL", "2016", "Complu", "Amsterdam");
			AddProject ("EHL", "2016", "Complu", "HCG");
			AddProject ("EHL", "2016", "Complu", "ATHC");
		}

		[SetUp]
		public void SetUp ()
		{
			projectsFilter = new ProjectsFilter { Projects = projects };
		}

		void AddProject (string competition, string season, string homeTeam, string awayTeam)
		{
			LMProject p = new LMProject ();
			p.Description = new ProjectDescription {
				Competition = competition,
				Season = season,
				LocalName = homeTeam,
				VisitorName = awayTeam,
			};
			projects.Add (p);
		}

		[Test ()]
		public void TestApplyChanges ()
		{
			Assert.IsEmpty (projectsFilter.VisibleProjects);
			projectsFilter.ApplyChanges ();
			Assert.IsEmpty (projectsFilter.VisibleProjects);
			projectsFilter.FilterCompetition ("Liga", true);
			Assert.IsEmpty (projectsFilter.VisibleProjects);
			projectsFilter.ApplyChanges ();
			Assert.IsNotEmpty (projectsFilter.VisibleProjects);
		}

		[Test ()]
		public void TestFilterCompetitions ()
		{
			projectsFilter.FilterCompetition ("Liga", true);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (7, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterCompetition ("Liga", false);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (0, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterCompetition ("Liga", true);
			projectsFilter.FilterCompetition ("EHL", true);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (10, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterCompetition (Constants.EMPTY_OR_NULL, true);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (12, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterCompetition ("Liga", false);
			projectsFilter.FilterCompetition ("EHL", false);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (2, projectsFilter.VisibleProjects.Count);
		}

		[Test ()]
		public void TestFilterSeasons ()
		{
			projectsFilter.FilterSeason ("2015", true);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (4, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterSeason ("2015", false);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (0, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterSeason ("2015", true);
			projectsFilter.FilterSeason ("2016", true);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (10, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterSeason (Constants.EMPTY_OR_NULL, true);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (12, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterSeason ("2015", false);
			projectsFilter.FilterSeason ("2016", false);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (2, projectsFilter.VisibleProjects.Count);
		}

		[Test ()]
		public void TestFilterTeams ()
		{
			projectsFilter.FilterTeam ("Complu", true);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (12, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterTeam ("Club", true);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (12, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterTeam ("Complu", false);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (5, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterTeam ("Tenis", true);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (7, projectsFilter.VisibleProjects.Count);
		}

		[Test ()]
		public void TestEmptyFilter ()
		{
			Assert.IsEmpty (projectsFilter.VisibleProjects);
			projectsFilter.ApplyChanges ();
			Assert.IsEmpty (projectsFilter.VisibleProjects);
		}

		[Test ()]
		public void TestNullOrEmptyFilters ()
		{
			projectsFilter.FilterCompetition (Constants.EMPTY_OR_NULL, true);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (2, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterSeason (Constants.EMPTY_OR_NULL, true);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (1, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterCompetition (Constants.EMPTY_OR_NULL, false);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (2, projectsFilter.VisibleProjects.Count);
		}

		[Test ()]
		public void TestCombinations ()
		{
			projectsFilter.FilterCompetition ("Liga", true);
			projectsFilter.FilterSeason ("2015", true);
			projectsFilter.FilterTeam ("Complu", true);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (3, projectsFilter.VisibleProjects.Count);
			projectsFilter.FilterSeason ("2016", true);
			projectsFilter.ApplyChanges ();
			Assert.AreEqual (6, projectsFilter.VisibleProjects.Count);
		}
	}
}

