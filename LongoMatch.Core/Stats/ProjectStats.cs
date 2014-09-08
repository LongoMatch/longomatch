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
using System.Linq;

using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.Core.Stats
{
	public class ProjectStats: IDisposable
	{
		List<EventTypeStats> catStats;
		EventsFilter filter;
		Project project;
		
		public ProjectStats (Project project)
		{
			catStats = new List<EventTypeStats>();
			this.project = project;
			CreateStats ();
		}
		
		public void Dispose ()
		{
		}
		
		public Project Project {
			get;
			protected set;
		}

		public List<EventTypeStats> EventTypeStats {
			get;
			protected set;
		}
		
		public EventsFilter Filter {
			set {
				filter = value;
				UpdateStats ();
			}
		}
		
		public void CreateStats () {
			EventTypeStats = new List <EventTypeStats> ();
			
			foreach (EventType evt in project.EventTypes) {
				EventTypeStats evstats = new EventTypeStats (project, filter, evt);
				evstats.Update ();
			}
		}

		public void UpdateStats ()
		{
			foreach (EventTypeStats e in EventTypeStats) {
				e.Update();
			}
		}

//		void GetSubcategoryStats (List<TimelineEvent> subcatPlays, SubCategoryStat subcatStat, string desc,
//			int totalCount, out int localTeamCount, out int visitorTeamCount)
//		{
//			int count;
//			
//			count = subcatPlays.Count(); 
//			CountPlaysInTeam(subcatPlays, out localTeamCount, out visitorTeamCount);
//			PercentualStat pStat = new PercentualStat(totalCount);
//			pStat.Name = desc;
//			pStat.TotalCount = count;
//			pStat.LocalTeamCount = localTeamCount;
//			pStat.VisitorTeamCount = visitorTeamCount;
//			subcatStat.AddOptionStat(pStat);
//		}
//		
//		void GetPlayersStats (Project project, List<TimelineEvent> subcatPlays, string optionName,
//			SubCategoryStat subcatStat, EventType cat)
//		{
//			foreach (SubCategory subcat in cat.SubCategories) {
//				Dictionary<Player, int> localPlayerCount = new Dictionary<Player, int>();
//				Dictionary<Player, int> visitorPlayerCount = new Dictionary<Player, int>();
//				
//				if (!(subcat is PlayerSubCategory))
//					continue;
//				
//				playerSubcat = subcat as PlayerSubCategory;
//				
//				if (playerSubcat.Contains(Team.LOCAL) || playerSubcat.Contains(Team.BOTH)){
//					foreach (Player player in project.LocalTeamTemplate.List) {
//						localPlayerCount.Add(player, GetPlayerCount(subcatPlays, player, subcat as PlayerSubCategory));
//					}
//					subcatStat.AddPlayersStats(optionName, subcat.Name, Team.LOCAL, localPlayerCount);
//				}
//				
//				if (playerSubcat.Contains(Team.VISITOR) || playerSubcat.Contains(Team.BOTH)){
//					foreach (Player player in project.VisitorTeamTemplate.List) {
//						visitorPlayerCount.Add(player, GetPlayerCount(subcatPlays, player, subcat as PlayerSubCategory));
//					}
//					subcatStat.AddPlayersStats(optionName, subcat.Name, Team.VISITOR, visitorPlayerCount);
//				}
//			}
//		}
//		
	}
}

