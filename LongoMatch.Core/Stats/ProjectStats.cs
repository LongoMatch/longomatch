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

using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Stats
{
	public class ProjectStats
	{
		List<CategoryStats> catStats;
		GameUnitsStats guStats;
		
		public ProjectStats (Project project)
		{
			catStats = new List<CategoryStats>();
			
			ProjectName = project.Description.Title;
			Date = project.Description.MatchDate;
			LocalTeam = project.LocalTeamTemplate.TeamName;
			VisitorTeam = project.VisitorTeamTemplate.TeamName;
			Competition = project.Description.Competition;
			Season = project.Description.Season;
			Results = String.Format("{0}-{1}", project.Description.LocalGoals, project.Description.VisitorGoals);
			UpdateStats (project);
			UpdateGameUnitsStats (project);
		}
		
		public string ProjectName {
			set;
			get;
		}
		
		public string Competition {
			get;
			set;
		}
		
		public string Season {
			get;
			set;
		}
		
		public string LocalTeam {
			get;
			set;
		}
		
		public string VisitorTeam {
			get;
			set;
		}
		
		public DateTime Date {
			get;
			set;
		}
		
		public string Results {
			get;
			set;
		}
		
		public List<CategoryStats> CategoriesStats {
			get {
				return catStats;
			}
		}
		
		public GameUnitsStats GameUnitsStats {
			get {
				return guStats;
			}
		}
		
		void CountPlaysInTeam (List<Play> plays, out int localTeamCount, out int visitorTeamCount) {
			localTeamCount = plays.Where(p => p.Team == Team.LOCAL || p.Team == Team.BOTH).Count();
			visitorTeamCount = plays.Where(p => p.Team == Team.VISITOR || p.Team == Team.BOTH).Count();
		}
		
		void UpdateGameUnitsStats (Project project) {
			guStats = new GameUnitsStats(project.GameUnits, (int)project.Description.File.Length);
		}
		
		void UpdateStats (Project project) {
			catStats.Clear();
			
			foreach (Category cat in project.Categories) {
				CategoryStats stats;
				List<Play> plays;
				int localTeamCount, visitorTeamCount;
				
				plays = project.PlaysInCategory (cat);
				CountPlaysInTeam(plays, out localTeamCount, out visitorTeamCount);
				stats = new CategoryStats(cat, plays.Count, localTeamCount, visitorTeamCount);
				catStats.Add (stats);
				
				foreach (ISubCategory subcat in cat.SubCategories) {
					SubCategoryStat subcatStat;
					
					if (subcat is PlayerSubCategory)
						continue;
						
					subcatStat = new SubCategoryStat(subcat);
					stats.AddSubcatStat(subcatStat);
					
					 if (subcat is TagSubCategory) {
						foreach (string option in subcat.ElementsDesc()) {
							List<Play> subcatPlays;
							StringTag tag;
							
							tag = new StringTag{SubCategory=subcat, Value=option};
							subcatPlays = plays.Where(p => p.Tags.Tags.Contains(tag)).ToList();
							GetSubcategoryStats(subcatPlays, subcatStat, option,
								stats.TotalCount, out localTeamCount, out visitorTeamCount);
							GetPlayersStats(project, subcatPlays, option, subcatStat, cat);
						}
					 } 
					 
					 if (subcat is TeamSubCategory) {
						List<Team> teams = new List<Team>();
						teams.Add(Team.LOCAL);
						teams.Add(Team.VISITOR);
						
						foreach (Team team in teams) {
							List<Play> subcatPlays;
							TeamTag tag;
							
							tag = new TeamTag{SubCategory=subcat, Value=team};
							subcatPlays = plays.Where(p => p.Teams.Tags.Contains(tag)).ToList();
							GetSubcategoryStats(subcatPlays, subcatStat, team.ToString(),
								stats.TotalCount, out localTeamCount, out visitorTeamCount);
						}
					 }
				}
			}
		}
		
		void GetSubcategoryStats (List<Play> subcatPlays, SubCategoryStat subcatStat, string desc,
			int totalCount, out int localTeamCount, out int visitorTeamCount)
		{
			int count;
			
			count = subcatPlays.Count(); 
			CountPlaysInTeam(subcatPlays, out localTeamCount, out visitorTeamCount);
			PercentualStat pStat = new PercentualStat(desc, count, localTeamCount,
				visitorTeamCount, totalCount);
			subcatStat.AddOptionStat(pStat);
		}
		
		void GetPlayersStats (Project project, List<Play> subcatPlays, string optionName,
			SubCategoryStat subcatStat, Category cat)
		{
			foreach (ISubCategory subcat in cat.SubCategories) {
				PlayerSubCategory playerSubcat;
				Dictionary<Player, int> localPlayerCount = new Dictionary<Player, int>();
				Dictionary<Player, int> visitorPlayerCount = new Dictionary<Player, int>();
				
				if (!(subcat is PlayerSubCategory))
					continue;
				
				playerSubcat = subcat as PlayerSubCategory;
				
				if (playerSubcat.Contains(Team.LOCAL) || playerSubcat.Contains(Team.BOTH)){
					foreach (Player player in project.LocalTeamTemplate) {
						localPlayerCount.Add(player, GetPlayerCount(subcatPlays, player, subcat as PlayerSubCategory));
					}
					subcatStat.AddPlayersStats(optionName, subcat.Name, Team.LOCAL, localPlayerCount);
				}
				
				if (playerSubcat.Contains(Team.VISITOR) || playerSubcat.Contains(Team.BOTH)){
					foreach (Player player in project.VisitorTeamTemplate) {
						visitorPlayerCount.Add(player, GetPlayerCount(subcatPlays, player, subcat as PlayerSubCategory));
					}
					subcatStat.AddPlayersStats(optionName, subcat.Name, Team.VISITOR, visitorPlayerCount);
				}
			}
		}
		
		int GetPlayerCount(List<Play> plays, Player player, PlayerSubCategory subcat)
		{
			PlayerTag tag;
			
			tag = new PlayerTag{SubCategory=subcat, Value=player};
			return plays.Where(p => p.Players.Contains(tag)).Count();
		}
	}
}

