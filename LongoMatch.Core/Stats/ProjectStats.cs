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
		
		public ProjectStats (Project project)
		{
			catStats = new List<CategoryStats>(); 
			
			UpdateStats (project);
			
			ProjectName = project.Description.Title;
			Date = project.Description.MatchDate;
			LocalTeam = project.LocalTeamTemplate.TeamName;
			VisitorTeam = project.VisitorTeamTemplate.TeamName;
			Competition = project.Description.Competition;
			Season = project.Description.Season;
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
		
		public List<CategoryStats> CategoriesStats {
			get {
				return catStats;
			}
		}
		
		void CountPlaysInTeam (List<Play> plays, out int localTeamCount, out int visitorTeamCount) {
			localTeamCount = plays.Where(p => p.Team == Team.LOCAL || p.Team == Team.BOTH).Count();
			visitorTeamCount = plays.Where(p => p.Team == Team.VISITOR || p.Team == Team.BOTH).Count();
		}
		
		void UpdateStats (Project project) {
			catStats.Clear();
			
			foreach (Category cat in project.Categories) {
				CategoryStats stats;
				List<Play> plays;
				int localTeamCount, visitorTeamCount;
				
				plays = project.PlaysInCategory (cat);
				CountPlaysInTeam(plays, out localTeamCount, out visitorTeamCount);
				stats = new CategoryStats(cat.Name, plays.Count, localTeamCount, visitorTeamCount);
				catStats.Add (stats);
				
				foreach (ISubCategory subcat in cat.SubCategories) {
					SubCategoryStat subcatStat;
					
					if (subcat is PlayerSubCategory)
						continue;
						
					subcatStat = new SubCategoryStat(subcat.Name);
					stats.AddSubcatStat(subcatStat);
					
					 if (subcat is TagSubCategory) {
						foreach (string option in subcat.ElementsDesc()) {
							List<Play> subcatPlays;
							int count;
							StringTag tag;
							
							tag = new StringTag();
							tag.SubCategory = subcat;
							tag.Value = option;
							
							subcatPlays = plays.Where(p => p.Tags.Tags.Contains(tag)).ToList();
							count = subcatPlays.Count(); 
							CountPlaysInTeam(subcatPlays, out localTeamCount, out visitorTeamCount);
							PercentualStat pStat = new PercentualStat(option, count, localTeamCount,
								visitorTeamCount, stats.TotalCount);
							subcatStat.AddOptionStat(pStat);
						}
					 } 
					 
					 if (subcat is TeamSubCategory) {
						List<Team> teams = new List<Team>();
						teams.Add(Team.LOCAL);
						teams.Add(Team.VISITOR);
						
						foreach (Team team in teams) {
							List<Play> subcatPlays;
							int count;
							TeamTag tag;
							
							tag = new TeamTag();
							tag.SubCategory = subcat;
							tag.Value = team;
							
							subcatPlays = plays.Where(p => p.Teams.Tags.Contains(tag)).ToList();
							count = subcatPlays.Count(); 
							CountPlaysInTeam(subcatPlays, out localTeamCount, out visitorTeamCount);
							PercentualStat pStat = new PercentualStat(team.ToString(), count, localTeamCount,
								visitorTeamCount, stats.TotalCount);
							subcatStat.AddOptionStat(pStat);
						}
					 } 
				}
			}
		}
	}
}

