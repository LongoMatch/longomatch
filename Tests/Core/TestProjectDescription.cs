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
using NUnit.Framework;
using LongoMatch.Common;
using LongoMatch.Store;

namespace Tests.Core
{
	[TestFixture()]
	public class TestProjectDescription
	{
		[Test()]
		public void TestSerialization ()
		{
			MediaFile mf = new MediaFile ("path", 34000, 25, true, true, "mp4", "h264",
			                              "aac", 320, 240, 1.3, new Image (null));
			ProjectDescription pd = new ProjectDescription ();
			Utils.CheckSerialization (pd);
			
			pd.File = mf;
			pd.Competition = "Comp";
			pd.LastModified = DateTime.Now;
			pd.LocalGoals = 1;
			pd.VisitorGoals = 2;
			pd.MatchDate = DateTime.Now;
			pd.Season = "Season";
			
			Utils.CheckSerialization (pd);
			
			ProjectDescription newpd = Utils.SerializeDeserialize(pd);
			Assert.AreEqual (pd.CompareTo (newpd), 0);
			Assert.AreEqual (pd.ID, newpd.ID);
		}
	}
}

