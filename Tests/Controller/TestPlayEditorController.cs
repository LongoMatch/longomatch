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
using System.Linq;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.Controller;
using LongoMatch.Services.ViewModel;
using NUnit.Framework;
using VAS.Core.Common;

namespace Tests.Controller
{
	[TestFixture]
	public class TestPlayEditorController
	{
		PlayEditorController controller;
		LMTeamTaggerController teamTaggerController;
		PlayEditorVM viewModel;
		LMProject project;

		[SetUp]
		public void SetUp ()
		{
			controller = new PlayEditorController ();
			teamTaggerController = new LMTeamTaggerController ();
			viewModel = new PlayEditorVM ();

			project = Utils.CreateProject ();
			viewModel.Project = new LMProjectVM { Model = project };
			var play = project.Timeline [0] as LMTimelineEvent;
			play.Players.Add (project.LocalTeamTemplate.List [0]);
			play.Teams.Add (project.LocalTeamTemplate);
			viewModel.Play = new LMTimelineEventVM () { Model = play };

			controller.SetViewModel (viewModel);
			teamTaggerController.SetViewModel (viewModel);
			controller.Start ();
			teamTaggerController.Start ();
		}

		[TearDown]
		public void TearDown ()
		{
			controller.Stop ();
			teamTaggerController.Stop ();
		}

		[Test]
		public void TestClickNewPlayerAddsToEvent ()
		{
			var playerClicked = viewModel.TeamTagger.HomeTeam.FieldPlayersList.ElementAt (2);

			Assert.IsFalse (viewModel.Play.Players.Contains (playerClicked.Model));

			viewModel.TeamTagger.PlayerClick (playerClicked, ButtonModifier.None);

			Assert.IsTrue (viewModel.Play.Players.Contains (playerClicked.Model));
		}

		[Test]
		public void TestClickNewPlayerInAwayTeamAddsToEvent ()
		{
			var playerClicked = viewModel.TeamTagger.AwayTeam.FieldPlayersList.ElementAt (2);

			Assert.IsFalse (viewModel.Play.Players.Contains (playerClicked.Model));

			viewModel.TeamTagger.PlayerClick (playerClicked, ButtonModifier.None);

			Assert.IsTrue (viewModel.Play.Players.Contains (playerClicked.Model));
		}

		[Test]
		public void TestClickPlayerRemovesFromEvent ()
		{
			var playerClicked = viewModel.TeamTagger.HomeTeam.FieldPlayersList.ElementAt (0);

			Assert.IsTrue (viewModel.Play.Players.Contains (playerClicked.Model));

			viewModel.TeamTagger.PlayerClick (playerClicked, ButtonModifier.None);

			Assert.IsFalse (viewModel.Play.Players.Contains (playerClicked.Model));
		}

		[Test]
		public void TestClickTeamAddsToEvent ()
		{
			Assert.IsFalse (viewModel.Play.Teams.Contains (viewModel.Project.AwayTeam.Model));
			Assert.IsFalse (viewModel.TeamTagger.AwayTeam.Tagged);

			viewModel.TeamTagger.AwayTeam.Tagged = true;

			Assert.IsTrue (viewModel.Play.Teams.Contains (viewModel.Project.AwayTeam.Model));
		}

		[Test]
		public void TestClickTeamRemoveFromEvent ()
		{
			Assert.IsTrue (viewModel.Play.Teams.Contains (viewModel.Project.HomeTeam.Model));
			Assert.IsTrue (viewModel.TeamTagger.HomeTeam.Tagged);

			viewModel.TeamTagger.HomeTeam.Tagged = false;

			Assert.IsFalse (viewModel.Play.Teams.Contains (viewModel.Project.HomeTeam.Model));
		}
	}
}
