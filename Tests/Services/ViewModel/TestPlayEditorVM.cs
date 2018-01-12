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

namespace Tests.Services.ViewModel
{
	[TestFixture]
	public class TestPlayEditorVM
	{
		PlayEditorVM viewModel;

		[SetUp]
		public void SetUp ()
		{
			viewModel = new PlayEditorVM ();
		}

		[Test]
		public void TestTeamTaggerInitialization ()
		{
			Assert.IsFalse (viewModel.TeamTagger.ShowSubstitutionButtons);
			Assert.IsTrue (viewModel.TeamTagger.Compact);
			Assert.AreEqual (MultiSelectionMode.Multiple, viewModel.TeamTagger.SelectionMode);
			Assert.IsTrue (viewModel.TeamTagger.ShowTeamsButtons);
			Assert.IsFalse (viewModel.TeamTagger.SubstitutionMode);
		}

		[Test]
		public void TestTeamTaggerUpdatesWhenSettingProject ()
		{
			var lmProject = Utils.CreateProject ();
			viewModel.Project = new LMProjectVM { Model = lmProject };

			Assert.AreSame (lmProject.LocalTeamTemplate, viewModel.TeamTagger.HomeTeam.Model);
			Assert.AreSame (lmProject.VisitorTeamTemplate, viewModel.TeamTagger.AwayTeam.Model);
			Assert.AreSame (viewModel.Project.HomeTeam, viewModel.TeamTagger.HomeTeam);
			Assert.AreSame (viewModel.Project.AwayTeam, viewModel.TeamTagger.AwayTeam);
			Assert.AreSame (lmProject.Dashboard.FieldBackground, viewModel.TeamTagger.Background);
		}

		[Test]
		public void TestTeamTaggerUpdatesWhenSettingPlay ()
		{
			var lmProject = Utils.CreateProject ();
			viewModel.Project = new LMProjectVM { Model = lmProject };
			var play = lmProject.Timeline [0] as LMTimelineEvent;
			play.Players.Add (lmProject.LocalTeamTemplate.List [0]);
			play.Teams.Add (lmProject.LocalTeamTemplate);
			viewModel.Play = new LMTimelineEventVM () { Model = play };

			Assert.IsTrue (viewModel.TeamTagger.HomeTeam.Tagged);
			Assert.IsFalse (viewModel.TeamTagger.AwayTeam.Tagged);
			Assert.AreEqual (1, viewModel.TeamTagger.HomeTeam.Selection.Count);
			Assert.AreSame (lmProject.LocalTeamTemplate.List [0],
							viewModel.TeamTagger.HomeTeam.Selection [0].Model);
			Assert.IsTrue (viewModel.TeamTagger.HomeTeam.Selection [0].Tagged);
			Assert.AreEqual (play.EventTime, viewModel.TeamTagger.CurrentTime);
		}
	}
}
