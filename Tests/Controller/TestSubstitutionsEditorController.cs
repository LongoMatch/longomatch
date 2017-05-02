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
using LongoMatch.Services.Controller;
using LongoMatch.Services.ViewModel;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;

namespace Tests.Controller
{
	[TestFixture]
	public class TestSubstitutionsEditorController
	{
		SubstitutionsEditorController controller;
		LMTeamTaggerController teamTaggerController;
		SubstitutionsEditorVM viewModel;
		LMProject project;

		void InitializeWithLineupEvent ()
		{
			viewModel.Play = project.Lineup;
			StartControllers ();
		}

		void InitializeWithSubstitutionEvent ()
		{
			var substitutionEvent = new SubstitutionEvent ();
			substitutionEvent.EventTime = new Time (1000);
			substitutionEvent.In = project.LocalTeamTemplate.Players [0];
			substitutionEvent.Out = project.LocalTeamTemplate.Players [1];
			substitutionEvent.Teams.Add (project.LocalTeamTemplate);
			viewModel.Play = substitutionEvent;
			StartControllers ();
		}

		void StartControllers ()
		{
			controller.SetViewModel (viewModel);
			teamTaggerController.SetViewModel (viewModel);
			controller.Start ();
			teamTaggerController.Start ();
		}

		[SetUp]
		public void SetUp ()
		{
			controller = new SubstitutionsEditorController ();
			teamTaggerController = new LMTeamTaggerController ();
			viewModel = new SubstitutionsEditorVM ();

			project = Utils.CreateProject ();
			viewModel.Model = project;
		}

		[TearDown]
		public void TearDown ()
		{
			controller.Stop ();
			teamTaggerController.Stop ();
		}

		[Test]
		public void TestLineupEventIsNotUpdatedWhenClickingPlayers ()
		{
			InitializeWithLineupEvent ();
			var clickedPlayer1 = viewModel.HomeTeam.FieldPlayersList.ElementAt (0);
			var clickedPlayer2 = viewModel.HomeTeam.BenchPlayersList.ElementAt (0);

			Assert.IsFalse (((LineupEvent)viewModel.Play).HomeStartingPlayers.Contains (clickedPlayer2.Model));
			Assert.IsFalse (((LineupEvent)viewModel.Play).HomeBenchPlayers.Contains (clickedPlayer1.Model));

			viewModel.TeamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			viewModel.TeamTagger.PlayerClick (clickedPlayer2, ButtonModifier.None);

			Assert.IsTrue (viewModel.HomeTeam.FieldPlayersList.Contains (clickedPlayer2));
			Assert.IsTrue (viewModel.HomeTeam.BenchPlayersList.Contains (clickedPlayer1));
			Assert.IsFalse (((LineupEvent)viewModel.Play).HomeStartingPlayers.Contains (clickedPlayer2.Model));
			Assert.IsFalse (((LineupEvent)viewModel.Play).HomeBenchPlayers.Contains (clickedPlayer1.Model));
		}

		[Test]
		public void TestLineupEventIsSaved ()
		{
			InitializeWithLineupEvent ();
			LineupEvent lineupEvent = (LineupEvent)viewModel.Play.Clone ();
			var clickedPlayer1 = viewModel.HomeTeam.FieldPlayersList.ElementAt (0);
			var clickedPlayer2 = viewModel.HomeTeam.BenchPlayersList.ElementAt (0);

			viewModel.TeamTagger.PlayerClick (clickedPlayer1, ButtonModifier.None);
			viewModel.TeamTagger.PlayerClick (clickedPlayer2, ButtonModifier.None);
			viewModel.SaveCommand.Execute ();

			Assert.IsTrue (((LineupEvent)viewModel.Play).HomeStartingPlayers.Contains (clickedPlayer2.Model));
			Assert.IsTrue (((LineupEvent)viewModel.Play).HomeBenchPlayers.Contains (clickedPlayer1.Model));
			Assert.AreNotSame (lineupEvent.HomeStartingPlayers, ((LineupEvent)viewModel.Play).HomeStartingPlayers);
			Assert.AreNotSame (lineupEvent.HomeBenchPlayers, ((LineupEvent)viewModel.Play).HomeBenchPlayers);
		}

		[Test]
		public void TestSubstitutionEventOnlyOneIsTagged ()
		{
			InitializeWithSubstitutionEvent ();

			viewModel.InPlayer.Tagged = true;
			viewModel.OutPlayer.Tagged = true;

			Assert.IsFalse (viewModel.InPlayer.Tagged);
			Assert.IsTrue (viewModel.OutPlayer.Tagged);

			viewModel.InPlayer.Tagged = true;

			Assert.IsTrue (viewModel.InPlayer.Tagged);
			Assert.IsFalse (viewModel.OutPlayer.Tagged);
		}

		[Test]
		public void TestSubstitutionEventChangesInPlayer ()
		{
			InitializeWithSubstitutionEvent ();
			var clickedPlayer = viewModel.HomeTeam.FieldPlayersList.ElementAt (2);
			Assert.AreNotSame (clickedPlayer.Model, viewModel.InPlayer.Model);

			viewModel.InPlayer.Tagged = true;
			viewModel.TeamTagger.PlayerClick (clickedPlayer, ButtonModifier.None);

			Assert.AreSame (clickedPlayer.Model, viewModel.InPlayer.Model);
		}

		[Test]
		public void TestSubstitutionEventChangesOutPlayer ()
		{
			InitializeWithSubstitutionEvent ();
			var clickedPlayer = viewModel.HomeTeam.FieldPlayersList.ElementAt (2);
			Assert.AreNotSame (clickedPlayer.Model, viewModel.OutPlayer.Model);

			viewModel.OutPlayer.Tagged = true;
			viewModel.TeamTagger.PlayerClick (clickedPlayer, ButtonModifier.None);

			Assert.AreSame (clickedPlayer.Model, viewModel.OutPlayer.Model);
		}

		[Test]
		public void TestSubstitutionEventChangesAreNotSavedAutomatically ()
		{
			InitializeWithSubstitutionEvent ();
			var clickedPlayer = viewModel.HomeTeam.FieldPlayersList.ElementAt (2);
			Assert.AreSame (((SubstitutionEvent)viewModel.Play).In, viewModel.InPlayer.Model);

			viewModel.InPlayer.Tagged = true;
			viewModel.TeamTagger.PlayerClick (clickedPlayer, ButtonModifier.None);

			Assert.AreNotSame (((SubstitutionEvent)viewModel.Play).In, viewModel.InPlayer.Model);
		}

		[Test]
		public void TestSubstitutionEventChangesAreSaved ()
		{
			InitializeWithSubstitutionEvent ();
			var clickedPlayer = viewModel.HomeTeam.FieldPlayersList.ElementAt (2);

			viewModel.InPlayer.Tagged = true;
			viewModel.TeamTagger.PlayerClick (clickedPlayer, ButtonModifier.None);
			viewModel.SaveCommand.Execute ();

			Assert.AreSame (((SubstitutionEvent)viewModel.Play).In, viewModel.InPlayer.Model);
		}
	}
}
