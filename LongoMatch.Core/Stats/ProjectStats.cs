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
	public class ProjectStats: IDisposable
	{
		List<CategoryStats> catStats;
		PlaysFilter filter;
		Project project;
		
		public ProjectStats (Project project)
		{
			catStats = new List<CategoryStats>();
			this.project = project;
			
			ProjectName = project.Description.Title;
			Date = project.Description.MatchDate;
			LocalTeam = project.LocalTeamTemplate.TeamName;
			VisitorTeam = project.VisitorTeamTemplate.TeamName;
			Competition = project.Description.Competition;
			Season = project.Description.Season;
			Results = String.Format("{0}-{1}", project.Description.LocalGoals, project.Description.VisitorGoals);
			UpdateStats ();
			UpdateGameUnitsStats ();
		}
		
		public void Dispose () {
			if (HalfField != null)
				HalfField.Dispose ();
			if (Field != null)
				Field.Dispose ();
			if (Goal != null)
				Goal.Dispose ();
			catStats.Clear ();
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
		
		public Image Field {
			get; set;
		}
		
		public Image HalfField {
			get; set;
		}
		
		public Image Goal {
			get; set;
		}
	
		public List<CategoryStats> CategoriesStats {
			get {
				return catStats;
			}
		}
		
		public PlaysFilter Filter {
			set {
				filter = value;
				UpdateStats ();
			}
		}
		
		void UpdateGameUnitsStats () {
		}
		
		void CountPlaysInTeam (List<TimelineEvent> plays, out int localTeamCount, out int visitorTeamCount) {
			localTeamCount = plays.Where(p => p.Team == Team.LOCAL || p.Team == Team.BOTH).Count();
			visitorTeamCount = plays.Where(p => p.Team == Team.VISITOR || p.Team == Team.BOTH).Count();
		}

		public void UpdateStats () {
			catStats.Clear();
			
			Field = project.Dashboard.FieldBackground;
			HalfField = project.Dashboard.HalfFieldBackground;
			Goal = project.Dashboard.GoalBackground;
			
			foreach (AnalysisEventType evt in project.EventTypes.OfType<AnalysisEventType> ()) {
//				CategoryStats stats;
//				List<Event> plays, homePlays, awayPlays, untagged;
//				int localTeamCount, visitorTeamCount;
//				
//				plays = project.PlaysInCategory (cat);
//				if (filter != null) {
//					plays = plays.Where(p => filter.IsVisible (p)).ToList();
//				}
//				homePlays =plays.Where(p => p.Team == Team.LOCAL || p.Team == Team.BOTH).ToList();
//				awayPlays =plays.Where(p => p.Team == Team.VISITOR || p.Team == Team.BOTH).ToList();
//				
//				/* Get the plays where the team is not tagged but we have at least one player from a team tagged */
//				untagged = plays.Where (p=> p.Team ==  Team.NONE).ToList();
//				homePlays.AddRange (untagged.Where (p => p.Players.Where (pt => project.LocalTeamTemplate.List.Contains(pt)).Count() != 0).ToList());
//				awayPlays.AddRange (untagged.Where (p => p.Players.Where (pt => project.VisitorTeamTemplate.List.Contains(pt)).Count() != 0).ToList());
//				
//				stats = new CategoryStats(cat, plays.Count, homePlays.Count(), awayPlays.Count());
//				
//				/* Fill zonal tagging stats */
//				stats.FieldCoordinates = plays.Select (p => p.FieldPosition).Where(p =>p != null).ToList();
//				stats.HalfFieldCoordinates = plays.Select (p => p.HalfFieldPosition).Where(p =>p != null).ToList();
//				stats.GoalCoordinates = plays.Select (p => p.GoalPosition).Where(p =>p != null).ToList();
//				stats.HomeFieldCoordinates = homePlays.Select (p => p.FieldPosition).Where(p =>p != null).ToList();
//				stats.HomeHalfFieldCoordinates = homePlays.Select (p => p.HalfFieldPosition).Where(p =>p != null).ToList();
//				stats.HomeGoalCoordinates = homePlays.Select (p => p.GoalPosition).Where(p =>p != null).ToList();
//				stats.AwayFieldCoordinates = awayPlays.Select (p => p.FieldPosition).Where(p =>p != null).ToList();
//				stats.AwayHalfFieldCoordinates = awayPlays.Select (p => p.HalfFieldPosition).Where(p =>p != null).ToList();
//				stats.AwayGoalCoordinates = awayPlays.Select (p => p.GoalPosition).Where(p =>p != null).ToList();
//				catStats.Add (stats);
			}
		}
		
		void GetSubcategoryStats (List<TimelineEvent> subcatPlays, SubCategoryStat subcatStat, string desc,
			int totalCount, out int localTeamCount, out int visitorTeamCount)
		{
			int count;
			
			count = subcatPlays.Count(); 
			CountPlaysInTeam(subcatPlays, out localTeamCount, out visitorTeamCount);
			PercentualStat pStat = new PercentualStat(desc, count, localTeamCount,
				visitorTeamCount, totalCount);
			subcatStat.AddOptionStat(pStat);
		}
		
		void GetPlayersStats (Project project, List<TimelineEvent> subcatPlays, string optionName,
			SubCategoryStat subcatStat, EventType cat)
		{
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
		}
		
		int GetPlayerCount(List<TimelineEvent> plays, Player player)
		{
			return plays.Where(p => p.Players.Contains(player)).Count();
		}
	}
}

