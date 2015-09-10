//
//  Copyright (C) 2015 vguzman
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
using NUnit.Framework;
using System;
using LongoMatch.DB;
using System.IO;
using LongoMatch.Core.Store;
using Moq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;

namespace Tests.DB
{
	[TestFixture ()]
	public class TestDataBaseFileDB
	{

		DataBase db;
		Mock<ISerializer> mockSerializer;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string dbPath = Path.Combine (Path.GetTempPath (), "TestDB");
			if (Directory.Exists (dbPath)) {
				Directory.Delete (dbPath, true);
			}
			db = new DataBase ("TestDB");
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			db.Delete ();
		}

		[SetUp]
		public void SetUp ()
		{
			mockSerializer = new Mock<ISerializer> ();

		}

		[TearDown]
		public void TearDown ()
		{
			foreach (var d in db.GetAllProjects()) {
				db.RemoveProject (d.ProjectID);
			}
			db.SerializerObject = new Serializer ();

			mockSerializer.ResetCalls ();
		}

		[Test ()]
		public void TestAddProject ()
		{
			Project project = new Project ();
			project.Description = new ProjectDescription ();
			db.AddProject (project);

			Assert.AreEqual (1, db.Count);
			Assert.IsTrue (project.Equals (db.GetProject (project.ID)));
		}

		[Test ()]
		public void TestErrorSavingAndReopen ()
		{
			mockSerializer.Setup (x => x.Save<Project> (It.IsAny<Project> (), It.IsAny<string> (), It.IsAny<SerializationType> ())).Throws (new Exception ("mocked exception"));

			Project project = new Project ();
			project.Description = new ProjectDescription ();
			db.AddProject (project);

			Assert.AreEqual (1, db.Count);


			db.SerializerObject = mockSerializer.Object;

			Assert.Throws<Exception> (() => db.UpdateProject (project));

			mockSerializer.Verify (x => x.Save<Project> (It.IsAny<Project> (), It.IsAny<string> (), It.IsAny<SerializationType> ()), Times.Once ());
		}
	}
}

