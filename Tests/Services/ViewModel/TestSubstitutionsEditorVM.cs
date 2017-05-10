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
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;

namespace Tests.Services.ViewModel
{
	[TestFixture]
	public class TestSubstitutionsEditorVM
	{
		SubstitutionsEditorVM viewModel;

		[SetUp]
		public void SetUp ()
		{
			viewModel = new SubstitutionsEditorVM ();
		}

		[Test]
		public void TestTeamTaggerInitialization ()
		{
			Assert.IsFalse (viewModel.TeamTagger.ShowSubstitutionButtons);
			Assert.IsFalse (viewModel.TeamTagger.Compact);
			Assert.IsFalse (viewModel.TeamTagger.ShowTeamsButtons);
			Assert.AreEqual (MultiSelectionMode.Single, viewModel.TeamTagger.SelectionMode);
			Assert.IsFalse (viewModel.TeamTagger.SubstitutionMode);
		}

		[Test]
		public void TestTeamTaggerUpdatesWhenSettingProjectAndSubstitutionEvent ()
		{
			var lmProject = Utils.CreateProject ();
			var substitutionEvent = new SubstitutionEvent ();
			substitutionEvent.EventTime = new Time (1000);
			substitutionEvent.In = lmProject.LocalTeamTemplate.Players [0];
			substitutionEvent.Out = lmProject.LocalTeamTemplate.Players [1];
			substitutionEvent.Teams.Add (lmProject.LocalTeamTemplate);
			viewModel.Project = new LMProjectVM { Model = lmProject };
			viewModel.Play = substitutionEvent;

			Assert.AreSame (viewModel.TeamTagger.HomeTeam, viewModel.Project.HomeTeam);
			Assert.IsNull (viewModel.TeamTagger.AwayTeam);
			Assert.AreSame (substitutionEvent.In, viewModel.InPlayer.Model);
			Assert.AreSame (substitutionEvent.Out, viewModel.OutPlayer.Model);
			Assert.AreEqual (substitutionEvent.EventTime, viewModel.TeamTagger.CurrentTime);
			Assert.IsFalse (viewModel.LineupMode);
			Assert.IsFalse (viewModel.TeamTagger.SubstitutionMode);
			Assert.AreSame (viewModel.TeamTagger.Background, lmProject.Dashboard.FieldBackground);
		}

		[Test]
		public void TestTeamTaggerUpdatesWhenSettingProjectAndLineupEvent ()
		{
			var lmProject = Utils.CreateProject ();
			viewModel.Project = new LMProjectVM { Model = lmProject };
			viewModel.Play = lmProject.Lineup;

			Assert.AreSame (viewModel.TeamTagger.HomeTeam, viewModel.Project.HomeTeam);
			Assert.AreSame (viewModel.TeamTagger.AwayTeam, viewModel.Project.AwayTeam);
			Assert.AreSame (viewModel.TeamTagger.Background, lmProject.Dashboard.FieldBackground);
			Assert.IsTrue (viewModel.LineupMode);
			Assert.IsTrue (viewModel.TeamTagger.SubstitutionMode);
		}
	}
}
