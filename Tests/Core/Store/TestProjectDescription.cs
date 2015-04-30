//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.IO;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using NUnit.Framework;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestProjectDescription
	{
		[Test ()]
		public void TestSerialization ()
		{
			MediaFile mf = new MediaFile ("path", 34000, 25, true, true, "mp4", "h264",
				               "aac", 320, 240, 1.3, null, "Test asset");
			ProjectDescription pd = new ProjectDescription ();
			Utils.CheckSerialization (pd);
			
			pd.FileSet = new MediaFileSet ();
			pd.FileSet.Add (mf);
			pd.Competition = "Comp";
			pd.Category = "Cat";
			pd.Group = "Group";
			pd.Phase = "Phase";
			pd.Season = "Season";
			pd.LastModified = DateTime.UtcNow.ToUniversalTime ();
			pd.LocalGoals = 1;
			pd.VisitorGoals = 2;
			pd.MatchDate = DateTime.UtcNow.ToUniversalTime ();

			Utils.CheckSerialization (pd);
			
			ProjectDescription newpd = Utils.SerializeDeserialize (pd);
			Assert.AreEqual (pd.CompareTo (newpd), 0);
			Assert.AreEqual (pd.FileSet.First ().FilePath,
				newpd.FileSet.First ().FilePath);
			Assert.AreEqual (pd.ID, newpd.ID);
			Assert.AreEqual (pd.Competition, newpd.Competition);
			Assert.AreEqual (pd.Category, newpd.Category);
			Assert.AreEqual (pd.Group, newpd.Group);
			Assert.AreEqual (pd.Phase, newpd.Phase);
			Assert.AreEqual (pd.Season, newpd.Season);
			Assert.AreEqual (pd.LocalGoals, newpd.LocalGoals);
			Assert.AreEqual (pd.VisitorGoals, newpd.VisitorGoals);
			Assert.AreEqual (pd.MatchDate, newpd.MatchDate);
		}

		[Test ()]
		public void TestsProjectIDMigration ()
		{
			String oldJson = @"{ 
							      ""$id"": ""88"",
							      ""$type"": ""LongoMatch.Core.Store.ProjectDescription, LongoMatch.Core"",
							      ""ID"": ""49bb0f28-506b-452a-8158-f3007d3b4910"",
                                }";
			MemoryStream stream = new MemoryStream ();
			StreamWriter writer = new StreamWriter (stream);
			writer.Write (oldJson);
			writer.Flush ();
			stream.Position = 0;

			// Deserialize and check the ProjectID
			var newobj = Serializer.Load<ProjectDescription> (stream);

			Assert.AreEqual (Guid.Parse ("49bb0f28-506b-452a-8158-f3007d3b4910"), newobj.ID);
			Assert.AreEqual (Guid.Parse ("49bb0f28-506b-452a-8158-f3007d3b4910"), newobj.ProjectID);
		}

		[Test ()]
		[Ignore ("Not implemented")]
		public void TestSort ()
		{
		}

		[Test ()]
		[Ignore ("Not implemented")]
		public void TestSearch ()
		{
		}
	}
}

