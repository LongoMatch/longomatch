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

namespace LongoMatch.Stats
{
	public class CategoryStats: Stat
	{
		List <SubCategoryStat> subcatStats;
		
		public CategoryStats (string name, int totalCount, int localTeamCount, int visitorTeamCount):
			base (name, totalCount, localTeamCount, visitorTeamCount)
		{
			subcatStats = new List<SubCategoryStat>();
		}
		
		public List<SubCategoryStat> SubcategoriesStats {
			get {
				return subcatStats;
			}
		}
		
		public void AddSubcatStat (SubCategoryStat subcatStat) {
			subcatStats.Add(subcatStat);
		}
	}
}

