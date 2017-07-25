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
using LongoMatch;
using LongoMatch.Core.Events;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Services.Controller;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Interfaces.GUI;

namespace Tests.Controller
{
	[TestFixture]
	public class TestLMTeamEditorController
	{
		LMTeamEditorController controller;
		LMTeamEditorVM viewModel;
		Mock<IDialogs> mockToolkit;
		IDialogs dialogs;

		[TestFixtureSetUp]
		public void SetUpFixture ()
		{
			dialogs = App.Current.Dialogs;
			mockToolkit = new Mock<IDialogs> ();
			mockToolkit.Setup (g => g.QuestionMessage (It.IsAny<string> (), null, null)
							  ).Returns (AsyncHelpers.Return<bool> (true));
			App.Current.Dialogs = mockToolkit.Object;
			Mock<IGUIToolkit> mockGui = new Mock<IGUIToolkit> ();
			mockGui.SetupGet (g => g.DeviceScaleFactor).Returns (1);
			App.Current.GUIToolkit = mockGui.Object;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown ()
		{
			App.Current.Dialogs = dialogs;
		}

		[SetUp]
		public void SetUp ()
		{
			controller = new LMTeamEditorController ();
			var teamsManager = new TeamsManagerVM ();
			teamsManager.LoadedTemplate.Model = LMTeam.DefaultTemplate (5);
			viewModel = teamsManager.TeamEditor;

			controller.SetViewModel (teamsManager);
			controller.Start ();
		}

		[TearDown]
		public void TearDown ()
		{
			controller.Stop ();
		}

		[Test]
		public void TestAddPlayer ()
		{
			int count = viewModel.Team.Model.Players.Count;

			viewModel.NewPlayerCommand.Execute ();

			Assert.AreEqual (count + 1, viewModel.Team.Model.Players.Count);
		}

		[Test]
		public void TestAddPlayerEmitsUpdateLineup ()
		{
			bool emitted = false;
			App.Current.EventsBroker.Subscribe<UpdateLineup> ((obj) => emitted = true);

			viewModel.NewPlayerCommand.Execute ();

			Assert.IsTrue (emitted);
		}

		[Test]
		public void TestDeletePlayer ()
		{
			int count = viewModel.Team.Model.Players.Count;

			viewModel.Team.Selection.Add (viewModel.Team.ViewModels.FirstOrDefault ());
			viewModel.DeletePlayersCommand.Execute ();

			Assert.AreEqual (count - 1, viewModel.Team.Model.Players.Count);
		}

		[Test]
		public void TestDeletePlayerUpdateLineup ()
		{
			bool emitted = false;
			App.Current.EventsBroker.Subscribe<UpdateLineup> ((obj) => emitted = true);

			viewModel.Team.Selection.Add (viewModel.Team.ViewModels.FirstOrDefault ());
			viewModel.DeletePlayersCommand.Execute ();

			Assert.IsTrue (emitted);
		}
	}
}
