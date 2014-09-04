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
using LongoMatch.Store.Templates;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Core
{
	[TestFixture()]
	public class TestProject
	{
	
		Project CreateProject () {
			Project p = new Project ();
			p.Dashboard = Dashboard.DefaultTemplate (10);
			p.LocalTeamTemplate = TeamTemplate.DefaultTemplate (10);
			p.VisitorTeamTemplate = TeamTemplate.DefaultTemplate (12);
			MediaFile mf = new MediaFile ("path", 34000, 25, true, true, "mp4", "h264",
			                              "aac", 320, 240, 1.3, null);
			ProjectDescription pd = new ProjectDescription ();
			pd.File = mf;
			p.Description = pd;
			return p;
		}
		
		[Test()]
		public void TestSerialization ()
		{
			Project p = new Project ();
			
			Utils.CheckSerialization (p);
			
			p = CreateProject ();
			Utils.CheckSerialization (p);
			p.AddEvent (new TimelineEvent());
			Utils.CheckSerialization (p);
			
			Project newp = Utils.SerializeDeserialize (p);
			Assert.AreEqual (newp.CompareTo (p), 0);
			Assert.AreEqual (newp.Description.CompareTo (p.Description), 0);
			Assert.AreEqual (newp.Timeline.Count, p.Timeline.Count);
		}
		
		[Test ()]
		public void TestPlaysGrouping () {
			Project p = CreateProject ();
			p.AddPlay (p.Dashboard.CategoriesList[0], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[0], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[1], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[2], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[2], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[2], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[6], new Time (1000), new Time (2000), null);
			
			IEnumerable<IGrouping<DashboardButton, TimelineEvent>> g = p.PlaysGroupedByEventType;
			Assert.AreEqual (g.Count(), 4);
			IGrouping<DashboardButton, TimelineEvent> gr = g.ElementAt (0);
			Assert.AreEqual (gr.Key, p.Dashboard.List[0]);
			Assert.AreEqual (gr.Count(), 2);
			
			gr = g.ElementAt (1);
			Assert.AreEqual (gr.Key, p.Dashboard.List[1]);
			Assert.AreEqual (gr.Count(), 1);
			
			gr = g.ElementAt (2);
			Assert.AreEqual (gr.Key, p.Dashboard.List[2]);
			Assert.AreEqual (gr.Count(), 3);
			
			gr = g.ElementAt (3);
			Assert.AreEqual (gr.Key, p.Dashboard.List[6]);
			Assert.AreEqual (gr.Count(), 1);
		}
		
		[Test()]
		public void Clear() {
		}


		[Test ()]
		public void TestAddPlay () {
			Project p = CreateProject ();
			p.AddPlay (p.Dashboard.CategoriesList[0], new Time (1000), new Time (2000), null);
			Assert.AreEqual (p.Timeline.Count, 1);
			p.AddPlay (p.Dashboard.CategoriesList[0], new Time (1000), new Time (2000), null);
			Assert.AreEqual (p.Timeline.Count, 2);
			p.AddEvent (new TimelineEvent());
			Assert.AreEqual (p.Timeline.Count, 3);
			p.AddEvent (new TimelineEvent());
			Assert.AreEqual (p.Timeline.Count, 4);
		}
		
		[Test ()]
		public void TestRemovePlays () {
			TimelineEvent p1, p2, p3;
			List<TimelineEvent> plays = new List<TimelineEvent> ();
			Project p = CreateProject ();
			
			p1 = new TimelineEvent();
			p2 = new TimelineEvent();
			p3 = new TimelineEvent();
			p.AddEvent (p1);
			p.AddEvent (p2);
			p.AddEvent (p3);
			plays.Add(p1);
			plays.Add(p2);
			p.RemovePlays (plays);
			Assert.AreEqual (p.Timeline.Count, 1);
			Assert.AreEqual (p.Timeline[0], p3);
		}

		[Test ()] 
		public void TestRemoveCategory () {
			Project p = CreateProject ();
			p.AddPlay (p.Dashboard.CategoriesList[0], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[0], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[0], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[2], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[3], new Time (1000), new Time (2000), null);
			p.RemoveCategory(p.Dashboard.CategoriesList[0]);
			Assert.AreEqual(p.Timeline.Count, 2);
			Assert.AreEqual(p.Dashboard.CategoriesList.Count, 9);
			p.RemoveCategory(p.Dashboard.CategoriesList[0]);
			p.RemoveCategory(p.Dashboard.CategoriesList[0]);
			p.RemoveCategory(p.Dashboard.CategoriesList[0]);
			p.RemoveCategory(p.Dashboard.CategoriesList[0]);
			p.RemoveCategory(p.Dashboard.CategoriesList[0]);
			p.RemoveCategory(p.Dashboard.CategoriesList[0]);
			p.RemoveCategory(p.Dashboard.CategoriesList[0]);
			p.RemoveCategory(p.Dashboard.CategoriesList[0]);
			Assert.Throws<Exception>(
				delegate {p.RemoveCategory(p.Dashboard.CategoriesList[0]);});
		}
		
		[Test ()] 
		public void TestRemovePlayer () {
			TimelineEvent play = new TimelineEvent();
			Project project = CreateProject ();
			Player player = project.LocalTeamTemplate.List[0];
			play.Players.Add (player);
			project.AddEvent (play);
			project.RemovePlayer (project.LocalTeamTemplate, player);
			Assert.AreEqual (project.LocalTeamTemplate.List.Count, 9);
			Assert.IsFalse (play.Players.Contains (player));
		}
		
		[Test ()] 
		[Ignore ("FIXME")]
		public void TestDeleteSubcategoryTags () {
		}

		[Test ()] 
		public void TestPlaysInCategory () {
			Project p = CreateProject ();
			p.AddPlay (p.Dashboard.CategoriesList[0], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[0], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[0], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[2], new Time (1000), new Time (2000), null);
			p.AddPlay (p.Dashboard.CategoriesList[3], new Time (1000), new Time (2000), null);
			Assert.AreEqual (p.PlaysInCategory (p.Dashboard.CategoriesList[0]).Count, 3);
			Assert.AreEqual (p.PlaysInCategory (p.Dashboard.CategoriesList[1]).Count, 0);
			Assert.AreEqual (p.PlaysInCategory (p.Dashboard.CategoriesList[2]).Count, 1);
			Assert.AreEqual (p.PlaysInCategory (p.Dashboard.CategoriesList[3]).Count, 1);
		}

		[Test ()] 
		public void TestEquals () {
			Project p1 = CreateProject();
			Project p2 = Utils.SerializeDeserialize (p1);
			Project p3 = new Project ();
			
			Assert.IsTrue (p1.Equals(p2));
			Assert.IsFalse (p1.Equals(p3));
		}
	}
}
