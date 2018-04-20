//
//  Copyright (C) 2017 ${CopyrightHolder}
using System;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using NUnit.Framework;

namespace Tests.Core.ViewModel
{
	[TestFixture]
	public class TestLMProjectVM
	{
		LMProjectVM viewModel;
		LMProject model;

		[SetUp]
		public void SetUp ()
		{
			model = Utils.CreateProject (true);
			viewModel = new LMProjectVM { Model = model };
			model.IsChanged = false;
			viewModel.IsChanged = false;
		}

		[TearDown]
		public void TearDown ()
		{

		}

		[Test]
		public void ModifyModel_ViewModelIsChanged ()
		{
			model.Description.Season = "newseason";

			Assert.IsTrue (viewModel.Edited);
			Assert.IsTrue (viewModel.IsChanged);
			Assert.IsTrue (model.IsChanged);
			Assert.AreEqual ("newseason", viewModel.Season);
			Assert.AreEqual ("newseason", model.Description.Season);
		}

		[Test]
		public void ModifyViewModel_ModelIsChanged ()
		{
			viewModel.Season = "newseason";

			Assert.IsTrue (viewModel.Edited);
			Assert.IsTrue (viewModel.IsChanged);
			Assert.IsTrue (model.IsChanged);
			Assert.AreEqual ("newseason", viewModel.Season);
			Assert.AreEqual ("newseason", model.Description.Season);
		}
	}
}
