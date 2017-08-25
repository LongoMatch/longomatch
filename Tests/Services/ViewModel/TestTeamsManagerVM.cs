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
using LongoMatch;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;

namespace Tests.Services.ViewModel
{
	[TestFixture]
	public class TestTeamsManagerVM
	{
		TeamsManagerVM viewModel;

		[OneTimeSetUp]
		public void TestFixtureSetUp ()
		{
			Mock<IGUIToolkit> mockGui = new Mock<IGUIToolkit> ();
			mockGui.SetupGet (g => g.DeviceScaleFactor).Returns (1);
			App.Current.GUIToolkit = mockGui.Object;
		}

		[SetUp]
		public void SetUp ()
		{
			viewModel = new TeamsManagerVM ();
		}

		[Test]
		public void TestLoadedTemplateType ()
		{
			Assert.IsInstanceOf (typeof (LMTeamVM), viewModel.LoadedTemplate);
		}

		[Test]
		public void TestTeamsManagerInitialization ()
		{
			Assert.AreSame (viewModel.LoadedTemplate, viewModel.TeamTagger.HomeTeam);
			Assert.AreEqual (viewModel.TeamTagger.AwayTeam, null);
			Assert.IsTrue (viewModel.TeamTagger.ShowSubstitutionButtons);
			Assert.IsFalse (viewModel.TeamTagger.Compact);
			Assert.AreEqual (MultiSelectionMode.MultipleWithModifier, viewModel.TeamTagger.SelectionMode);
			Assert.IsFalse (viewModel.TeamTagger.ShowTeamsButtons);
			Assert.IsFalse (viewModel.TeamTagger.SubstitutionMode);
		}

		[Test]
		public void TestTeamEditorInitialization ()
		{
			Assert.AreSame (viewModel.LoadedTemplate, viewModel.TeamEditor.Team);
			Assert.IsTrue (viewModel.TeamEditor.Team.TemplateEditorMode);
		}
	}
}
