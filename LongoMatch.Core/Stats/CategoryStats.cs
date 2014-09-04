// 
//  Copyright (C) 2012 Andoni Morales Alastruey
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
using System.Collections.Generic;

using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Common;

namespace LongoMatch.Stats
{
	public class CategoryStats: Stat
	{
		List <SubCategoryStat> subcatStats;
		EventType cat;
		
		public CategoryStats (EventType cat, int totalCount, int localTeamCount, int visitorTeamCount):
			base (cat.Name, totalCount, localTeamCount, visitorTeamCount)
		{
			subcatStats = new List<SubCategoryStat>();
			this.cat = cat;
		}
		
		public List<SubCategoryStat> SubcategoriesStats {
			get {
				return subcatStats;
			}
		}
		
		public EventType Category {
			get {
				return cat;
			}
		}
		
		public List<Coordinates> FieldCoordinates {
			get; set;
		}
		
		public List<Coordinates> HalfFieldCoordinates {
			get; set;
		}
		
		public List<Coordinates> GoalCoordinates {
			get; set;
		}
		
		public List<Coordinates> HomeFieldCoordinates {
			get; set;
		}
		
		public List<Coordinates> HomeHalfFieldCoordinates {
			get; set;
		}
		
		public List<Coordinates> HomeGoalCoordinates {
			get; set;
		}
		
		public List<Coordinates> AwayFieldCoordinates {
			get; set;
		}
		
		public List<Coordinates> AwayHalfFieldCoordinates {
			get; set;
		}
		
		public List<Coordinates> AwayGoalCoordinates {
			get; set;
		}
		
		public void AddSubcatStat (SubCategoryStat subcatStat) {
			subcatStats.Add(subcatStat);
		}
		
	}
}

