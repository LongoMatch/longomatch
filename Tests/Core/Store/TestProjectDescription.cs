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
using System.Linq;
using NUnit.Framework;
using VAS.Core.Store;
using LongoMatch.Core.Store;

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
			var pd = new ProjectDescription ();
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
			Assert.AreEqual (pd.FileSet.First ().FilePath,
				newpd.FileSet.First ().FilePath);
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

