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

using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Store;

namespace LongoMatch.Stats
{
	public class SubCategoryStat
	{
		
		List<PercentualStat> optionStats;
		Dictionary<string, List<PlayersStats>> localPlayersStats;
		Dictionary<string, List<PlayersStats>> visitorPlayersStats;
		ISubCategory subcat;
		
		public SubCategoryStat (ISubCategory subcat)
		{
			Name = subcat.Name;
			this.subcat = subcat;
			optionStats = new List<PercentualStat>();
			localPlayersStats = new Dictionary<string, List<PlayersStats>>(); 
			visitorPlayersStats = new Dictionary<string, List<PlayersStats>>(); 
			
		}
		
		public string Name {
			get;
			set;
		}
		
		public List<PercentualStat> OptionStats {
			get {
				return optionStats; 
			}
		}
		
		public ISubCategory SubCategory {
			get {
				return subcat;
			}
		}
		
		public Dictionary<string, List<PlayersStats>> LocalPlayersStats {
			get {
			 return localPlayersStats;
			}
		}
		
		public Dictionary<string, List<PlayersStats>> VisitorPlayersStats {
			get {
			 return visitorPlayersStats;
			}
		}
		
		public void AddOptionStat (PercentualStat stat) {
			optionStats.Add(stat);
		}
		
		public void AddPlayersStats (string optionName, string playerSubcatName, Team team,
			Dictionary<Player, int> playersCount)
		{
			Dictionary<string, List<PlayersStats>> playersStats;
			
			if (team == Team.LOCAL)
				playersStats = localPlayersStats;
			else 
				playersStats = visitorPlayersStats;
				
			PlayersStats stats = new PlayersStats(playerSubcatName, playersCount);
			if (playersStats.ContainsKey(optionName)) {
				playersStats[optionName].Add(stats);
			} else{
				List<PlayersStats> list = new List<PlayersStats>();
				list.Add(stats);
				playersStats.Add(optionName, list);
			}
		}
	}
}

