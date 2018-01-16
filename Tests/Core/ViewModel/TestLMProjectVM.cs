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

		[Test]
		public void Stateful_ModifyModel_ViewModelIsChanged ()
		{
			viewModel.Stateful = true;
			viewModel.IsChanged = false;

			model.Description.Season = "newseason";

			Assert.IsTrue (viewModel.Edited);
			Assert.IsTrue (viewModel.IsChanged);
			Assert.IsTrue (model.IsChanged);
			Assert.AreEqual ("newseason", viewModel.Season);
			Assert.AreEqual ("newseason", model.Description.Season);
		}

		[Test]
		public void Stateful_ModifyViewModel_ModelIsNotChanged ()
		{
			viewModel.Stateful = true;
			viewModel.IsChanged = false;

			viewModel.Season = "newseason";

			Assert.IsFalse (viewModel.Edited);
			Assert.IsTrue (viewModel.IsChanged);
			Assert.IsFalse (model.IsChanged);
			Assert.AreEqual ("newseason", viewModel.Season);
			Assert.AreNotEqual ("newseason", model.Description.Season);
		}

		[Test]
		public void StatefulModifiedViewModel_CommitChanges_ModelIsChanged ()
		{
			viewModel.Stateful = true;
			viewModel.IsChanged = false;

			viewModel.Season = "newseason";
			viewModel.CommitState ();

			Assert.IsTrue (viewModel.Edited);
			Assert.IsTrue (viewModel.IsChanged);
			Assert.IsTrue (model.IsChanged);
			Assert.AreEqual ("newseason", viewModel.Season);
			Assert.AreEqual ("newseason", model.Description.Season);
		}
	}
}
