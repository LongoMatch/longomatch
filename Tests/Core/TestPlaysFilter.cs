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
using NUnit.Framework;
using System;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Common;

namespace Tests.Core
{
	[TestFixture()]
	public class TestPlaysFilter
	{
	
		Project CreateProject () {
			TimelineEvent pl;
			Project p = new Project ();
			p.Dashboard = Dashboard.DefaultTemplate (10);
			p.LocalTeamTemplate = TeamTemplate.DefaultTemplate (5);
			p.VisitorTeamTemplate = TeamTemplate.DefaultTemplate (5);
			MediaFile mf = new MediaFile ("path", 34000, 25, true, true, "mp4", "h264",
			                              "aac", 320, 240, 1.3, null);
			ProjectDescription pd = new ProjectDescription ();
			pd.File = mf;
			p.Description = pd;
			
			/* No tags, no players */
			pl = new TimelineEvent {EventType = p.Dashboard.CategoriesList[0]};
			p.Timeline.Add (pl);
			/* tags, but no players */
			pl = new TimelineEvent {EventType = p.Dashboard.CategoriesList[1]};
			pl.Tags.Add (p.Dashboard.CategoriesList[1].Tags[0]);
			p.Timeline.Add (pl);
			/* tags and players */
			pl = new TimelineEvent {EventType = p.Dashboard.CategoriesList[2]};
			pl.Tags.Add (p.Dashboard.CategoriesList[2].Tags[1]);
			pl.Players.Add (p.LocalTeamTemplate.List[0]);
			p.Timeline.Add (pl);
			return p;
		}
		
		[Test()]
		public void TestEmptyFilter ()
		{
			Project p = CreateProject ();
			PlaysFilter filter = new PlaysFilter (p);
			
			Assert.AreEqual (17, filter.VisibleCategories.Count);
			Assert.AreEqual (10, filter.VisiblePlayers.Count);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
		}
		
		[Test()]
		public void TestFilterCategory ()
		{
			Project p = CreateProject ();
			PlaysFilter filter = new PlaysFilter (p);
			
			filter.FilterEventType (p.Dashboard.CategoriesList[0], true);
			Assert.AreEqual (1, filter.VisibleCategories.Count);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			
			filter.FilterEventType (p.Dashboard.CategoriesList[1], true);
			Assert.AreEqual (2, filter.VisibleCategories.Count);
			Assert.AreEqual (2, filter.VisiblePlays.Count);

			filter.FilterEventType (p.Dashboard.CategoriesList[2], true);
			Assert.AreEqual (3, filter.VisibleCategories.Count);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			
			filter.FilterEventType (p.Dashboard.CategoriesList[0], true);
			Assert.AreEqual (3, filter.VisibleCategories.Count);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			
			filter.FilterEventType (p.Dashboard.CategoriesList[0], false);
			Assert.AreEqual (2, filter.VisibleCategories.Count);
			Assert.AreEqual (2, filter.VisiblePlays.Count);

			filter.FilterEventType (p.Dashboard.CategoriesList[1], false);
			Assert.AreEqual (1, filter.VisibleCategories.Count);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			
			filter.FilterEventType (p.Dashboard.CategoriesList[2], false);
			Assert.AreEqual (17, filter.VisibleCategories.Count);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
		}
		
		[Test()]
		public void TestFilterCategoryTags ()
		{
			Project p = CreateProject ();
			PlaysFilter filter = new PlaysFilter (p);
			
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			
			filter.FilterCategoryTag (p.Dashboard.CategoriesList[0], p.Dashboard.CategoriesList[0].Tags[0], true);
			Assert.AreEqual (1, filter.VisibleCategories.Count);
			Assert.AreEqual (0, filter.VisiblePlays.Count);

			filter.FilterCategoryTag (p.Dashboard.CategoriesList[1], p.Dashboard.CategoriesList[1].Tags[0], true);
			Assert.AreEqual (2, filter.VisibleCategories.Count);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			
			filter.FilterCategoryTag (p.Dashboard.CategoriesList[2], p.Dashboard.CategoriesList[2].Tags[0], true);
			Assert.AreEqual (3, filter.VisibleCategories.Count);
			Assert.AreEqual (1, filter.VisiblePlays.Count);

			filter.FilterCategoryTag (p.Dashboard.CategoriesList[2], p.Dashboard.CategoriesList[2].Tags[1], true);
			Assert.AreEqual (2, filter.VisiblePlays.Count);
			
			filter.FilterCategoryTag (p.Dashboard.CategoriesList[0], p.Dashboard.CategoriesList[0].Tags[0], false);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			
			filter.FilterCategoryTag (p.Dashboard.CategoriesList[1], p.Dashboard.CategoriesList[1].Tags[0], false);
			filter.FilterCategoryTag (p.Dashboard.CategoriesList[1], p.Dashboard.CategoriesList[1].Tags[1], true);
			Assert.AreEqual (2, filter.VisiblePlays.Count);
			Assert.AreEqual (p.Timeline[0], filter.VisiblePlays[0]);
			Assert.AreEqual (p.Timeline[2], filter.VisiblePlays[1]);
			
			/* One tag filtered now, but not the one of this play */
			filter.FilterCategoryTag (p.Dashboard.CategoriesList[2], p.Dashboard.CategoriesList[2].Tags[1], false);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			Assert.AreEqual (p.Timeline[0], filter.VisiblePlays[0]);
			/* No more tags filtered, if the category matches we are ok */
			filter.FilterCategoryTag (p.Dashboard.CategoriesList[2], p.Dashboard.CategoriesList[2].Tags[0], false);
			Assert.AreEqual (2, filter.VisiblePlays.Count);
			Assert.AreEqual (p.Timeline[0], filter.VisiblePlays[0]);
			Assert.AreEqual (p.Timeline[2], filter.VisiblePlays[1]);

			filter.ClearCategoriesFilter ();
			Assert.AreEqual (3, filter.VisiblePlays.Count);
		}
		
		[Test()]
		public void TestFilterPlayers ()
		{
			Project p = CreateProject ();
			PlaysFilter filter = new PlaysFilter (p);
			
			Assert.AreEqual (10, filter.VisiblePlayers.Count);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			filter.FilterPlayer (p.LocalTeamTemplate.List[4], true);
			Assert.AreEqual (0, filter.VisiblePlays.Count);
			Assert.AreEqual (1, filter.VisiblePlayers.Count);
			filter.FilterPlayer (p.LocalTeamTemplate.List[0], true);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			Assert.AreEqual (2, filter.VisiblePlayers.Count);
			filter.FilterPlayer (p.LocalTeamTemplate.List[0], true);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			Assert.AreEqual (2, filter.VisiblePlayers.Count);
			filter.FilterPlayer (p.LocalTeamTemplate.List[0], false);
			Assert.AreEqual (0, filter.VisiblePlays.Count);
			Assert.AreEqual (1, filter.VisiblePlayers.Count);
			filter.FilterPlayer (p.LocalTeamTemplate.List[4], false);
			Assert.AreEqual (10, filter.VisiblePlayers.Count);
			Assert.AreEqual (3, filter.VisiblePlays.Count);
		}
		
		[Test()]
		public void TestClearFilters ()
		{
			Project p = CreateProject ();
			PlaysFilter filter = new PlaysFilter (p);

			filter.FilterPlayer (p.LocalTeamTemplate.List[0], true);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			Assert.AreEqual (1, filter.VisiblePlayers.Count);
			filter.ClearPlayersFilter();
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			Assert.AreEqual (10, filter.VisiblePlayers.Count);
			
			filter.FilterEventType (p.Dashboard.CategoriesList[0], true);
			Assert.AreEqual (1, filter.VisiblePlays.Count);
			Assert.AreEqual (1, filter.VisibleCategories.Count);
			filter.ClearCategoriesFilter ();
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			Assert.AreEqual (17, filter.VisibleCategories.Count);
			
			filter.FilterCategoryTag (p.Dashboard.CategoriesList[0], p.Dashboard.CategoriesList[0].Tags[0], true);
			Assert.AreEqual (0, filter.VisiblePlays.Count);
			Assert.AreEqual (1, filter.VisibleCategories.Count);
			filter.ClearAll ();
			Assert.AreEqual (3, filter.VisiblePlays.Count);
			Assert.AreEqual (17, filter.VisibleCategories.Count);
		}
	}
}
