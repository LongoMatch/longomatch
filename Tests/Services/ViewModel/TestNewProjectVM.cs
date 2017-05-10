//
//  Copyright (C) 2017 Fluendo S.A.
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
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
using NUnit.Framework;
using VAS.Core.Common;

namespace Tests.Services.ViewModel
{
	[TestFixture]
	public class TestNewProjectVM
	{
		NewProjectVM viewModel;

		[SetUp]
		public void SetUp ()
		{
			viewModel = new NewProjectVM ();
		}

		[Test]
		public void TestTeamTaggerInitialization ()
		{
			Assert.IsFalse (viewModel.TeamTagger.ShowSubstitutionButtons);
			Assert.IsFalse (viewModel.TeamTagger.Compact);
			Assert.AreEqual (MultiSelectionMode.Single, viewModel.TeamTagger.SelectionMode);
			Assert.IsFalse (viewModel.TeamTagger.ShowTeamsButtons);
			Assert.IsTrue (viewModel.TeamTagger.SubstitutionMode);
		}

		[Test]
		public void TestTeamTaggerUpdatesWhenSettingProject ()
		{
			var lmProject = Utils.CreateProject ();
			viewModel.Project = new LMProjectVM { Model = lmProject };

			Assert.AreSame (viewModel.TeamTagger.AwayTeam, viewModel.Project.AwayTeam);
			Assert.AreSame (viewModel.TeamTagger.HomeTeam, viewModel.Project.HomeTeam);
			Assert.AreSame (viewModel.TeamTagger.Background, lmProject.Dashboard.FieldBackground);
		}
	}
}
